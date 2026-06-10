# M23 Alert Center — Implementation Summary

**Status:** Ready for re-review — remediation complete (P1 logging + P2 tests); partial manual verification on local JUDE7  
**Date:** 2026-06-09 (remediation 2026-06-09)

## Delivered

- **Route:** `/alerts` — **Alert Center** (sidebar: Alert Center, after Executive)
- **API:** `GET /api/dashboard/alerts`
- **Architecture:** Read-time aggregator — **no new snapshot tables or refresh workers**
- **M16 entry:** **Open Alert Center** button on Management Attention Center (`/dashboard`)
- **Landing page:** `/dashboard` unchanged (M16 remains first screen after login)

## Scope delivered

| Area | Behavior |
| ---- | -------- |
| Categories | Sales · Customer · Collection · Inventory · Purchasing · Location · Platform |
| Entity alerts | Top 20 per category; producer `SortOrder` within category |
| M19 inventory | Summary KPI panel only + link to `/dashboard/inventory-risk` — no SKU rows |
| Sections | Platform (pinned) · Category summary · Alerts · Inventory risk summary · Concentrations · Domain navigation |
| Deduplication | M20 wins customer overdue; Legacy Debt suppresses Dormant; M20 workload suppresses M18 High Overdue Exposure |
| Drill-down | Alert row → domain dashboard (primary); report `?q=` when `ReportRoute` present (secondary) |
| Registry | `AlertCenterRegistry.cs` mirrors [ALERT-REGISTRY.md](../../features/btr-portal/ALERT-REGISTRY.md) |

## Architecture

```text
Existing M17–M22 snapshot tables (unchanged producers)
  ↓ read-only snapshot DALs
DashboardAlertCenterDal
  ↓ DashboardAlertCenterComposer
    ↳ AlertCenterRegistry (SignalKey metadata)
    ↳ DashboardSnapshotHealthHelper (shared with executive)
  ↓ GET /api/dashboard/alerts
AlertCenterView.vue (/alerts)
```

M23 is a **consumer only** — never queries transactional tables or defines new `SignalKey` values.

## Key files

| Layer | Path |
| --- | --- |
| Registry | `btr.application/.../DashboardAlertCenterAgg/Services/AlertCenterRegistry.cs` |
| Health helper | `btr.application/.../DashboardAlertCenterAgg/Services/DashboardSnapshotHealthHelper.cs` |
| Composer | `btr.application/.../DashboardAlertCenterAgg/Services/DashboardAlertCenterComposer.cs` |
| Query/DTOs | `btr.application/.../DashboardAlertCenterAgg/Queries/GetDashboardAlertCenterQuery.cs` |
| DAL | `btr.infrastructure/.../DashboardAlertCenterAgg/DashboardAlertCenterDal.cs` |
| API | `btr.portal.api/Controllers/Dashboard/AlertCenterDashboardController.cs` |
| Frontend view | `btr.portal.web/src/views/alerts/AlertCenterView.vue` |
| Frontend components | `btr.portal.web/src/components/alerts/AlertCenter*.vue` |
| Tests | `btr.test/ReportingContext/AlertCenterRegistryTest.cs`, `DashboardAlertCenterComposerTest.cs` |

## Shared refactor

`DashboardExecutiveComposer` delegates freshness/unavailable/health resolution to `DashboardSnapshotHealthHelper` — executive API response shape unchanged.

## Rejection remediation (2026-06-09)

Addressed [Review Report](./M23%20Alert%20Center%20-%20Review%20Report.md) blocking findings:

| Item | Change |
| --- | --- |
| **P1 — Unknown SignalKey logging** | `DashboardAlertCenterComposer` logs `Logger.Warn` via NLog when `TryGetForProducer` fails in `ApplyDeduplication` and `BuildConcentrations` (Plan §5.3). NLog added to `btr.application`. |
| **P2 — Registry test** | `Registry_M20Overdue_ResolvesToCollectionCategory`; removed duplicate `Overdue` InlineData from theory. |
| **P2 — Warning band test** | `Compose_SalesWarningBand_CreatesSyntheticAlert` (92% achievement → Warning). |
| **API DI fixes (found during verification)** | Registered `AlertCenterDashboardController` and `InventoryRiskDashboardController` in `PortalPresentationExtensions.cs`; added Collection/Location DAL usings in `InfrastructurePortalExtensions.cs`. |

## Verification

### Unit tests

```text
dotnet build src/j05-btr-distrib/btr.test/btr.test.csproj
dotnet vstest btr.test/bin/Debug/btr.test.dll --TestCaseFilter:"FullyQualifiedName~AlertCenter"
```

**Result:** 57 passed, 0 failed.

