# M32.B1 — Entity Analytics Historical Backfill  
## Feasibility Study & Implementation Plan

**Status:** Analysis complete — no implementation  
**Date:** 2026-06-24  
**Inputs reviewed:** `entity-analytics-roadmap-authoritative.md`, `entity-analytics-architecture.md`, M32.1–M32.11 summaries, `materialized-dashboard-domain.md`, `Create_BTRPD_PortalDashboard_Tables.sql`, Entity Analytics producers/registrars

---

## Executive Answer

**Can Customer, Salesman, Supplier, and Item Entity Analytics history be reconstructed from existing Dashboard Snapshot history without replaying years of raw transaction data?**

**No — not in the general case.**

Portal dashboard snapshots overwhelmingly use `SnapshotKey = 'CURRENT'` (delete-and-replace each refresh). **Only one entity-level historical store exists:** `BTRPD_SalesmanRepHistory`. That table preserves `SalesPersonId` / `SalesPersonCode` and monthly KPI columns, but it does **not** cover Customer, Supplier, or Item, and it does not support L3 Attention, L4 Relationships, or full L2/L5 reconstruction on its own.

**What is feasible:**

| Path | Scope |
|------|--------|
| **Snapshot-sourced (no transaction replay)** | Salesman **L1 partial** via RepHistory → `BTRPD_EntityAnalytics_Monthly` |
| **Transaction-period replay (aggregator replay)** | Customer, Supplier, Item L1–L5 for closed months, using existing domain aggregators + `IEntityAnalyticsProducer` pipeline |
| **Not feasible from snapshots alone** | Customer/Supplier/Item trends, all ranking history, all attention timelines, relationships, radar for prior periods |

**Recommendation:** **Proceed with Partial Backfill** — hybrid architecture: RepHistory migration for Salesman L1, then transaction-period replay for other entity types and layers.

---

# Part 1 — Historical Snapshot Inventory

## Platform retention policy (authoritative)

From `materialized-dashboard-domain.md`:

| Policy | Value |
|--------|--------|
| Default snapshot pattern | `SnapshotKey = 'CURRENT'` — single active row, delete-and-replace |
| Historical dashboard retention | **CURRENT row only** (explicit product decision; Phase 5 historical retention deferred) |
| Entity Analytics L1 retention (post-M32) | **36 months** default (`EntityAnalyticsOptions.HistoryRetentionMonths`) |
| Exception | `BTRPD_SalesmanRepHistory` — period-keyed upsert, **retained indefinitely** |

`PeriodYear` / `PeriodMonth` on KPI header tables (e.g. `BTRPD_CustomerKpi`) describe the **current** reporting month, not a historical archive.

---

## Inventory by entity domain

### Customer (M17 / M29 / M31)

| Table | Grain | Period support | Entity key preserved? | Historical depth | Retention |
|-------|-------|----------------|----------------------|------------------|-----------|
| `BTRPD_CustomerKpi` | Company aggregate | CURRENT month label only | N/A (no entity) | 0 | CURRENT |
| `BTRPD_CustomerTopOmzet` | Top 10 | CURRENT | **CustomerCode** ✓ | 0 | CURRENT |
| `BTRPD_CustomerTopPiutang` | Top 10 | CURRENT | **CustomerCode** ✓ | 0 | CURRENT |
| `BTRPD_CustomerAttention` | Customer × signal | CURRENT | **CustomerCode** ✓ | 0 | CURRENT |
| `BTRPD_CustomerSegmentation` | Segment rollup | CURRENT | No per-customer | 0 | CURRENT |
| `BTRPD_CustomerPortfolioCustomer` | **Full portfolio** (~all customers) | CURRENT | **CustomerCode**, CustomerKey ✓ | 0 | CURRENT |
| `BTRPD_CustomerPortfolioPriority` | Priority subset | CURRENT | **CustomerCode** ✓ | 0 | CURRENT |
| `BTRPD_CustomerRiskForecastCustomer` | Top 20 risk | CURRENT | Customer identifiers ✓ | 0 | CURRENT |

**Finding:** Entity codes exist on per-customer CURRENT rows, but **no month dimension** — prior months were overwritten and are unrecoverable from snapshots.

---

### Salesman (M18)

