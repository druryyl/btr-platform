# Review Report (Re-Review)

**Phase:** M24 — Dashboard Drill Down & Investigation Framework  
**Review Date:** 2026-06-09 (re-review)  
**Reviewer:** Reviewer Agent  
**Plan:** [M24 Dashboard Drill Down - Plan.md](./M24%20Dashboard%20Drill%20Down%20-%20Plan.md)  
**Implementation Summary:** [M24 Dashboard Drill Down - Implementation Summary.md](./M24%20Dashboard%20Drill%20Down%20-%20Implementation%20Summary.md)  
**Prior review:** Rejected 2026-06-09 — documentation and test gaps (see remediation section below)

---

## Reviewed Artifacts

### Remediation (this re-review focus)
- `docs/features/btr-portal/btr-portal-domain.md` — M24 platform capability section
- `docs/features/btr-portal/ALERT-REGISTRY.md` — Investigation column group + ranking keys subsection
- `btr.test/ReportingContext/InvestigationRegistryTest.cs` — expanded key coverage + route/period theories
- `btr.portal.web/src/services/buildInvestigationQuery.spec.ts` — Vitest query encoder tests
- `btr.portal.web/vitest.config.ts`, `package.json` (`vitest run` script)
- Domain dashboard ranking retrofit: `CustomerDashboardView`, `CollectionDashboardView`, `SalesmanDashboardView`, `PurchasingDashboardView`, `InventoryRiskDashboardView`, `LocationDashboardView` + backend ranking `Investigation` DAL mapping

