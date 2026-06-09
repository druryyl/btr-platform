# Knowledge Extraction Report — M21 Purchasing Management Dashboard

**Source artifacts:**

- `docs/work/btr-portal/M21-purchasing-dashboard-analysis.md`
- `docs/work/btr-portal/M21 Purchasing Dashboard - Plan.md`
- `docs/work/btr-portal/M21 Purchasing Dashboard - Implementation Summary.md`
- `docs/work/btr-portal/M21 Purchasing Dashboard - Review Report.md`
- `docs/work/btr-portal/M21 Purchasing Dashboard - Re-Review Report.md`

**Extraction date:** 2026-06-09  
**Curator role:** Knowledge Curator Agent

---

## Summary

Completed work on **M21 Purchasing Management Dashboard** has been distilled into permanent BTR Portal and materialized-dashboard feature artifacts. M21 evolves `/dashboard/purchasing` from purchasing statistics toward **purchasing management attention** — qualified posting backlog, principal × signal attention list, and cross-domain principal exposure (M15 inventory + M19 at-risk). V1 purchasing snapshot domain and traceability KPIs remain unchanged. Executive `RequiresAttention` for purchasing now uses qualified backlog only. Phase 2 executive signal promotion is **deferred** per Product Owner decision (Q10).

---

## Knowledge Extracted

### M21 — Domain (WHY)

- Same route `/dashboard/purchasing`; page title **Purchasing Management Dashboard**
- Question answered: *Which suppliers and purchasing activities require management attention?*
- **Extends** V1 purchasing statistics — does not replace traceability sections (Grand Total Purchase, Total Invoice, weekly trend, posting pie)
- **BELUM semantics:** `SaveInvoiceWorker` always saves unposted; fresh `BELUM` is normal staging — not automatic management attention
- **Qualified backlog:** `PostingStok = BELUM` AND `(today − LastUpdate.Date).TotalDays ≥ 3` (default; `PurchasingQualifiedBacklogDays`)
- **No Purchase Order entity** — attention derived from invoice lifecycle and cross-domain inventory signals
- **Approved attention signals:** `QualifiedBacklog`, `PrincipalSpendConcentration`, `PrincipalInventoryConcentration`, `PrincipalAtRiskExposure`, `CompoundDependency`, `PurchasingInactivity`, `PrincipalInventoryNoPurchase`, `UnknownPrincipal`
- Attention list grain: **Principal × Signal** (Company row only for `PurchasingInactivity`)
- Drill-down: principal row → Purchasing Report with `?q=` pre-filter (M17 pattern)
- Cross-links mandatory to Inventory Dashboard and Inventory Risk Dashboard
- Executive **RequiresAttention (Phase 1):** `QualifiedBacklogCount > 0` — not unqualified BELUM count/value
- Phase 2 (deferred): promote Compound Dependency, Qualified Backlog Value, Top 3 Principal % to executive purchasing card
- Explicitly out of scope: Retur Beli, PF2 line analytics, purchase-to-sales ratio, automated weekly spike flags, "buying into slow/dead stock", Purchasing Line Report, PO/budget workflows

### M21 — Business rules (authoritative)

| Rule | Definition |
| ---- | ---------- |
| Qualified backlog age anchor | `LastUpdate` on `BTR_Invoice` (via extended `InvoiceView`) |
| Qualified backlog threshold | ≥ 3 calendar days (configurable) |
| Purchasing Inactivity | `TotalInvoice = 0` for current month **AND** `DateTime.Today.Day ≥ 15` |
| Compound Dependency | Principal in Top 10 MTD purchase **AND** (Top 10 inventory value **OR** Top 10 at-risk value) |
| PrincipalInventoryNoPurchase | M15 supplier Top 10 with zero MTD purchase |
| Concentration signals | Informational — principal in respective Top 10; no threshold-heavy alert engine |
| Weekly purchase trend | Visual only — no automated spike/deceleration attention flags |
| Traceability | Grand Total Purchase and Total Invoice from **V1** snapshot unchanged; qualified backlog is dashboard-only |

### M21 — Architecture (WHAT)