| Table | Grain | Period support | Entity key preserved? | Historical depth | Retention |
|-------|-------|----------------|----------------------|------------------|-----------|
| `BTRPD_SalesmanKpi` | Team aggregate | CURRENT month label | N/A | 0 | CURRENT |
| `BTRPD_SalesmanTopOmzet` / `TopAchievement` / `TopPiutang` | Top 10 each | CURRENT | **SalesPersonId**, **SalesPersonCode** ✓ | 0 | CURRENT |
| `BTRPD_SalesmanAttention` | Rep × signal | CURRENT | **SalesPersonId**, **SalesPersonCode** ✓ | 0 | CURRENT |
| `BTRPD_SalesmanPrincipalAchievement` | Rep × principal | CURRENT | **SalesPersonId** ✓ | 0 | CURRENT |
| **`BTRPD_SalesmanRepHistory`** | **Rep × calendar month** | **PeriodYear, PeriodMonth** | **SalesPersonId**, **SalesPersonCode** ✓ | **From M18 deploy → present** | **Indefinite upsert** |

**RepHistory columns:** TargetAmount, CompletedOmzet, AchievementPercent, AchievementBand, OpenBalance, IsActive.

**Finding:** Only table suitable for snapshot-based L1 backfill. Depth = months since M18 RepHistory was enabled (not pre-M18).

---

### Supplier / Principal (M21)

| Table | Grain | Period support | Entity key preserved? | Historical depth | Retention |
|-------|-------|----------------|----------------------|------------------|-----------|
| `BTRPD_PurchasingKpi` | Company aggregate | CURRENT month label | N/A | 0 | CURRENT |
| `BTRPD_PurchasingTopPrincipal` | Top 10 | CURRENT | **PrincipalName only** — no SupplierId/Code | 0 | CURRENT |
| `BTRPD_PurchasingManagementKpi` | Company aggregate | CURRENT | N/A | 0 | CURRENT |
| `BTRPD_PurchasingManagementAttention` | Principal × signal | CURRENT | **EntityName only** — no SupplierId/Code | 0 | CURRENT |
| `BTRPD_PurchasingManagementTopPrincipal` | Top 10 | CURRENT | **PrincipalName only** | 0 | CURRENT |

**Finding:** Supplier snapshots lack stable **SupplierId/SupplierCode** on attention and Top-N tables. Entity Analytics uses `SupplierId`/`SupplierCode` from M21 portfolio rows (in-memory at refresh, persisted only in `BTRPD_EntityAnalytics_*` going forward). **No historical supplier-per-month archive exists.**

---

### Item (M19 / M28 / M28.5)

| Table | Grain | Period support | Entity key preserved? | Historical depth | Retention |
|-------|-------|----------------|----------------------|------------------|-----------|
| `BTRPD_InventoryRiskAttention` | Item × signal | CURRENT | **BrgId**, **BrgCode** ✓ | 0 | CURRENT |
| `BTRPD_InventoryRiskTopDead` / `TopSlow` | Top 10 | CURRENT | **BrgId**, **BrgCode** ✓ | 0 | CURRENT |
| `BTRPD_InventoryForecastRisk` | Per-item forecast | CURRENT | BrgId ✓ | 0 | CURRENT |
| `BTRPD_InventoryOptimizationAction` | Per-item action | CURRENT | BrgId ✓ | 0 | CURRENT |

**Finding:** Item codes exist on CURRENT per-item rows, but coverage is **attention/forecast subsets**, not full active SKU population, and **no monthly history**.

---

### Company-level “trend” tables (not entity-backfillable)

| Table | Content | Entity keys? |
|-------|---------|--------------|
| `BTRPD_SalesWeekTrend` | Current-month weekly company sales | No |
| `BTRPD_PurchasingWeekTrend` | Current-month weekly company purchase | No |
| `BTRPD_CashFlowRecoveryTrend` | Collection recovery series | No |

These support dashboard charts for the **open month only**; prior months are not retained.

---

### Entity Analytics tables (target, not source)

`BTRPD_EntityAnalytics_Current`, `_Monthly`, `_Ranking`, `_Attention`, `_Relationship`, `_Radar`, `_MonthClose` — populated from deployment date forward only.

---

# Part 2 — L1 Trend Feasibility

## Customer

