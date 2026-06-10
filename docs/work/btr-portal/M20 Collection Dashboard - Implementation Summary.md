# M20 Collection Dashboard — Implementation Summary

## Document Status

| Field | Value |
| --- | --- |
| Initiative | M20 Collection Dashboard |
| Plan reference | `docs/work/btr-portal/M20 Collection Dashboard - Plan.md` |
| Analysis reference | `docs/work/btr-portal/M20-collection-dashboard-analysist.md` |
| Status | **Implemented (Phase 1)** |
| Date | 2026-06-09 |

**Phase 2 deferred:** Executive Dashboard promotion (Cash Collected MTD, Recovery vs Billing %, Overdue Concentration %) — separate delivery per PO Q25.

---

## 1. Delivered Outcomes

| Acceptance item | Status |
| --- | --- |
| Route `/dashboard/collection` with Attention-First + Recovery Summary layout | Done |
| Sidebar **Dashboard → Collection** (after Salesmen, before Inventory) | Done |
| Dedicated `BTRPD_Collection*` snapshot domain (6 tables) | Done |
| `GET /api/dashboard/collection` read API | Done |
| Recovery KPIs: Cash Collected MTD, Recovery vs Billing %, Payment Mix | Done |
| Overdue-only aging (4 buckets), Top 10 overdue rankings (customer/salesman/wilayah) | Done |
| Seven approved attention signals with priority suppression | Done |
| Customer/Salesman drill-down → Piutang Report `?q=`; Wilayah display-only | Done |
| Staleness banner when snapshot exceeds 30 minutes | Done |
| Piutang / Customer / Salesman / Executive dashboards unchanged | Done |
| Aggregator + key resolver unit tests | Done |
| Health endpoint includes Collection domain | Done |
| Worker CLI `--domain Collection` | Done |

---

## 2. Architecture Delivered

```text
Source DALs (refresh time)
  IPiutangOpenBalanceDal
  IPiutangOpenBalanceWithSalesmanDal
  IPiutangOpenBalanceWithWilayahDal (NEW)
  IPenerimaanPelunasanSalesDal (+ SalesPersonId)
  IFakturViewDal, ICustomerLastFakturDal, ICustomerDal, ISalesPersonDal
    ↓
RefreshDashboardCollectionSnapshotWorker
    ↓ DashboardCollectionAggregator
BTRPD_Collection* (6 tables)
    ↓
GET /api/dashboard/collection
    ↓ DashboardCollectionDal
CollectionDashboardView.vue
```

Refresh order in `RefreshAll`: … → Customer → Salesman → **Collection** (last).

---

## 3. Database Changes

Six new tables under `src/j05-btr-distrib/btr.sql/Tables/ReportingContext/`:

| Table | Purpose |
| --- | --- |
| `BTRPD_CollectionKpi` | Exposure, recovery, portfolio counts |
| `BTRPD_CollectionAging` | Overdue-only 4-bucket aging |
| `BTRPD_CollectionAttention` | Entity × signal attention list |
| `BTRPD_CollectionTopOverdueCustomer` | Top 10 overdue customers |
| `BTRPD_CollectionTopOverdueSalesman` | Top 10 overdue salesmen |
| `BTRPD_CollectionTopOverdueWilayah` | Top 10 overdue wilayah |

All use `SnapshotKey = 'CURRENT'` delete-and-replace pattern. Registered in `btr.sql.sqlproj`.

**Deploy note:** Run SQL project deploy before first Collection refresh.

---

## 4. Backend Changes

### Application layer

| Component | Path |
| --- | --- |
| `DashboardCollectionAggregator` | `btr.application/.../Services/DashboardCollectionAggregator.cs` |
| `DashboardCollectionKeyResolver` | `btr.application/.../Services/DashboardCollectionKeyResolver.cs` |
| `DashboardCollectionAggregateResult` | `btr.application/.../Models/DashboardCollectionAggregateResult.cs` |
| `IDashboardCollectionSnapshotDal` | `btr.application/.../Contracts/` |
| `IPiutangOpenBalanceWithWilayahDal` | `btr.application/.../Contracts/` |
| `RefreshDashboardCollectionSnapshotWorker` | `btr.application/.../UseCases/` |
| `GetDashboardCollectionQuery` | `btr.application/.../DashboardCollectionAgg/Queries/` |

### Infrastructure layer

