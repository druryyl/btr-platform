# Materialized Dashboard Data — Feasibility Analysis

**Author role:** Analyst  
**Date:** 2026-06-07  
**Audience:** Product Owner, Board stakeholders, Architect  
**Status:** Analysis complete — ready for Architect planning  

**Related documents:** `docs/features/btr-portal/btr-portal-domain.md`, `docs/work/btr-portal-api-scaffolding/portal-analysis-m13-m15-final.md`, `btr-reporting-investigation.md`, `src/j05-btr-distrib/docs/plans/sales-omzet-aggregate-implementation.md`

---

## 1. Executive Summary

BTR Portal dashboards today compute analytics **on every page load** by querying operational tables (often with wide date windows). The Piutang dashboard is the heaviest offender: it scans receivables from **2000-01-01 through today** with multi-table joins, then aggregates aging buckets and rankings in application memory. Inventory loads the full stock-balance catalog. Sales reads **`BTR_SalesOmzet`**, which is designed for **salesperson fee / incentive calculation**, not board-level sales reporting.

**Conclusion: materializing dashboard data first is feasible and recommended** for all **analytical dashboard routes** (`/dashboard/sales`, `/dashboard/piutang`, `/dashboard/inventory`). BTR already uses this pattern successfully for stock balances (`BTR_StokBalanceWarehouse`) and sales omzet reconciliation (`BTR_SalesOmzet` + background worker).

A dedicated **dashboard snapshot store** (one or more tables) populated by a **background refresh service** would:

- Move CPU-heavy aggregation off the HTTP request path
- Make analytical pages responsive for Board and stakeholder users
- Allow controlled refresh cadence (e.g. every 15–60 minutes, or on-demand after close-of-day)
- Decouple portal sales analytics from `BTR_SalesOmzet` by sourcing from **`BTR_Faktur`** as requested

**Out of scope for this initiative (per product direction):** the Overview Dashboard at `/dashboard` (Operational Summary). That page may continue to use lightweight live queries or read only headline KPI columns from the same snapshot — but the materialization design is driven by analytical dashboard needs, not the home summary cards.

---

## 2. Business Problem

### 2.1 Objective

Give Board of Directors and stakeholders **fast, reliable management analytics** without overloading the database server during concurrent portal sessions.

### 2.2 Current pain

| Symptom | Business impact |
| ------- | ---------------- |
| Slow dashboard load (especially Piutang) | Executives abandon the portal; decisions delayed |
| Server CPU spikes on each refresh | Operational BTR Desktop users compete for database resources |
| Full-history scan (`2000-01-01 → today`) | Unnecessary work — only **open balances** matter for Piutang analytics |
| Sales dashboard tied to `BTR_SalesOmzet` | Board metrics conflate with **salesperson fee / RO2 incentive** logic (Completed vs Pipeline, omzet recognition rules) |

### 2.3 Affected users

| User | Need |
| ---- | ---- |
| Board of Directors | Monthly/quarterly performance, receivable exposure, inventory capital |
| Stakeholders / owners | High-level KPIs, trends, concentration risk (top customers, categories, suppliers) |
| Finance leadership | Aging distribution, overdue customer counts |
| Operations leadership | Inventory composition, sales ranking |

### 2.4 Expected outcome

After implementation, analytical dashboard pages should:

- Load in seconds (read pre-aggregated snapshot), not tens of seconds
- Show a **“data as of”** timestamp from the last background refresh
- Reconcile with existing report footer totals where traceability is defined today
- Use **`BTR_Faktur`** as the authoritative source for Sales analytical dashboard metrics

---

## 3. Current State — How Dashboards Get Data

### 3.1 Portal architecture (today)

```
Browser → GET /api/dashboard/{sales|piutang|inventory}
       → MediatR handler
       → Dashboard*Dal
       → Existing Desktop DAL(s) + C# aggregation
       → SQL Server operational tables
```

The Overview page (`/dashboard`) calls **all three endpoints in parallel** on mount. Each analytical route calls the same endpoint again when opened.

### 3.2 Dashboard inventory