| Entity Analytics KPI | Trend? | Dashboard snapshot source | Assessment |
|---------------------|--------|---------------------------|------------|
| CU-KPI-009 MTD Omzet | ✓ | `CustomerPortfolioCustomer.MtdOmzet` (CURRENT) | **Not reconstructable** — no monthly archive |
| CU-KPI-010 Open Balance | ✓ | Portfolio `OpenBalance` | **Not reconstructable** — point-in-time; no month-end piutang history in snapshots |
| FI-KPI-013 Overdue Exposure | ✓ | Portfolio `OverdueBalance` | **Not reconstructable** |
| EA meta (FakturCount6Mo, PortfolioPriority) | L0 dims | Portfolio CURRENT | **Not reconstructable** for trends |

**Via transaction replay:** CU-KPI-009 fully reconstructable from `FakturView` by month. CU-KPI-010/FI-KPI-013 require **month-end piutang reconstruction** (harder — open balance as-of date, not stored in snapshots).

**L1 verdict:** **Partially reconstructable** — omzet yes via replay; piutang/overdue trends need piutang-as-of logic or accept gaps.

---

## Salesman

| Entity Analytics KPI | Trend? | Dashboard snapshot source | Assessment |
|---------------------|--------|---------------------------|------------|
| SF-KPI-008 MTD Omzet | ✓ | `BTRPD_SalesmanRepHistory.CompletedOmzet` | **Fully reconstructable** from RepHistory |
| SF-KPI-009 Achievement % | ✓ | RepHistory `AchievementPercent` | **Fully reconstructable** |
| SF-KPI-010 Open Balance | ✓ | RepHistory `OpenBalance` | **Fully reconstructable** (month-end capture when month was active) |

**L1 verdict:** **Fully reconstructable from snapshots** for months present in RepHistory. Pre-M18 months require transaction replay.

---

## Supplier

| Entity Analytics KPI | Trend? | Dashboard snapshot source | Assessment |
|---------------------|--------|---------------------------|------------|
| PU-KPI-001 MTD Purchase | ✓ | No per-supplier monthly table | **Not reconstructable** from snapshots |
| PU-KPI-002 Invoice Count | ✓ | — | **Not reconstructable** |
| PU-KPI-003 Posted % | ✓ | — | **Not reconstructable** |
| EA-DIM-INVENTORY-VALUE | Rank only | CURRENT portfolio (transient) | **Not reconstructable** for trend |

**L1 verdict:** **Not reconstructable** from dashboard snapshots. Requires invoice replay by month × supplier.

---

## Item

| Entity Analytics KPI | Trend? | Dashboard snapshot source | Assessment |
|---------------------|--------|---------------------------|------------|
| IN-KPI-001 Inventory Value | ✓ | No monthly per-SKU table | **Not reconstructable** |
| IN-KPI-020 Days of Supply | ✓ | — | **Not reconstructable** |
| IN-KPI-021 Recommended Purchase Qty | ✓ | — | **Not reconstructable** |

**L1 verdict:** **Not reconstructable** from snapshots. Requires monthly stock + consumption replay (highest cardinality).

---

# Part 3 — L2 Ranking Feasibility

| Option | Finding |
|--------|---------|
| **A — Recompute from historical dashboard snapshots** | **Not feasible.** Top-N tables (`CustomerTopOmzet`, `SalesmanTopOmzet`, etc.) store only **10 entities** with **no period key**. Cannot recover rank for entity #47 in March 2024. |
| **B — Historical ranking already exists** | **No.** `BTRPD_EntityAnalytics_Ranking` exists but only from EA deploy date. No domain ranking history table. |

**Feasible approach:** After L1 backfill per period, run existing `EntityRankingEngine` over the full eligible population for each `(EntityType, Year, Month)` — same as live worker, but batched historically.

**L2 verdict:** **Recomputable only via transaction-period replay → L1 → RankingEngine**, not from dashboard snapshots.

---

# Part 4 — L3 Attention Feasibility

Attention signals today are **current-state diffs** written to `BTRPD_EntityAnalytics_Attention` at refresh. Domain attention tables (`CustomerAttention`, `SalesmanAttention`, `InventoryRiskAttention`, `PurchasingManagementAttention`) are all **CURRENT only**.

