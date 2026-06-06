# Knowledge Extraction Report — BTR Portal M1–M15

**Status:** Optional reference — not authoritative knowledge.  
**Date:** 2026-06-06  
**Purpose:** Document the knowledge extraction process from temporary delivery artifacts into permanent feature documentation.

**Authoritative knowledge lives in:**

- [btr-portal-domain.md](./btr-portal-domain.md)
- [btr-portal-operational.md](./btr-portal-operational.md)
- [btr-portal-architecture.md](./btr-portal-architecture.md)

---

## Artifacts Reviewed

### Milestone History

| Artifact | Location |
| -------- | -------- |
| `btr-portal-milestone.md` | `docs/work/btr-portal-api-scaffolding/` |

### Analysis (Final)

| Artifact | Scope |
| -------- | ----- |
| `portal-analysis-m10-m12-final.md` | M10–M12 product decisions |
| `portal-analysis-m13-m15-final.md` | M13–M15 product decisions |

### Analysis (Superseded — consulted for context only)

| Artifact | Notes |
| -------- | ----- |
| `portal-analysis-m10-m12.md` | Superseded by final |
| `portal-analysis-m13-m15.md` | Superseded by final |

### Delivery Plans

| Artifact | Scope |
| -------- | ----- |
| `portal-delivery-plan-m10-m12.md` | Shared report architecture |
| `portal-delivery-plan-m13-m15.md` | Shared dashboard extension architecture |

### Implementation Plans

| Artifact | Scope |
| -------- | ----- |
| `implementation-plan-btr-portal-api-scaffolding.md` | M1–M6 foundation |
| `implementation-plan-m10-piutang-report-v1.md` | M10 |
| `implementation-plan-m11-inventory-report-v1.md` | M11 |
| `implementation-plan-m12-purchasing-report-v1.md` | M12 |
| `implementation-plan-m13-sales-dashboard-v3.md` | M13 |
| `implementation-plan-m14-piutang-dashboard-v2.md` | M14 |
| `implementation-plan-m15-inventory-dashboard-v2.md` | M15 |

### Implementation Summaries

| Artifact | Milestone |
| -------- | --------- |
| `implementation-summary.md` | M1 |
| `implementation-summary-milestone-2.md` through `milestone-6.md` | M2–M6 |
| `implementation-summary-milestone-7.md` | M7 |
| `implementation-summary-milestone-8.md` | M8 |
| `implementation-summary-milestone-9.md` | M9 |
| `implementation-summary-m10.md` | M10 |
| `implementation-summary-m11.md` | M11 |
| `implementation-summary-m12.md` | M12 |
| `implementation-summary-milestone-13.md` | M13 |
| `implementation-summary-milestone-14.md` | M14 |
| `implementation-summary-milestone-15.md` | M15 |

### Project Knowledge

| Artifact | Location |
| -------- | -------- |
| `PRODUCT.md` | `docs/foundation/` |
| `DOMAIN.md` | `docs/foundation/` |
| `LANDSCAPE.md` | `docs/foundation/` |

### Source Code (validation only)

| Area | Purpose |
| ---- | ------- |
| `btr.portal.web/src/router/index.ts` | Route verification |
| `btr.portal.web/src/layouts/MainLayout.vue` | Menu hierarchy verification |

### Not Extracted

| Artifact | Reason |
| -------- | ------ |
| `btr-reporting-investigation.md` | Investigation history — rules captured in analysis finals |
| Screenshots | Visual reference only — not organizational knowledge |
| Build/compile issue tables | Development effort — excluded per extraction rules |

---

## Knowledge Extracted

### Into `btr-portal-domain.md`

| Category | Content |
| -------- | ------- |
| Purpose & vision | Read-only analytics portal; BTR Desktop as source of truth |
| Scope & non-goals | Transactional exclusions; deferred features list |
| Business areas | Sales, Finance, Inventory, Purchasing coverage map |
| Dashboard definitions | Home + three detail dashboards with sections and periods |
| Report definitions | Four reports with columns, filters, footer behavior |
| KPI definitions | 20+ KPIs with definition, formula, business meaning |
| Business rules | 18 approved calculation and filter rules |
| Current product state | M1–M15 capability inventory |
| Future direction | M16+ deferred items |

### Into `btr-portal-operational.md`

| Category | Content |
| -------- | ------- |
| Login workflow | Credentials, persistence, logout, 401 handling |
| Dashboard usage | Home vs detail; per-domain page walkthrough |
| Report usage | Per-report columns, sorting, pagination, footer interpretation |
| Navigation | Menu hierarchy, routes, flow diagram |
| User workflows | Sales review, overdue monitoring, inventory value, purchasing |
| FAQ | 10 common user questions |

### Into `btr-portal-architecture.md`

| Category | Content |
| -------- | ------- |
| System overview | Three-tier diagram, technology stack, constraints |
| ReportingContext | Aggregate structure, DAL reuse strategy |
| Dashboard architecture | API/routing/DTO/aggregation/ranking/chart patterns |
| Report architecture | API/routing/DataTable/footer/DTO patterns |
| Backend conventions | MediatR, API shape, response envelope, project layout |
| Frontend conventions | Vue structure, Pinia, components, formatters |
| Architectural rules | 15 rules + traceability matrix |

### Deliberately Excluded from Permanent Artifacts

- Milestone execution steps and phase checklists
- Build/compile troubleshooting history
- File-by-file change lists per milestone
- Rejected alternatives and open questions (resolved in finals)
- Desktop file path indexes from analysis appendices
- Branch names, commit history, developer effort estimates
- NuGet vulnerability advisories and package version tables

---

## Permanent Artifacts Created

