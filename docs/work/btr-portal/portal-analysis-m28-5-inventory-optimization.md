# BTR Portal Analysis — M28.5 Inventory Optimization Dashboard

**Status:** Analysis complete — ready for Architect review and Product Owner approval before implementation.  
**Scope:** Business analysis, optimization framework, recommendation definitions, KPI definitions, wireframe, and business rules only. No production code.  
**Date:** 2026-06-21  
**Author role:** Analyst  
**Companion document:** [implementation-plan-m28-5-inventory-optimization.md](./implementation-plan-m28-5-inventory-optimization.md)

**Reference documents:** `docs/foundation/PRODUCT.md`, `docs/foundation/DOMAIN.md`, `docs/foundation/LANDSCAPE.md`, `docs/foundation/WORKFLOW.md`, `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/inventory-forecast/feature.md`, `docs/work/btr-portal/portal-analysis-m28-inventory-forecast.md`, `docs/work/btr-portal/implementation-plan-m28-inventory-forecast.md`, `docs/work/btr-portal/implementation-summary-m28-inventory-forecast.md`, `docs/work/btr-portal/M19 Slow Moving & Dead Stock - Analysis.md`, `docs/archive/btr-portal/m21-purchasing-management/M21-purchasing-dashboard-analysis.md`, `docs/work/btr-portal/M22 Branch Warehouse Performance - Analysis.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md`

---

## 1. Executive Summary

BTR Portal has evolved through five business maturity levels:

| Level | Question | Example dashboards |
| ----- | -------- | ------------------ |
| Reporting | What happened? | Inventory Report, Purchasing Report |
| Analytics | How is inventory distributed? | Inventory Dashboard, Slow Moving & Dead Stock |
| Decision Support | What requires attention today? | Executive Dashboard, Alert Center, Purchasing Management |
| Forecasting | What will probably happen? | Sales Forecast (M26), Cash Flow Forecast (M27), Inventory Forecast (M28) |
| **Optimization** | **Given the forecast, what should management do?** | **Inventory Optimization (M28.5)** |

**M28 answers:** *Which products are likely to run out, and when should purchasing review replenishment?*

**M28.5 answers:** *Given forecast, risk, purchasing constraints, and warehouse distribution, what are the highest-priority actions management should take today — purchase, delay, transfer, promote, or review — and why?*

### Key findings

| Finding | Implication for M28.5 |
| ------- | --------------------- |
| M28 already computes ADC, DOS, reorder date, recommended qty, purchase urgency, and Top 10 purchase recommendations | Optimization **extends** M28 outputs — does not recalculate forecast |
| M19 owns obsolescence classification (90/180-day idle, Never Sold) | Overstock reduction and dead-stock recovery **reuse M19 signals** |
| M21 owns qualified backlog, compound dependency, principal inventory no purchase | Purchase delay and posting-first recommendations **cross-read M21** |
| M15/M19 use BrgId-first company aggregation; M22 deferred SKU-level warehouse imbalance | Warehouse balancing requires **new Item × Warehouse grain** for optimization only |
| BTR has no PO entity, budget master, EOQ, or per-SKU lead time | Budget allocation and priority use **configurable defaults** and **rule-based ranking** — no solvers |
| Desktop `MutasiKeluar`/`MutasiMasuk` supports inter-warehouse transfer | Portal recommends transfer pairs — **never creates mutasi transactions** |
| M28 purchase urgency (Critical/High/Medium/Low) is deterministic | M28.5 maps urgency bands to **recommendation categories** and **priority score components** |

### Product intent

At any point during the planning cycle, management should answer:

> **Given our current inventory position, sales forecast, purchasing constraints, supplier lead times, and warehouse distribution, what are the highest-priority actions we should take today to maximize product availability, minimize inventory cost, reduce capital tied up in stock, and improve overall inventory health?**

**Explicit constraints (non-negotiable):**

- Read-only — no automatic purchasing, no Desktop write-back
- Deterministic — every recommendation reproducible from business formulas
- Explainable — every row includes human-readable reason text
- No AI, ML, optimization solvers, or mathematical programming
- Traceable to Forecast (M28), Inventory (M15), Inventory Risk (M19), and Purchasing (M21) artifacts

---

## 2. Business Objective

### Problem

M28 tells management **what may happen** and surfaces indicative reorder hints. Management still must manually synthesize:

- Which purchases matter most when budget is limited
- Whether to buy or transfer between warehouses
- Which replenishment should wait because of overstock or dead stock
- Whether unposted purchases (M21 backlog) should be prioritized before new orders
- How to improve overall inventory health this week

This synthesis is inconsistent, not available in one portal surface, and does not scale across hundreds of SKUs and multiple warehouses.

### Outcome

A dedicated **Inventory Optimization Dashboard** that:

1. Converts forecast and risk signals into **actionable recommendations** (purchase, delay, transfer, promote, review posting).
2. Ranks recommendations by **deterministic priority score** — not probability.
3. Estimates **recommended purchase budget** and **deferrable spend** using rule-based allocation.
4. Surfaces **warehouse rebalancing** opportunities where internal transfer may avoid purchase.
5. Presents an **executive summary** answering *"What should management do today?"*