| Signal class | Historical from snapshots? | Replay feasibility |
|--------------|---------------------------|-------------------|
| Dormant (90-day rule) | No | **Yes** — re-evaluate last Faktur as-of month-end |
| Overdue / plafond breach | No | **Partial** — needs piutang as-of month-end |
| Below Target (salesman) | No | **Yes** — Faktur + target for that month |
| Dead Stock / Slow Moving (item) | No | **Partial** — needs stock + last Faktur as-of each month-end |
| Supplier backlog / compound dependency | No | **Hard** — cross-domain M21 rules + inventory snapshot as-of date |

**L3 verdict:** **Not reconstructable from snapshot history.** Approximate reconstruction possible via **period-scoped aggregator replay** with documented semantic caveats (especially piutang and inventory point-in-time).

---

# Part 5 — L5 Radar Feasibility

Radar (`EntityRadarEngine`) requires:

1. Per-entity axis KPI values for a period (from L0/L1)
2. **Peer group distribution** for that same period (Customer → Wilayah, Item → Category, etc.)

| Question | Answer |
|----------|--------|
| Can axes be rebuilt from historical snapshot data? | **No** — no historical per-entity KPI store except Salesman RepHistory (3 KPIs only) |
| Can peer groups be recreated historically? | **Yes, after L1 backfill** — peer groups use master dimensions (Wilayah, Category) + L1 population for that month |
| Band fallback axes | Require achievement bands / movement class per period — needs replay |

**L5 verdict:** **Derivable only after L1 (and partial L0) backfill** via `EntityRadarEngine` per period. **Not snapshot-replayable** directly.

---

# Part 6 — Backfill Architecture Options

```text
                    ┌─────────────────────────────────────┐
                    │     Option A: Transaction Replay     │
                    │  Transactions → Aggregator(period)   │
                    │       → EntityAnalyticsProducer      │
                    │       → BTRPD_EntityAnalytics_*        │
                    └─────────────────────────────────────┘

                    ┌─────────────────────────────────────┐
                    │  Option B: Dashboard Snapshot Replay │
                    │  BTRPD_* history → EA Producer       │
                    │  (only RepHistory has history)       │
                    └─────────────────────────────────────┘

                    ┌─────────────────────────────────────┐
                    │         Option C: Hybrid             │
                    │  Salesman L1 ← RepHistory            │
                    │  Others ← Transaction Replay (A)     │
                    │  L2/L5 ← Engines on backfilled L1    │
                    └─────────────────────────────────────┘
```

| Criterion | Option A — Transaction Replay | Option B — Snapshot Replay | Option C — Hybrid |
|-----------|------------------------------|---------------------------|-------------------|
| **Accuracy** | Highest — same aggregators as live worker | N/A for 3/4 entity types | High; Salesman L1 matches SF01 trend chart |
| **Complexity** | Medium-high — period-parameterized aggregators | Low for Salesman only | Medium — two paths |
| **Runtime** | Hours for 36 mo × all entities (Item worst) | Minutes for Salesman RepHistory copy | Optimized |
| **Risk** | DB load; piutang-as-of semantics | **False confidence** if team assumes snapshots exist | Manageable |
| **Operational impact** | Off-hours worker; throttle concurrency | Minimal | Recommended |

### Recommendation: **Option C (Hybrid)**

1. **Salesman L1:** Direct `BTRPD_SalesmanRepHistory` → `BTRPD_EntityAnalytics_Monthly` migration (already planned in M32.9).
2. **All other entity types + all L2–L5:** Transaction-period replay using existing producers/engines.
3. **Do not** build snapshot-replay infrastructure for `BTRPD_CustomerTopOmzet`-style tables — wrong grain and no history.

---

# Part 7 — Worker Design (Conceptual)

## `EntityAnalyticsBackfillWorker`

```text
CLI: btr.portal.worker --domain EntityAnalyticsBackfill [options]

Options:
  --entity-type       Customer | Salesman | Supplier | Item | All
  --from-period       YYYY-MM
  --to-period         YYYY-MM
  --layers            L1 | L1,L2,L3,L4,L5 (default L1,L2,L5)
  --resume-from       EntityId or period checkpoint
  --dry-run           Validate + count rows; no writes
  --batch-size        Entities per transaction (default 500)
  --max-parallel      Months in parallel (default 1)
  --source            RepHistory | TransactionReplay (auto per entity)
```