| # | Artifact | Path | Lines (approx.) |
| - | -------- | ---- | --------------- |
| 1 | Domain knowledge | `docs/feature/btr-portal/btr-portal-domain.md` | ~350 |
| 2 | Operational guide | `docs/feature/btr-portal/btr-portal-operational.md` | ~300 |
| 3 | Architecture reference | `docs/feature/btr-portal/btr-portal-architecture.md` | ~400 |
| 4 | Extraction report (this file) | `docs/feature/btr-portal/knowledge-extraction-report-m1-m15.md` | ~200 |

### Cross-Reference Map

| Topic | Authoritative Location |
| ----- | --------------------- |
| KPI definitions & formulas | `btr-portal-domain.md` |
| Business rules | `btr-portal-domain.md` |
| Navigation & routes | `btr-portal-operational.md` |
| User workflows & FAQ | `btr-portal-operational.md` |
| API endpoints & DTO patterns | `btr-portal-architecture.md` |
| DAL reuse & ReportingContext | `btr-portal-architecture.md` |
| BTR business vocabulary | `docs/foundation/DOMAIN.md` |
| BTR product scope (platform-wide) | `docs/foundation/PRODUCT.md` |
| BTR business area ownership | `docs/foundation/LANDSCAPE.md` |

---

## Recommended Archive/Delete List

After team review, the following temporary artifacts in `docs/work/btr-portal-api-scaffolding/` may be **archived** (moved to `docs/archive/btr-portal-api-scaffolding/` or equivalent) or **deleted**:

### Safe to Archive (high confidence)

| Artifact | Rationale |
| -------- | --------- |
| `portal-analysis-m10-m12.md` | Superseded by `-final` |
| `portal-analysis-m13-m15.md` | Superseded by `-final` |
| `implementation-plan-btr-portal-api-scaffolding.md` | M1–M6 delivered; conventions extracted |
| `implementation-plan-m10-piutang-report-v1.md` | Delivered |
| `implementation-plan-m11-inventory-report-v1.md` | Delivered |
| `implementation-plan-m12-purchasing-report-v1.md` | Delivered |
| `implementation-plan-m13-sales-dashboard-v3.md` | Delivered |
| `implementation-plan-m14-piutang-dashboard-v2.md` | Delivered |
| `implementation-plan-m15-inventory-dashboard-v2.md` | Delivered |
| `portal-delivery-plan-m10-m12.md` | Patterns extracted to architecture doc |
| `portal-delivery-plan-m13-m15.md` | Patterns extracted to architecture doc |
| `implementation-summary.md` | M1 complete |
| `implementation-summary-milestone-2.md` through `milestone-6.md` | M2–M6 complete |
| `implementation-summary-milestone-7.md` through `milestone-9.md` | M7–M9 complete |
| `implementation-summary-m10.md` through `m12.md` | M10–M12 complete |
| `implementation-summary-milestone-13.md` through `15.md` | M13–M15 complete |

### Archive with Caution (retain briefly)

| Artifact | Rationale |
| -------- | --------- |
| `btr-portal-milestone.md` | Useful milestone index; knowledge now in domain doc `Current Product State` |
| `portal-analysis-m10-m12-final.md` | Authoritative decisions — extracted; keep until archive policy confirmed |
| `portal-analysis-m13-m15-final.md` | Authoritative decisions — extracted; keep until archive policy confirmed |
| `screenshots/` | Training/visual QA reference — not knowledge, but useful for onboarding |

### Do Not Archive as BTR Portal Knowledge

| Artifact | Rationale |
| -------- | --------- |
| `btr-reporting-investigation.md` | General investigation; not portal-specific permanent knowledge |

---

## Knowledge Gaps

Items not fully captured in permanent artifacts (intentional or pending):

| Gap | Severity | Notes |
| --- | -------- | ----- |
| Deployment / IIS hosting guide | Medium | M1 plan mentions IIS publish; no production deployment runbook extracted |
| Environment configuration | Medium | `appsettings.json`, `VITE_API_BASE_URL`, CORS origins — operational for DevOps, not end users |
| Test credentials & dev DB setup | Low | Implementation summaries reference `DIMAS`/`1111` — excluded as dev-only |
| Role-based access design | Low | JWT carries role claims; menu visibility not implemented — noted as non-goal |
| Sales dashboard ↔ Sales report numeric reconciliation | Low | Different grains documented; no single reconciliation formula exists |
| `KurangBayar > 1` threshold in DOMAIN.md | Low | Portal rule documented; foundation DOMAIN.md not updated (out of scope) |
| Error message catalog | Low | Generic error handling documented; no exhaustive message list |
| Performance / row-volume guidance | Low | M14 notes ~11K piutang rows; no SLA or pagination strategy for large datasets |
| M16+ roadmap prioritization | Low | Deferred items listed; no committed sequence beyond "filtering phase" |

### Recommended Follow-Up (outside this extraction)

1. Create `btr-portal-deployment.md` if production hosting becomes active.
2. Add `KurangBayar > 1` threshold to `DOMAIN.md` if finance terminology standardization is desired.
3. Archive temporary artifacts after one sprint of permanent-doc validation.

---

## Extraction Validation

Success criteria check:

| Criterion | Met? |
| --------- | ---- |
| Analyst can understand BTR Portal purpose without reading temporary artifacts | Yes — via domain doc |
| Product Owner can understand KPIs and scope | Yes — via domain doc |
| Support team can guide users | Yes — via operational doc |
| Developer can extend portal following conventions | Yes — via architecture doc |
| No significant duplication across permanent artifacts | Yes — cross-references used |
| Implementation history excluded | Yes |
| Rejected alternatives excluded | Yes |

**Extraction status: Complete.**
