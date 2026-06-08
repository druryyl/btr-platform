# M16 Executive Dashboard — Implementation Summary

**Initiative:** M16 Executive Dashboard — Management Attention Center  
**Status:** Implemented  
**Date:** 2026-06-08  
**Plan:** [`M16 Executive Dashboard - Plan.md`](./M16%20Executive%20Dashboard%20-%20Plan.md)  
**Analysis:** [`M16 Executive Dashboard - Analysis.md`](./M16%20Executive%20Dashboard%20-%20Analysis.md)

---

## Summary

M16 replaces the operational dashboard home (`/dashboard`) with a **Management Attention Center** that composes attention-oriented metrics from existing materialized snapshot tables. No new database tables, workers, or aggregators were added. The home page now answers *What requires management attention today?* for all authenticated users.

---

## What Was Built

### Backend

| Component | Purpose |
| --------- | ------- |
| `GET /api/dashboard/executive` | Composition read endpoint (JWT required) |
| `DashboardExecutiveAgg` | Query, handler, DTOs, composer, band resolver |
| `DashboardExecutiveDal` | Orchestrates four snapshot DALs + refresh log |
| `ExecutiveDashboardController` | Thin MediatR controller |

**Composition logic (`DashboardExecutiveComposer`):**

- Sales Achievement bands: ≥100% Healthy · 80–99% Warning · <80% Critical · null Unknown
- Piutang >90 Days from aging bucket `DaysOver90`
- Concentration ratios: Top Customer/Category/Supplier/Principal %
- Pending posting value from `BELUM` posting status row
- Top 5 exposure lists (truncated from Top 10 snapshots)
- `LastRefreshed` = `Min(GeneratedAt)` across domains
- `IsDataFresh` when all available domains are within configured intervals
- `OverallHealthStatus` via existing `DashboardSnapshotHealthStatusResolver`

### Frontend

| Component | Purpose |
| --------- | ------- |
| `DashboardHomeView.vue` | Rewritten as Management Attention Center (Proposal A layout) |
| `ExecutiveAttentionCard.vue` | Domain cards with band badges / attention indicator |
| `ExecutiveExposureSection.vue` | Top 5 ranking tables per exposure list |
| `ExecutiveDomainSummaryRow.vue` | Summary line + domain dashboard link |
| `MainLayout.vue` | Sidebar: Overview → **Executive** |
| `dashboardStore.loadExecutive()` | Home loads executive endpoint |

### Tests

| Test class | Coverage |
| ---------- | -------- |
| `ExecutiveSalesAchievementBandResolverTest` | Band boundaries (null, 79.9, 80, 99.9, 100, 150) |
| `DashboardExecutiveComposerTest` | Full/partial snapshots, ratios, truncation, freshness |

---

## Files Added

### Application

- `btr.application/ReportingContext/DashboardExecutiveAgg/Contracts/IDashboardExecutiveDal.cs`
- `btr.application/ReportingContext/DashboardExecutiveAgg/Queries/GetDashboardExecutiveQuery.cs`
- `btr.application/ReportingContext/DashboardExecutiveAgg/Services/DashboardExecutiveComposer.cs`
- `btr.application/ReportingContext/DashboardExecutiveAgg/Services/ExecutiveSalesAchievementBandResolver.cs`

### Infrastructure

- `btr.infrastructure/ReportingContext/DashboardExecutiveAgg/DashboardExecutiveDal.cs`

### API

- `btr.portal.api/Controllers/Dashboard/ExecutiveDashboardController.cs`

### Frontend

- `btr.portal.web/src/components/dashboard/ExecutiveAttentionCard.vue`
- `btr.portal.web/src/components/dashboard/ExecutiveExposureSection.vue`
- `btr.portal.web/src/components/dashboard/ExecutiveDomainSummaryRow.vue`

### Tests

- `btr.test/ReportingContext/ExecutiveSalesAchievementBandResolverTest.cs`
- `btr.test/ReportingContext/DashboardExecutiveComposerTest.cs`

---

