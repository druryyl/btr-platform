# Knowledge Extraction Report — M18 Salesman Performance

**Source folder:** `docs/work/btr-portal`  
**Extraction date:** 2026-06-08  
**Curator role:** Knowledge Curator Agent

---

## Summary

Completed work on **M18 Salesman Performance** has been distilled into permanent BTR Portal and materialized-dashboard feature artifacts. M18 adds a dedicated Salesman snapshot domain (six tables) with cross-domain attention signals across Sales and Piutang — following the M17 Customer Analytics pattern. Executive dashboard, Sales Top 10 Salesman snapshot, and domain snapshots remain unchanged.

---

## Knowledge Extracted

### M18 — Domain (WHY)

- New route `/dashboard/salesmen` — **Salesman Performance** — answers *Which salesman requires management attention and why?*
- Cross-domain lens: **Sales + Piutang only** (not Inventory/Purchasing)
- **Supplements** Sales Dashboard Top 10 Salesman ranking; does not replace Management Attention Center or executive dashboard
- Dedicated `BTR_PortalDashboardSalesman*` snapshot — not live composition from Sales/Piutang/Customer snapshot tables
- Six approved attention signals: Below Target · No Target · High Overdue Exposure · High Piutang Exposure · Customer Concentration · Dormant Customer Portfolio
- Mandatory rankings: Top 10 Omzet (current month) + Top 10 Achievement % + Top 10 Piutang (all open) with `SalesPersonCode` and % of total
- Segmentation: Wilayah, Segment, Active vs Inactive (current-month Faktur)
- Drill-down: salesman row → Sales or Piutang Report with name pre-filter (`?q=`)
- Achievement bands: M16 thresholds (≥100% Healthy · 80–99% Warning · <80% Critical · no target Unknown)
- Attribution: sales omzet from `Faktur.SalesPersonId`; piutang from invoicing salesman; dormant customers via last-invoicing salesman (90-day rule)
- Out of scope: pipeline omzet, Effective Call, route coverage, GPS, retur, collection effectiveness/DSO (M20), field activity (M25), Bottom 10 rankings, historical trends, unified salesman score, new Salesman Report route, executive changes, Sales Top 10 table changes

### M18 — Architecture (WHAT)

- `DashboardSalesmanAgg` read path; `DashboardSalesmanAggregator` + `DashboardSalesmanKeyResolver` + `ExecutiveSalesAchievementBandResolver`
- Source DAL extensions: `FakturView` (+`SalesPersonId`, `SalesPersonCode`); `IPiutangOpenBalanceWithSalesmanDal` (FF1 join); `ICustomerLastFakturDal.ListLastFakturWithSalesmanByCustomer()`; `ISalesOmzetTargetDal.ListTargetsForMonth()`
- Worker `RefreshDashboardSalesmanSnapshotWorker` reads source DALs — not Sales/Piutang/Customer snapshot tables
- Six snapshot tables: Kpi, TopOmzet, TopAchievement, TopPiutang, Attention, Segmentation
- API: `GET /api/dashboard/salesmen`
- Refresh cadence: 30 minutes; runs **last** in `--domain All` sequence (after Customer)
- Empty snapshot: `IsAvailable = false` (graceful) — unlike Sales/Piutang/Inventory/Purchasing that return HTTP 503
- Protected modules unchanged: `DashboardSalesFakturAggregator`, `BTR_PortalDashboardSalesTopSalesman`, `DashboardExecutiveComposer`
- Frontend: `SalesmanDashboardView`, section components (`SalesmanAttentionCardGroup`, `SalesmanAttentionList`, `SalesmanSegmentationSection`, `SalesmanNavigationSection`), extended `dashboardStore.loadSalesman()`, `navigateToReport` with `?q=` pre-filter
- Counter prefix for child rows: **PDS**

### M18 — Operational (HOW — end user)

- Sidebar: Dashboard → **Salesmen** (after Customers, before Inventory)
- Layout (Proposal A): Attention Cards → Attention List → Performance Rankings → Exposure Rankings → Segmentation → Navigation
- Three attention card groups: Performance (Below Target, No Target) · Collection Exposure (High Overdue, High Piutang) · Portfolio (Dormant Portfolio, concentration %)
- Staleness banner when snapshot exceeds 30-minute interval
- Cross-check Top 10 Omzet against Sales Dashboard Top 10 Salesman (same omzet by name; M18 adds Code, target, achievement %)