### Users

| User | Need |
| ---- | ---- |
| Owner / GM | Daily action list — critical purchases, deferrals, capital recovery |
| Purchasing administration | Prioritized reorder queue with quantities and suppliers |
| Inventory administration | Transfer vs purchase decisions; dead/slow stock actions |
| Operations leadership | Warehouse balancing and availability vs cost trade-offs |

### Explicitly out of scope (V1)

- AI / ML recommendation engines or probabilistic scoring
- Automatic purchase order or mutasi creation
- Multi-objective optimization solvers or linear programming
- Supplier lead-time master (reuse M28 default lead time)
- EOQ, safety stock statistical models
- Custom budget entry in portal UI (configurable default budget cap in appsettings only)
- Alert Center / Executive Dashboard integration (deferred)
- Scenario simulation / what-if (future milestone hooks only)

---

## 3. Phase 1 — Existing Business Capability Analysis

### 3.1 Inventory Dashboard (M15)

**Source:** `DashboardInventoryAggregator` → `BTRPD_InventoryKpi`, `BTRPD_InventoryBreakdown`

| Capability | Rule | M28.5 reuse |
| ---------- | ---- | ----------- |
| Total Inventory Value | `SUM(Hpp × Qty)` BrgId-first | Denominator; health KPI; budget context |
| BrgId-first grouping | Sum qty across warehouses | Company-level recommendations |
| In-Transit exclusion | `WarehouseName = "In-Transit"` excluded | Same |
| Category / Supplier breakdown | Top 10 concentration | Strategic product context — supplier grouping for batch purchase review |

**M28.5 does not duplicate composition analytics.**

### 3.2 Slow Moving & Dead Stock Dashboard (M19)

**Source:** `DashboardInventoryRiskAggregator` → `BTRPD_InventoryRisk*`

| Capability | Rule | M28.5 reuse |
| ---------- | ---- | ----------- |
| Active | Last sale ≤ 89 days | Eligible for purchase recommendations |
| Slow Moving | 90–179 days idle | **Delay purchase**, **promotion review** triggers |
| Dead Stock | ≥ 180 days idle | **Do not reorder**, **clearance review** triggers |
| Never Sold | No Faktur history | **Do not reorder** |
| At-Risk Inventory % | Never + Slow + Dead ÷ total value | Overall health KPI |
| Attention list | Item × signal | Cross-link drill-down |

**Classification authority:** M19 remains authoritative for backward-looking movement health.

### 3.3 Inventory Forecast Dashboard (M28)

**Source:** `DashboardInventoryForecastAggregator` → `BTRPD_InventoryForecast*`

| Capability | Rule | M28.5 reuse |
| ---------- | ---- | ----------- |
| ADC (30-day) | `S₃₀ ÷ 30` from FakturItem | Demand rate for all purchase/transfer logic |
| Days of Supply (DOS) | `Q ÷ ADC` | Stock-out and overstock thresholds |
| Projected Stock-Out Date | `B + CEILING(DOS)` | Urgency timing |
| Reorder Review Date | Stock-out − LT | Purchase timing |
| Recommended Purchase Qty | `MAX(0, CEILING(ADC × (LT + CD) − Q))` | Purchase recommendation qty |
| Purchase Urgency | Critical / High / Medium / Low | Maps to recommendation category |
| Overstock threshold | DOS > 90 (default) | Delay purchase triggers |
| Top 10 risks + recommendations | Priority-ordered | **Seed set** for optimization ranking — expanded in M28.5 |

**M28.5 consumes M28 item-level forecast context in the same refresh cycle — no duplicate ADC calculation.**

### 3.4 Purchasing Dashboard and Purchasing Management (M21)

| Capability | Rule | M28.5 reuse |
| ---------- | ---- | ----------- |
| Grand Total Purchase (MTD) | Current month invoice spend | Budget context — "already committed" |
| Qualified Backlog | `BELUM` + age ≥ 3 days | **Post purchase first** before new order |
| Principal Inventory No Purchase | Top inventory principal, zero MTD purchase | Supports **delay new purchase** for legacy stock principals |
| Compound Dependency | Top spend AND (top inventory OR top at-risk) | Elevates supplier review priority |
| Purchasing Inactivity | Zero purchases mid-month | Company-level replenishment gap context |
| Principal Spend Concentration | Top 10 MTD spend | Batch supplier review grouping |

**No PO entity exists.** Purchase recommendations remain indicative invoice planning hints.

### 3.5 Branch / Warehouse Performance (M22)

| Capability | Rule | M28.5 reuse |
| ---------- | ---- | ----------- |
| Top Warehouse by Inventory | Warehouse-level value ranking | Transfer source candidates |
| Warehouse Inactive With Stock | `IsAktif = false` AND qty > 0 | **Do not transfer from inactive** warehouses |
| Warehouse No Sales With Inventory | Stock without MTD Faktur from warehouse | Transfer source bias (excess at idle site) |
| SKU-level warehouse imbalance | **Deferred in M22** | **M28.5 introduces** for optimization only |