| Route | Label | API | Primary data source | Period / scope |
| ----- | ----- | --- | ------------------- | -------------- |
| `/dashboard` | Overview (Operational Summary) | All three dashboard GETs | Same as detail dashboards | Same |
| `/dashboard/sales` | Sales Analytics | `GET /api/dashboard/sales` | `BTR_SalesOmzet` via `ISalesOmzetDal` | Current calendar month; Omzet Period mode |
| `/dashboard/piutang` | Piutang Analytics | `GET /api/dashboard/piutang` | `BTR_Piutang` + joins via `IPiutangSalesWilayahDal` | **`2000-01-01 → today`**, open balances only |
| `/dashboard/inventory` | Inventory Analytics | `GET /api/dashboard/inventory` | `BTR_Brg` + `BTR_StokBalanceWarehouse` via `IStokBalanceViewDal` | Point-in-time; all items × warehouses |

Reports (`/reports/*`) remain separate tabular views; they are **not** in scope for this materialization analysis except where dashboard KPIs must reconcile with report footers.

### 3.3 Performance root causes

#### Piutang (highest severity)

`DashboardPiutangDal` loads rows from `PiutangSalesWilayahDal`, which executes a heavy SQL query:

- Filters `BTR_Piutang.PiutangDate BETWEEN @Tgl1 AND @Tgl2` with `@Tgl1 = 2000-01-01`
- Joins `BTR_Faktur`, `BTR_Customer`, `BTR_SalesPerson`, `BTR_Wilayah`
- Runs **five correlated subqueries** per piutang row (`BTR_PiutangLunas`, `BTR_PiutangElement`) to derive payment components — even though `BTR_Piutang.Sisa` is already persisted
- Application layer then filters `KurangBayar > 1`, computes aging buckets, overdue customers, and Top 10

**Key insight:** Open receivables are a **current-state snapshot** (~rows where `Sisa > 1`), not a historical time-series. Scanning 25+ years of piutang dates to find today's open balances is structurally inefficient.

#### Inventory (moderate severity)

`DashboardInventoryDal` calls `StokBalanceViewDal.ListData()` with **no filter**, returning every item × warehouse row, then:

- Excludes In-Transit warehouse
- Groups by `BrgId` (BrgId-first rollup)
- Rolls up to category and supplier for charts and Top 10

Stock **quantities** are already materialized in `BTR_StokBalanceWarehouse` (refreshed by `GenStokBalanceWorker`), but **dashboard KPIs and dimensional rollups** are computed on every request.

#### Sales (lower severity for current month, wrong source for board)

`DashboardSalesDal` reads **`BTR_SalesOmzet`** for the current month and applies:

- `SalesOmzetChartSummaryBuilder` (recognized omzet, pipeline, weekly buckets)
- `BuildManagerComparison` (Top 10 salesman)
- `BTR_SalesOmzetTarget` (target vs achievement)

Current-month scope limits row volume, but:

- `BTR_SalesOmzet` reflects **omzet recognition / salesperson fee semantics** (Completed, Outstanding, Pending, Void)
- Sales Report already uses **`BTR_Faktur`** via `FakturViewDal` — board stakeholders expect invoice-based sales, aligned with the report

---

## 4. Feasibility Assessment

### 4.1 Is materialization possible?

**Yes.** Evidence from existing BTR patterns:

| Existing pattern | What it materializes | Refresh mechanism |
| ---------------- | -------------------- | ----------------- |
| `BTR_StokBalanceWarehouse` | Item × warehouse quantity | `GenStokBalanceWorker` on stock events / manual balance run |
| `BTR_SalesOmzet` | Sale-thread omzet row for RO2 | `ReconcileSalesOmzetWorker` (manual or scheduled) |
| `BTR_SalesOmzetHealthWeekly` | Weekly materialize health score | Post-reconcile worker |

Dashboard materialization follows the same principle: **pre-compute what the UI needs, refresh asynchronously, read fast at request time.**

### 4.2 What must be preserved (business rules)

Materialization must not change KPI meaning. Existing approved rules from portal domain documentation:

