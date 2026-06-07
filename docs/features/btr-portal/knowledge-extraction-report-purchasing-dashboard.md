# Knowledge Extraction Report — Purchasing Dashboard V1

**Source folder:** `docs/work/purchasing-dashboard`  
**Extraction date:** 2026-06-07  
**Curator role:** Knowledge Curator Agent

---

## Summary

Completed work on **Purchasing Dashboard V1** has been distilled into permanent BTR Portal and materialized-dashboard feature artifacts. The fourth portal analytics domain extends the established snapshot pattern with mandatory traceability to the Purchasing Report footer KPIs.

---

## Knowledge Extracted

### Domain (WHY)

- Purchasing as fourth dashboard domain alongside Sales, Piutang, and Inventory
- Business objective: management visibility into monthly purchase spend, invoice volume, posting backlog, weekly pace, and principal concentration
- V1 period: current calendar month; void exclusion; Retur Beli excluded (same invoice source as report)
- Authoritative KPI definitions: Grand Total Purchase, Total Invoice, Pending Posting Invoice Count (count only)
- Dashboard-only analytics: weekly trend, posting-status breakdown (`SUDAH` / `BELUM`), Top 10 Principal
- UI terminology: **Principal** in labels; data field remains `SupplierName`
- Mandatory traceability: Grand Total Purchase and Total Invoice must match Purchasing Report footer
- Deferred V1.1+: warehouse breakdown, pending posting value KPI, Retur Beli analytics, budget vs actual

### Architecture (WHAT)

- `DashboardPurchasingAgg` aggregate; `DashboardPurchasingInvoiceAggregator` in snapshot layer
- Data source: `IInvoiceViewDal.ListData(current month)` — independent of `PurchasingReportDal` (no cross-aggregate coupling)
- Four snapshot tables: KPI, WeekTrend, PostingStatus, TopPrincipal
- Refresh worker: `RefreshDashboardPurchasingSnapshotWorker`; domain value `Purchasing`; 30-minute cadence
- API: `GET /api/dashboard/purchasing`; overview extension with 4th home card section
- Frontend: `/dashboard/purchasing`, `PurchasingDashboardView`, `PostingStatusPieChart`, extended `WeeklyTrendChart`
- Traceability guard: `DashboardPurchasingReportTraceabilityTest`

### Operational (HOW)

- Sidebar: Dashboard → Purchasing
- Home card: Grand Total Purchase, Total Invoice, link to detail dashboard
- User workflow: dashboard → posting backlog review → Purchasing Report → BTR Desktop posting
- Administrator: fourth Task Scheduler job `BTR-Portal-Dashboard-Purchasing` (30 min)
- Health monitoring includes Purchasing domain in `GET /api/health/dashboard-snapshots`

---

## Permanent Artifacts Updated

| Artifact | Change |
| -------- | ------ |
| `docs/features/btr-portal/btr-portal-domain.md` | Purchasing dashboard definitions, KPIs, traceability, product state (prior + curator pass) |
| `docs/features/btr-portal/btr-portal-architecture.md` | Purchasing API, aggregation, DTOs, charts, traceability matrix (prior + `PostingStatusPieChart`) |
| `docs/features/btr-portal/btr-portal-operational.md` | Purchasing dashboard UX, navigation, workflows, admin ops (prior session) |
| `docs/features/materialized-dashboard/materialized-dashboard-domain.md` | Purchasing KPI rules, tables, cadence, scope (prior + curator pass) |
| `docs/features/materialized-dashboard/materialized-dashboard-architecture.md` | Purchasing worker, tables, endpoints, CLI, config (curator pass) |
| `docs/features/materialized-dashboard/materialized-dashboard-operational.md` | Fourth scheduler job, backfill verification, smoke tests (curator pass) |
| `docs/archive/materialize-dashboard-data/dashboard-snapshot-worker-runbook.md` | Purchasing task instructions (prior session) |

---

## Temporary Artifacts Reviewed

| File | Disposition |
| ---- | ----------- |
| `analysis-report.md` | Archive — product scope and KPI rules extracted to domain docs |
| `implementation-plan.md` | Archive — architecture decisions captured in permanent docs |
| `implementation-summary.md` | Archive — acceptance evidence; durable ops merged into operational docs |

---

## Recommended Archival

Move entire working folder to:

```text
docs/archive/purchasing-dashboard/
```

Reason: All durable knowledge preserved in permanent artifacts. Temporary analysis, plan, and summary are disposable after extraction.

---

## Information Deliberately Excluded

Per knowledge curator rules, the following were **not** copied to permanent docs:

- Phase implementation step sequences and file-by-file checklists
- Test counts, dev-machine SQL evidence, and reviewer closeout session notes
- Bug-fix narrative (`GetLatestPerDomain` SQL filter) — fix is in code; ops impact covered by health monitoring
- Alternative proposals and M12 "no dashboard" decision history — only approved V1 scope retained
- Counter prefix collision resolution detail (`PDT` vs `PDR`) — ParamNo seed is authoritative in SQL

---

## Inconsistencies Noted (Resolved in Permanent Docs)

| Issue | Resolution |
| ----- | ---------- |
| Materialized-dashboard architecture/operational still listed three domains | Updated to four domains including Purchasing |
| `btr-portal-domain.md` product state omitted Purchasing detail dashboard | Added to Current Product State table |
| Deferred purchasing items incomplete in Future Direction | Added warehouse breakdown and pending posting value KPI |

---

## Success Criteria Check

A future agent can determine:

| Question | Answer location |
| -------- | --------------- |
| What does the Purchasing Dashboard show? | `btr-portal-domain.md` — Purchasing Dashboard section |
| What KPIs reconcile with the report? | `btr-portal-domain.md` KPI Definitions + traceability matrix in architecture |
| How is it built? | `btr-portal-architecture.md` + `materialized-dashboard-architecture.md` |
| How do users and admins operate it? | `btr-portal-operational.md` + `materialized-dashboard-operational.md` |
| What snapshot tables exist? | `materialized-dashboard-domain.md` Purchasing section |
| What is deferred beyond V1? | `btr-portal-domain.md` Future Direction |

Extraction complete.