**Warehouse balancing in M28.5 is the first portal SKU × warehouse optimization grain.**

### 3.6 Stock movement and replenishment (Desktop)

| Component | Role | M28.5 use |
| --------- | ---- | --------- |
| `IStokBalanceViewDal` | Item × Warehouse balance | Warehouse DOS and transfer pairs |
| `BTR_StokMutasi` / Mutasi | Inter-warehouse transfer workflow | Evidence path — Desktop `MutasiInfoForm` |
| `KartuStokSummaryDal` | Period movement buckets per warehouse | **Not V1 source** — deferred validation |
| `SaveInvoiceWorker` + PT2 Posting | Purchase → stock | Backlog context via M21 |
| Item master `IsAktif` | Active flag | Exclude inactive from purchase recommendations |

**No Desktop replenishment formula discovered.** Reorder logic in portal derives from M28 forecast policy only.

### 3.7 Existing purchasing priorities (discovered)

| Priority concept | Where defined | M28.5 treatment |
| ---------------- | ------------- | --------------- |
| Stock-out urgency by DOS | M28 `ResolvePurchaseUrgency` | **Reuse** — primary purchase priority |
| Obsolescence by idle days | M19 classification | **Reuse** — delay / do-not-reorder |
| Qualified posting backlog | M21 QualifiedBacklog | **Reuse** — post-before-purchase signal |
| Supplier dependency | M21 CompoundDependency | **Reuse** — supplier batch review elevation |
| Principal with high inventory, no purchase | M21 PrincipalInventoryNoPurchase | **Reuse** — delay purchase signal |
| Overstock by DOS | M28 overstock threshold | **Reuse** — delay / reduce qty |
| Warehouse concentration | M22 Top Warehouse % | Context only — not optimization driver V1 |

**Do not invent parallel rules when equivalent rules exist.**

### 3.8 Reusable calculations — do not duplicate

| Calculation | Reuse from |
| ----------- | ---------- |
| Item forecast (ADC, DOS, reorder, rec qty) | M28 `InventoryForecastPolicy` + aggregator item context |
| Movement classification | M19 classifier |
| Total Inventory Value | M15 item groups |
| Qualified backlog by supplier | M21 snapshot read at refresh |
| Purchase urgency bands | M28 `ResolvePurchaseUrgency` |
| Top-N ranking pattern | M28 `InventoryForecastRiskBuilder` |

---

## 4. Phase 2 — Inventory Optimization Opportunities

### 4.1 Reorder prioritization

**Business question:** Which products should be purchased first?

| Factor | Source | Rule role |
| ------ | ------ | --------- |
| Stock-out risk | M28 DOS ≤ H | Primary urgency |
| Sales velocity | M28 ADC | Tie-break — higher ADC = higher availability impact |
| Supplier lead time | Configurable default LT | Escalate when `DOS ≤ LT` |
| Current inventory | M15 qty | Input to recommended qty |
| Customer demand | M28 consumption (Faktur qty) | Proxy for demand |
| Strategic products | Top 10 by `ADC × Hpp` (value velocity) | +100 priority score boost |

**Output:** Ranked **Purchase Product** recommendations with explainable priority score.

### 4.2 Purchase budget allocation

**Business question:** If purchasing budget is limited, which purchases matter most and which can wait?

| Concept | Rule (V1) |
| ------- | --------- |
| Required budget | `SUM(RecommendedPurchaseQty × Hpp)` for Critical + High urgency items |
| Recommended budget | Required + Medium urgency items |
| Optional / deferrable | Low urgency + overstock-delay items |
| Budget cap | Configurable `InventoryOptimizationDefaultBudgetCapIdr` (optional — when set, cumulative priority sort until cap exhausted marks remainder **Defer**) |

**No optimization algorithm.** Simple **priority-ordered cumulative allocation**:

1. Sort all purchase recommendations by PriorityScore descending.
2. Accumulate `RecQty × Hpp` until cap reached.
3. Items after cap → recommendation category **Defer** with reason *"Exceeds configured review budget — lower priority than items above."*

When no cap configured, show Required/Recommended/Optional totals without deferral.

### 4.3 Warehouse balancing

**Business question:** Can we fix a shortage by moving stock instead of purchasing?

| Condition | Rule |
| --------- | ---- |
| Shortage warehouse | Item × Warehouse DOS ≤ 14 (configurable) AND warehouse ADC > 0 |
| Source warehouse | Same BrgId, another warehouse with DOS > 60 (excess) AND qty transfer ≥ `CEILING(ADC_dest × 7)` |
| Feasibility | Both warehouses active (`IsAktif = true`); exclude In-Transit and IsSpecial |
| Transfer qty hint | `MIN(source_excess_qty, dest_shortage_gap)` where `shortage_gap = CEILING(ADC_dest × (LT + 3) − Q_dest)` |
| Priority | Below critical purchase, above overstock delay |

