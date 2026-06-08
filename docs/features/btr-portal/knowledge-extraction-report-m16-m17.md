# Knowledge Extraction Report — M16 Executive Dashboard & M17 Customer Analytics

**Source folder:** `docs/work/btr-portal`  
**Extraction date:** 2026-06-08  
**Curator role:** Knowledge Curator Agent

---

## Summary

Completed work on **M16 Management Attention Center** and **M17 Customer Analytics** has been distilled into permanent BTR Portal and materialized-dashboard feature artifacts. M16 composes existing domain snapshots at read time (no new tables). M17 adds a dedicated Customer snapshot domain with cross-domain attention signals.

---

## Knowledge Extracted

### M16 — Domain (WHY)

- Replaces `/dashboard` home with **Management Attention Center** — daily question: *What requires management attention today?*
- Audience: all authenticated users; no RBAC in M16
- **Proposal A (Attention First)** layout: Attention Cards → Critical Exposures (Top 5) → Domain Summaries
- Promoted metrics per domain: Sales Achievement % (with Healthy/Warning/Critical bands), Piutang overdue and >90d exposure, Purchasing pending posting, Inventory concentration
- Concentration ratios derived: Top Customer %, Top Category %, Top Supplier %, Top Principal % — informational only
- **Supplements** domain dashboards; does not replace them
- Navigation: Executive → Domain Dashboard → Report (no direct report links on executive page)
- Excluded from executive: Total Faktur, Total Customer, Total Item, Total Invoice, weekly trends, pipeline omzet

### M16 — Architecture (WHAT)

- `DashboardExecutiveAgg` — `GetDashboardExecutiveQuery`, `DashboardExecutiveComposer`, `ExecutiveSalesAchievementBandResolver`
- `DashboardExecutiveDal` reads four domain snapshot DALs + refresh log; **no new snapshot tables**
- API: `GET /api/dashboard/executive`; home page uses executive endpoint (overview retained as legacy)
- Frontend: `DashboardHomeView` (Management Attention Center), sidebar label **Executive** for `/dashboard`
- Freshness: `LastRefreshed = Min(GeneratedAt)`; `IsDataFresh` when all available domains within interval

### M16 — Operational (HOW)

- Default landing after login: `/dashboard` (Management Attention Center)
- Sales achievement bands: ≥100% Healthy · 80–99% Warning · <80% Critical · no target Unknown
- Staleness banner when any domain exceeds refresh interval
- Unavailable warning when snapshot tables not populated

### M17 — Domain (WHY)

- New route `/dashboard/customers` — **Customer Analytics** — answers *Which customers require management attention?*
- Cross-domain lens: **Sales + Piutang only** (not Inventory/Purchasing)
- **Supplements** executive Top 5 Customers; does not replace executive dashboard
- Dedicated `BTR_PortalDashboardCustomer*` snapshot — not live composition from domain snapshots
- Approved attention signals: Overdue · Dormant (90-day) · Plafond breach · Suspended + Sales
- Mandatory rankings: Top 10 Omzet (current month) + Top 10 Piutang (all open) with `CustomerCode` and % of total
- Segmentation: Klasifikasi, Wilayah, Active vs Dormant
- Drill-down: customer row → Sales or Piutang Report with name pre-filter (`?q=`)
- Out of scope: Retur, Effective Call, GPS, Faktur Kembali aggregates, collection effectiveness/DSO, new Customer Report, executive changes

### M17 — Architecture (WHAT)

- `DashboardCustomerAgg` read path; `DashboardCustomerAggregator` + `DashboardCustomerKeyResolver`
- `ICustomerLastFakturDal` — `MAX(FakturDate)` per customer for dormant rule
- Worker `RefreshDashboardCustomerSnapshotWorker` reads source DALs: `IFakturViewDal`, `IPiutangOpenBalanceDal`, `ICustomerLastFakturDal`, `ICustomerDal`
- Five snapshot tables: Kpi, TopOmzet, TopPiutang, Attention, Segmentation
- API: `GET /api/dashboard/customers`
- Refresh cadence: 30 minutes; runs **last** in `--domain All` sequence
- Empty snapshot: `IsAvailable = false` (graceful) — unlike other domains that return HTTP 503
- Protected modules unchanged: `DashboardPiutangAggregator`, `BTR_PortalDashboardPiutangTopCustomer`, `DashboardExecutiveComposer`
- Frontend: `CustomerDashboardView`, section components, extended `Top10RankingTable`, `navigateToReport.ts`, report `?q=` pre-filter

### M17 — Operational / Deployment (HOW)

- Sidebar: Dashboard → **Customers** (between Piutang and Inventory)
- Fifth Task Scheduler job: `BTR-Portal-Dashboard-Customer` (30 min)
- Admin refresh and worker CLI accept `--domain Customer`
- Health endpoint lists Customer domain
- Initial backfill must populate five Customer tables before dashboard is available

### Roadmap (from M17 analysis)

| Milestone | Focus |
| --------- | ----- |
| M18 | Salesman Performance |
| M19 | Slow Moving & Dead Stock |
| M20 | Collection Dashboard |

---

## Permanent Artifacts Updated

| Artifact | Change |
| -------- | ------ |
| `docs/features/btr-portal/btr-portal-domain.md` | Customer Analytics dashboard definition, scope, business rules, product state M16/M17, roadmap |
| `docs/features/btr-portal/btr-portal-architecture.md` | Executive composition, Customer snapshot domain, API, aggregators, DTOs, frontend components, architectural rules |
| `docs/features/btr-portal/btr-portal-operational.md` | Customer workflow, navigation, admin refresh/monitoring (prior partial content consolidated) |
| `docs/features/materialized-dashboard/materialized-dashboard-domain.md` | Customer domain rules, acceptance criteria |
| `docs/features/materialized-dashboard/materialized-dashboard-architecture.md` | Customer worker, tables, API, config, code locations |
| `docs/features/materialized-dashboard/materialized-dashboard-operational.md` | Fifth scheduler job, backfill verification, CLI domains |

---

## Temporary Artifacts Reviewed

| File | Disposition |
| ---- | ----------- |
| `M16 Executive Dashboard - Analysis.md` | **Recommend archive** — product decisions extracted to domain/operational |
| `M16 Executive Dashboard - Plan.md` | **Recommend archive** — architecture captured in permanent docs |
| `M16 Executive Dashboard - Implementation.md` | **Recommend archive** — implementation history; durable knowledge preserved |
| `M17-Customer-Analytics-Analysis.md` | **Recommend archive** — Section 11 decisions extracted to domain doc |
| `M17 Customer Analytics - Plan.md` | **Recommend archive** — tables, workers, API captured in architecture docs |

---

## Recommended Archival

Move working folder to:

```text
docs/archive/btr-portal/m16-m17/
```

Reason: Durable knowledge preserved in permanent artifacts. Analysis, plans, and implementation notes are disposable after extraction.

**Retain in work folder until:** operations confirms Customer snapshot deployed and manual checklist (plan §8.3) signed off.

---

## Inconsistencies Reported (Not Resolved)

| Item | Note |
| ---- | ---- |
| Plan §7.5 card navigation links | **Resolved** — Collection → Piutang Dashboard, Activity → Sales Dashboard, Credit → in-page Attention List anchor |
| `appsettings.{MACHINE}.json` | `CustomerIntervalMinutes` may be missing in environment-specific overrides — code default is 30 |
| M17 acceptance | SQL deploy and populated snapshots require operational verification before full sign-off |

---

## Extraction Complete

A future Analyst, Architect, or Implementer can understand M16 and M17 from permanent artifacts without reading `docs/work/btr-portal`.