- Dedicated snapshot domain: `BTR_PortalDashboardPurchasingManagement*` (3 tables) — separate from V1 `BTR_PortalDashboardPurchasing*`
- `DashboardPurchasingManagementAggregator` + `RefreshDashboardPurchasingManagementSnapshotWorker`; domain value `PurchasingManagement`
- Refresh reads: extended `IInvoiceViewDal`, V1 purchasing snapshot, M15 inventory snapshot, M19 inventory-risk snapshot — **no duplicate inventory SQL**
- `GET /api/dashboard/purchasing` merges V1 + management in single response; `GeneratedAt` = min(V1, Management)
- `DashboardExecutiveComposer.ComposePurchasing` revised for qualified backlog `RequiresAttention`
- V1 `DashboardPurchasingInvoiceAggregator` and four V1 tables **protected unchanged**
- Frontend: attention-first layout; `PurchasingAttentionCards`, `PurchasingAttentionList`, `PurchasingPrincipalExposureTable`, `PurchasingNavigationSection`
- Empty management snapshot: V1 sections still render; management sections unavailable
- Refresh cadence: 30 minutes (`PurchasingManagementIntervalMinutes`)
- Refresh order in `All`: … → Purchasing (V1) → **PurchasingManagement** → Customer → …

### M21 — Operational (HOW)

- Sidebar: Dashboard → **Purchasing** (unchanged label)
- Staleness banner when snapshot exceeds 30-minute interval
- Ninth Task Scheduler job: `BTR-Portal-Dashboard-PurchasingManagement`
- Admin refresh and worker CLI accept `--domain PurchasingManagement`
- User workflow: attention cards/list → Purchasing Report validation → optional Inventory/Inventory Risk context → Desktop PT2 posting

---

## Permanent Artifacts Updated

| Artifact | Change |
| -------- | ------ |
| `docs/features/btr-portal/btr-portal-domain.md` | M21 dashboard definition, management KPIs, business rules, product state, future direction |
| `docs/features/btr-portal/btr-portal-architecture.md` | PurchasingManagement snapshot topology, API response sections, aggregation, refresh order |
| `docs/features/btr-portal/btr-portal-operational.md` | Extraction report link (prior session had M21 user guide) |
| `docs/features/materialized-dashboard/materialized-dashboard-domain.md` | PurchasingManagement section (prior session) |
| `docs/features/materialized-dashboard/materialized-dashboard-architecture.md` | PurchasingManagement worker, tables, CLI domains, config |
| `docs/features/materialized-dashboard/materialized-dashboard-operational.md` | Ninth scheduler job, backfill verification, CLI domains |
| `docs/features/btr-portal/ALERT-REGISTRY.md` | M21 purchasing signals (prior session — no change) |

---

## Temporary Artifacts — Archived

| Artifact | Disposition |
| -------- | ----------- |
| `M21-purchasing-dashboard-analysis.md` | **Archived** — PO decisions preserved in domain doc |
| `M21 Purchasing Dashboard - Plan.md` | **Archived** — architecture captured in permanent docs |
| `M21 Purchasing Dashboard - Implementation Summary.md` | **Archived** — file-level change list disposable |
| `M21 Purchasing Dashboard - Review Report.md` | **Archived** — review gate complete; defects fixed |
| `M21 Purchasing Dashboard - Re-Review Report.md` | **Archived** — approval record; durable rules in permanent docs |

**Archive location:** `docs/archive/btr-portal/m21-purchasing-management/`

---

## Information Deliberately Excluded

- Phase implementation step sequences and file-by-file checklists
- Test counts, build-fix narrative, reviewer session notes
- Alternative layout proposals B and C (rejected)
- Desktop PF2/PF4 investigation detail — excluded from M21 scope
- `IOptions<>` dependency fix detail — in code only

---

## Inconsistencies / Open Items (Report Only)

| Item | Status |
| ---- | ------ |
| Executive purchasing signal promotion beyond qualified backlog | **Deferred Phase 2** — documented in domain doc |
| Cross-domain null snapshot warning | Trace-only in worker — acceptable Phase 1; consider structured logging later |
| Manual deploy checklist (Plan §9.4) | Operational confirmation at first staging deploy — not a code gate |

No unresolved business rule conflicts found during extraction.

---

*End of M21 Knowledge Extraction Report*
