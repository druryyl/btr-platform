# BTR Portal Analysis — M28 Inventory Forecast Dashboard

**Status:** Analysis complete — ready for Architect review and Product Owner approval before implementation.  
**Scope:** Business analysis, forecasting model design, KPI definitions, wireframe, business rules, inventory risk concepts, and purchasing recommendation concepts only. No production code.  
**Date:** 2026-06-21  
**Author role:** Analyst  
**Companion document:** [implementation-plan-m28-inventory-forecast.md](./implementation-plan-m28-inventory-forecast.md)

**Reference documents:** `docs/foundation/PRODUCT.md`, `docs/foundation/DOMAIN.md`, `docs/foundation/LANDSCAPE.md`, `docs/foundation/WORKFLOW.md`, `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/btr-portal/btr-portal-architecture.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md`, `docs/features/materialized-dashboard/materialized-dashboard-architecture.md`, `docs/features/sales-forecast/feature.md`, `docs/features/cash-flow-forecast/feature.md`, `docs/work/btr-portal/M19 Slow Moving & Dead Stock - Analysis.md`, `docs/work/btr-portal/portal-analysis-m26-sales-forecast.md`, `docs/work/btr-portal/portal-analysis-m27-cash-flow-forecast.md`, `docs/archive/btr-portal-api-scaffolding/portal-analysis-m13-m15-final.md`

---

## 1. Executive Summary

BTR Portal has evolved through four product layers:

| Layer | Question | Example dashboards |
| ----- | -------- | ------------------ |
| Reporting | What happened? | Inventory Report, Purchasing Report |
| Analytics | How is inventory distributed? | Inventory Dashboard, Slow Moving & Dead Stock |
| Decision Support | What requires attention today? | Executive Dashboard, Alert Center, Purchasing Management |
| **Forecasting** | What is likely to happen before it becomes a problem? | Sales Forecast (M26), Cash Flow Forecast (M27), **Inventory Forecast (M28)** |

**M28 extends forecasting from sales and cash collection into inventory planning.**

Management today can answer:

- How much capital is in stock (Inventory Dashboard)
- Which stock is slow moving or dead (Slow Moving & Dead Stock Dashboard)
- How much was purchased this month (Purchasing Dashboard)
- Which suppliers require purchasing attention (Purchasing Management Dashboard)

Management cannot yet answer:

> **"Which products are likely to run out of stock, which are likely to become overstocked or dead stock, when should we reorder, and what inventory risks require immediate management attention?"**

### Key findings

| Finding | Implication for M28 |
| ------- | ------------------- |
| Inventory position rules are mature — `DashboardInventoryItemGroupBuilder` + `IStokBalanceViewDal` power M15 and M11 with BrgId-first grouping, HPP × Qty, In-Transit exclusion | Forecast position layer **must reuse** the same item groups — no new valuation rules |
| M19 already classifies movement health via `IBrgLastFakturDal` + `DashboardInventoryRiskAggregator` (90/180-day idle thresholds) | Forward-looking **obsolescence risk** reuses M19 classification; M28 adds **consumption-based depletion** |
| Item-level sales consumption data exists in `BTR_FakturItem` / `IFakturBrgViewDal` (Desktop `FakturBrgInfoForm`) but is **not aggregated in Portal** | M28 needs a new **item consumption aggregation** during snapshot refresh — not a new Desktop report |
| `KartuStokSummaryDal` uses `BTR_StokMutasi` with warehouse + period scope — Desktop-only, warehouse-grain | V1 company-level forecast uses **Faktur sales qty** as consumption proxy (same demand signal as M19 last-sale); mutasi ledger deferred |
| No lead time, safety stock, EOQ, or reorder-point master data in BTR item master (`BTR_Brg` has `IsAktif` only) | V1 reorder recommendations use **configurable default lead time** and **coverage days** — not automatic PO creation |
| M26/M27 established **Current Pace (linear extrapolation)** as the approved deterministic forecast pattern | M28 applies the **inventory analog**: rolling average daily consumption + linear stock depletion |
| Inventory and Inventory Risk snapshots refresh every **60 minutes** | Forecast extends the same refresh path for temporal consistency with M15/M19 |
| Purchasing Management already surfaces supplier dependency and qualified backlog | M28 **complements** purchasing dashboards with SKU-level stock-out timing — does not replace M21 |

### Product intent

At any point during the month, management should answer:

> **"Which products are likely to run out of stock before the planning horizon ends, which products are building excess relative to demand, when should purchasing review replenishment, and what inventory risks require immediate attention?"**

---

## 2. Business Objective

### Problem

Inventory and Inventory Risk dashboards describe **current state** — capital on hand and items that have already stopped moving. Purchasing dashboards describe **what was bought** and **posting backlog**. None of these surfaces answer whether active SKUs will **run out** or **accumulate excess** in the near future based on observed sales consumption.

Warehouse managers and owners currently perform mental calculations: average recent sales versus qty on hand. This is inconsistent, not available in the portal, and does not scale across hundreds of SKUs.

### Outcome

A dedicated **Inventory Forecast Dashboard** that:

1. Projects **days of supply** and **stock-out timing** for active inventory items using recent sales consumption.
2. Surfaces **stock-out risk**, **overstock risk**, and **future slow-moving risk** before the event occurs.
3. Provides **purchasing decision support** — recommended reorder review date and indicative purchase quantity — without creating purchase transactions.
4. Shows **company-level projected inventory value** at the planning horizon.
5. Presents a deterministic **inventory health score** and ranked risk list for management review.

### Users

| User | Need |
| ---- | ---- |
| Owner / GM | Anticipate working-capital trap (overstock) and lost sales (stock-out) before month-end |
| Inventory administration | Prioritize SKUs requiring replenishment or clearance review |
| Purchasing administration | Know which principals/items need purchase planning and when |
| Operations leadership | Understand whether inventory coverage matches sales pace |

### Explicitly out of scope (V1)

- AI / ML / statistical demand forecasting or probability scoring
- Automatic purchase order creation or Desktop write-back
- Per-warehouse or per-depo forecast dashboards (company-level BrgId-first only)
- Custom period or horizon selection (fixed 30-day planning horizon)
- Working-day or holiday-adjusted consumption
- Lead time per supplier/SKU from master data (no BTR source — configurable default only)
- EOQ, safety stock formulas requiring historical variance
- Seasonal demand models (year-over-year comparison)
- Kartu Stok / `BTR_StokMutasi` movement ledger in portal
- Alert Center / Executive Dashboard integration
- Item-level sales report in portal (evidence gap remains — Desktop `FakturBrgInfoForm`)
- ABC classification dashboard

---

## 3. Phase 1 — Existing Business Capability Analysis

### 3.1 Inventory Dashboard (M6 / M15)

**Source:** `DashboardInventoryAggregator` → `BTRPD_InventoryKpi`, `BTRPD_InventoryBreakdown`  
**Worker:** `RefreshDashboardInventorySnapshotWorker` (~60 min)

| Capability | Rule | M28 reuse |
| ---------- | ---- | --------- |
| **Total Inventory Value** | `SUM(Hpp × Qty)` per BrgId group, warehouses summed | Denominator; traceability anchor (IFR-50) |
| **Total Item** | Count BrgId with aggregated `Qty > 0` | Context KPI |
| **BrgId-first grouping** | Group by item, sum qty across warehouses | **Mandatory** — same grain for forecast |
| **In-Transit exclusion** | `WarehouseName = "In-Transit"` excluded | Same |
| **Zero-qty exclusion** | Only `Qty > 0` items | Same |
| **Unknown dimensions** | Blank category/supplier → `"Unknown"` | Same on risk rows |
| **Category / Supplier breakdown** | Roll up inventory value by dimension | Context charts — not duplicated as primary story |
| **Top 10 rankings** | By inventory value | Pattern reference for Top 10 risk tables |

**M28 does not duplicate Inventory Dashboard composition analytics.**

### 3.2 Slow Moving & Dead Stock Dashboard (M19)

**Source:** `DashboardInventoryRiskAggregator` → `BTRPD_InventoryRisk*`  
**Worker:** `RefreshDashboardInventoryRiskSnapshotWorker` (~60 min)

| Capability | Rule | M28 reuse |
| ---------- | ---- | --------- |
| **Last Faktur Date** | `MAX(FakturDate)` per BrgId, non-void Faktur only | Reuse `IBrgLastFakturDal` |
| **Active** | Last sale ≤ 89 days ago | Exclude from stock-out forecast when already idle? **No** — active items get consumption forecast; idle items get obsolescence path |
| **Slow Moving** | 90–179 days idle | **Future slow-moving risk** when ADC > 0 but DOS high |
| **Dead Stock** | ≥ 180 days idle | Excluded from stock-out/reorder logic — obsolescence only |
| **Never Sold** | No Faktur history | Excluded from consumption forecast — flagged as demand failure |
| **At-Risk Inventory %** | Never + Slow + Dead value ÷ total inventory value | Context KPI on forecast dashboard |
| **Attention list** | Item × signal (DeadStock, SlowMoving, NeverSold) | Cross-link drill-down to `/dashboard/inventory-risk` |
| **Shared item builder** | `DashboardInventoryItemGroupBuilder` | **Same builder** in forecast aggregator |

**Classification authority:** Retur and non-sales outflows do not reset M19 clock — M28 consumption uses **gross Faktur qty sold** (`QtyJual`) for demand rate, consistent with sales-outflow signal.

### 3.3 Stock balance and valuation (Desktop + Portal)

| Component | Role | Portal use |
| --------- | ---- | ---------- |
| `IStokBalanceViewDal` | Point-in-time balance rows (Item × Warehouse) | M15, M11, M19, M28 position |
| `StokBalanceView` | Qty, Hpp, NilaiSediaan, category, supplier | Valuation at item group |
| `InventoryReportDal` | Report footer reconciliation | Evidence drill-down |
| `StokBalanceHealthDal` | FIFO vs balance mismatch | Out of scope — data integrity, not planning |
| `StokPeriodikDal` | Historical qty from mutasi cumulative | Desktop only — deferred |

### 3.4 Sales history and consumption data