| Domain | Non-negotiable rules |
| ------ | -------------------- |
| Piutang | `KurangBayar > 1` (equivalent: `Sisa > 1`); aging from `JatuhTempo`; 5 inclusive buckets; customer key = `CustomerCode` with `CustomerName` fallback |
| Inventory | BrgId-first grouping; `HPP × Qty`; exclude In-Transit; exclude aggregated `Qty ≤ 0`; blank category/supplier → `"Unknown"` |
| Sales (new direction) | Source = **`BTR_Faktur`**; exclude void (`VoidDate = '3000-01-01'`); current month for M13 metrics; Top 10 rankings |

Traceability targets (must still hold after materialization):

- Piutang Total Piutang / Total Customer = Piutang Report footer
- Inventory Total Inventory Value / Total Item = Inventory Report footer
- Sales metrics should reconcile with Sales Report (`BTR_Faktur` grain) for the same month

### 4.3 Feasibility by dashboard

| Dashboard | Materialize? | Feasibility | Rationale |
| --------- | ------------ | ----------- | --------- |
| Overview `/dashboard` | **No** (explicitly excluded) | N/A | Product scope — keep as operational summary; may optionally read snapshot KPIs later |
| Sales Analytics | **Yes** | **High** | Bounded monthly dataset from `BTR_Faktur`; clear dimensions (salesperson, week, customer); targets from `BTR_SalesOmzetTarget` |
| Piutang Analytics | **Yes** | **High** | Snapshot semantics — only open rows; aging is deterministic from `DueDate` + refresh date |
| Inventory Analytics | **Yes** | **High** | Quantities already materialized; rollups are deterministic given stock balance + master data |

### 4.4 What cannot be materialized trivially

These are **future** analytical features (deferred beyond M15), not blockers for current dashboards:

- Custom date-range filters (requires parameterized snapshots or on-demand re-aggregation)
- Drilldown from chart to transaction
- Sales pipeline / order-based metrics (requires `BTR_Order` — intentionally excluded when using `BTR_Faktur`-only board view)
- Real-time sub-minute freshness (materialization implies acceptable lag)

---

## 5. Proposed Materialization Approach (Business View)

> **Note:** Table names, service topology, and API shape are for the Architect to design. This section describes **what** to materialize, not **how** to implement it.

### 5.1 Background refresh service

A scheduled background process (Windows service, hangfire job, or dedicated worker — Architect decides) should:

1. Run aggregation logic equivalent to today's `Dashboard*Dal` calculations (and new Faktur-based sales logic)
2. Write results to dedicated dashboard snapshot storage
3. Stamp each snapshot with `GeneratedAt` (already exposed in API responses today)
4. Optionally record refresh status / duration (pattern: `BTR_SalesOmzetHealthWeekly`)

**Suggested refresh cadence for Board use:**

| Trigger | Cadence |
| ------- | ------- |
| Scheduled | Every 30–60 minutes during business hours; every 2–4 hours overnight |
| On-demand | Manual “Refresh dashboard data” action for finance/admin (optional) |
| Event-driven (future) | After end-of-day close, after bulk piutang recalculation, after stock balance generation |

**Freshness expectation for stakeholders:** Data may be up to one refresh interval stale. Display `GeneratedAt` prominently on analytical pages (already partially shown on Overview cards).

### 5.2 Snapshot content model (conceptual)

Two complementary layers are recommended:

#### Layer A — Headline KPI snapshot (one row per domain per refresh)

Fast read for KPI cards and Overview **if** product later chooses to consume it:

| Domain | KPI fields |
| ------ | ---------- |
| Sales | Total Omzet, Total Faktur, Total Customer, Total Target, Total Achievement, Achievement % |
| Piutang | Total Piutang, Total Customer, Overdue Customer |
| Inventory | Total Inventory Value, Total Item |

#### Layer B — Dimensional aggregates (many rows per domain)

Pre-grouped data for charts and Top 10 tables:

| Domain | Aggregate types |
| ------ | --------------- |
| Sales | Weekly trend rows; salesman ranking rows; optional customer / wilayah breakdown |
| Piutang | Aging bucket amounts (5 rows); Top customer rows |
| Inventory | Category breakdown rows; supplier breakdown rows; Top 10 category/supplier rows |