### Full implementation (spot-checked; unchanged from prior review)
- Investigation contract, registry, builder (C# + TypeScript)
- `navigateToInvestigation`, `buildInvestigationQuery`, `applyInvestigationQuery`, `useReportInvestigationFilter`
- All four report views + `InvestigationBreadcrumb.vue`, `InvestigationStepsList.vue`
- Piutang `allOpenBalances` API/DAL/UI path
- Executive Top 5, legacy M11/M14/M15 rankings, M17–M22 attention lists, `AlertCenterAlertTable.vue`
- `btr-portal-operational.md`, `btr-portal-architecture.md`

---

## Checklist

| Area | Result | Notes |
| ---- | ------ | ----- |
| Requirement implementation | **PASS** | All mandatory tier outcomes (Plan §2.1) present |
| Acceptance criteria (mandatory tier) | **PASS** | PO success test (Appendix C) satisfied in code |
| Architecture compliance | **PASS** | No new endpoints; additive DTOs; router state for non-URL metadata |
| Test coverage | **PASS** | Registry cross-check, query encoder, Piutang all-open, executive composer — executed and passing |
| Documentation | **PASS** | All Plan §10 documents updated including prior gaps |
| Scope control | **PASS** | No unauthorized KPI/snapshot/alert-rule changes |

---

## Prior Findings — Remediation Verification

| # | Prior finding | Required action | Evidence | Status |
| - | ------------- | --------------- | -------- | ------ |
| 1 | `btr-portal-domain.md` not updated | Add M24 platform capability | § Investigation Framework (M24) — depth model, report-first defaults, Piutang alignment, contract cross-refs | ✅ Resolved |
| 2 | `ALERT-REGISTRY.md` missing Investigation columns | Add report route, period mode, desktop next step | All registry tables include **Investigate →**, **Period mode**, **Desktop next step**; ranking keys subsection added | ✅ Resolved |
| 3 | `InvestigationRegistryTest` incomplete | Cross-check all M17–M22 aggregator keys | 34 aggregator constants + 4 executive + 4 legacy + 8 ranking keys enumerated; route/period theories for piutang, sales, executive | ✅ Resolved |
| 4 | `buildInvestigationQuery` tests absent | Plan §8.2 three cases | `buildInvestigationQuery.spec.ts` — piutang all-open, Qualified Backlog BELUM, minimal sales `q`-only | ✅ Resolved |
| 5 | Non-legacy ranking drill-down gap (recommended) | Retrofit M17/M18/M20 rankings | Backend `Investigation` on ranking DTOs/DALs; six domain dashboard views call `navigateToInvestigation` | ✅ Resolved |

---

## Mandatory Scope Verification

| Mandatory outcome (Plan §2.1) | Evidence | Status |
| ----------------------------- | -------- | ------ |
| Investigation Metadata Contract (C# + TS) | `InvestigationMetadata.cs`, `models/investigation.ts` | ✅ |
| Purchasing Report `?q=` hydration | `PurchasingReportView.vue` + `useReportInvestigationHydration` | ✅ |
| Piutang `allOpenBalances` mode | API flag, `ListAllOpenBalances`, UI "All open balances" label | ✅ |
| Executive Critical Exposure Top 5 clickable | `DashboardExecutiveComposer`, `ExecutiveExposureSection.vue` | ✅ |
| Legacy M11/M14/M15 ranking drill-down | Sales/Piutang/Inventory snapshot DALs + dashboard views | ✅ |
| Investigation breadcrumb on all four reports | `InvestigationBreadcrumb.vue` in all `*ReportView.vue` | ✅ |
| Stable entity ID report filtering | `SalesPersonId`, `CustomerCode`, `useReportInvestigationFilter` | ✅ |
| Qualified Backlog `posting=BELUM` | Registry default + Purchasing client filter | ✅ |
| **Investigate** label | All M17–M22 attention list components | ✅ |
| Report-first Alert Center | `AlertCenterAlertTable.vue` — Investigate primary; Company/Wilayah suppressed | ✅ |

Nice-to-have items (investigation steps list, desktop next-step text) remain delivered.

---

## Test Execution (re-review)

### Backend

```text
dotnet build src/j05-btr-distrib/btr.test/btr.test.csproj
dotnet vstest btr.test/bin/Debug/btr.test.dll --TestCaseFilter:"FullyQualifiedName~InvestigationRegistry|FullyQualifiedName~PiutangReportDal|FullyQualifiedName~DashboardExecutiveComposer"
```

**Result:** 36 passed, 0 failed

### Frontend

```text
cd src/j05-btr-distrib/btr.portal.web && npm test
```

**Result:** 3 passed (`buildInvestigationQuery.spec.ts`)

---

## Findings

### Critical

None.

### Major

None. All prior Major findings are resolved with verified evidence.

### Minor

#### 1. `CustomerCode` snapshot DDL still deferred

**Evidence:** Implementation summary documents optional DDL not applied; executive Top Customer drill-down uses aggregator-populated code with name/`q` fallback.

**Note:** Plan Section 6.5 explicitly allows this deferral. Does not block approval.

### Observation

- Domain dashboard ranking retrofit (Finding 5) exceeds mandatory Plan §2.4.7 scope and strengthens horizontal framework consistency for M17/M18/M20/M21/M22 ranking clicks.
- `navigateToReport` remains a thin deprecated wrapper delegating to `navigateToInvestigation`.
- `WarehouseAtRiskConcentration` correctly has no direct report route; multi-step metadata guides users via dashboard context (per plan §2.4.9).
- Manual checklist (Plan §8.5) not executed in review environment; recommend spot-check before production deploy.

---

## Re-Review Criteria (from prior report)

- [x] `btr-portal-domain.md` updated
- [x] `ALERT-REGISTRY.md` Investigation columns added
- [x] `InvestigationRegistryTest` covers all attention signal keys
- [x] `buildInvestigationQuery` unit tests exist and pass
- [x] Backend M24-related unit tests pass (36/36 verified in re-review)

---

## Status

**APPROVED**

All required implementation work has been reviewed. No unresolved Critical or Major findings remain. M24 mandatory scope is complete, documented, and covered by passing unit tests. The project may proceed past the M24 phase gate.

**Approved By:** Reviewer Agent