| Component | Grain | Fields | M28 use |
| --------- | ----- | ------ | ------- |
| `IFakturBrgViewDal` | Faktur line (item per invoice) | `QtyJual`, `FakturDate`, item name, category, supplier | **Primary consumption source** — needs `BrgId` added to portal query |
| `IBrgLastFakturDal` | Item | `LastFakturDate` | Movement class + insufficient-history detection |
| `BTR_Faktur` / `BTR_FakturItem` | Transaction | Gross sales outflow | Authoritative demand signal |
| Sales Report (portal) | Faktur header | No item columns | **Cannot** validate item consumption — gap documented |

**Consumption definition (V1):** Sum of `QtyJual` from non-void Fakturs where `FakturDate` falls in the consumption lookback window. Returns (`RETURJUAL`) are **not** netted in V1 — same gross-demand philosophy as M19 last-Faktur clock. Future milestone may net retur for true demand.

### 3.5 Stock movement reports (Desktop only)

| Desktop form | Data source | M28 relevance |
| ------------ | ----------- | ------------- |
| `KartuStokSummaryForm` (IF8) | `BTR_StokMutasi` per warehouse × period | `MovingStok`, Invoice/Faktur/Retur buckets — validation path, not V1 portal source |
| `KartuStokInfoForm` (IF2) | Transaction-level mutasi | Item drill-down in Desktop |
| `FakturBrgInfoForm` | `IFakturBrgViewDal` | **Primary evidence** for item sales velocity |

**Decision:** Portal V1 uses Faktur-item consumption (proven DAL path) rather than importing Kartu Stok Summary (warehouse-scoped, period-form driven, not materialized).

### 3.6 Purchasing reports and dashboards

| Capability | Source | M28 relationship |
| ---------- | ------ | ---------------- |
| Grand Total Purchase (MTD) | `DashboardPurchasingAggregator` | Context — cash outflow, not forecast input |
| Pending / Qualified Backlog | M21 Purchasing Management | **Purchase delay risk** — complementary signal |
| Principal concentration | M21 Top Principal % | **Supplier dependency risk** context |
| Principal Inventory No Purchase | M21 attention signal | High inventory + zero MTD purchase — overstock context |
| Compound Dependency | M21 | Supplier in purchase top 10 AND inventory/at-risk top 10 |

**M28 does not duplicate purchasing statistics.** It adds **when to reorder** and **how much to consider buying** at SKU level.

### 3.7 Materialized reporting tables (relevant)

| Table | Role for M28 |
| ----- | ------------ |
| `BTRPD_InventoryKpi` | Traceability — Total Inventory Value (IFR-50) |
| `BTRPD_InventoryBreakdown` | Category/supplier composition context |
| `BTRPD_InventoryRiskKpi` | At-Risk %, dead/slow counts (IFR-51) |
| `BTRPD_InventoryRiskAttention` | Cross-link for obsolescence signals |
| `BTRPD_PurchasingManagementKpi` | Qualified backlog count — footer context link |
| `BTRPD_SalesKpi` | Company billing pace — optional executive context only |

### 3.8 Reusable calculations — do not duplicate

| Calculation | Reuse from |
| ----------- | ---------- |
| Item groups (position) | `DashboardInventoryItemGroupBuilder.BuildItemGroups` |
| Total Inventory Value | Same builder sum — must match M15 |
| Movement classification | `DashboardInventoryRiskAggregator.ClassifyItem` logic (or shared classifier) |
| Last Faktur per item | `IBrgLastFakturDal.ListLastFakturByBrg` |
| Achievement / pace policy pattern | `SalesForecastPolicy`, `CashFlowForecastPolicy` → new `InventoryForecastPolicy` |
| Top-N risk ranking pattern | `CashFlowCollectionRiskBuilder`, M19 attention list |
| Snapshot refresh orchestration | `RefreshDashboardInventoryRiskSnapshotWorker` extension pattern (like M27 on Collection) |

### 3.9 Item master scope

| Field | Source | M28 rule |
| ----- | ------ | -------- |
| `IsAktif` | `BTR_Brg` | Include in forecast only when `IsAktif = true` AND `Qty > 0` |
| Discontinued flag | **Not found** in `BrgModel` | V1: treat **inactive** (`IsAktif = false`) as excluded |
| Lead time | **Not in master** | Configurable `DefaultLeadTimeDays` in appsettings |
| Safety stock | **Not in master** | Configurable `CoverageDays` for reorder qty hint |

---

## 4. Phase 2 — Inventory Forecast Model Analysis

### 4.1 Candidate algorithms

All candidates use **company-level BrgId-first** position and **non-void Faktur sales qty** as consumption. Planning horizon **H = 30 calendar days** forward from business date (configurable constant).