#### Layer C — Optional detail grain (Piutang only, if needed)

If aging/top-customer logic requires row-level open faktur between refreshes, a slim **open receivable fact** table (`PiutangId`, `CustomerCode`, `CustomerName`, `JatuhTempo`, `KurangBayar`, dimension keys) filtered to `Sisa > 1` avoids the 2000-01-01 scan. Aggregations run at refresh time, not request time.

### 5.3 Sales data source change — `BTR_Faktur` not `BTR_SalesOmzet`

Per product direction, **`BTR_SalesOmzet` must not drive the Sales analytical dashboard.** That table serves salesperson fee / RO2 incentive materialization with omzet-status semantics unrelated to board reporting.

**Recommended Sales dashboard metrics from `BTR_Faktur`:**

| Metric | Business definition (Faktur-based) |
| ------ | -------------------------------- |
| Total Omzet | Sum of `GrandTotal` for non-void Fakturs in current calendar month |
| Total Faktur | Count of those Fakturs |
| Total Customer | Distinct customers with at least one Faktur in the month |
| Weekly trend | Sum of `GrandTotal` grouped by calendar week within the month |
| Top 10 Salesman | Sum of `GrandTotal` by `SalesPersonName`, descending |
| Total Target | Sum of `TargetAmount` from `BTR_SalesOmzetTarget` for current month (target table remains valid — it is not omzet materialization) |
| Total Achievement | Same as Total Omzet (Faktur-based) for board view |
| Achievement % | Total Achievement ÷ Total Target |

**Semantic shift to communicate to stakeholders:**

| Aspect | Old (`BTR_SalesOmzet`) | New (`BTR_Faktur`) |
| ------ | ---------------------- | ------------------- |
| Revenue basis | Omzet recognition status (Completed / Pending / Outstanding) | Invoiced `GrandTotal` |
| Pipeline | Included in data model | **Excluded** — board view is billed sales, not pipeline |
| Alignment with Sales Report | Indirect | **Direct** — same source as `/reports/sales` |

`BTR_SalesOmzet` continues unchanged for Desktop RO2 and salesperson fee workflows.

### 5.4 Piutang materialization strategy

Replace the `2000-01-01 → today` scan with a **current open receivable snapshot**:

1. Select from `BTR_Piutang` where `Sisa > 1` (matches `KurangBayar > 1` rule)
2. Join dimension data (`BTR_Faktur`, `BTR_Customer`, `BTR_SalesPerson`, `BTR_Wilayah`) once at refresh time
3. Prefer persisted `Sisa` over recomputing from `PiutangLunas` / `PiutangElement` subqueries (same business result, less work)
4. Compute aging buckets using refresh-date as “today”
5. Persist bucket totals, KPI totals, and Top 10 customer rows

**Refresh triggers:** After payment posting (pelunasan), piutang recalculation, or on schedule. Piutang balances change intraday — refresh cadence should be **more frequent than inventory** if finance users expect near-current figures.

### 5.5 Inventory materialization strategy

Build on existing `BTR_StokBalanceWarehouse`:

1. At refresh, load stock balance view equivalent (or join Brg + balance + category + supplier + warehouse)
2. Apply BrgId-first grouping and In-Transit exclusion
3. Persist total KPIs plus category/supplier dimensional sums and Top 10 lists

Inventory changes less frequently than piutang during the day; hourly refresh is typically sufficient unless stock posting is continuous.

---

## 6. Proposed Data Dimensions for Board & Stakeholder Analytics

Dimensions define **how executives slice the business**. The tables below recommend dimensions for **current portal dashboards (M13–M15)** and **near-term board analytics** that fit the materialization model.

### 6.1 Cross-cutting dimensions

| Dimension | Business meaning | Primary source | Board relevance |
| --------- | ---------------- | -------------- | --------------- |
| **Calendar Month** | Reporting period | System date / `FakturDate` / refresh date | Primary period for sales review |
| **Calendar Week** | Short-term trend | Week of `FakturDate` | Weekly sales momentum |
| **Quarter / Year** | Longer comparison (future) | Derived from dates | YoY/QoQ board packs |
| **Snapshot Date (`GeneratedAt`)** | When analytics were computed | Refresh service | Trust and auditability |