**Output:** **Transfer Inventory** recommendation — source warehouse → destination warehouse, with qty hint. Management executes mutasi in Desktop.

**Warehouse ADC:** Rolling 30-day Faktur qty where `Faktur.WarehouseId` matches — new warehouse-scoped consumption load (optimization-only grain).

### 4.4 Overstock reduction

**Business question:** Which inventory should not be reordered?

| Recommendation | Trigger |
| -------------- | ------- |
| **Delay Purchase** | M28 overstock (DOS > 90) OR M19 Slow Moving + DOS > 60 |
| **Reduce Purchase Quantity** | MTD purchase to supplier > 0 AND M28 overstock on same item |
| **Promote / Campaign Review** | M19 Slow Moving + Active ADC > 0 + DOS > 60 |
| **Bundle Products Review** | Same supplier has ≥ 2 overstock SKUs (supplier-level grouping) |
| **Do Not Reorder** | M19 Dead Stock or Never Sold |

### 4.5 Inventory health improvement

**Business question:** What actions improve overall inventory quality?

| Action | Trigger | Expected benefit |
| ------ | ------- | ---------------- |
| Clearance review | Dead stock value in Top 10 M19 | Recover capital |
| Post pending purchase | M21 QualifiedBacklog + M28 stock-out on same supplier | Improve availability without new PO |
| Transfer to high-velocity warehouse | Warehouse imbalance pair | Improve turnover at selling site |
| Delay intake on slow SKUs | Slow moving + overstock DOS | Reduce capital tie-up |
| Prioritize critical stock-out | DOS ≤ 7 | Avoid lost sales |

**Overall Inventory Health Score:** Reuse M28 `InventoryHealthScore` — displayed as context KPI, not recomputed.

---

## 5. Phase 3 — Recommendation Categories

Every recommendation maps to exactly one category. Categories drive color, sort weight, and executive summary counts.

| Category | Business meaning | Typical management response | Default sort weight |
| -------- | ---------------- | --------------------------- | ------------------- |
| **Critical** | Immediate attention — stock-out imminent or overdue reorder review | Purchase or transfer today; escalate to GM | 1000 |
| **High** | Address this week — meaningful availability or capital risk | Schedule purchasing / inventory meeting | 750 |
| **Medium** | Monitor closely — action within planning horizon | Include in weekly review | 500 |
| **Low** | Informational — no immediate action | Awareness only | 250 |

### Category assignment rules

| Recommendation type | Critical | High | Medium | Low |
| ------------------- | -------- | ---- | ------ | --- |
| Purchase Product | DOS ≤ 7 OR reorder date < B | 8 ≤ DOS ≤ 14 | 15 ≤ DOS ≤ 30 | DOS > 30 and not overstock |
| Delay Purchase | — | Overstock value ≥ Top 10 item value floor AND Slow Moving | Overstock DOS only | Minor overstock |
| Transfer Inventory | Dest DOS ≤ 7 | Dest DOS ≤ 14 | Dest DOS ≤ 30 | — |
| Post Purchase First | Stock-out SKU supplier has qualified backlog | Backlog exists, DOS ≤ 14 | Backlog exists, DOS > 14 | — |
| Clearance Review | Dead stock value ≥ company dead stock P75 | Dead stock Top 10 | Slow moving high value | — |
| Do Not Reorder | Dead / Never Sold (always at least Medium) | — | Dead / Never Sold default | — |
| Defer Purchase | — | — | Budget cap exceeded | — |

**Every row includes `ReasonText`** — plain-language explanation built from rule inputs (DOS, LT, ADC, supplier backlog flag, warehouse names).

---

## 6. Phase 4 — Dashboard Wireframe

Text layout only. Route: `/dashboard/inventory-optimization`.

