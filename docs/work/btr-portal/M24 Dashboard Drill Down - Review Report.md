# Review Report

**Phase:** M24 — Dashboard Drill Down & Investigation Framework  
**Review Date:** 2026-06-09  
**Reviewer:** Reviewer Agent  
**Plan:** [M24 Dashboard Drill Down - Plan.md](./M24%20Dashboard%20Drill%20Down%20-%20Plan.md)  
**Implementation Summary:** [M24 Dashboard Drill Down - Implementation Summary.md](./M24%20Dashboard%20Drill%20Down%20-%20Implementation%20Summary.md)

---

## Reviewed Artifacts

### Backend
- `btr.application/ReportingContext/Shared/InvestigationMetadata.cs`
- `btr.application/ReportingContext/Shared/InvestigationRegistry.cs`
- `btr.application/ReportingContext/Shared/InvestigationMetadataBuilder.cs`
- `btr.application/ReportingContext/DashboardExecutiveAgg/Services/DashboardExecutiveComposer.cs`
- `btr.application/ReportingContext/DashboardAlertCenterAgg/Services/DashboardAlertCenterComposer.cs`
- `btr.application/ReportingContext/PiutangReportAgg/Queries/GetPiutangReportQuery.cs`
- `btr.infrastructure/ReportingContext/PiutangReportAgg/PiutangReportDal.cs`
- `btr.infrastructure/FinanceContext/PiutangSalesWilayahRpt/PiutangSalesWilayahDal.cs`
- Dashboard infrastructure DALs (Customer, Salesman, Collection, Inventory Risk, Purchasing, Location, Sales, Piutang, Inventory snapshot)

### Frontend
- `btr.portal.web/src/models/investigation.ts`
- `btr.portal.web/src/services/navigateToInvestigation.ts`
- `btr.portal.web/src/services/buildInvestigationQuery.ts`
- `btr.portal.web/src/services/applyInvestigationQuery.ts`
- `btr.portal.web/src/composables/useReportInvestigationFilter.ts`
- `btr.portal.web/src/composables/useReportInvestigationHydration.ts`
- `btr.portal.web/src/components/reports/InvestigationBreadcrumb.vue`
- `btr.portal.web/src/components/reports/InvestigationStepsList.vue`
- Report views: Sales, Piutang, Inventory, Purchasing
- Attention components + `AlertCenterAlertTable.vue`
- Legacy dashboards: Sales, Piutang, Inventory + `ExecutiveExposureSection.vue`

### Tests
- `btr.test/ReportingContext/InvestigationRegistryTest.cs`
- `btr.test/ReportingContext/PiutangReportDalTest.cs`
- `btr.test/ReportingContext/DashboardExecutiveComposerTest.cs`

### Documentation
- `docs/features/btr-portal/btr-portal-operational.md`
- `docs/features/btr-portal/btr-portal-architecture.md`
- `docs/features/btr-portal/ALERT-REGISTRY.md` *(see finding — partial)*
- `docs/features/btr-portal/btr-portal-domain.md` *(see finding — missing)*

---

## Checklist

| Area | Result | Notes |
| ---- | ------ | ----- |
| Requirement implementation | **PASS** | Mandatory investigation contract, navigation, reports, executive Top 5, legacy M11/M14/M15 rankings, attention retrofit, Alert Center report-first |
| Acceptance criteria (mandatory tier) | **PASS** | PO mandatory outcomes (Section 2.1) are implemented in code |
| Architecture compliance | **PASS** | Matches plan topology; no new APIs/endpoints; additive DTO fields |
| Test coverage | **FAIL** | Registry cross-check incomplete; frontend query encoder tests absent |
| Documentation | **FAIL** | Two required doc updates not delivered per plan Sections 5.5 and 10 |
| Scope control | **PASS** | No unauthorized KPI/snapshot/alert-rule changes observed |

---

## Mandatory Scope Verification

