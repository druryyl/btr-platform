# M18 Salesman Performance — Implementation Summary

## Document Status

| Field | Value |
| --- | --- |
| Initiative | M18 Salesman Performance |
| Plan reference | `docs/work/btr-portal/M18 Salesman Performance - Plan.md` |
| Analysis reference | `docs/work/btr-portal/M18 Salesman Performance - Analysis.md` |
| Status | **Implemented** |
| Date | 2026-06-08 |

---

## 1. Delivered Outcomes

M18 delivers a dedicated **Salesman Performance** dashboard at `/dashboard/salesmen` answering *Which salesman requires management attention and why?*

| Acceptance item | Status |
| --- | --- |
| Route `/dashboard/salesmen` with Proposal A layout | Done |
| Sidebar **Dashboard → Salesmen** (after Customers, before Inventory) | Done |
| Dedicated `BTRPD_Salesman*` snapshot domain (6 tables) | Done |
| `GET /api/dashboard/salesmen` read API | Done |
| Attention cards, list, Top 10 Omzet/Achievement/Piutang, segmentation | Done |
| Six approved attention signals | Done |
| M16 achievement bands (80/100 thresholds) | Done |
| Current-month Faktur omzet + all-time open piutang | Done |
| Dormant customers via last-invoicing salesman (90-day rule) | Done |
| Row click → report with `?q=` salesman name pre-filter | Done |
| Staleness banner when snapshot exceeds 30 minutes | Done |
| Executive dashboard and Sales Top 10 unchanged | Done (no changes to those modules) |
| Aggregator + key resolver unit tests | Done |
| Health endpoint includes Salesman domain | Done |

---

## 2. Architecture Delivered

```text
Source DALs (refresh time)
  FakturViewDal (+ SalesPersonId, SalesPersonCode)
  PiutangOpenBalanceWithSalesmanDal (FF1 join)
  CustomerLastFakturDal.ListLastFakturWithSalesmanByCustomer()
  ISalesPersonDal + ISalesOmzetTargetDal.ListTargetsForMonth()
    ↓
RefreshDashboardSalesmanSnapshotWorker
    ↓ DashboardSalesmanAggregator
BTRPD_Salesman* (6 tables)
    ↓
GET /api/dashboard/salesmen
    ↓ DashboardSalesmanDal
SalesmanDashboardView.vue
```

Refresh order in `RefreshAll`: Piutang → Inventory → Sales → Purchasing → Customer → **Salesman** (last).

---

## 3. Database Changes

Six new tables under `src/j05-btr-distrib/btr.sql/Tables/ReportingContext/`:

| Table | Purpose |
| --- | --- |
| `BTRPD_SalesmanKpi` | Headline KPIs + attention card counts |
| `BTRPD_SalesmanTopOmzet` | Top 10 omzet ranking |
| `BTRPD_SalesmanTopAchievement` | Top 10 achievement % ranking |
| `BTRPD_SalesmanTopPiutang` | Top 10 piutang ranking |
| `BTRPD_SalesmanAttention` | Attention list rows |
| `BTRPD_SalesmanSegmentation` | Wilayah / Segment / Activity segmentation |

All use `SnapshotKey = 'CURRENT'` delete-and-replace pattern. Registered in `btr.sql.sqlproj`.

**Deploy note:** Run SQL project deploy (or apply scripts manually) before first refresh.

---

## 4. Backend Changes

### 4.1 Source DAL extensions

| File | Change |
| --- | --- |
| `FakturView.cs` | Added `SalesPersonId`, `SalesPersonCode` |
| `FakturViewDal.cs` | SELECT extended for both `ListData` and `ListTerhapus` |
| `ICustomerLastFakturDal.cs` | Added `ListLastFakturWithSalesmanByCustomer()` + DTO |
| `CustomerLastFakturDal.cs` | ROW_NUMBER SQL for last Faktur per customer with salesman |
| `ISalesOmzetTargetDal.cs` | Added `ListTargetsForMonth(year, month)` |
| `SalesOmzetTargetDal.cs` | Batch target map implementation |
| `IPiutangOpenBalanceWithSalesmanDal.cs` | New contract + DTO |
| `PiutangOpenBalanceWithSalesmanDal.cs` | FF1 piutang open balance with invoicing salesman |

### 4.2 Snapshot aggregation (new)

| Component | Path |
| --- | --- |
| `DashboardSalesmanAggregator` | `btr.application/.../Services/DashboardSalesmanAggregator.cs` |
| `DashboardSalesmanKeyResolver` | `btr.application/.../Services/DashboardSalesmanKeyResolver.cs` |
| `DashboardSalesmanAggregateResult` | `btr.application/.../Models/DashboardSalesmanAggregateResult.cs` |
| `IDashboardSalesmanSnapshotDal` | `btr.application/.../Contracts/` |
| `DashboardSalesmanSnapshotDal` | `btr.infrastructure/.../DashboardSalesmanSnapshotDal.cs` |
| `RefreshDashboardSalesmanSnapshotWorker` | `btr.application/.../UseCases/` |

