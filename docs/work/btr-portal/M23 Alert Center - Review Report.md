# Review Report

**Phase:** M23 — Alert Center  
**Review Date:** 2026-06-09  
**Reviewer:** Reviewer Agent

**Authoritative plan:** [M23 Alert Center - Plan.md](./M23%20Alert%20Center%20-%20Plan.md)  
**Implementation summary:** [M23 Alert Center - Implementation Summary.md](./M23%20Alert%20Center%20-%20Implementation%20Summary.md)  
**Analysis reference:** [M23-Alert-Center-Analysis.md](./M23-Alert-Center-Analysis.md)  
**Alert catalog:** [ALERT-REGISTRY.md](../../features/btr-portal/ALERT-REGISTRY.md)

---

## Reviewed Artifacts

### Application / infrastructure

- `btr.application/ReportingContext/DashboardAlertCenterAgg/Services/AlertCenterRegistry.cs`
- `btr.application/ReportingContext/DashboardAlertCenterAgg/Services/DashboardAlertCenterComposer.cs`
- `btr.application/ReportingContext/DashboardAlertCenterAgg/Services/DashboardSnapshotHealthHelper.cs`
- `btr.application/ReportingContext/DashboardAlertCenterAgg/Queries/GetDashboardAlertCenterQuery.cs`
- `btr.application/ReportingContext/DashboardAlertCenterAgg/Contracts/IDashboardAlertCenterDal.cs`
- `btr.infrastructure/ReportingContext/DashboardAlertCenterAgg/DashboardAlertCenterDal.cs`
- `btr.application/ReportingContext/DashboardExecutiveAgg/Services/DashboardExecutiveComposer.cs` (health helper refactor)
- `btr.portal.api/Controllers/Dashboard/AlertCenterDashboardController.cs`
- `btr.portal.api/Configurations/InfrastructurePortalExtensions.cs`

### Frontend

- `btr.portal.web/src/views/alerts/AlertCenterView.vue`
- `btr.portal.web/src/components/alerts/AlertCenterPlatformSection.vue`
- `btr.portal.web/src/components/alerts/AlertCenterCategorySummary.vue`
- `btr.portal.web/src/components/alerts/AlertCenterAlertTable.vue`
- `btr.portal.web/src/components/alerts/AlertCenterInventoryRiskSummary.vue`
- `btr.portal.web/src/components/alerts/AlertCenterConcentrationsSection.vue`
- `btr.portal.web/src/components/alerts/AlertCenterNavigationSection.vue`
- `btr.portal.web/src/router/index.ts`
- `btr.portal.web/src/layouts/MainLayout.vue`
- `btr.portal.web/src/views/dashboard/DashboardHomeView.vue`
- `btr.portal.web/src/models/dashboard.ts`
- `btr.portal.web/src/api/dashboardApi.ts`
- `btr.portal.web/src/stores/dashboardStore.ts`

### Tests

- `btr.test/ReportingContext/AlertCenterRegistryTest.cs`
- `btr.test/ReportingContext/DashboardAlertCenterComposerTest.cs`

### Documentation

- `docs/features/btr-portal/ALERT-REGISTRY.md`
- `docs/features/btr-portal/btr-portal-domain.md`
- `docs/features/btr-portal/btr-portal-operational.md`
- `docs/features/btr-portal/btr-portal-architecture.md`
- `docs/features/materialized-dashboard/materialized-dashboard-domain.md`
- `docs/work/btr-portal/M23-Alert-Center-Analysis.md` (plan/summary links)

### Build verification

```text
dotnet build btr.test/btr.test.csproj
dotnet vstest btr.test/bin/Debug/btr.test.dll --TestCaseFilter:"FullyQualifiedName~AlertCenter"
```

**Result:** Build succeeded. **55 passed**, 0 failed.

---

## Checklist

| Item | Result |
| --- | --- |
| Requirement implementation | **PARTIAL** — core aggregator delivered; one explicit runtime behavior missing |
| Acceptance criteria (§7.3) | **PARTIAL** — unit-tested logic passes; manual/integration checks not evidenced |
| Architecture compliance | **PASS** — read-time composition, no new snapshots/workers, shared health helper |
| Scope control | **PASS** — no new SignalKeys, M16 backend unchanged, M19 summary-only |
| Test coverage (§7.1–7.2) | **PASS** — all planned composer cases covered; 55 unit tests pass |
| Documentation (§9) | **PASS** — feature docs and registry updated |
| Phase 4 verification (§10) | **FAIL** — manual checklist §7.3 not executed |

---

## Acceptance Criteria Evaluation

### Unit-tested criteria (Plan §7.1–7.2) — PASS