```text
┌─────────────────────────────────────────────────────────────────────────────┐
│  Inventory Optimization Dashboard                            [↻ Refresh]    │
│  Action recommendations from forecast, risk, and purchasing context         │
│  As of: {GeneratedAt}  ·  Planning horizon: {H} days  ·  Budget cap: {Cap} │
├─────────────────────────────────────────────────────────────────────────────┤
│  EXECUTIVE SUMMARY — Today's Recommendations                                │
│  "Purchase {N} critical products · Delay {N} products · Transfer {N}       │
│   pairs · Defer {N} purchases · Review {N} dead stock items ·               │
│   Recommended budget Rp {X} · Recoverable capital Rp {Y}"                     │
├─────────────────────────────────────────────────────────────────────────────┤
│  KPI ROW 1 — Overall Health                                                 │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐       │
│  │ Inventory    │ │ Critical     │ │ Recommended  │ │ Deferrable   │       │
│  │ Health Score │ │ Actions      │ │ Purchase     │ │ Spend        │       │
│  │ (M28 link)   │ │ Count        │ │ Budget       │ │              │       │
│  └──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘       │
├─────────────────────────────────────────────────────────────────────────────┤
│  KPI ROW 2 — Action Mix                                                     │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐       │
│  │ Purchase     │ │ Delay        │ │ Transfer     │ │ Clearance    │       │
│  │ Now Count    │ │ Count        │ │ Count        │ │ Review Count │       │
│  └──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘       │
├─────────────────────────────────────────────────────────────────────────────┤
│  CHART 1 — Priority Score Distribution (full width)                         │
│  Horizontal bar: count of recommendations by category (Critical→Low)        │
├─────────────────────────────────────────────────────────────────────────────┤
│  CHART 2 — Business Impact Summary (half)  │  CHART 3 — Action Heat (half) │
│  Stacked value: Purchase vs Delay vs       │  Recommendation type ×         │
│  Transfer savings vs Recoverable dead stock  │  category grid (counts)      │
├─────────────────────────────────────────────────────────────────────────────┤
│  TOP OPTIMIZATION ACTIONS (table — max 25 visible, paginated)               │
│  Columns: Priority | Category | Action | Item/Pair | Reason | Impact | →    │
│  Sorted by PriorityScore descending                                         │
├─────────────────────────────────────────────────────────────────────────────┤
│  RECOMMENDED REORDER LIST (table — purchase actions only, Top 15)           │
│  Columns: Item | Supplier | Rec Qty | Est Cost | DOS | Reorder Date | Cat. │
├─────────────────────────────────────────────────────────────────────────────┤
│  WAREHOUSE REBALANCING (table — transfer actions, Top 10)                    │
│  Columns: Item | From WH | To WH | Transfer Qty | Dest DOS | Category       │
├─────────────────────────────────────────────────────────────────────────────┤
│  OVERSTOCK & DELAY PURCHASING (table — Top 10)                                │
│  Columns: Item | Supplier | DOS | Movement Class | Action | Reason           │
├─────────────────────────────────────────────────────────────────────────────┤
│  DEAD STOCK RECOVERY (table — Top 10)                                       │
│  Columns: Item | Value | Idle Days | Recommended Action | Category          │
├─────────────────────────────────────────────────────────────────────────────┤
│  FOOTER — Traceability                                                      │
│  "Recommendations derive from Inventory Forecast (M28), Inventory Risk (M19),│
│   Purchasing Management (M21). Portal does not execute purchases or         │
│   transfers. Confirm in BTR Desktop before acting."                         │
│  Links: Inventory Forecast · Inventory Risk · Purchasing Mgmt · Reports     │
└─────────────────────────────────────────────────────────────────────────────┘
```

**Navigation placement:** Sidebar under **Dashboard → Inventory Optimization** (after Inventory Forecast, before Purchasing).

**Dashboard relationships:**

| Dashboard | Relationship |
| --------- | ------------ |
| Inventory Forecast (M28) | **Upstream forecast** — DOS, ADC, reorder hints |
| Inventory Risk (M19) | **Obsolescence authority** — delay / clearance |
| Purchasing Management (M21) | **Posting backlog** — post-first recommendations |
| Inventory (M15) | **Value denominator** |
| Branch / Warehouse (M22) | **Warehouse master context** — active/inactive rules |

---

## 7. Phase 5 — Recommendation Definitions

### 7.1 Purchase Product

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Management should consider purchasing this SKU to avoid stock-out |
| **Trigger** | M28 forecast-eligible AND `ADC > 0` AND (`DOS ≤ H` OR reorder date ≤ B + 7) AND NOT M19 Dead/Never Sold |
| **Business rules** | Rec qty = M28 `RecommendedPurchaseQty`; urgency = M28 `ResolvePurchaseUrgency` |
| **Priority calculation** | `CategoryWeight + ValueComponent + LeadTimePenalty + StrategicBoost` (see §9) |
| **Expected benefit** | Avoid lost sales; maintain availability |
| **Management interpretation** | *"Order soon — stock may run out before lead time."* |
| **Drill-down** | Inventory Report `?q={item}` → Desktop purchase entry |

**Example row:**

| Field | Value |
| ----- | ----- |
| Action | Purchase Product |
| Item | ABC Shampoo 250ml |
| Reason | Projected stock-out in 8 days; lead time 12 days; reorder review overdue |
| Category | Critical |
| Impact | Est. purchase Rp 15,000,000 |

### 7.2 Defer Purchase

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Lower-priority purchase that exceeds configured budget cap |
| **Trigger** | Would qualify as Purchase Product but cumulative priority sort exceeded budget cap |
| **Business rules** | Same qty/cost as underlying purchase recommendation |
| **Priority calculation** | Inherits underlying score; category forced to Medium |
| **Expected benefit** | Focus limited budget on highest-impact SKUs |
| **Management interpretation** | *"Buy only if budget allows after critical items."* |
| **Drill-down** | Inventory Forecast dashboard — item row |