## Files Modified

| File | Change |
| ---- | ------ |
| `btr.application/btr.application.csproj` | Compile includes for DashboardExecutiveAgg |
| `btr.infrastructure/btr.infrastructure.csproj` | DashboardExecutiveDal compile include |
| `btr.portal.api/btr.portal.api.csproj` | ExecutiveDashboardController compile include |
| `btr.portal.api/Configurations/InfrastructurePortalExtensions.cs` | DI registration |
| `btr.portal.api/Configurations/PortalPresentationExtensions.cs` | Controller registration |
| `btr.test/btr.test.csproj` | Test compile includes |
| `btr.portal.web/src/models/dashboard.ts` | Executive DTO types |
| `btr.portal.web/src/api/dashboardApi.ts` | `fetchDashboardExecutive()` |
| `btr.portal.web/src/stores/dashboardStore.ts` | `executive` ref + `loadExecutive()` |
| `btr.portal.web/src/views/dashboard/DashboardHomeView.vue` | Management Attention Center layout |
| `btr.portal.web/src/layouts/MainLayout.vue` | Executive sidebar label |
| `docs/features/btr-portal/btr-portal-operational.md` | Executive user guide |
| `docs/features/btr-portal/btr-portal-domain.md` | Executive composition layer |
| `docs/features/btr-portal/btr-portal-architecture.md` | Executive API and routing |
| `docs/work/btr-portal/M16 Executive Dashboard - Analysis.md` | Implementation link |

---

## Unchanged (Per Plan)

- Snapshot workers, aggregators, `BTR_PortalDashboard*` tables
- `GET /api/dashboard/overview` (retained; home no longer calls it)
- Domain dashboard APIs and views (M13–M15)
- Report APIs, DALs, and views
- Refresh cadence and health endpoint behavior

---

## Verification

### Automated

| Check | Result |
| ----- | ------ |
| Vue frontend build (`npm run build`) | **Pass** |
| `ExecutiveSalesAchievementBandResolverTest` | Written — run via Visual Studio / `dotnet test` |
| `DashboardExecutiveComposerTest` | Written — run via Visual Studio / `dotnet test` |

### Manual Checklist (Section 8.3)

| # | Criterion | Notes |
| - | --------- | ----- |
| 1 | Login → Management Attention Center at `/dashboard` | Implemented |
| 2 | Sidebar shows **Executive** | Implemented |
| 3 | Sales Achievement % band badges | Healthy/Warning/Critical/Unknown styling |
| 4 | Piutang/Purchasing match domain dashboards | Same snapshot source |
| 5 | Top 5 lists match domain Top 10 first five rows | Server-side truncation |
| 6 | Staleness banner when stale | `!IsDataFresh` → "⚠ Dashboard Data Not Fresh" |
| 7 | Single Last Refreshed in header | No per-domain timestamps on executive page |
| 8 | Links to domain dashboards only | No report links on executive page |
| 9 | Weekly trends absent from executive | Charts remain on domain dashboards |
| 10 | Volume totals absent from executive | Total Faktur/Customer/Item/Invoice removed |
| 11 | Domain dashboards and reports unchanged | No modifications to detail/report layers |

---

## Known Limitations

- **Piutang semantics:** Executive and Piutang Dashboard show all-time open balance; Piutang Report defaults to current month on Jatuh Tempo — footer totals differ unless report scope encompasses all open items.
- **Last Refreshed:** Uses `Min(GeneratedAt)` — conservative; reflects oldest domain data visible on screen.
- **Top Principal %:** Denominator is `GrandTotalPurchase` from purchasing KPI — not displayed on purchasing attention card.

---

## Out of Scope (Deferred)

Slow-moving/dead stock (M17), collection effectiveness, pipeline omzet, Faktur Kembali aggregate, Sales Omzet Health, weekly trends on executive page, mixed-domain risk table, generic severity engine, role-based routing, chart drilldown.

---

## Acceptance Criteria

All 12 acceptance criteria from the implementation plan are satisfied by this delivery.