### 6.2 Sales dimensions (`BTR_Faktur` grain)

| Dimension | Grouping field | Typical board questions |
| --------- | -------------- | ----------------------- |
| **Sales Person** | `SalesPersonName` | Who are top performers? Is target allocation working? |
| **Customer** | `CustomerName` / `CustomerCode` | Revenue concentration — dependency on key accounts |
| **Customer Classification (Klasifikasi)** | `KlasifikasiName` | Which customer segments drive revenue? |
| **Wilayah (Region)** | `WilayahName` | Geographic performance |
| **Warehouse** | `WarehouseName` | Which distribution branch books sales? |
| **Faktur Week** | ISO/calendar week of `FakturDate` | Is weekly billing accelerating or slowing? |
| **Faktur Status (Kembali)** | `StatusFaktur == 2` | Operational follow-up on signed faktur returns (secondary KPI) |

**Recommended materialized aggregates for Sales (priority order):**

1. **Month × Company** — headline KPIs (omzet, faktur count, customer count, target, achievement)
2. **Month × Week** — weekly trend series
3. **Month × Sales Person** — ranking table and target comparison (future per-rep achievement)
4. **Month × Wilayah** — regional board view (future chart)
5. **Month × Customer Classification** — segment mix (future chart)

### 6.3 Piutang dimensions (open balance grain)

| Dimension | Grouping field | Typical board questions |
| --------- | -------------- | ----------------------- |
| **Aging Bucket** | Derived from `DueDate` vs snapshot date | How much debt is severely overdue? |
| **Customer** | `CustomerCode` / `CustomerName` | Who owes the most? |
| **Wilayah** | `WilayahName` | Regional collection exposure |
| **Sales Person** | `SalesName` | Which sales territories carry receivable risk? |
| **Due Month** | Month of `JatuhTempo` | When is cash expected to arrive? |
| **Invoice Age (Piutang vintage)** | Month of `PiutangDate` | Are old invoices stuck unpaid? |

**Recommended materialized aggregates for Piutang (priority order):**

1. **Snapshot × Aging Bucket** — pie chart (5 buckets) — **required for M14**
2. **Snapshot × Customer (Top N)** — concentration table — **required for M14**
3. **Snapshot × Wilayah** — regional exposure (future)
4. **Snapshot × Sales Person** — accountability view (future)

### 6.4 Inventory dimensions (item × warehouse → rolled up)

| Dimension | Grouping field | Typical board questions |
| --------- | -------------- | ----------------------- |
| **Category (Kategori)** | `KategoriName` | Where is working capital tied up by product type? |
| **Supplier (Principal)** | `SupplierName` | Which principals dominate inventory value? |
| **Warehouse** | `WarehouseName` | Branch-level stock concentration (future M16+) |
| **Item (Top N)** | `BrgName` / `BrgCode` | Slow movers / top SKU value (future ABC) |
| **Unknown bucket** | Blank category/supplier → `"Unknown"` | Data quality signal for master data |

**Recommended materialized aggregates for Inventory (priority order):**

1. **Snapshot × Category** — horizontal bar + Top 10 — **required for M15**
2. **Snapshot × Supplier** — horizontal bar + Top 10 — **required for M15**
3. **Snapshot × Warehouse** — branch inventory (deferred)
4. **Snapshot × ABC Class** — strategic inventory policy (deferred)

### 6.5 Purchasing dimensions (future dashboard)

No purchasing dashboard exists today (report only). If extended for board use:

| Dimension | Source | Board relevance |
| --------- | ------ | --------------- |
| Month | Invoice date | Spend vs budget |
| Supplier | `BTR_Supplier` | Supplier dependency |
| Warehouse | Posting warehouse | Branch purchasing |
| Posting Stok status | `SUDAH` / `BELUM` | Working capital in transit vs on hand |

---

## 7. Scope Boundaries

### 7.1 In scope

