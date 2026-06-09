# M24 Dashboard Drill Down — Implementation Summary



**Status:** Complete — remediation delivered for re-review  

**Date:** 2026-06-09  

**Plan:** [M24 Dashboard Drill Down - Plan.md](./M24%20Dashboard%20Drill%20Down%20-%20Plan.md)  

**Review:** [M24 Dashboard Drill Down - Review Report.md](./M24%20Dashboard%20Drill%20Down%20-%20Review%20Report.md)



## Delivered



M24 introduces a **horizontal investigation framework** unifying drill-down from KPIs, alerts, rankings, and attention signals (M16–M23) to report evidence with traceability metadata.



| Outcome | Status |

| ------- | ------ |

| Investigation Metadata Contract (C# + TypeScript) | Done |

| `InvestigationRegistry` + `InvestigationMetadataBuilder` | Done |

| `navigateToInvestigation` + structured query params | Done |

| Purchasing Report `?q=` hydration | Done |

| Piutang Report `allOpenBalances` mode | Done |

| Executive Critical Exposure Top 5 clickable | Done |

| Legacy M11/M14/M15 ranking drill-down | Done |

| Domain dashboard ranking drill-down (M17–M22) | Done (remediation) |

| Investigation breadcrumb on all four reports | Done |

| Stable entity ID report filtering | Done |

| Qualified Backlog `posting=BELUM` filter | Done |

| **Investigate** label on attention lists + Alert Center | Done |

| Report-first Alert Center (Investigate primary) | Done |

| Desktop next-step + multi-step metadata (nice to have) | Done |



## Remediation (review rejection)



| Finding | Resolution |

| ------- | ---------- |

| `btr-portal-domain.md` missing M24 | Added Investigation Framework platform capability section |

| `ALERT-REGISTRY.md` missing Investigation columns | Added Investigate → / Period mode / Desktop next step columns + ranking keys subsection |

| `InvestigationRegistryTest` incomplete | Expanded to all M17–M22 aggregator keys + executive/legacy/ranking keys + route/period theories |

| `buildInvestigationQuery` tests absent | Added Vitest + `buildInvestigationQuery.spec.ts` (3 plan §8.2 cases) |

| Non-legacy ranking drill-down gap | Backend `Investigation` on ranking DTOs/DALs; frontend `navigateToInvestigation` on 6 dashboard views |



## Architecture



```text

Dashboard producers (M11–M22 + M16 executive composer)

  Attention / Ranking / Top 5 rows → InvestigationMetadata

Alert Center composer → maps producer Investigation

Vue dashboards + Alert Center

  navigateToInvestigation(metadata, sourceLabel)

    ↓ flat query params

Report views

  applyInvestigationQuery → filters + InvestigationBreadcrumb

```



No new API endpoints, snapshot tables, or alert qualification rules.



## Key files



| Layer | Path |

| ----- | ---- |

| Contract | `btr.application/.../Shared/InvestigationMetadata.cs` |

| Registry | `btr.application/.../Shared/InvestigationRegistry.cs` |

| Builder | `btr.application/.../Shared/InvestigationMetadataBuilder.cs` |

| Ranking keys | `SignalRanking*` constants in `InvestigationRegistry.cs` |

| Piutang all-open | `PiutangReportDal.GetAllOpenBalancesReport`, `PiutangSalesWilayahDal.ListAllOpenBalances` |

| Executive Top 5 | `DashboardExecutiveComposer.ComposeCriticalExposures` |

| Alert Center | `DashboardAlertCenterComposer.BuildAlertInvestigation` |

| Navigation | `btr.portal.web/src/services/navigateToInvestigation.ts` |

| Query hydration | `applyInvestigationQuery.ts`, `useReportInvestigationFilter.ts` |

| Breadcrumb | `components/reports/InvestigationBreadcrumb.vue` |

| Backend tests | `InvestigationRegistryTest.cs`, `PiutangReportDalTest`, `DashboardExecutiveComposerTest` |

| Frontend tests | `buildInvestigationQuery.spec.ts` |



## Query contract (reports)



| Param | Purpose |

| ----- | ------- |

| `q` | Free-text pre-fill |

| `customerId`, `salesmanId`, `brgId`, `warehouseId`, `supplierId` | Stable ID filters (preferred over name) |

| `periodMode=allOpenBalances` | Piutang all-open mode |

| `posting=BELUM` | Purchasing posting filter |

| `signalKey`, `signalLabel`, `source`, `entityType` | Breadcrumb context |



Router state carries `dashboardRoute`, `desktopNextStep`, and `investigationSteps` (not URL-polluting).



## Verification



### Backend unit tests



```text

dotnet build src/j05-btr-distrib/btr.test/btr.test.csproj

dotnet vstest btr.test/bin/Debug/btr.test.dll --TestCaseFilter:"FullyQualifiedName~InvestigationRegistry|FullyQualifiedName~PiutangReportDal|FullyQualifiedName~DashboardExecutiveComposer"

```



**Result:** 36 passed (InvestigationRegistry expanded + Piutang report + Executive composer).



### Frontend unit tests



```text

cd src/j05-btr-distrib/btr.portal.web

npm install

npm test

```



**Result:** 3 passed (`buildInvestigationQuery.spec.ts`).



### Manual checklist (recommended)



1. Executive Top 5 — each list row opens correct report with filters.

2. Sales / Piutang / Inventory legacy Top 10 — row click → report with breadcrumb.

3. Customer / Salesman / Collection rankings — piutang-bound rows use all-open mode + entity ID.

4. Customer attention — **Investigate** → Piutang (all-open) or Sales per signal.

5. Alert Center — **Investigate** primary; **View Dashboard** secondary; Wilayah/Company dashboard-only.

6. Qualified Backlog — Purchasing Report shows BELUM rows only.

7. Report breadcrumb shows signal + source after drill-down.

8. Direct `/reports/piutang` without query — unchanged current-month default.



## Deferred (per plan)



- Clickable charts · Desktop deep links · Export · Server search · New reports · Investigation telemetry

- `CustomerCode` snapshot DDL (name/`q` fallback until aggregator refresh)



## Documentation updated



- `docs/features/btr-portal/btr-portal-operational.md` — Investigation workflow section

- `docs/features/btr-portal/btr-portal-architecture.md` — Investigation contract + report layout

- `docs/features/btr-portal/btr-portal-domain.md` — M24 platform capability (remediation)

- `docs/features/btr-portal/ALERT-REGISTRY.md` — Investigation columns + ranking keys (remediation)

- `docs/work/btr-portal/M24 Dashboard Drilldown - Analysis.md` — plan/summary links



## Notes



- `navigateToReport` retained as thin deprecated wrapper for backward compatibility.

- Location at-risk warehouse ranking navigates to Inventory Risk dashboard (no report route on `WarehouseAtRiskConcentration` registry entry).