| # | Model | Formula (conceptual) | Simplicity | Explainability | Data availability | Stability | Business usefulness |
| - | ----- | -------------------- | ---------- | -------------- | ----------------- | --------- | ------------------- |
| A | **Rolling 30-Day ADC + Days of Supply** | `ADC = SoldQty₃₀ ÷ 30`; `DOS = Qty ÷ ADC`; stock-out ≈ Today + DOS | ★★★★★ | ★★★★★ | ★★★★★ | ★★★★☆ | ★★★★★ |
| B | Rolling 90-Day ADC | Same with 90-day window | ★★★★☆ | ★★★★★ | ★★★★★ | ★★★★★ | ★★★☆☆ |
| C | MTD daily consumption pace | `ADC = MTD Sold ÷ days elapsed` | ★★★★★ | ★★★★★ | ★★★★☆ | ★★☆☆☆ | ★★★☆☆ |
| D | Remaining Days of Supply only | Output of A — not standalone algorithm | — | — | — | — | — |
| E | Inventory turnover projection | `Turnover = COGS period ÷ Avg Inventory` | ★★★☆☆ | ★★★☆☆ | ★★★☆☆ | ★★★★☆ | ★★★☆☆ |
| F | Historical same-month prior year | Compare to last year month consumption | ★★★☆☆ | ★★★★☆ | ★★★☆☆ | ★★★☆☆ | ★★★☆☆ |
| G | Purchase lead-time projection | `Reorder = StockOut − LeadTime` | ★★★★☆ | ★★★★★ | ★★☆☆☆ | ★★★☆☆ | ★★★★☆ |
| H | Safety stock (statistical) | `SS = Z × σ × √LT` | ★★☆☆☆ | ★★☆☆☆ | ★☆☆☆☆ | ★★☆☆☆ | ★★★☆☆ |
| I | Seasonal consumption comparison | YoY or MoM index adjustment | ★★☆☆☆ | ★★★☆☆ | ★★☆☆☆ | ★★☆☆☆ | ★★★☆☆ |

### 4.2 Evaluation notes

**A — Rolling 30-Day ADC (recommended primary):**

- Plain language: *"On average we sold X units per day over the last 30 days; at that rate, current stock lasts Y days."*
- Stable enough for distribution businesses with continuous SKU sales
- Reacts within one month to demand changes — appropriate for replenishment planning
- Direct inventory analog to M26/M27 pace models
- Weakness: new SKUs with sparse history — mitigated by explicit **Insufficient History** band (IFR-20)

**B — Rolling 90-Day ADC:**

- Smoother, slower to detect demand spikes or drops
- Better for highly seasonal SKUs but **blunts stock-out early warning** — wrong primary objective for V1
- Recommended as **secondary input for scenario bands** (Best/Worst consumption), not primary

**C — MTD pace:**

- Unstable in first week of month — same weakness as early-month sales forecast
- Useful only as fallback when item has sales in current month but < 30 days of history

**E — Turnover projection:**

- Requires average inventory over period (`StokPeriodik`) — not materialized in portal
- Harder for warehouse managers to act on than DOS

**G — Lead-time projection:**

- Essential for **reorder date** output but **cannot be primary** without per-SKU lead time master
- V1 uses configurable default lead time (IFR-30)

**H/I — Safety stock / Seasonal:**

- Require variance history, calendars, or ML-like tuning — violate V1 simplicity principle

### 4.3 Recommended primary algorithm (V1)

**Rolling 30-Day Average Daily Consumption (ADC) with Linear Stock Depletion**

Management explanation:

> *"We total how many units of each product were sold on invoices over the last 30 days, divide by 30 to get the average daily consumption, then divide current stock by that rate to estimate how many days of supply remain. If consumption continues at that pace, we estimate when stock will run out and when purchasing should review replenishment."*

#### Symbols

| Symbol | Meaning |
| ------ | ------- |
| B | Business date (`IBusinessDateProvider.Today`) |
| H | Planning horizon days (default **30**) |
| Q | Current on-hand qty (BrgId-first aggregated) |
| V | Current inventory value = `Hpp × Q` (from item group) |
| S₃₀ | Units sold (`SUM(QtyJual)`) in rolling 30 calendar days `(B−29)..B` inclusive |
| ADC | Average daily consumption = `S₃₀ ÷ 30` |
| DOS | Days of supply = `Q ÷ ADC` when `ADC > 0` |
| FQ | Forecast qty at horizon = `MAX(0, Q − ADC × H)` |
| FV | Forecast inventory value at horizon = `FQ × unit Hpp` (weighted avg Hpp from balance rows) |
| LT | Default lead time days (configurable, default **7**) |
| CD | Coverage days for reorder qty hint (configurable, default **14**) |

#### Core formulas (item grain, then company roll-up)

| KPI | Formula |
| --- | ------- |
| **Average Daily Consumption (ADC)** | `S₃₀ ÷ 30` |
| **Days of Supply (DOS)** | `Q ÷ ADC` when `ADC > 0`; null when `ADC = 0` |
| **Projected Stock-Out Date** | `B + CEILING(DOS)` calendar days when DOS is not null and DOS ≤ H |
| **Forecast Consumption (horizon)** | `ADC × H` |
| **Forecast Qty at Horizon** | `MAX(0, Q − ADC × H)` |
| **Projected Inventory Value** | `SUM(FQ × Hpp)` across items |
| **Reorder Review Date** | `Projected Stock-Out Date − LT` when stock-out within horizon |
| **Recommended Purchase Qty (hint)** | `MAX(0, CEILING(ADC × (LT + CD) − Q))` when item is Active and `ADC > 0` |

#### Scenario bands (consumption momentum — secondary)