| Item | Included |
| ---- | -------- |
| Materialized data for `/dashboard/sales` | Yes |
| Materialized data for `/dashboard/piutang` | Yes |
| Materialized data for `/dashboard/inventory` | Yes |
| Background service to prepare snapshot data | Yes |
| Sales analytics sourced from **`BTR_Faktur`** | Yes |
| Preserve existing KPI definitions and traceability rules | Yes |
| Display snapshot freshness (`GeneratedAt`) | Yes |

### 7.2 Out of scope

| Item | Excluded |
| ---- | -------- |
| Overview Dashboard (`/dashboard` Operational Summary) as primary materialization target | Per product direction |
| Changes to `BTR_SalesOmzet` materialization / RO2 workflows | Fee calculation unchanged |
| Report pages (`/reports/*`) — may continue live queries | Separate initiative |
| Custom date filters, drilldown, export | Deferred (M16+) |
| Real-time streaming analytics | Not required for board cadence |

### 7.3 Overview Dashboard relationship

Today, Overview calls the **same three APIs** as analytical pages, so it inherits the same performance cost. Product has excluded Overview from this initiative's **design driver**, but practically:

- **Option A (minimal change):** Overview continues calling live APIs until separately optimized
- **Option B (recommended follow-up):** Overview reads **Layer A headline KPIs only** from snapshot tables (fast), while analytical routes read Layer A + B

Architect should clarify with Product Owner which option applies; neither blocks analytical materialization.

---

## 8. Gap Analysis

| Area | Current behavior | Desired behavior | Gap |
| ---- | ---------------- | ---------------- | --- |
| Piutang load time | Full-history SQL + in-memory aggregation per request | Pre-built open-balance snapshot | New snapshot store + refresh worker |
| Inventory load time | Full catalog query + BrgId rollup per request | Pre-built dimensional rollups | Extend materialization beyond `BTR_StokBalanceWarehouse` |
| Sales source | `BTR_SalesOmzet` (fee semantics) | `BTR_Faktur` (invoice semantics) | New sales aggregation rules + snapshot; **do not reuse `BTR_SalesOmzet`** |
| Server load | Spikes on concurrent portal users | Bounded refresh job | Background service |
| Staleness visibility | `GeneratedAt` on API | Must reflect snapshot refresh time, not request time | Worker writes timestamp at refresh completion |
| Piutang SQL efficiency | Recomputes components via subqueries | Use `BTR_Piutang.Sisa` | Business-equivalent optimization at refresh |

---

## 9. Risks and Considerations

| Risk | Severity | Mitigation (business) |
| ---- | -------- | --------------------- |
| Snapshot stale vs live BTR Desktop | Medium | Show `GeneratedAt`; define acceptable lag with finance; optional manual refresh |
| Sales KPI shift (Omzet → Faktur) | Medium | Communicate to board before go-live; align with Sales Report; document in portal domain doc |
| Piutang `Sisa` out of sync | Medium | Refresh after pelunasan workflows; monitor reconciliation with Piutang Report sample |
| Inventory balance stale | Low–Medium | Chain refresh after stock balance generation; show freshness |
| Dual code paths during migration | Low | Acceptance testing: materialized output must match current dashboard output (except sales source change) |
| Overview still slow (Option A) | Low | Accept temporarily or read Layer A KPIs only |

---

## 10. Acceptance Criteria (for verification)

The initiative succeeds when:

1. **Performance:** Analytical dashboard API endpoints read from materialized storage and respond consistently faster than today's live aggregation (measurable p95 target — Architect to define, e.g. &lt; 500 ms).
2. **Correctness — Piutang:** Materialized Total Piutang, Total Customer, aging bucket sums, and Top 10 match current `DashboardPiutangDal` output for the same `GeneratedAt` date.
3. **Correctness — Inventory:** Materialized totals and category/supplier breakdowns match current `DashboardInventoryDal` output.
4. **Correctness — Sales:** Materialized metrics match **`BTR_Faktur`-based** definitions in Section 5.3 and reconcile with Sales Report row counts/totals for the current month.
5. **Freshness:** API `GeneratedAt` reflects last successful background refresh, not request execution time.
6. **Isolation:** `BTR_SalesOmzet` and RO2 materialization workflows are unaffected.
7. **Overview scope:** No regression to Overview functionality; materialization scope documented as analytical-first.