Counter prefix for child rows: **PDS** (per plan). Refresh log prefix: **PDR**.

### 4.3 Read API (new)

| Component | Path |
| --- | --- |
| `GetDashboardSalesmanQuery` + DTOs | `btr.application/.../DashboardSalesmanAgg/Queries/` |
| `IDashboardSalesmanDal` | `btr.application/.../DashboardSalesmanAgg/Contracts/` |
| `DashboardSalesmanDal` | `btr.infrastructure/.../DashboardSalesmanAgg/` |
| `SalesmanDashboardController` | `btr.portal.api/Controllers/Dashboard/` — `GET /api/dashboard/salesmen` |

### 4.4 Configuration and wiring

| Location | Change |
| --- | --- |
| `DashboardSnapshotOptions` | `SalesmanIntervalMinutes = 30` |
| `ApplicationPortalExtensions` | `DashboardSalesmanAggregator` registration |
| `InfrastructurePortalExtensions` | Snapshot DAL, read DAL, piutang-with-salesman DAL |
| `PortalPresentationExtensions` | `SalesmanDashboardController` |
| `RefreshAllDashboardSnapshotsWorker` | Salesman worker last |
| `RefreshDashboardSnapshotsCommand` | `"Salesman"` domain case |
| `HealthController` | Salesman in domain list + interval mapping |
| `btr.portal.worker/Program.cs` | `--domain Salesman` support |

---

## 5. Frontend Changes

| Item | Path |
| --- | --- |
| Route | `router/index.ts` — `dashboard/salesmen` |
| Sidebar | `layouts/MainLayout.vue` — **Salesmen** menu item |
| Page | `views/dashboard/SalesmanDashboardView.vue` |
| Components | `SalesmanAttentionCardGroup.vue`, `SalesmanAttentionList.vue`, `SalesmanSegmentationSection.vue`, `SalesmanNavigationSection.vue` |
| API / store / types | `dashboardApi.ts`, `dashboardStore.ts`, `models/dashboard.ts` |

**Layout (Proposal A):** Attention Cards → Attention List → Performance Rankings → Exposure Rankings → Segmentation → Navigation.

Row clicks and attention list report links use `navigateToReport(router, route, salesPersonName)` with `?q=` pre-filter (M17 pattern).

---

## 6. Tests

| Test file | Coverage |
| --- | --- |
| `DashboardSalesmanAggregatorTest.cs` | Rankings, signals, dormant, segmentation, multi-signal, concentration |
| `DashboardSalesmanKeyResolverTest.cs` | Id preference, name fallback map |
| `RefreshAllDashboardSnapshotsWorkerTest.cs` | Updated for 6-domain order including Salesman |
| `RefreshDashboardSnapshotsHandlerTest.cs` | Updated constructor + domain validation message |

Stub `ISalesOmzetTargetDal` implementations updated in existing tests for `ListTargetsForMonth`.

**Build verification:** `btr.application`, `btr.infrastructure`, and `btr.test` compile successfully.

---

## 7. Operational Steps (post-deploy)

1. Deploy six `BTRPD_Salesman*` tables to the target database.
2. Run initial refresh:
   - Worker: `btr.portal.worker --domain Salesman --triggered-by Manual`
   - Or API: `POST /api/admin/dashboard/refresh` with `{ "Domain": "Salesman" }`
3. Add Task Scheduler job: **BTR Portal Dashboard Salesman Refresh** every 30 minutes (documented in plan Section 10).
4. Verify `GET /api/health/dashboard-snapshots` shows Salesman domain.
5. Navigate to `/dashboard/salesmen` and confirm all six sections render.

---

## 8. Explicitly Not Changed (per plan)

- `DashboardExecutiveComposer` / executive dashboard
- `DashboardSalesFakturAggregator` / `BTRPD_SalesTopSalesman`
- Existing Sales/Piutang/Customer snapshot workers and read APIs
- Report DALs and API contracts
- BTR Desktop

---

## 9. Documentation Follow-up (Section 10 of plan)

Permanent feature docs were **not** updated in this delivery. Recommended post-merge updates:

- `docs/features/btr-portal/btr-portal-operational.md`
- `docs/features/btr-portal/btr-portal-domain.md`
- `docs/features/btr-portal/btr-portal-architecture.md`
- `docs/features/materialized-dashboard/materialized-dashboard-domain.md`

---

## 10. Known Reconciliation Notes

- Top 10 Omzet should reconcile with Sales Report grouped by salesman for current month.
- Top 10 Piutang should reconcile with Piutang Report FF1 totals by `SalesName`.
- Piutang drill-down uses report period defaults; dashboard uses all-open semantics — same caveat as M17.
- Piutang rows without invoicing salesman (blank `SalesPersonId`) are excluded from salesman aggregates.
