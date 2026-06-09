# Review Report

**Phase:** M21 — Purchasing Management Dashboard (Phase 1)  
**Review Date:** 2026-06-09  
**Reviewer:** Reviewer Agent

**Authoritative plan:** [M21 Purchasing Dashboard - Plan.md](./M21%20Purchasing%20Dashboard%20-%20Plan.md)  
**Implementation summary:** [M21 Purchasing Dashboard - Implementation Summary.md](./M21%20Purchasing%20Dashboard%20-%20Implementation%20Summary.md)  
**Analysis reference:** [M21-purchasing-dashboard-analysis.md](./M21-purchasing-dashboard-analysis.md)

---

## Reviewed Artifacts

### Database

- `src/j05-btr-distrib/btr.sql/Tables/ReportingContext/BTR_PortalDashboardPurchasingManagementKpi.sql`
- `src/j05-btr-distrib/btr.sql/Tables/ReportingContext/BTR_PortalDashboardPurchasingManagementAttention.sql`
- `src/j05-btr-distrib/btr.sql/Tables/ReportingContext/BTR_PortalDashboardPurchasingManagementTopPrincipal.sql`
- `src/j05-btr-distrib/btr.sql/btr.sql.sqlproj`

### Application / infrastructure

- `DashboardPurchasingManagementAggregator.cs`
- `DashboardPurchasingManagementKeyResolver.cs`
- `RefreshDashboardPurchasingManagementSnapshotWorker.cs`
- `DashboardPurchasingManagementSnapshotDal.cs`
- `DashboardPurchasingDal.cs`
- `DashboardExecutiveComposer.cs`
- `InvoiceView.cs` / `InvoiceViewDal.cs`
- `DashboardSnapshotOptions.cs`
- `RefreshAllDashboardSnapshotsWorker.cs`
- `HealthController.cs`, `btr.portal.worker/Program.cs`, DI extensions

### Frontend

- `PurchasingDashboardView.vue`
- `PurchasingAttentionCards.vue`, `PurchasingAttentionCardGroup.vue`
- `PurchasingSummaryRow.vue`, `PurchasingAttentionList.vue`
- `PurchasingPrincipalExposureTable.vue`, `PurchasingNavigationSection.vue`
- `dashboard.ts`

### Tests

- `DashboardPurchasingManagementAggregatorTest.cs` (includes `DashboardPurchasingManagementKeyResolverTest`)
- `DashboardExecutiveComposerTest.cs`
- `RefreshAllDashboardSnapshotsWorkerTest.cs`
- `RefreshDashboardSnapshotsHandlerTest.cs`

### Documentation

- `docs/features/btr-portal/btr-portal-domain.md`
- `docs/features/btr-portal/btr-portal-operational.md`
- `docs/features/materialized-dashboard/materialized-dashboard-domain.md`

### Build verification

- `dotnet build btr.test/btr.test.csproj` — **FAILED**

---

## Checklist

| Item | Result |
| --- | --- |
| Requirement implementation | **PARTIAL** — core design present; two functional/build gaps |
| Acceptance criteria | **PARTIAL** — most criteria met in code; not verifiable end-to-end |
| Architecture compliance | **PASS** — dedicated snapshot domain, refresh order, API merge pattern |
| Scope control | **PASS** — V1 preserved; Phase 2 deferred; exclusions respected |
| Test coverage | **FAIL** — solution does not compile; tests not executed |
| Documentation | **PARTIAL** — feature docs updated; analysis plan link missing |
| Deployment readiness | **FAIL** — build break blocks worker/API deployment |

---

## Acceptance Criteria Evaluation

| Criterion (Plan / Summary) | Evidence | Status |
| --- | --- | --- |
| Route `/dashboard/purchasing`, attention-first layout, V1 charts retained | `PurchasingDashboardView.vue` section order matches Plan §7.3 | **PASS** |
| Page title **Purchasing Management Dashboard** | View title + subtitle | **PASS** |
| 3 management snapshot tables | SQL files + sqlproj | **PASS** |
| Extended `GET /api/dashboard/purchasing` | `DashboardPurchasingDal.GetSummary()` merges V1 + management | **PASS** |
| Qualified backlog (3-day `LastUpdate` rule) | Aggregator lines 74–77; config default 3 | **PASS** |
| 8 attention signals + sort priority | Aggregator constants + `SignalPriority` dict | **PASS** |
| Cross-domain Principal Exposure (M15 + M19) | Aggregator Pass 3; exposure table component | **PASS** |
| Drill-down `?q=` | `navigateToReport` in list, Top 10, exposure table | **PASS** |
| Executive `RequiresAttention` = qualified backlog only | `ComposePurchasing` → `qualifiedBacklogCount > 0` | **PASS** |
| Staleness banner (30 min) | `IsDataFresh` + warn banner in view | **PASS** |
| V1 traceability unchanged | V1 snapshot/DAL untouched; summary uses V1 root fields | **PASS** |
| Health + worker `PurchasingManagement` domain | `HealthController`, `Program.cs` | **PASS** |
| Unit tests pass | **Build fails** — tests not run | **FAIL** |
| Solution compiles and deploys | **CS0234/CS0246** in worker | **FAIL** |

---

## Findings

### Critical

#### 1. Solution does not compile — `RefreshDashboardPurchasingManagementSnapshotWorker`