| Scenario | ADC basis | Plain language |
| -------- | --------- | -------------- |
| **Expected** | 30-day ADC (primary) | Recent month average continues |
| **Best Case (slower depletion)** | `MIN(ADC₃₀, ADC₉₀)` | Demand slows — stock lasts longer |
| **Worst Case (faster depletion)** | `MAX(ADC₃₀, ADC₉₀)` | Demand accelerates — stock runs out sooner |

When fewer than 30 days of item history exist: fall back per IFR-20.

**ADC₉₀:** `S₉₀ ÷ 90` over rolling 90 calendar days.

---

## 5. Phase 3 — Dashboard Wireframe

Text layout only. Route: `/dashboard/inventory-forecast`.

```text
┌─────────────────────────────────────────────────────────────────────────────┐
│  Inventory Forecast Dashboard                                [↻ Refresh]    │
│  30-day forward planning — active inventory (BrgId-first)                   │
│  As of: {GeneratedAt}  ·  Horizon: {H} days from {BusinessDate}             │
├─────────────────────────────────────────────────────────────────────────────┤
│  EXECUTIVE SUMMARY (plain-language sentence)                                │
│  "{N} items may stock out within {H} days; projected inventory value falls  │
│   from {CurrentValue} to {ProjectedValue}. {Top risk sentence}."           │
├─────────────────────────────────────────────────────────────────────────────┤
│  KPI ROW 1 — Position vs Projection                                         │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐       │
│  │ Current      │ │ Projected    │ │ Avg Days of  │ │ Inventory    │       │
│  │ Inventory    │ │ Inventory    │ │ Supply       │ │ Health Score │       │
│  │ Value        │ │ Value @ H    │ │ (active SKUs)│ │ {0–100}      │       │
│  └──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘       │
├─────────────────────────────────────────────────────────────────────────────┤
│  KPI ROW 2 — Risk Exposure (value)                                          │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐       │
│  │ Stock-Out    │ │ Overstock    │ │ Understock   │ │ At-Risk      │       │
│  │ Risk Value   │ │ Risk Value   │ │ Value        │ │ Inventory %  │       │
│  │              │ │              │ │ (reorder)    │ │ (M19 link)   │       │
│  └──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘       │
├─────────────────────────────────────────────────────────────────────────────┤
│  KPI ROW 3 — Scenario Bands                                                 │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐       │
│  │ Best Case    │ │ Expected     │ │ Worst Case   │ │ Forecast     │       │
│  │ Projected    │ │ Projected    │ │ Projected    │ │ Confidence   │       │
│  │ Value        │ │ Value        │ │ Value        │ │ {Low/Med/Hi} │       │
│  └──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘       │
├─────────────────────────────────────────────────────────────────────────────┤
│  CHART 1 — Forecast Inventory Level (full width)                            │
│  Line/area: Current value → projected depletion over days 0..H                │
│  Optional second series: cumulative consumption pace                        │
├─────────────────────────────────────────────────────────────────────────────┤
│  CHART 2 — Consumption Trend (half)     │  CHART 3 — Risk Heat Summary    │
│  Daily units sold (30d lookback bars)   │  Stock-Out | Overstock | Slow    │
│  + ADC reference line                   │  bucket counts by severity       │
├─────────────────────────────────────────────────────────────────────────────┤
│  TOP INVENTORY RISKS (table)                                                │
│  Columns: Item | Signal | DOS | Stock-Out Date | Value | Urgency            │
│  Priority-ordered Top 10                                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│  PURCHASING RECOMMENDATIONS (table)                                         │
│  Columns: Item | Supplier | Reorder Date | Rec. Qty | ADC | Current Qty     │
│  Top 10 by urgency — decision support only                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│  FOOTER — Traceability                                                      │
│  "Forecast uses 30-day Faktur sales qty. Position rules match Inventory     │
│   Dashboard. View evidence → Inventory Report · Inventory Risk · Purchasing"│
└─────────────────────────────────────────────────────────────────────────────┘
```

**Navigation placement:** Sidebar under **Dashboard → Inventory Forecast** (after Slow Moving & Dead Stock, before Purchasing).

**Dashboard relationships:**

| Existing dashboard | Relationship |
| ------------------ | ------------ |
| Inventory (`/dashboard/inventory`) | Current composition — denominator for value |
| Inventory Risk (`/dashboard/inventory-risk`) | Backward-looking obsolescence — linked for dead/slow context |
| Purchasing Management | Supplier backlog — linked for purchase delay context |
| Sales Forecast | Demand pace context — optional footer link only |

---

## 6. Phase 4 — KPI Definitions

### 6.1 Company-level KPIs

#### Current Inventory Value

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Capital tied up in stock on hand today |
| **Formula** | `SUM(Hpp × Qty)` BrgId-first — same as M15 |
| **Management interpretation** | "How much working capital is in inventory right now?" |
| **Color/status** | Neutral |
| **Drill-down** | Inventory Report (`/reports/inventory`) footer |

#### Projected Inventory Value

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Estimated inventory capital at end of planning horizon if consumption continues at 30-day ADC |
| **Formula** | `SUM(MAX(0, Q − ADC×H) × Hpp)` across forecast-eligible items |
| **Management interpretation** | "Where is inventory capital heading in the next 30 days?" |
| **Color/status** | Warning if drop > configurable % vs current; neutral otherwise |
| **Drill-down** | Forecast Inventory Level chart (in-page) |