| File | Coverage |
| --- | --- |
| `AlertCenterRegistryTest.cs` | All M17–M22 producer SignalKey constants; M20 `Overdue` disambiguation; M19 item keys excluded from Alerts section |
| `DashboardAlertCenterComposerTest.cs` | Dedup rules, category cap 20, M19 summary only, platform pinned, concentrations separation, sales Critical/Warning synthetic alerts, CompoundDependency route, unknown key skipped (warning logged at runtime) |

### Frontend build

```text
cd src/j05-btr-distrib/btr.portal.web && npm run build
```

**Result:** Success (vue-tsc + vite build).

## Manual Verification Checklist (Plan §7.3)

**Environment:** Local JUDE7 — `btr.portal.api` IIS Express `:5050`, `npm run dev` `:5173/portal`, user DIMAS. SQL: `JUDE7/btr`.

**Prerequisite gap:** `BTRPD_CollectionKpi` (and related M20/M21/M22 tables) not present on JUDE7 — `GET /api/dashboard/alerts` returns HTTP 500 until snapshot worker refresh populates M17–M22 tables. Customer/Sales/Piutang/Inventory/InventoryRisk domain APIs return 200.

| # | Check | Status | Notes |
| --- | --- | --- | --- |
| 1 | `GET /api/dashboard/alerts` returns full response | **Blocked** | HTTP 500 — missing M20+ snapshot tables on JUDE7 DB; controller DI fixed during verification |
| 2 | `/alerts` — title Alert Center; sections in fixed order | **Pass** | Browser: title + subtitle; sections Alerts → Inventory Risk → Concentrations → Domain Dashboards render (Platform/Summary hidden when API unavailable) |
| 3 | Login lands on `/dashboard` | **Pass** | Post-login URL `/portal/dashboard` |
| 4 | Open Alert Center on M16 navigates to `/alerts` | **Pass** | Button present on M16 header; sidebar nav to `/portal/alerts` verified |
| 5 | Sidebar Alert Center entry works | **Pass** | Menu item after Executive; navigates to `/alerts` |
| 6 | Platform stale/degraded matches M16 | **Blocked** | Requires successful alerts/executive API response (executive also 500 without full snapshots) |
| 7 | Customer M17+M20 overdue → one M20 row | **Pass** | Unit test `Compose_M20ChronicOverdue_SuppressesM17OverdueForSameCustomer` |
| 8 | Salesman M18+M20 overdue → one M20 row | **Pass** | Unit test `Compose_M20HighOverdueWorkload_SuppressesM18HighOverdueExposure` |
| 9 | Collection max 20 rows; summary shows true total | **Pass** | Unit test category cap case |
| 10 | Inventory summary — no SKU rows | **Pass** | Unit tests; UI shows KPI panel only (no SKU table in Alerts) |
| 11 | Concentrations separate from Alerts | **Pass** | Unit tests; separate UI sections |
| 12 | Dashboard link → correct domain dashboard | **Pass** | Registry routes unit-tested; `AlertCenterAlertTable` wires `router.push(DashboardRoute)` |
| 13 | Report icon → `?q=` when applicable | **Pass** | `navigateToReport` wired in `AlertCenterAlertTable.vue` (code review) |
| 14 | View Inventory Risk → `/dashboard/inventory-risk` | **Pass** | `InventoryRiskSummary.DashboardRoute` = `/dashboard/inventory-risk` (unit test + component link) |
| 15 | Refresh reloads without error | **Pass** | Refresh button on `/alerts` reloads without UI crash |
| 16 | No new snapshot tables or workers | **Pass** | Code review — M23 read-time only |
| 17 | Domain dashboard attention lists unchanged | **Pass** | No producer/worker changes |

**Re-review note:** Items 1 and 6 require staging/dev with populated M17–M22 snapshot tables for live API comparison. All composer/dedup/cap behaviors covered by 57 unit tests.

## Out of scope (confirmed not implemented)

- New `SignalKey` types in M23
- Alert acknowledgment, history, snooze
- Real-time / event-driven refresh
- Cross-domain priority scoring
- M16 executive layout/API changes (navigation button only)
- M16 Top 5 lists in Alert Center
- M19 item-level rows in alert feed
- Desktop menu deep links
- Role-based alert views

## Documentation updated

- [btr-portal-operational.md](../../features/btr-portal/btr-portal-operational.md)
- [btr-portal-domain.md](../../features/btr-portal/btr-portal-domain.md)
- [btr-portal-architecture.md](../../features/btr-portal/btr-portal-architecture.md)
- [ALERT-REGISTRY.md](../../features/btr-portal/ALERT-REGISTRY.md)
- [materialized-dashboard-domain.md](../../features/materialized-dashboard/materialized-dashboard-domain.md)
- [M23-Alert-Center-Analysis.md](./M23-Alert-Center-Analysis.md)

## References

- [M23 Alert Center - Plan.md](./M23%20Alert%20Center%20-%20Plan.md)
- [M23-Alert-Center-Analysis.md](./M23-Alert-Center-Analysis.md) — Section 17 PO decisions