| Component | Path |
| --- | --- |
| `DashboardCollectionSnapshotDal` | `btr.infrastructure/.../DashboardSnapshotAgg/` |
| `PiutangOpenBalanceWithWilayahDal` | `btr.infrastructure/.../DashboardSnapshotAgg/` |
| `DashboardCollectionDal` | `btr.infrastructure/.../DashboardCollectionAgg/` |
| `PenerimaanPelunasanSalesDal` | Extended with `SalesPersonId` in SELECT/GROUP BY |

### API & orchestration

| Component | Change |
| --- | --- |
| `CollectionDashboardController` | `GET /api/dashboard/collection` |
| `DashboardSnapshotOptions` | `CollectionIntervalMinutes = 30` |
| `RefreshAllDashboardSnapshotsWorker` | Collection last |
| `RefreshDashboardSnapshotsCommand` | `"Collection"` domain |
| `HealthController` | Collection in domain list |
| `DashboardSnapshotRefreshLogDal` | `'Collection'` in `GetLatestPerDomain` filter |
| `btr.portal.worker/Program.cs` | `Collection` in ValidDomains |

---

## 5. Frontend Changes

| File | Role |
| --- | --- |
| `router/index.ts` | Route `dashboard/collection` |
| `MainLayout.vue` | Sidebar Collection item |
| `models/dashboard.ts` | `DashboardCollectionResponse` types |
| `api/dashboardApi.ts` | `fetchDashboardCollection()` |
| `stores/dashboardStore.ts` | `collection` + `loadCollection()` |
| `CollectionDashboardView.vue` | Page shell, 8 sections fixed order |
| `CollectionAttentionCardGroup.vue` | Exposure / Recovery / Portfolio cards |
| `CollectionRecoverySummary.vue` | Recovery KPIs + payment mix bar |
| `CollectionAgingRiskSummary.vue` | Overdue-only aging pie |
| `CollectionAttentionList.vue` | Multi-entity attention table |
| `CollectionNavigationSection.vue` | Cross-dashboard links |

---

## 6. Tests

| Test class | Coverage |
| --- | --- |
| `DashboardCollectionKeyResolverTest` | Customer code-first, salesman id, wilayah normalization |
| `DashboardCollectionAggregatorTest` | Exposure, aging, recovery, payment mix, rankings, all signals, WilayahHotspot threshold, zero omzet, blank WilayahId |
| `RefreshAllDashboardSnapshotsWorkerTest` | Collection runs last (8 domains) |
| `RefreshDashboardSnapshotsHandlerTest` | Updated for Collection worker dependency |

**Build verification:** `dotnet build btr.test/btr.test.csproj` — succeeded. Collection filter tests — passed.

---

## 7. Documentation Updated

| Document | Updates |
| --- | --- |
| `docs/features/btr-portal/btr-portal-operational.md` | M20 section, routes, signals, drill-down |
| `docs/features/btr-portal/btr-portal-domain.md` | Collection KPI definitions |
| `docs/features/btr-portal/btr-portal-architecture.md` | API route, frontend route |
| `docs/features/materialized-dashboard/materialized-dashboard-domain.md` | Collection domain, refresh order, tables |

---

## 8. Configuration

Added to `DashboardSnapshot` section:

- `CollectionIntervalMinutes: 30` in `btr.portal.api/appsettings.json` and `btr.portal.worker/appsettings.json`

---

## 9. Manual Verification Checklist

| # | Check | Notes |
| --- | --- | --- |
| 1 | Deploy 6 SQL tables | Required before refresh |
| 2 | `btr.portal.worker --domain Collection --triggered-by Manual` | After SQL deploy |
| 3 | `GET /api/dashboard/collection` | 8-section response |
| 4 | `/dashboard/collection` UI | Fixed section order; sidebar placement |
| 5 | FF2 Cash Collected MTD reconciliation | Spot-check current month |
| 6 | Top Overdue Customers ≠ Piutang Top 10 | When non-overdue balance exists |
| 7 | Row click → Piutang Report `?q=` | Customer/Salesman only |
| 8 | Staleness banner after 30+ min | `IsDataFresh = false` |
| 9 | Executive dashboard unchanged | Phase 2 not included |
| 10 | Health endpoint Collection domain | `GET /api/health/dashboard-snapshots` |
| 11 | Admin refresh `{ "Domain": "Collection" }` | `POST /api/admin/dashboard/refresh` |

---

## 10. Out of Scope (Confirmed)

- Executive Dashboard recovery KPI promotion (Phase 2)
- Tagihan pipeline KPIs
- Event-driven refresh after pelunasan
- Wilayah drill-down (Piutang Report has no wilayah filter)
- Changes to existing Piutang / Customer / Salesman snapshot domains

---

*End of M20 Collection Dashboard implementation summary*