| Criterion | Evidence | Status |
| --- | --- | --- |
| M20 customer overlap suppresses M17 Overdue / Dormant / PlafondBreach | `DashboardAlertCenterComposerTest` dedup tests | **PASS** |
| M20 HighOverdueWorkload suppresses M18 HighOverdueExposure | Composer test | **PASS** |
| M17 SuspendedWithSales not suppressed | Composer test | **PASS** |
| Category cap 20 + TotalCount / HasMore | Composer test | **PASS** |
| M19 item rows excluded; KPI summary populated | Composer tests | **PASS** |
| Sales achievement Critical synthetic alert | Composer test | **PASS** |
| Platform degraded pinned first | Composer test | **PASS** |
| Concentrations vs Alerts separation | Composer tests | **PASS** |
| CompoundDependency → `/dashboard/purchasing` | Composer test | **PASS** |
| Unknown SignalKey skipped, compose succeeds | Composer test | **PASS** |
| All producer SignalKey constants in registry | `AlertCenterRegistryTest` (55 cases) | **PASS** |
| M19 item signals not in Alerts section | Registry test | **PASS** |

### Manual / integration criteria (Plan §7.3) — NOT VERIFIED

| # | Criterion | Evidence | Status |
| --- | --- | --- | --- |
| 1 | `GET /api/dashboard/alerts` full response | No API run log | **PENDING** |
| 2 | `/alerts` layout and section order | No browser verification | **PENDING** |
| 3 | Login lands on `/dashboard` | Router code correct; not runtime-tested | **PENDING** |
| 4 | Open Alert Center from M16 | Button present; not runtime-tested | **PENDING** |
| 5 | Sidebar Alert Center entry | Menu item present; not runtime-tested | **PENDING** |
| 6 | Platform stale/degraded matches M16 | Shared helper; not compared live | **PENDING** |
| 7–8 | Live dedup reconciliation | Unit tests only | **PENDING** |
| 9–11 | Cap, inventory summary, concentrations UI | Unit tests only | **PENDING** |
| 12–14 | Drill-down dashboard / report / inventory risk link | UI wired; not clicked | **PENDING** |
| 15 | Refresh button | Wired; not runtime-tested | **PENDING** |
| 16–17 | No new snapshots; domain dashboards unchanged | Code inspection **PASS**; deploy diff not verified | **PARTIAL** |

Implementation summary acknowledges all manual items as **Pending**. Reviewer has no contradictory evidence.

---

## Findings

### Critical

None.

### Major

1. **Unknown `SignalKey` logging not implemented (Plan §5.3)**  
   Plan requires: *"Unknown SignalKey at runtime → log warning, skip row."*  
   `DashboardAlertCenterComposer.ApplyDeduplication` silently `continue`s when `TryGetForProducer` fails (lines 311–314). Compose succeeds (unit-tested), but operational observability for future producer keys is missing. This is an explicit implementer requirement, not optional.

2. **Phase 4 manual verification incomplete (Plan §10 steps 20–21)**  
   Unit tests pass, but Plan §7.3 manual checklist (17 items) has no execution record. Stage-gate cannot confirm end-to-end acceptance (API response shape in production, UI section order, drill-down navigation, refresh) without deploy-time verification.

### Minor

1. **Registry test duplicate `Overdue` InlineData**  
   xUnit skips duplicate test ID for `Overdue`. `TryGet("Overdue")` resolves to M17 Customer entry only; M20 Collection `Overdue` category disambiguation via `TryGetForProducer` is not independently asserted in `AlertCenterRegistryTest`.

2. **No unit test for `SalesAchievementWarning` synthetic alert**  
   Plan §7.1 lists Critical band test; Warning band is symmetric but untested.

3. **No dedicated unit tests for `DashboardSnapshotHealthHelper`**  
   Plan §8 risk mitigation recommends shared health tests. Executive and alert-center paths share helper; drift risk is low but untested in isolation.

### Observation

- `DashboardExecutiveComposer` correctly delegates to `DashboardSnapshotHealthHelper` without changing executive API response shape.
- Documentation §9 deliverables are present and consistent with implementation.
- `ALERT-REGISTRY.md` aligns with `AlertCenterRegistry.cs` entries reviewed.
- No unauthorized scope: no new snapshot tables, workers, or M16 layout changes.

---

## Required Actions

1. **Add runtime warning for unknown producer `SignalKey`**  
   Inject `ILogger<DashboardAlertCenterComposer>` (or equivalent project logging pattern). When `TryGetForProducer` returns false, log warning with `Source`, `SignalKey`, `EntityType`, `EntityName`; then skip row. Add/adjust unit test to assert logging behavior if the project tests log output.

2. **Execute Plan §7.3 manual verification checklist**  
   On dev/staging with populated snapshots: run all 17 checks; record pass/fail in Implementation Summary or a short verification log. Minimum: API GET smoke, `/alerts` page walkthrough, one dedup spot-check (customer M17+M20), one drill-down (dashboard + report).

3. **(Recommended)** Add `Registry_M20Overdue_ResolvesToCollectionCategory` test using `TryGetForProducer("M20", SignalOverdue, ...)`.

4. **(Recommended)** Add `Compose_SalesWarningBand_CreatesSyntheticAlert` test.

---

## Status

**REJECTED**

Implementation delivers the approved architecture and passes unit tests, but two Major gaps remain: missing runtime logging required by Plan §5.3, and Phase 4 manual acceptance verification not demonstrated. Correct and re-submit for review after Required Actions 1–2 are complete.

---

*Reviewer Agent — independent implementation review per `docs/agents/reviewer-agent.md`*