---

## 11. Recommended Delivery Sequence

| Phase | Deliverable | Business value |
| ----- | ----------- | -------------- |
| **1** | Piutang snapshot + refresh | Largest performance win; finance/board receivable view |
| **2** | Inventory dimensional snapshot | Capital composition analytics |
| **3** | Sales snapshot from `BTR_Faktur` + target join | Correct board sales narrative; Sales Report alignment |
| **4** | Portal API switch to read snapshots | User-facing improvement |
| **5** (optional) | Overview KPI fast path from Layer A | Home page responsiveness |

---

## 12. Handoff to Architect

The Architect should produce an implementation plan covering:

- Snapshot table design (Layer A + B; optional Layer C for piutang detail)
- Background worker placement (portal host vs BTR Desktop vs standalone service)
- Refresh schedule, idempotency, and failure handling
- Migration strategy: shadow-run materialized output vs current DAL until verified
- Sales dashboard rewrite using `IFakturViewDal` / `BTR_Faktur` aggregation rules
- Index recommendations on source tables (e.g. `BTR_Piutang.Sisa` filtered index — technical detail for Architect)
- API changes (if any) — likely same endpoints, different DAL backing store

---

## 13. Open Questions for Product Owner

1. **Overview Dashboard:** After analytical materialization ships, should `/dashboard` home read snapshot KPIs only (Option B) or remain on live queries (Option A)?
2. **Sales KPI communication:** Is board Total Omzet definitively **`Sum(Faktur GrandTotal)`** for the month, excluding pipeline — confirming departure from omzet recognition?
3. **Refresh cadence:** What maximum staleness is acceptable for Board review (15 min, 1 hour, end-of-day)?
4. **Manual refresh:** Should finance/admin trigger an on-demand snapshot rebuild from BTR Desktop or portal?
5. **Historical analytics:** Will future date-range filters require **monthly snapshot history** (slowly changing aggregates) or on-demand recompute?

---

## Appendix A — Current Code References

| Component | Location |
| --------- | -------- |
| Sales dashboard DAL | `btr.infrastructure/ReportingContext/DashboardSalesAgg/DashboardSalesDal.cs` |
| Piutang dashboard DAL | `btr.infrastructure/ReportingContext/DashboardPiutangAgg/DashboardPiutangDal.cs` |
| Inventory dashboard DAL | `btr.infrastructure/ReportingContext/DashboardInventoryAgg/DashboardInventoryDal.cs` |
| Piutang source SQL (heavy) | `btr.infrastructure/FinanceContext/PiutangSalesWilayahRpt/PiutangSalesWilayahDal.cs` |
| Stock balance view | `btr.infrastructure/InventoryContext/StokBalanceRpt/StokBalanceViewDal.cs` |
| Sales report (Faktur source) | `btr.infrastructure/ReportingContext/SalesReportAgg/SalesReportDal.cs` |
| Faktur view SQL | `btr.infrastructure/SalesContext/FakturInfoAgg/FakturViewDal.cs` |
| Portal domain KPI definitions | `docs/features/btr-portal/btr-portal-domain.md` |

## Appendix B — Dimension Summary Matrix

| Dimension ↓ / Domain → | Sales | Piutang | Inventory | Purchasing (future) |
| ---------------------- | ----- | ------- | --------- | ------------------- |
| Month / Week | ● | ○ | ○ | ● |
| Sales Person | ● | ● | ○ | ○ |
| Customer | ● | ● | ○ | ○ |
| Wilayah | ● | ● | ○ | ○ |
| Category | ○ | ○ | ● | ○ |
| Supplier / Principal | ○ | ○ | ● | ● |
| Warehouse | ● | ○ | ○ (future) | ● |
| Aging Bucket | ○ | ● | ○ | ○ |
| Customer Classification | ● | ○ | ○ | ○ |
| Posting Status | ○ | ○ | ○ | ● |

**Legend:** ● = recommended for initial materialization priority; ○ = future extension