### Architecture

```text
EntityAnalyticsBackfillOrchestrator
  ├── BackfillJobStore (BTRPD_EntityAnalytics_BackfillJob — new table in implementation)
  │     JobId, EntityType, FromPeriod, ToPeriod, Layers, Status, LastCheckpoint, RowCounts
  ├── IBackfillSourceStrategy
  │     ├── RepHistoryBackfillSource (Salesman L1 only)
  │     └── TransactionPeriodBackfillSource (all entities)
  │           → loads DAL data filtered to [periodStart, periodEnd]
  │           → invokes domain aggregator with PeriodContext
  │           → invokes IEntityAnalyticsProducer.Produce(backfillContext)
  ├── Layer runners (reuse existing engines)
  │     L1: producer SaveMonthlyHistory
  │     L2: EntityRankingEngine.ComputeAndPersistRanks(period)
  │     L3: EntityAttentionEngine.DiffSignals(period) — optional phase
  │     L4: EntityRelationshipEngine — optional phase (expensive)
  │     L5: EntityRadarEngine.ComputeScores(period)
  └── Idempotency: UPSERT on (EntityType, EntityId, Period, KpiId); BackfillJobId in audit column
```

### Capabilities

| Capability | Design |
|------------|--------|
| Entity selection | `--entity-type` + optional `--entity-ids` file |
| Date range | Closed months only; reject current open month unless `--include-open-month` |
| Resume | Checkpoint per `(EntityType, PeriodYear, PeriodMonth, LastEntityId)` |
| Idempotent | Upsert semantics identical to live producer; `IsClosed=true` after backfill |
| Dry run | Count entities × periods × KPIs; estimate storage; no writes |
| Audit | `BTRPD_RefreshLog` entry per backfill batch with `Domain=EntityAnalyticsBackfill` |

### Salesman fast path

```text
INSERT INTO BTRPD_EntityAnalytics_Monthly (...)
SELECT ... FROM BTRPD_SalesmanRepHistory rh
JOIN KPI mapping (SF-KPI-008/009/010)
WHERE NOT EXISTS (existing L1 row)
```

Then run L2/L5 engines per period over migrated L1.

---

# Part 8 — Performance Analysis

**Assumptions (from feasibility study):** ~3,000 customers, ~80 salesmen, ~250 suppliers, ~8,000 active items; ~6 trend KPIs/entity; 36-month default retention.

## Storage growth (`BTRPD_EntityAnalytics_*`)

| Horizon | Customer | Salesman | Supplier | Item (active subset) | Total EA incremental |
|---------|----------|----------|----------|----------------------|----------------------|
| 12 mo | ~11 MB | ~0.3 MB | ~0.9 MB | ~29 MB | ~45 MB |
| 24 mo | ~22 MB | ~0.6 MB | ~1.8 MB | ~58 MB | ~90 MB |
| 36 mo | ~32 MB | ~1 MB | ~3 MB | ~86 MB | ~170 MB |

*Aligns with architecture estimate (~170 MB for 36 mo). Item dominates.*

## Runtime estimates (transaction replay, single-threaded, off-peak)

| Entity | 12 mo | 24 mo | 36 mo | Notes |
|--------|-------|-------|-------|-------|
| Customer | 2–4 h | 4–8 h | 6–12 h | ~3k entities; Faktur + piutang loads per month |
| Salesman | **5–15 min** (RepHistory) | 10–30 min | 15–45 min | Fast path; replay only for pre-RepHistory gaps |
| Supplier | 30–60 min | 1–2 h | 1.5–3 h | ~250 principals; invoice aggregation |
| Item | 8–16 h | 16–32 h | 24–48 h | Highest risk; scope to ADR-EA-011 active subset |

**L3/L4 add ~50–100%** to runtime if included (relationship rollups = FakturItem joins per month).

## Worker load risks

| Risk | Severity | Mitigation |
|------|----------|------------|
| Item 36-mo backfill blocks SQL Server | High | Active-subset gate; run Item last; batch by category |
| Concurrent live refresh + backfill | Medium | Mutex per entity type; off-hours schedule |
| Piutang-as-of approximation errors | Medium | Document limitations; phase piutang trends after omzet |
| RepHistory vs replay mismatch (Salesman) | Low | Reconciliation test per rep/period |