#### Average Daily Consumption (company)

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Company-wide units consumed per day (sum of item ADC) |
| **Formula** | `SUM(ADC)` across forecast-eligible items — units, not currency |
| **Management interpretation** | "How fast is the business burning through stock in units?" |
| **Color/status** | Neutral |
| **Drill-down** | Consumption Trend chart |

#### Days of Supply (company average)

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Average days of supply across active forecast-eligible SKUs (qty-weighted) |
| **Formula** | `SUM(Q) ÷ SUM(ADC)` when `SUM(ADC) > 0` |
| **Management interpretation** | "On average, how long does current stock last?" |
| **Color/status** | Healthy ≥ 30d · Warning 14–29d · Critical < 14d (configurable) |
| **Drill-down** | Top Inventory Risks table |

#### Projected Stock-Out Date (item; company shows count)

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Calendar date when item qty is projected to reach zero |
| **Formula** | `B + CEILING(DOS)` when `ADC > 0` and item forecast-eligible |
| **Management interpretation** | "When will we likely run out if we do nothing?" |
| **Color/status** | Critical ≤ 7d · Warning 8–14d · Normal > 14d |
| **Drill-down** | Inventory Report — filter by item name (`?q=`) |

#### Reorder Date (Recommended)

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Date purchasing should **review** replenishment to avoid stock-out |
| **Formula** | `Projected Stock-Out Date − LT` (calendar days) |
| **Management interpretation** | "When should we start the purchase process?" |
| **Color/status** | Critical when reorder date ≤ B (overdue review) |
| **Drill-down** | Purchasing Report + Desktop purchase entry |

#### Recommended Purchase Quantity

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Indicative qty to restore coverage — **not** an approved PO |
| **Formula** | `MAX(0, CEILING(ADC × (LT + CD) − Q))` |
| **Management interpretation** | "Roughly how much should we consider ordering?" |
| **Color/status** | Neutral — informational |
| **Drill-down** | Desktop purchase workflow |

#### Inventory Turnover Forecast

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Projected times stock rotates over horizon at current ADC |
| **Formula** | `(SUM(ADC) × H) ÷ SUM(Q)` when `SUM(Q) > 0` — simplified V1 |
| **Management interpretation** | "How many times will stock turn in the next 30 days?" |
| **Color/status** | Neutral |
| **Drill-down** | None in V1 |

#### Inventory Coverage

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Share of active SKUs with DOS above minimum threshold |
| **Formula** | `Count(items with DOS ≥ MinDosThreshold) ÷ Count(forecast-eligible) × 100` |
| **Management interpretation** | "What % of SKUs have adequate cover?" |
| **Color/status** | Healthy ≥ 80% · Warning 60–79% · Critical < 60% |
| **Drill-down** | Top Inventory Risks (understock rows) |

#### Overstock Value

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Value of items with excess cover relative to demand |
| **Formula** | `SUM(V)` where `DOS > OverstockDosThreshold` (default 90) and item Active |
| **Management interpretation** | "How much capital is tied in likely excess?" |
| **Color/status** | Warning when > configurable % of total inventory value |
| **Drill-down** | Inventory Risk dashboard (slow moving overlap) |

#### Understock Value

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Value of items projected to stock out within horizon |
| **Formula** | `SUM(V)` where `DOS ≤ H` and `ADC > 0` |
| **Management interpretation** | "How much inventory value is at near-term availability risk?" |
| **Color/status** | Critical / Warning by DOS bands |
| **Drill-down** | Top Inventory Risks table |

#### Forecast Consumption

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Total units expected to sell over horizon at current ADC |
| **Formula** | `SUM(ADC × H)` |
| **Management interpretation** | "How many units will we likely ship in 30 days?" |
| **Color/status** | Neutral |
| **Drill-down** | Consumption Trend chart |

#### Forecast Inventory Level

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Time series of projected total inventory value days 0..H |
| **Formula** | Day `d`: `SUM(MAX(0, Q − ADC×d) × Hpp)` |
| **Management interpretation** | "How does inventory capital deplete day by day?" |
| **Color/status** | Chart only |
| **Drill-down** | In-page chart |

#### Inventory Health Score

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Single deterministic score summarizing coverage vs risk |
| **Formula** | Start at 100; subtract weighted penalties: stock-out risk item % × 40, overstock value % × 30, at-risk inventory % (M19) × 30 (caps at 0) |
| **Management interpretation** | "Overall inventory planning health — higher is better" |
| **Color/status** | Healthy ≥ 75 · Warning 50–74 · Critical < 50 |
| **Drill-down** | Top Inventory Risks table |

#### Best / Expected / Worst Case Projection

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Projected inventory value under slower/expected/faster consumption |
| **Formula** | Expected = primary; Best = use `MIN(ADC₃₀,ADC₉₀)`; Worst = use `MAX(ADC₃₀,ADC₉₀)` in depletion formula |
| **Management interpretation** | "Range of outcomes if demand slows or accelerates" |
| **Color/status** | Band display only |
| **Drill-down** | Scenario KPI row |