### 7.3 Delay Purchase

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Do not reorder now — excess cover or slow movement |
| **Trigger** | M28 overstock (`DOS > 90`) OR (M19 Slow Moving AND `DOS > 60`) |
| **Business rules** | Suppresses purchase recommendation for item |
| **Priority calculation** | Category High if slow moving + value ≥ P75; else Medium |
| **Expected benefit** | Reduce capital tie-up; avoid deepening overstock |
| **Management interpretation** | *"Hold replenishment — stock lasts longer than demand warrants."* |
| **Drill-down** | Inventory Risk dashboard |

### 7.4 Reduce Purchase Quantity

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | If purchasing, buy less than full replenishment hint |
| **Trigger** | MTD purchase exists for item's supplier AND M28 overstock on item |
| **Business rules** | Suggested qty = `MAX(0, CEILING(RecommendedPurchaseQty × 0.5))` — 50% reduction hint |
| **Priority calculation** | Medium default |
| **Expected benefit** | Limit additional overstock from repeat intake |
| **Management interpretation** | *"We already bought this month — consider half the normal replenishment."* |
| **Drill-down** | Purchasing Report filtered by supplier |

### 7.5 Transfer Inventory

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Move stock between warehouses instead of purchasing |
| **Trigger** | Warehouse imbalance pair per §4.3 |
| **Business rules** | Transfer qty hint deterministic; no mutasi created |
| **Priority calculation** | Category from destination DOS; +50 if avoids Critical purchase same item |
| **Expected benefit** | Improve availability without cash outflow |
| **Management interpretation** | *"Warehouse B is low; Warehouse A has excess — consider internal transfer."* |
| **Drill-down** | Inventory Report (warehouse column) → Desktop Mutasi |

### 7.6 Post Purchase First

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Complete PT2 posting before placing new order |
| **Trigger** | M28 stock-out risk item AND supplier in M21 QualifiedBacklog set |
| **Business rules** | Cross-read M21 at refresh — supplier name match (trimmed, case-insensitive) |
| **Priority calculation** | Critical if DOS ≤ 7; else High |
| **Expected benefit** | Stock may already be on order but not sellable |
| **Management interpretation** | *"Post pending invoice first — goods may already be in pipeline."* |
| **Drill-down** | Purchasing Management attention list → Desktop PT2 |

### 7.7 Promote / Campaign Review

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Sales push to accelerate turnover on slow-moving active stock |
| **Trigger** | M19 Slow Moving AND M28 `ADC > 0` AND `DOS > 60` |
| **Business rules** | Informational — no qty formula |
| **Priority calculation** | Medium; High if value in Top 10 slow moving |
| **Expected benefit** | Improve turnover; reduce future dead stock |
| **Management interpretation** | *"Stock is moving slowly but not dead — consider promotion."* |
| **Drill-down** | Inventory Risk → item attention |

### 7.8 Bundle Products Review

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Multiple overstock SKUs from same supplier — review as bundle |
| **Trigger** | Supplier with ≥ 2 items in overstock set |
| **Business rules** | One row per supplier; lists up to 3 item names in reason |
| **Priority calculation** | Medium |
| **Expected benefit** | Supplier negotiation, bundled promotion |
| **Management interpretation** | *"Three slow products from same principal — negotiate bundle deal or return."* |
| **Drill-down** | Purchasing Management → supplier |

### 7.9 Clearance Review

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Dead stock requiring write-off, return, or clearance pricing |
| **Trigger** | M19 Dead Stock classification |
| **Business rules** | Reuse M19 value and idle days |
| **Priority calculation** | Critical/High by value rank in dead stock Top 10 |
| **Expected benefit** | Recover capital; free warehouse space |
| **Management interpretation** | *"No sales 180+ days — clearance or return to supplier."* |
| **Drill-down** | Inventory Risk Top Dead Stock |

### 7.10 Do Not Reorder

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Purchasing should not replenish this SKU |
| **Trigger** | M19 Dead Stock OR Never Sold OR `IsAktif = false` |
| **Business rules** | Supersedes any M28 purchase hint for same item |
| **Priority calculation** | Medium minimum |
| **Expected benefit** | Prevent capital accumulation on non-performing SKUs |
| **Management interpretation** | *"Do not buy — demand failure or discontinued."* |
| **Drill-down** | Inventory Risk dashboard |

---

## 8. Phase 6 — Optimization Rules

### 8.1 Priority score (deterministic integer)

Every recommendation receives **PriorityScore** — an integer for sort order only. **Not a probability.**

```
PriorityScore = CategoryWeight
              + ValueComponent
              + LeadTimePenalty
              + StrategicBoost
              + TypeModifier
```

| Component | Formula |
| --------- | ------- |
| **CategoryWeight** | Critical=1000, High=750, Medium=500, Low=250 |
| **ValueComponent** | `MIN(500, FLOOR(ImpactValueIdr / 1_000_000) × 10)` — cap 500 |
| **LeadTimePenalty** | 200 if `DOS ≤ DefaultLeadTimeDays` |
| **StrategicBoost** | 100 if item in company Top 10 by `ADC × Hpp` |
| **TypeModifier** | Post Purchase First +150; Transfer (avoids critical purchase) +100; Clearance +50 |