---

# Part 9 — Migration Strategy

**Recommended: Hybrid — one-time historical migration + forward-only live worker**

| Phase | Approach | Rationale |
|-------|----------|-----------|
| **1. Salesman L1** | One-time admin job: RepHistory → L1 | Minutes; zero transaction load; closes SF01 ↔ Profile trend gap |
| **2. Customer L1+L2+L5** | Admin-triggered backfill, 36 mo | Highest management value; moderate runtime |
| **3. Supplier L1+L2+L5** | Scheduled after Customer validated | Lower cardinality |
| **4. Item L1+L2+L5** | Admin-triggered; active subset only | Longest job — separate maintenance window |
| **5. L3 Attention** | Optional second pass | Lower priority than trends/ranking |
| **6. L4 Relationships** | Defer or last 12 mo only | Expensive; profiles usable without historical L4 |
| **Forward** | Existing domain workers unchanged | Live worker continues appending L1 from deploy date |

**Not recommended:** Continuous scheduled full backfill — one-time + forward accumulation is sufficient.

---

# Part 10 — Deliverables

## 1. Feasibility Report

| Dimension | Assessment |
|-----------|------------|
| **Technical feasibility** | **Partial** — platform engines (L1–L5) are ready; source history is not |
| **Data completeness** | **Low from snapshots** (RepHistory only); **High from transaction replay** for transactional KPIs |
| **Performance impact** | Manageable for Customer/Salesman/Supplier; **Item requires careful scheduling** |
| **Operational risk** | Medium — DB load, semantic caveats on piutang/inventory point-in-time |
| **Implementation effort** | **Medium** (~3–4 milestones after analysis) |

---

## 2. Historical Data Coverage Matrix

| Entity | KPI / Layer | Historical snapshot source | Coverage from snapshots | Coverage via transaction replay |
|--------|-------------|---------------------------|------------------------|--------------------------------|
| **Customer** | L1 Omzet (CU-KPI-009) | None | ❌ None | ✅ Full |
| **Customer** | L1 Open Balance (CU-KPI-010) | None | ❌ None | ⚠️ Partial (as-of piutang) |
| **Customer** | L1 Overdue (FI-KPI-013) | None | ❌ None | ⚠️ Partial |
| **Customer** | L2 Ranking | Top 10 CURRENT only | ❌ None | ✅ Full |
| **Customer** | L3 Attention | CURRENT attention | ❌ None | ⚠️ Partial |
| **Customer** | L4 Relationships | None | ❌ None | ⚠️ Optional / expensive |
| **Customer** | L5 Radar | None | ❌ None | ✅ After L1 |
| **Salesman** | L1 SF-KPI-008/009/010 | **RepHistory** | ✅ Months since M18 | ✅ Pre-M18 gaps |
| **Salesman** | L2–L5 | None | ❌ None | ✅ After L1 |
| **Supplier** | L1 PU-KPI-001/002/003 | None (Top 10 name-only) | ❌ None | ✅ Full |
| **Supplier** | L2–L5 | None | ❌ None | ✅ After L1 |
| **Item** | L1 IN-KPI-001/020/021 | CURRENT subsets only | ❌ None | ⚠️ Active subset; high cost |
| **Item** | L2–L5 | None | ❌ None | ⚠️ Active subset |

---

## 3. Recommended Architecture

**Option C — Hybrid**

```text
Salesman:
  BTRPD_SalesmanRepHistory ──► BTRPD_EntityAnalytics_Monthly (L1)
                          └──► EntityRankingEngine + EntityRadarEngine per period

Customer / Supplier / Item:
  BTR_Faktur / BTR_Invoice / BTR_Piutang / StokBalance / …
        └──► Domain Aggregator(periodContext)
              └──► IEntityAnalyticsProducer
                    └──► L0/L1 → L2 → (L3) → (L4) → L5
```

**Explicitly rejected:** Replay of `BTRPD_CustomerTopOmzet` or other Top-N CURRENT tables.

---

## 4. Worker Design

See Part 7. Conceptual components:

- `EntityAnalyticsBackfillWorker` CLI domain
- `IBackfillSourceStrategy` (RepHistory vs TransactionReplay)
- `BackfillJobStore` for resume/checkpoint
- Reuse `EntityAnalyticsProducerOrchestrator` + existing engines
- Dry-run and idempotent upsert semantics

---

## 5. Implementation Plan (Suggested Milestones)

| Milestone | Scope | Deliverables |
|-----------|-------|--------------|
| **M32.B1.1** | Analysis & design sign-off | This document; Product Owner approval; piutang-as-of policy decision |
| **M32.B1.2** | Infrastructure | `EntityAnalyticsBackfillWorker`; job/checkpoint table; `--dry-run`; RepHistory → L1 script/service |
| **M32.B1.3** | Salesman backfill | RepHistory migration; L2+L5 historical; reconciliation vs SF01 trend chart |
| **M32.B1.4** | Customer backfill | Period-scoped `DashboardCustomerAggregator` + producer; L1 omzet; L2+L5; piutang trends phased |
| **M32.B1.5** | Supplier backfill | PurchasingManagement period replay; L1–L2–L5 |
| **M32.B1.6** | Item backfill (active subset) | InventoryRisk period replay; ADR-EA-011 gate; L1–L2–L5 |
| **M32.B1.7** | L3 Attention (optional) | Period-scoped signal evaluation; documented limitations |
| **M32.B1.8** | Operational hardening | Runbook; monitoring; `BTRPD_RefreshLog` audit; developer guide update |

---

## 6. Risks & Limitations

| Limitation | Impact |
|------------|--------|
| **No dashboard snapshot history** except RepHistory | Option B invalid for 75% of entity types |
| **Top-N tables** have entity codes but wrong grain + no periods | Cannot infer full-population ranks |
| **Supplier attention/top tables lack SupplierId** | Snapshot replay impossible even if history existed |
| **Pre-M18 / pre-M31 months** | No RepHistory or portfolio snapshots ever existed |
| **Piutang historical trend** | Open balance is point-in-time; month-end reconstruction is approximate |
| **Inventory historical trend** | Stock position + movement class as-of prior months is expensive |
| **L4 relationship history** | Low ROI vs cost; defer |
| **Semantic drift** | Backfilled months use current aggregator rules, not rules active then (document per ADR-EA-005) |
| **Entity Analytics 36-mo purge** | RepHistory indefinite ≠ L1 indefinite; align retention policy before migration |

---

## 7. Final Recommendation

### **Proceed with Partial Backfill**

**Justification:**

1. Management need (immediate meaningful trends) **cannot** be met from dashboard snapshot replay alone — the critical verification confirms most snapshot tables are CURRENT-only Top-N or aggregates without period history.
2. **Salesman** can deliver value in days via RepHistory → L1 (already deferred in M32.9).
3. **Customer** transaction replay delivers the highest business value (M32 MVP success criterion) with acceptable runtime.
4. **Supplier** is straightforward after Customer pattern proof.
5. **Item** should be **scoped** (active subset, possibly 12–24 mo first) due to cardinality.
6. L3/L4 historical are **nice-to-have**; L1+L2+L5 address the core “trajectory and rank context” ask.

**Not recommended:** “Do Not Proceed” — the platform is built; the gap is source history, solvable via hybrid replay.

**Not recommended:** “Customer-only forever” — Salesman RepHistory path is nearly free; Supplier is low cost once infrastructure exists.

---

## Entity Key Verification Summary (Critical Finding)

| Table type | Entity code/key preserved? | Sufficient for backfill? |
|------------|---------------------------|-------------------------|
| Full-portfolio CURRENT (`CustomerPortfolioCustomer`) | ✅ CustomerCode | ❌ No period history |
| Attention CURRENT | ✅ Codes/Ids | ❌ No period history |
| Top-N CURRENT | ✅ Codes (except Supplier name-only) | ❌ Top 10 only + no period |
| **`SalesmanRepHistory`** | ✅ SalesPersonId/Code | ✅ **Only viable snapshot source** |
| Company KPI / week trend | N/A | ❌ Not entity grain |

**This single finding determines M32.B1 scope:** dashboard snapshot replay is a **Salesman L1 shortcut**, not a general backfill strategy. General backfill requires **transaction-period replay** through existing aggregators and Entity Analytics producers.