### 6.2 Traceability rules

| Rule ID | Rule |
| ------- | ---- |
| IFR-50 | `CurrentInventoryValue` must equal `BTRPD_InventoryKpi.TotalInventoryValue` for same refresh cycle |
| IFR-51 | `AtRiskInventoryPercent` must equal `BTRPD_InventoryRiskKpi.AtRiskInventoryPercent` when both refreshed in same `All` run |
| IFR-52 | `TotalItem` forecast denominator ⊆ Inventory Report footer item count (forecast-eligible subset) |

---

## 7. Phase 5 — Inventory Risk Forecast

All predictions are **rule-based** and evaluated at snapshot refresh. Priority-ordered for Top 10 table.

### 7.1 Product-level forward risks

| Risk concept | Deterministic rule | Signal key |
| ------------ | ------------------ | ---------- |
| **Stock-out likely** | `ADC > 0` AND `DOS ≤ H` (default 30) | `StockOutRisk` |
| **Critical stock-out** | `DOS ≤ 7` | `CriticalStockOut` |
| **Overstock likely** | `ADC > 0` AND `DOS > OverstockThreshold` (default 90) | `OverstockRisk` |
| **Future slow moving** | Active (≤ 89d idle) AND `ADC > 0` AND `DOS > 60` AND rising qty vs MTD purchases | `FutureSlowMoving` |
| **Future dead stock** | Active AND `ADC = 0` over 30d but `Q > 0` AND last sale 60–89d ago | `FutureDeadStock` |
| **Never sold with stock** | M19 NeverSold classification | Link to M19 — not re-derived |
| **Insufficient history** | Item active, `S₃₀ = 0` but `0 < days since first sale < 30` | `InsufficientHistory` |

### 7.2 Concentration and dependency risks (company-level)

| Risk concept | Rule |
| ------------ | ---- |
| **Inventory concentration risk** | Top category inventory % ≥ 25% (reuse M15 breakdown) AND top category has ≥ 1 stock-out risk item |
| **Supplier dependency risk** | Top supplier inventory % ≥ 25% AND supplier has ≥ 2 stock-out risk SKUs |
| **Purchase delay risk** | Principal has stock-out risk SKU AND qualified backlog > 0 for same supplier (cross-read M21) |
| **Warehouse capacity risk** | **Deferred** — company-level BrgId-first V1 has no warehouse grain |

### 7.3 Risk priority order (Top 10)

1. Critical Stock-Out (DOS ≤ 7)
2. Stock-Out Risk (DOS ≤ H)
3. Future Dead Stock
4. Overstock Risk (highest value first)
5. Future Slow Moving
6. Purchase Delay Risk (supplier compound)
7. Insufficient History (high value items only)

---

## 8. Phase 6 — Purchasing Recommendation

M28 provides **decision support only** — no automatic purchasing.

| Concept | Formula / rule | Urgency classification |
| ------- | -------------- | ------------------------ |
| **Reorder review date** | Stock-out date − `DefaultLeadTimeDays` | Overdue when < B |
| **Recommended purchase qty** | `MAX(0, CEILING(ADC × (LT + CD) − Q))` | N/A |
| **Purchase urgency** | See table below | Critical / High / Medium / Low |
| **Safety stock warning** | `DOS < LT + SafetyBufferDays` (default buffer 3) | Warning |
| **Critical stock warning** | `DOS ≤ 7` | Critical |
| **Overstock warning** | `DOS > 90` | Info — review promotion/intake |

**Purchase urgency classification:**

| Urgency | Condition |
| ------- | --------- |
| **Critical** | `DOS ≤ 7` OR reorder date < B |
| **High** | `8 ≤ DOS ≤ 14` |
| **Medium** | `15 ≤ DOS ≤ 30` |
| **Low** | `DOS > 30` but below overstock threshold |

**Explicit disclaimer (UI):** *"Recommended quantities are indicative. Confirm with supplier, pending postings, and in-transit stock in BTR Desktop before purchasing."*

---

## 9. Phase 7 — Business Rules