**Tie-break:** Impact value descending → item name ascending.

### 8.2 Reorder priority list

Purchase recommendations sorted by:

1. M28 urgency rank (Critical → Low)
2. PriorityScore descending
3. Recommended purchase cost descending

### 8.3 Purchase budget allocation

| Metric | Formula |
| ------ | ------- |
| **Required Purchase Budget** | `SUM(RecQty × Hpp)` where category ∈ {Critical, High} |
| **Recommended Purchase Budget** | Required + `SUM(RecQty × Hpp)` where category = Medium |
| **Deferrable Spend** | `SUM(RecQty × Hpp)` for Delay + Defer + overstock items that had purchase hint |
| **Budget cap deferral** | When cap C configured: walk priority-sorted purchase list, accumulate cost until > C → remainder = Defer |

### 8.4 Warehouse balance rules

| Rule ID | Rule |
| ------- | ---- |
| IO-01 | Destination warehouse DOS ≤ `WarehouseShortageDosDays` (default 14) |
| IO-02 | Source warehouse DOS ≥ `WarehouseExcessDosDays` (default 60) for same BrgId |
| IO-03 | Transfer qty = `MIN(Q_source − ADC_source×60, CEILING(ADC_dest×(LT+3) − Q_dest))` when positive |
| IO-04 | Exclude In-Transit, IsSpecial, inactive warehouses |
| IO-05 | Max 10 transfer recommendations materialized |
| IO-06 | Warehouse ADC from Faktur qty grouped by `WarehouseId` + `BrgId`, 30-day window |

### 8.5 Purchase delay rules

| Rule ID | Rule |
| ------- | ---- |
| IO-10 | Delay when M28 overstock threshold met (`DOS > 90` default) |
| IO-11 | Delay when M19 Slow Moving AND `DOS > 60` |
| IO-12 | Reduce qty when MTD purchase to supplier AND item overstock |
| IO-13 | Do Not Reorder when Dead, Never Sold, or inactive item |
| IO-14 | M21 Principal Inventory No Purchase elevates Delay to High for that supplier's overstock SKUs |

### 8.6 Health improvement rules

| Rule ID | Rule |
| ------- | ---- |
| IO-20 | Reuse M28 InventoryHealthScore — no separate optimization score |
| IO-21 | Recoverable capital = M19 Dead Stock Value + Slow Moving Value (Top 10 clearance candidates) |
| IO-22 | Post Purchase First when QualifiedBacklog supplier matches stock-out SKU supplier |

### 8.7 Scope and traceability rules

| Rule ID | Rule |
| ------- | ---- |
| IO-30 | Extend Inventory Risk snapshot refresh — same cadence as M28 (~60 min) |
| IO-31 | Reuse M28 item forecast context — no second ADC pass |
| IO-32 | `CurrentInventoryValue` traceability = IFR-50 (M15 match) |
| IO-33 | Read-only — no PO or mutasi creation |
| IO-34 | Max 25 top optimization actions; max 15 reorder; max 10 per specialized table |
| IO-35 | Every row includes `RuleId`, `ReasonText`, `ReportRoute` |

---

## 9. Phase 7 — Executive Summary

Server-composed plain-language block at top of dashboard.

### Template

```
Today's Recommendations (as of {BusinessDate}):

• Purchase {CriticalPurchaseCount} critical products (est. Rp {RequiredBudget})
• Delay purchasing for {DelayCount} products
• Transfer inventory for {TransferCount} warehouse pairs
• Post {PostFirstCount} pending purchases before new orders
• Defer {DeferCount} lower-priority purchases{if BudgetCap}(budget cap Rp {Cap}){/if}
• Review {ClearanceCount} dead stock items (Rp {RecoverableCapital} recoverable)

Highest priority: {TopActionSummary}
```

### TopActionSummary examples

- *"Purchase ABC Shampoo — stock-out in 5 days (Critical)"*
- *"Post pending invoice for Supplier XYZ before reordering DEF Item"*
- *"Transfer GHI Product from Warehouse Jakarta to Surabaya"*

### Questions answered

| Question | Summary element |
| -------- | ---------------- |
| What to buy today? | Critical purchase count + reorder list |
| What to postpone? | Delay count + defer count |
| What to move internally? | Transfer count |
| What capital to recover? | Clearance count + recoverable Rp |
| What is the budget impact? | Required / recommended budget KPIs |

---

## 10. Phase 8 — User Experience

### 10.1 Layout principles

**Action-first** — unlike M28 (forecast-first), M28.5 leads with *what to do*:

1. Executive summary (today's actions)
2. Health + budget KPIs
3. Action mix KPIs
4. Priority distribution + impact charts
5. Top Optimization Actions (unified queue)
6. Specialized tables (reorder, transfer, delay, clearance)
7. Traceability footer

### 10.2 Recommendation cards (Top Actions table)

Each row behaves as a **recommendation card** in table form:

| Element | UX |
| ------- | -- |
| Category badge | Color: Critical=red, High=orange, Medium=yellow, Low=gray |
| Action icon | Cart (purchase), Pause (delay), Arrow (transfer), Post (posting), Tag (clearance) |
| Reason | Always visible — no hover required |
| Impact | Currency or qty — right-aligned |
| Drill-down | Chevron → report or sibling dashboard |

### 10.3 Priority visualization

- **Priority Score Distribution** — horizontal bar by category count
- **Action Heat Map** — 5 recommendation types × 4 categories grid (CSS grid, counts in cells)
- **Business Impact Summary** — stacked bar: purchase spend, deferrable spend, recoverable capital

### 10.4 Audience considerations

| Audience | Primary sections |
| -------- | ---------------- |
| Director / Owner | Executive summary, Required budget, Critical count |
| Purchasing Manager | Recommended Reorder List, Post Purchase First rows |
| Inventory Manager | Warehouse Rebalancing, Delay/Overstock tables |

### 10.5 Drill-down behavior

| Action type | Primary | Secondary |
| ----------- | ------- | --------- |
| Purchase | Inventory Report `?q=` | Inventory Forecast |
| Delay / Clearance | Inventory Risk | Inventory Report |
| Transfer | Inventory Report (warehouse) | Desktop Mutasi |
| Post First | Purchasing Management | Purchasing Report |
| Defer | Inventory Optimization (in-page) | Inventory Forecast |

**Investigation path:** Recommendation → reason → evidence report → Desktop action.

---

## 11. KPI Definitions

### 11.1 Overall Inventory Health Score

| Attribute | Definition |
| --------- | ---------- |
| **Source** | M28 `InventoryHealthScore` — copied at refresh |
| **Formula** | 100 − weighted penalties (stock-out %, overstock %, at-risk %) |
| **Interpretation** | Higher = healthier planning position |
| **Drill-down** | Inventory Forecast dashboard |

### 11.2 Critical Actions Count

| Attribute | Definition |
| --------- | ---------- |
| **Formula** | Count of recommendations with category = Critical |
| **Interpretation** | Items needing same-day management attention |

### 11.3 Recommended Purchase Budget

| Attribute | Definition |
| --------- | ---------- |
| **Formula** | Required + Medium purchase est. cost (§8.3) |
| **Interpretation** | Indicative cash outflow if all recommended purchases executed |

### 11.4 Deferrable Spend

| Attribute | Definition |
| --------- | ---------- |
| **Formula** | Sum of est. purchase cost for Delay + Defer recommendations |
| **Interpretation** | Capital that could be avoided or postponed |

### 11.5 Recoverable Capital

| Attribute | Definition |
| --------- | ---------- |
| **Formula** | Dead stock value + high-value slow moving in clearance set (IO-21) |
| **Interpretation** | Upper-bound capital recovery if clearance succeeds |

### 11.6 Action Mix Counts

Purchase Now, Delay, Transfer, Clearance Review — counts by recommendation type in materialized snapshot.

### 11.7 Traceability rules

| Rule ID | Rule |
| ------- | ---- |
| IO-50 | `InventoryHealthScore` = M28 KPI same refresh |
| IO-51 | Sum of Critical+High purchase est. cost ≤ sum of underlying M28 rec qty × Hpp for same items |
| IO-52 | Do Not Reorder items never appear in Recommended Reorder List |

---

## 12. Future Extensibility

V1 architecture must support future milestones without redesign:

| Future capability | Extension point |
| ----------------- | --------------- |
| Purchase scenario simulation | Client reads snapshot + applies alternate budget cap locally |
| Budget what-if | User-editable cap in UI — filter only, no re-aggregate |
| Supplier comparison | Add supplier dimension to recommendation rows |
| Lead-time optimization | Replace default LT with master data column |
| Warehouse network optimization | Expand transfer pairs to multi-hop suggestions |
| Inventory policy simulation | New policy module reading same item context |
| EOQ analysis | Optional qty formula in policy — separate column |
| Multi-objective optimization | **Still rule-based** — weighted score config expansion |
| AI-assisted explanation | Optional narrative layer on existing `ReasonText` — never replaces rules |
| Alert Center | Read `BTRPD_InventoryOptimizationAction` Top N |
| Executive promotion | Subset KPIs on Management Attention Center |

---

## 13. Open Questions for Product Owner

| # | Question | Default if unanswered |
| - | -------- | --------------------- |
| 1 | Configure default budget cap in V1? | **Optional** — null = no deferral |
| 2 | Warehouse balancing in V1? | **Yes** — Top 10 pairs, optimization-only grain |
| 3 | Reduce purchase qty at 50%? | **Yes** (IO-12) |
| 4 | Promote to Executive / Alert Center in V1? | **No** — deferred |
| 5 | Max rows in unified Top Actions table? | **25** paginated |

---

## Document Maintenance

When M28.5 is implemented, promote concepts to `docs/features/inventory-optimization/feature.md` and update `btr-portal-domain.md` Section 12 roadmap.

**Success criterion:** Product Owner can approve recommendation framework, priority rules, and wireframe without reading source code.