`btr.application` uses `Microsoft.Extensions.Options.IOptions<>` but that package is not referenced in `btr.application/packages.config` (only in infrastructure, API, worker, test).

```
error CS0234: The type or namespace name 'Options' does not exist in the namespace 'Microsoft.Extensions'
error CS0246: The type or namespace name 'IOptions<>' could not be found
```

Other application-layer workers (e.g. `RefreshDashboardCollectionSnapshotWorker`) do not use `IOptions`. This worker breaks the established pattern and blocks the entire build chain including tests and worker host.

**Impact:** PurchasingManagement refresh cannot run; API/worker cannot be deployed; unit test claims are unverified.

---

### Major

#### 2. `PrincipalInventoryNoPurchaseCount` KPI is computed incorrectly

Plan §2.4 / §5.1: count principals in M15 supplier Top 10 with zero MTD purchase.

Implementation sets the KPI from purchase Top 10 rows only:

```csharp
var principalInventoryNoPurchaseCount = topPrincipalRows.Count(r => r.IsInventoryNoPurchase);
```

`IsInventoryNoPurchase` is only evaluated on **purchase Top 10** principals. A high-inventory supplier with **zero MTD purchase** is typically **not** in purchase Top 10, so:

- Attention list correctly emits `PrincipalInventoryNoPurchase` (inventory Top 10 loop)
- KPI count stays **0**
- **Inventory Cross-Risk** card `RequiresAttention` (`PrincipalInventoryNoPurchaseCount > 0` in `DashboardPurchasingDal`) does not fire

The unit test `Aggregate_PrincipalInventoryNoPurchase_EmitsWhenInventoryTop10ZeroPurchase` expects `PrincipalInventoryNoPurchaseCount == 1` with zero invoices — that would **fail** once the build is fixed, exposing the bug.

#### 3. Unit tests not executed — implementation summary overstates verification

Implementation summary claims aggregator/key resolver/executive tests pass. Review build failed before any test ran. Test execution is a required gate (Plan §9, §12 Phase 6).

---

### Minor

1. **Missing plan §9.1 test cases** — no tests for `PrincipalAtRiskExposure` or `PrincipalInventoryConcentration` signal emission.
2. **Page `GeneratedAt` shows V1 snapshot only** — Plan §4.3 recommends displaying the earlier of V1 and management `GeneratedAt`. Freshness logic uses the min, but the displayed timestamp may overstate management freshness.
3. **Worker missing cross-domain null warning** — Plan §6.3 asks for a warning in refresh result when inventory/risk snapshots are null; worker proceeds silently.
4. **Analysis doc plan link missing** — Plan §11 lists updating `M21-purchasing-dashboard-analysis.md` with a plan link; not present.
5. **Attention list column header** — Company `PurchasingInactivity` row uses header "Principal" (cosmetic).

---

### Observations

- Phase 2 executive signal promotion correctly deferred (PO Q10).
- V1 purchasing domain, aggregator, and tables appear untouched.
- Documentation updates in `btr-portal-domain.md`, `btr-portal-operational.md`, and `materialized-dashboard-domain.md` align with delivered behavior.
- Frontend layout, tooltips on qualified vs unqualified BELUM, and navigation cross-links match the plan.
- `PurchasingAttentionCards.vue` wrapper over `PurchasingAttentionCardGroup.vue` is a reasonable naming variation vs the plan.

---

## Required Actions

1. **Fix build** — Either add `Microsoft.Extensions.Options` to `btr.application`, or (preferred, consistent with other workers) inject `DashboardSnapshotOptions` / `qualifiedBacklogDays` without `IOptions` in the application layer.
2. **Fix `PrincipalInventoryNoPurchaseCount`** — Derive count from inventory Top 10 principals with `MtdPurchase == 0` (e.g. count attention-list emissions or a dedicated inventory-top-10 pass), not from `topPrincipalRows`.
3. **Run full unit test suite** — Confirm all M21 tests pass after fixes; add missing `PrincipalAtRiskExposure` / `PrincipalInventoryConcentration` cases if desired.
4. **Re-verify manually** — Plan §9.4 checklist (SQL deploy, API response, traceability, executive card, health endpoint, worker CLI) after build fix.
5. **(Optional)** Align displayed `GeneratedAt` with min(V1, Management); add analysis doc plan link.

---

## Risk Summary

| Risk | Severity | Notes |
| --- | --- | --- |
| Build failure | **High** | Blocks all deployment |
| Inventory-no-purchase attention card silent | **Medium** | Signal in list but card border/count wrong |
| Unverified tests | **Medium** | Regression risk on executive composer change |
| Principal name mismatch across domains | **Low** | Case-insensitive matching implemented; manual spot-check still advised |

---

## Status

**REJECTED**

M21 Phase 1 is **substantially implemented** against the approved plan — database schema, aggregator logic, API merge, executive revision, frontend layout, and documentation are largely correct and scope-compliant.

Approval is blocked by:

1. A **compile-breaking** dependency issue in the refresh worker
2. A **functional defect** in `PrincipalInventoryNoPurchaseCount` that breaks a planned attention-card rule
3. **Unverified** test execution

Correct these items and request re-review before treating M21 as complete or proceeding to Phase 2.

---

*End of review report — M21 Purchasing Management Dashboard*