### M18 — Deployment (HOW — maintainer)

- Sixth Task Scheduler job: `BTR-Portal-Dashboard-Salesman` (30 min)
- Admin refresh and worker CLI accept `--domain Salesman`
- Health endpoint lists Salesman domain with `SalesmanIntervalMinutes`
- Initial backfill must populate six Salesman tables before dashboard is available
- SQL deploy required before first refresh (`BTR_PortalDashboardSalesman*` tables in `btr.sql`)
- Refresh order in `All`: Piutang → Inventory → Sales → Purchasing → Customer → **Salesman**

### Roadmap (from M18 analysis)

| Milestone | Focus |
| --------- | ----- |
| M19 | Slow Moving & Dead Stock |
| M20 | Collection Dashboard |

---

## Permanent Artifacts Updated

| Artifact | Four-question coverage |
| -------- | ---------------------- |
| `docs/features/btr-portal/btr-portal-domain.md` | **WHY** — dashboard purpose, signals, attribution, KPIs, business rules, product state M18 |
| `docs/features/btr-portal/btr-portal-architecture.md` | **WHAT** — `DashboardSalesmanAgg`, snapshot topology, API, DTOs, aggregators, frontend, refresh order, architectural rules |
| `docs/features/btr-portal/btr-portal-operational.md` | **HOW (user)** — navigation, workflow, FAQ; **HOW (deploy)** — 6th scheduler job, CLI domains, smoke test |
| `docs/features/materialized-dashboard/materialized-dashboard-domain.md` | **WHY** — salesman KPI rules, acceptance criteria, refresh cadence (6 domains) |
| `docs/features/materialized-dashboard/materialized-dashboard-architecture.md` | **WHAT** — salesman worker, tables, API, config, code locations |
| `docs/features/materialized-dashboard/materialized-dashboard-operational.md` | **HOW (deploy)** — sixth scheduler job, backfill verification, CLI domains, smoke test |

---

## Temporary Artifacts Reviewed

| File | Disposition |
| ---- | ----------- |
| `M18 Salesman Performance - Analysis.md` | **Recommend archive** — product decisions extracted to domain doc |
| `M18 Salesman Performance - Plan.md` | **Recommend archive** — tables, workers, API captured in architecture docs |
| `M18 Salesman Performance - Implementation.md` | **Recommend archive** — implementation history; durable knowledge preserved |

---

## Recommended Archival

Move working folder to:

```text
docs/archive/btr-portal/m18/
```

Reason: Durable knowledge preserved in permanent artifacts. Analysis, plan, and implementation notes are disposable after extraction.

**Retain in work folder until:** operations confirms Salesman snapshot deployed and initial backfill shows `Success` for all six domains.

---

## Inconsistencies Reported (Not Resolved)

| Item | Note |
| ---- | ---- |
| `appsettings.{MACHINE}.json` | `SalesmanIntervalMinutes` may be missing in environment-specific overrides — code default is 30 |
| M18 acceptance | SQL deploy and populated snapshots require operational verification before full sign-off |
| Full solution build | `btr.portal.api` (WebApplication.targets) and `btr.sql` (SSDT) may fail in dotnet CLI — core projects (`btr.application`, `btr.infrastructure`, `btr.test`) compile |

---

## Extraction Complete

A future Analyst, Architect, or Implementer can understand M18 from permanent artifacts without reading `docs/work/btr-portal`.

**Entry points by question:**

| Question | Start here |
| -------- | ---------- |
| WHAT (technical) | [btr-portal-architecture.md](./btr-portal-architecture.md) · [materialized-dashboard-architecture.md](../materialized-dashboard/materialized-dashboard-architecture.md) |
| WHY (business) | [btr-portal-domain.md](./btr-portal-domain.md) · [materialized-dashboard-domain.md](../materialized-dashboard/materialized-dashboard-domain.md) |
| HOW to use (end-user) | [btr-portal-operational.md](./btr-portal-operational.md) |
| HOW to deploy (maintainer) | [btr-portal-operational.md](./btr-portal-operational.md) (admin sections) · [materialized-dashboard-operational.md](../materialized-dashboard/materialized-dashboard-operational.md) |
