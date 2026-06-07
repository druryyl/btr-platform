# Knowledge Extraction Report — Materialized Dashboard Data

**Source folder:** `docs/work/materialize-dashboard-data`  
**Extraction date:** 2026-06-07  
**Curator role:** Knowledge Curator Agent

---

## Summary

Completed work on **Materialized Dashboard Data** (Phases 1–5) has been distilled into permanent feature artifacts under `docs/features/materialized-dashboard/`. BTR Portal permanent docs already contained partial materialization updates; this extraction consolidates the initiative into a dedicated feature module and fixes stale Piutang period wording in portal domain/architecture docs.

---

## Knowledge Extracted

### Architecture (WHAT)

- Background snapshot topology: `btr.portal.worker` → aggregators → `BTR_PortalDashboard*` tables
- Snapshot layers A (KPI) and B (dimensional); `SnapshotKey = 'CURRENT'` pattern
- Per-domain refresh workers and orchestrator; CLI arguments and exit codes
- Read path: snapshot-only (no live fallback); overview fast path via Layer A
- Piutang refresh optimization: `Sisa > 1` query replaces 2000→today scan
- Sales source cutover: `BTR_Faktur` replaces `BTR_SalesOmzet` for dashboard
- API endpoints including admin refresh and health observability
- Database table inventory, indexes, ParamNo prefixes
- Key code locations and architectural rules

### Domain (WHY)

- Business problem: performance, server load, wrong sales source for board view
- Authoritative product decisions: Option B overview, Faktur-only sales, cadence 15/30/60, no history retention
- Stakeholder communication for sales semantic shift
- Freshness model and `GeneratedAt` meaning
- KPI rules preserved per domain and dashboard–report traceability matrix
- Scope boundaries and deferred future extensions

### Operational (HOW)

- End-user guidance: generated-at timestamps, Refresh button behavior, staleness expectations
- Administrator runbook: initial backfill, Task Scheduler setup, manual refresh (CLI vs API)
- Monitoring via health endpoint and SQL queries
- Troubleshooting matrix and post-deploy smoke tests
- Quick reference card

---

## Permanent Artifacts Created

| Artifact | Path | Role |
| -------- | ---- | ---- |
| Architecture | `docs/features/materialized-dashboard/materialized-dashboard-architecture.md` | Technical WHAT |
| Domain | `docs/features/materialized-dashboard/materialized-dashboard-domain.md` | Business WHY |
| Operational | `docs/features/materialized-dashboard/materialized-dashboard-operational.md` | End-user / admin HOW |
| This report | `docs/features/materialized-dashboard/knowledge-extraction-report.md` | Extraction audit trail |

---

## Permanent Artifacts Updated

| Artifact | Change |
| -------- | ------ |
| `docs/features/btr-portal/btr-portal-architecture.md` | Cross-link to materialized-dashboard feature; fix Piutang aggregation source |
| `docs/features/btr-portal/btr-portal-domain.md` | Cross-link; fix Piutang period semantics |
| `docs/features/btr-portal/btr-portal-operational.md` | Cross-link to materialized-dashboard operational doc |

---

## Temporary Artifacts Reviewed

| File | Disposition |
| ---- | ----------- |
| `analysis-report.md` | Archive — knowledge extracted to domain + architecture |
| `implementation-plan.md` | Archive — decisions captured in permanent docs |
| `implementation-phase-1-summary.md` | Archive |
| `implementation-summary-phase-2.md` | Archive |
| `implementation-summary-phase-3.md` | Archive |
| `implementation-summary-phase-4.md` | Archive |
| `implementation-summary-phase-5.md` | Archive |
| `implementation-phase-1-review.md` | Archive |
| `implementation-phase-2-review.md` | Archive |
| `dashboard-snapshot-worker-runbook.md` | Archive — operational content merged into permanent operational doc |

---

## Recommended Archival

Move entire working folder to:

```text
docs/archive/materialize-dashboard-data/
```

Reason: All durable knowledge preserved in permanent artifacts. Temporary plans, summaries, and reviews are disposable after extraction.

---

## Information Deliberately Excluded

Per knowledge curator rules, the following were **not** copied to permanent docs:

- Phase task sequences and step-by-step implementation checklists
- Test counts and pass/fail status per phase
- Reviewer checklist outcomes
- Branch names, commit references, development effort estimates
- Alternative proposals and rejected architecture options (Hangfire, portal internal timer, etc.) — only selected decisions retained
- Layer C piutang open fact design (deferred, not built)

---

## Inconsistencies Noted (Resolved in Permanent Docs)

| Issue | Resolution |
| ----- | ---------- |
| Piutang domain doc still said "2000-01-01 through today" | Updated to "open receivables snapshot (`Sisa > 1` at refresh time)" |
| Architecture aggregation table referenced `IPiutangSalesWilayahDal` for refresh | Updated to `IPiutangOpenBalanceDal` |
| Operational runbook duplicated in btr-portal-operational | Cross-linked; detailed ops consolidated in materialized-dashboard-operational |

---

## Success Criteria Check

A future agent can determine:

| Question | Answer location |
| -------- | --------------- |
| What does materialization do? | `materialized-dashboard-domain.md` |
| How is it built? | `materialized-dashboard-architecture.md` |
| How do users/admins operate it? | `materialized-dashboard-operational.md` |
| How does it relate to BTR Portal? | Cross-links in both feature folders |
| What KPI rules apply? | Domain doc + `btr-portal-domain.md` KPI sections |
| What is out of scope? | Domain doc scope section |

Extraction complete.