| Rule ID | Rule |
| ------- | ---- |
| IFR-01 | Planning horizon **H = 30 calendar days** forward from business date |
| IFR-02 | Business date from `IBusinessDateProvider.Today` |
| IFR-03 | Position: BrgId-first; exclude In-Transit; `Qty > 0` only |
| IFR-04 | Valuation: `Hpp × Qty` from `StokBalanceView` — same as M15 |
| IFR-05 | Consumption: `SUM(QtyJual)` from non-void Fakturs (`VoidDate = '3000-01-01'`) |
| IFR-06 | Consumption window: rolling 30 calendar days `(B−29)..B` inclusive for primary ADC |
| IFR-07 | Secondary 90-day window for scenario bands only |
| IFR-08 | Forecast-eligible: `IsAktif = true` AND not M19 Dead Stock or Never Sold |
| IFR-09 | Dead Stock (≥ 180d idle): obsolescence path only — no stock-out projection |
| IFR-10 | Never Sold: excluded from ADC; surface link to M19 |
| IFR-11 | Slow Moving (90–179d): included in forecast if `ADC > 0`; also flagged in future slow-moving risk |
| IFR-12 | Void Fakturs excluded from consumption |
| IFR-13 | Retur qty **not** netted in V1 consumption (gross Faktur outflow) |
| IFR-14 | Recalculate on every Inventory Risk snapshot refresh |
| IFR-20 | **Insufficient history:** if first sale within last 29 days and `S₃₀ = 0`, use MTD pace (`MTD sold ÷ days elapsed`, min 1) as ADC fallback; else ADC = 0 and flag Insufficient History |
| IFR-21 | **Seasonal products:** no special rule V1 — 30-day window is the seasonal proxy; YoY deferred |
| IFR-22 | **Beginning of month:** MTD fallback may be volatile days 1–5 — Forecast Confidence Low when `< 30` days of company consumption data |
| IFR-30 | **Missing lead time:** use `DashboardSnapshot:InventoryForecastDefaultLeadTimeDays` (default 7) |
| IFR-31 | **Coverage days:** `DashboardSnapshot:InventoryForecastCoverageDays` (default 14) for recommended qty |
| IFR-32 | **Overstock threshold:** `DashboardSnapshot:InventoryForecastOverstockDosDays` (default 90) |
| IFR-40 | No working-day calendar |
| IFR-41 | Read-only — no PO creation |
| IFR-42 | Pending purchase postings do **not** increase on-hand in forecast V1 — footer links M21 backlog |

---

## 10. Phase 8 — User Experience

### 10.1 Layout and card ordering

Attention-first within a forward-looking frame:

1. Executive summary (risk-forward wording)
2. KPI Row 1 — Position vs projection
3. KPI Row 2 — Risk exposure values
4. KPI Row 3 — Scenario bands + confidence
5. Forecast inventory level chart (primary visual)
6. Consumption trend + risk heat summary
7. Top Inventory Risks table
8. Purchasing Recommendations table
9. Traceability footer with cross-links

### 10.2 Visualization recommendations

| Element | Recommendation |
| ------- | -------------- |
| **Inventory heat map** | 3×3 grid: DOS bands (Low/Med/High) × Value bands (Low/Med/High) — item counts in cells |
| **Trend charts** | 30-day daily consumption bars + ADC line; projected value depletion area chart |
| **Risk visualization** | Severity badges (Critical/High/Medium) on table rows — reuse Collection Risk table pattern |
| **Executive summary** | *"At current 30-day sales pace, {N} active items may stock out within 30 days, representing {UnderstockValue} in inventory value. Projected inventory value moves from {Current} to {Projected}. Immediate attention: {Top item names}."* |

### 10.3 Drill-down destinations

| Signal / KPI | Primary drill-down | Secondary |
| ------------ | ------------------ | --------- |
| Stock-out risk item | Inventory Report `?q={item}` | Desktop Faktur Brg Info |
| Overstock / slow | Inventory Risk dashboard | Inventory Report |
| Reorder recommendation | Purchasing Report | Purchasing Management |
| Purchase delay | Purchasing Management attention list | Desktop posting |
| Company inventory value | Inventory Dashboard | Inventory Report |

### 10.4 Management question framing

| Avoid (backward-looking) | Prefer (forward-looking) |
| ------------------------ | ------------------------ |
| "What inventory do we have?" | "What will we likely run out of in 30 days?" |
| "Which items are dead?" | "Which active items are trending toward dead stock?" |
| "How much did we buy?" | "When should we review replenishment?" |

---

## 11. Future Extensibility

V1 architecture must allow extension without redesign:

| Future capability | Extension point |
| ----------------- | --------------- |
| Supplier lead-time forecasting | `LeadTimeDays` column per supplier/item on snapshot row; replace IFR-30 default |
| Warehouse-specific forecasting | Add `WarehouseId` grain to consumption DAL + item groups — new dashboard route |
| Branch inventory balancing | Warehouse grain + transfer mutasi |
| ABC inventory forecasting | ABC class on item snapshot; filter/score weights |
| EOQ / replenishment simulation | New policy module; optional simulation tables |
| Seasonal demand | Third consumption window (YoY same month); scenario band input |
| Purchase scenario simulation | Multiple ADC inputs — client-side what-if (read-only) |
| Net demand (retur-adjusted) | Optional consumption net of `RETURJUAL` mutasi |
| Alert Center integration | New signal keys from `BTRPD_InventoryForecastRisk` |
| Executive promotion | KPI subset on Management Attention Center |

---

## 12. Open Questions for Product Owner

| # | Question | Default if unanswered |
| - | -------- | --------------------- |
| 1 | Planning horizon 30 days vs calendar month-end? | **30 days** (inventory planning standard) |
| 2 | Default lead time days? | **7** |
| 3 | Include Slow Moving items in stock-out forecast? | **Yes** if ADC > 0 |
| 4 | Gross vs net-of-retur consumption? | **Gross** V1 (IFR-13) |
| 5 | Promote to Executive Dashboard in V1? | **No** — deferred like M26/M27 |

---

## Document Maintenance

When M28 is implemented, promote concepts to `docs/features/inventory-forecast/feature.md` and update `btr-portal-domain.md` Section 12 roadmap.

**Success criterion:** Product Owner can approve forecast algorithm, KPI set, and wireframe without reading source code.
