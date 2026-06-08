# Knowledge Extraction Report — M19 Slow Moving & Dead Stock

**Source artifacts:**

- `docs/work/btr-portal/M19 Slow Moving & Dead Stock - Analysis.md`
- `docs/work/btr-portal/M19 Slow Moving & Dead Stock - Plan.md`
- `docs/work/btr-portal/M19 Slow Moving & Dead Stock - Implementation Summary.md`

**Extraction date:** 2026-06-08  
**Curator role:** Knowledge Curator Agent

---

## Summary

Completed work on **M19 Slow Moving & Dead Stock** has been distilled into permanent BTR Portal and materialized-dashboard feature artifacts. M19 adds a dedicated **InventoryRisk** snapshot domain (six tables) with item-level classification based on **Last Faktur Date** — following the M17/M18 dedicated-snapshot pattern while reusing M15 BrgId-first position rules via a shared item group builder. Executive Dashboard promotion is **deferred to Phase 2** per Product Owner decision.

---

## Knowledge Extracted

### M19 — Domain (WHY)

- New route `/dashboard/inventory-risk` — **Slow Moving & Dead Stock Dashboard** — answers *Which inventory requires management attention and why?*
- **Supplements** Inventory Dashboard (`/dashboard/inventory`); does not replace composition analytics
- Dedicated `BTR_PortalDashboardInventoryRisk*` snapshot — not live composition from M15 Inventory snapshot tables
- Authoritative movement signal: `MAX(FakturDate)` per `BrgId` from gross non-void Faktur only
- Classification: **Never Sold** (no Faktur history) · **Slow Moving** (90–179 days idle) · **Dead Stock** (≥180 days) · **Active** (≤89 days, excluded from at-risk KPIs)
- Mutually exclusive KPI counts; Never Sold excluded from Slow/Dead Top 10
- At-Risk Inventory % = disjoint Never + Slow + Dead value ÷ TotalInventoryValue (same denominator as M15)
- Attention list: one row per item × signal (DeadStock, SlowMoving, NeverSold)
- Drill-down: item row → Inventory Report with name pre-filter (`?q=`)
- Portal vs Desktop: Last Faktur Date is portal authority; Kartu Stok Summary may differ
- Phase 2 (deferred): Executive Dashboard — Dead Stock Value, At-Risk %, Inventory Risk Attention Indicator
- Out of scope: salesman dimension, ABC, warehouse breakdown, export, Kartu Stok drill-down, retur-adjusted demand, mutasi-based classification

### M19 — Architecture (WHAT)

- `DashboardInventoryRiskAgg` read path; `DashboardInventoryRiskAggregator` + shared `DashboardInventoryItemGroupBuilder`
- New DAL: `IBrgLastFakturDal` / `BrgLastFakturDal` — `MAX(FakturDate)` per item
- Worker `RefreshDashboardInventoryRiskSnapshotWorker` reads `IStokBalanceViewDal` + `IBrgLastFakturDal`
- Six snapshot tables: Kpi, Aging, Attention, TopDead, TopSlow, Breakdown
- API: `GET /api/dashboard/inventory-risk`
- Refresh cadence: 60 minutes (`InventoryRiskIntervalMinutes`); runs after Inventory in `--domain All`
- Empty snapshot: `IsAvailable = false` (graceful) — like Customer/Salesman
- Protected unchanged: `DashboardInventoryAggregator`, `BTR_PortalDashboardInventory*`, `GET /api/dashboard/inventory`, `DashboardExecutiveComposer` (until Phase 2)
- Frontend: `InventoryRiskDashboardView`, `InventoryRiskAttentionList`, `InventoryRiskNavigationSection`; extended `AgingPieChart` for inventory buckets; `loadInventoryRisk()`; Inventory Report reads `?q=` on mount

### M19 — Operational (HOW — end user)

- Sidebar: Dashboard → **Inventory Risk** (after Inventory)
- Layout (Proposal A): Attention Cards → Aging pie → Category/Supplier risk bars → Attention List → Top 10 Dead/Slow → Navigation
- Staleness banner when snapshot exceeds 60-minute interval
- Cross-check Total Inventory Value with Inventory Dashboard and Inventory Report footer

### M19 — Deployment (HOW — maintainer)

- Seventh Task Scheduler job: `BTR-Portal-Dashboard-InventoryRisk` (60 min)
- Admin refresh and worker CLI accept `--domain InventoryRisk`
- Health endpoint lists InventoryRisk domain with `InventoryRiskIntervalMinutes`
- SQL deploy: six `BTR_PortalDashboardInventoryRisk*` tables before first refresh
- Initial backfill: include InventoryRisk in `--domain All` verification
- Refresh order in `All`: Piutang → Inventory → **InventoryRisk** → Sales → Purchasing → Customer → Salesman

---

## Permanent Artifacts Updated

| Artifact | Four-question coverage |
| -------- | ---------------------- |
| `docs/features/btr-portal/btr-portal-domain.md` | **WHY** — dashboard purpose, classification rules, KPIs, business rules, product state M19, roadmap Phase 2 |
| `docs/features/btr-portal/btr-portal-architecture.md` | **WHAT** — `DashboardInventoryRiskAgg`, snapshot topology, shared item builder, API, DTOs, refresh order, architectural rules |
| `docs/features/btr-portal/btr-portal-operational.md` | **HOW (user)** — navigation, workflow, FAQ; **HOW (deploy)** — 7th scheduler job, CLI domains, smoke test |
| `docs/features/materialized-dashboard/materialized-dashboard-domain.md` | **WHY** — inventory risk KPI rules, acceptance criteria, 7-domain cadence |
| `docs/features/materialized-dashboard/materialized-dashboard-architecture.md` | **WHAT** — InventoryRisk worker, tables, API, config, code locations |
| `docs/features/materialized-dashboard/materialized-dashboard-operational.md` | **HOW (deploy)** — seventh scheduler job, backfill verification, CLI domains |

---

## Temporary Artifacts — Archival Recommendation

The following work artifacts have served their delivery purpose. Knowledge has been extracted into permanent docs above.

| Artifact | Recommendation |
| -------- | ---------------- |
| `docs/work/btr-portal/M19 Slow Moving & Dead Stock - Analysis.md` | **Archive** — PO decisions preserved in domain doc; discussion history disposable |
| `docs/work/btr-portal/M19 Slow Moving & Dead Stock - Plan.md` | **Archive** — implementation steps superseded by code + permanent architecture |
| `docs/work/btr-portal/M19 Slow Moving & Dead Stock - Implementation Summary.md` | **Archive** — file-level change list disposable; durable rules in permanent docs |

Retain in `docs/work/btr-portal/` until Product Owner confirms archival, or move to `docs/archive/btr-portal/m19/` per project convention.

---

## Inconsistencies / Open Items (Report Only)

| Item | Status |
| ---- | ------ |
| Executive Dashboard inventory risk KPIs | **Deferred Phase 2** — documented in domain and materialized-dashboard docs |
| M19 Analysis Section 12 vs Plan boundary resolutions | **Resolved in plan** — mutually exclusive bands documented in permanent domain doc |
| Desktop Kartu Stok vs portal Last Faktur | **Expected divergence** — documented for support |

No unresolved business rule conflicts found during extraction.

---

*End of M19 Knowledge Extraction Report*