| Mandatory outcome (Plan §2.1) | Evidence | Status |
| ----------------------------- | -------- | ------ |
| Investigation Metadata Contract (C# + TS) | `InvestigationMetadata.cs`, `models/investigation.ts` | ✅ |
| Purchasing Report `?q=` hydration | `PurchasingReportView.vue` + `useReportInvestigationHydration` | ✅ |
| Piutang `allOpenBalances` mode | API flag, DAL `ListAllOpenBalances`, UI label "All open balances" | ✅ |
| Executive Critical Exposure Top 5 clickable | `DashboardExecutiveComposer.ComposeCriticalExposures`, `ExecutiveExposureSection.vue` | ✅ |
| Legacy M11/M14/M15 ranking drill-down | `DashboardSalesLiveDal`, `DashboardPiutangDal`, inventory snapshot DAL + dashboard views | ✅ |
| Investigation breadcrumb on all four reports | `InvestigationBreadcrumb.vue` wired in all `*ReportView.vue` | ✅ |
| Stable entity ID report filtering | `SalesPersonId` on `SalesReportRow`; `CustomerCode` on `PiutangReportRow`; `useReportInvestigationFilter` | ✅ |
| Qualified Backlog `posting=BELUM` | Registry default + Purchasing report client filter | ✅ |
| **Investigate** label | All M17–M22 attention list components | ✅ |
| Report-first Alert Center | `AlertCenterAlertTable.vue` — Investigate primary, View Dashboard secondary; Company/Wilayah blocked | ✅ |

Nice-to-have items (investigation steps list, desktop next-step text) are also delivered.

---

## Findings

### Critical

None identified. Core mandatory behavior is present and structurally sound.

### Major

#### 1. `btr-portal-domain.md` not updated

**Plan reference:** Section 10 — `btr-portal-domain.md` must include M24 as a platform capability reference.

**Evidence:** No `M24`, `Investigation`, or investigation-framework content in `docs/features/btr-portal/btr-portal-domain.md`.

**Impact:** Domain documentation does not reflect the new horizontal capability; future milestones lack an authoritative domain anchor.

---

#### 2. `ALERT-REGISTRY.md` missing Investigation column group

**Plan reference:** Section 5 implementer requirement #5 — add Investigation columns (report route, period mode, desktop next step).

**Evidence:** `ALERT-REGISTRY.md` references `InvestigationRegistry.cs` in the purpose paragraph only. Registry entry tables still have `Drill-down dashboard` columns only — no Investigation column group.

**Impact:** Alert catalog documentation is out of sync with the investigation routing contract; implementers cannot trace signal → report evidence from the registry doc alone.

---

#### 3. `InvestigationRegistryTest` does not fulfill stated cross-check requirement

**Plan reference:** Section 5.4 / 8.1 — `InvestigationRegistry_ContainsAllAttentionSignalKeys` must cross-check **all** M17–M22 attention `SignalKey` constants.

**Evidence:** Test asserts only ~15 keys (subset). Registry implementation includes all aggregator constants (verified by code review), but the test does not enforce completeness. A future missing registry entry would not be caught.

**Missing from test (examples):** `SlowMoving`, `NeverSold`, `SignalLegacyTopCustomer`, `SignalLegacyTopCategory`, `SignalLegacyTopSupplier`, all six `DashboardLocationAggregator` warehouse signals, several purchasing principal signals, `LowRecoveryVsBilling`, `LegacyDebt`, etc.

**Impact:** Regression guard for the investigation contract is weaker than specified.

---

#### 4. Frontend `buildInvestigationQuery` unit tests not delivered

**Plan reference:** Section 8.2 — unit tests for Piutang customer drill-down, Qualified Backlog `posting=BELUM`, minimal sales drill-down.

**Evidence:** No `*.spec.ts` / `*.test.ts` files in `btr.portal.web`. `buildInvestigationQuery.ts` has no automated coverage.

**Impact:** Query encoding is a critical navigation contract; regressions in param mapping would not be caught in CI.

---

### Minor

#### 5. Non-legacy ranking surfaces still use legacy navigation

**Evidence:** `CustomerDashboardView.vue`, `CollectionDashboardView.vue`, `SalesmanDashboardView.vue`, and others call `navigateToReport` for ranking row clicks without `Investigation` metadata. Piutang-bound collection rankings (e.g. Top Overdue Customers) navigate with `q=` only — no `periodMode=allOpenBalances`, no breadcrumb signal context.

**Note:** Plan mandatory legacy retrofit explicitly lists M11/M14/M15 only. This is **out of stated mandatory scope** but weakens the horizontal framework goal (Appendix C) for M17/M18/M20 ranking clicks.

---

#### 6. `CustomerCode` snapshot persistence deferred

**Evidence:** Implementation summary acknowledges optional DDL not applied; executive Top Customer drill-down relies on aggregator-populated code with name/`q` fallback.

**Note:** Plan Section 6.5 allows this deferral. Acceptable for approval once other findings are resolved.

---

### Observation

- `navigateToReport.ts` correctly delegates to `navigateToInvestigation` as a thin deprecated wrapper.
- Router state carries `dashboardRoute`, `desktopNextStep`, and `investigationSteps` without URL pollution — matches plan Section 2.4.2.
- Alert Center correctly suppresses **Investigate** for `Company` and `Wilayah` entity types.
- Backend unit tests could not be executed in the review environment (`.NET Framework 4.8` test project; `dotnet test` did not run test cases). Test source code for Piutang all-open and executive investigation assertions is present and appears correct.

---

## Required Actions

1. **Update `docs/features/btr-portal/btr-portal-domain.md`** — add M24 Investigation Framework as a platform capability (depth model, report-first navigation, contract reference).

2. **Update `docs/features/btr-portal/ALERT-REGISTRY.md`** — add Investigation column group to registry tables (report route, period mode, desktop next step) per plan Section 5.5.

3. **Expand `InvestigationRegistryTest`** — enumerate all M17–M22 aggregator `SignalKey` constants plus executive synthetic and legacy ranking keys; assert each has a registry entry with expected `ReportRoute` / `DefaultPeriodMode` where applicable.

4. **Add frontend unit tests for `buildInvestigationQuery`** — at minimum the three cases in plan Section 8.2 (Piutang all-open + customerId, Qualified Backlog posting, minimal sales `q`-only). Introduce a minimal test runner if none exists.

5. **(Recommended)** Retrofit M17/M18/M20 ranking row clicks to use producer `Investigation` metadata where rankings navigate to reports — especially collection piutang rankings — or document explicit deferral in the implementation summary.

---

## Re-Review Criteria

Re-review may approve when:

- [ ] `btr-portal-domain.md` updated
- [ ] `ALERT-REGISTRY.md` Investigation columns added
- [ ] `InvestigationRegistryTest` covers all attention signal keys
- [ ] `buildInvestigationQuery` unit tests exist and pass
- [ ] Backend M24-related unit tests pass in CI (InvestigationRegistry, PiutangReportDal all-open, DashboardExecutiveComposer)

---

## Status

**REJECTED**

Mandatory implementation work is substantially complete and architecturally aligned with the plan. Rejection is due to **unresolved Major findings** in required documentation and test artifacts specified in the implementation plan. Correct and re-submit for re-review.
