# M19 Slow Moving & Dead Stock — Implementation Summary

| Field | Value |
| --- | --- |
| Initiative | M19 Slow Moving & Dead Stock Dashboard |
| Plan | `docs/work/btr-portal/M19 Slow Moving & Dead Stock - Plan.md` |
| Status | **Phase 1 complete** (Executive integration deferred to Phase 2 per plan) |
| Implemented | 2026-06-08 |

---

## 1. Delivered Scope

Phase 1 delivers the **Slow Moving & Dead Stock Dashboard** at `/dashboard/inventory-risk` with a dedicated **InventoryRisk** materialized snapshot domain. Executive Dashboard promotion (Dead Stock Value, At-Risk %) is **not included** — deferred to Phase 2 per PO Q2.

### User-facing outcomes

- New route `/dashboard/inventory-risk` — page title **Slow Moving & Dead Stock Dashboard**
- Sidebar entry **Dashboard → Inventory Risk** (after Inventory)
- Proposal A layout: Attention Cards → Aging Distribution → Category/Supplier Risk Exposure → Attention List → Top 10 Rankings → Navigation
- Headline KPIs: Dead Stock Item Count/Value, Slow Moving Item Count/Value, At-Risk Inventory %
- Attention list with signals: Dead Stock · Slow Moving · Never Sold (mutually exclusive)
- Item row click → Inventory Report with `?q=` pre-filter (item name)
- Staleness banner when snapshot exceeds 60-minute interval
- Existing Inventory Dashboard (`/dashboard/inventory`) unchanged

---

## 2. Architecture Delivered

```text
IStokBalanceViewDal + IBrgLastFakturDal
    ↓ RefreshDashboardInventoryRiskSnapshotWorker
DashboardInventoryRiskAggregator (shared BuildItemGroups via DashboardInventoryItemGroupBuilder)
    ↓ ReplaceCurrent (transactional)
BTR_PortalDashboardInventoryRisk* (6 tables)
    ↓ GET /api/dashboard/inventory-risk
DashboardInventoryRiskDal → InventoryRiskDashboardView.vue
```

### Classification rules (implemented)

| Class | Rule |
| --- | --- |
| Never Sold | Qty > 0; no non-void FakturItem history |
| Slow Moving | Qty > 0; LastFakturDate idle 90–179 days |
| Dead Stock | Qty > 0; LastFakturDate idle ≥ 180 days |
| Active | LastFakturDate within last 89 days (excluded from at-risk KPIs) |

- Authoritative signal: `MAX(FakturDate)` per `BrgId` from gross Faktur only
- BrgId-first aggregation; In-Transit warehouse excluded; valuation = Hpp × Qty
- At-Risk Inventory % = (NeverSold + Slow + Dead value) / TotalInventoryValue

---

## 3. Files Changed / Added

### Database (6 new tables)

| Table | Purpose |
| --- | --- |
| `BTR_PortalDashboardInventoryRiskKpi` | Headline KPIs + RequiresAttention |
| `BTR_PortalDashboardInventoryRiskAging` | Four aging buckets (Active, Slow, Dead, Never Sold) |
| `BTR_PortalDashboardInventoryRiskAttention` | Item × signal attention list |
| `BTR_PortalDashboardInventoryRiskTopDead` | Top 10 dead stock by value |
| `BTR_PortalDashboardInventoryRiskTopSlow` | Top 10 slow moving by value |
| `BTR_PortalDashboardInventoryRiskBreakdown` | Category/Supplier at-risk exposure (top 10 each) |

Scripts: `src/j05-btr-distrib/btr.sql/Tables/ReportingContext/BTR_PortalDashboardInventoryRisk*.sql`

### Backend — Application

| File | Role |
| --- | --- |
| `DashboardInventoryItemGroupBuilder.cs` | Shared BrgId-first item grouping (extracted from M15) |
| `DashboardInventoryRiskAggregator.cs` | Risk classification, KPIs, aging, breakdowns, rankings |
| `DashboardInventoryRiskAggregateResult.cs` | Snapshot aggregate models |
| `IBrgLastFakturDal.cs` | Last Faktur per item contract |
| `IDashboardInventoryRiskSnapshotDal.cs` | Snapshot write/read contract |
| `RefreshDashboardInventoryRiskSnapshotWorker.cs` | Domain = `InventoryRisk` refresh worker |
| `GetDashboardInventoryRiskQuery.cs` | MediatR read path + API DTOs |
| `IDashboardInventoryRiskDal.cs` | Read-side contract |

Refactored: `DashboardInventoryAggregator.cs` — now delegates to shared item group builder.

### Backend — Infrastructure

| File | Role |
| --- | --- |
| `BrgLastFakturDal.cs` | SQL: MAX(FakturDate) per BrgId |
| `DashboardInventoryRiskSnapshotDal.cs` | GetCurrent + ReplaceCurrent (6-table transactional) |
| `DashboardInventoryRiskDal.cs` | IsAvailable, IsDataFresh, response mapping |

### Backend — API / Worker / Config

| File | Change |
| --- | --- |
| `InventoryRiskDashboardController.cs` | `GET /api/dashboard/inventory-risk` |
| `HealthController.cs` | Added `InventoryRisk` domain |
| `RefreshAllDashboardSnapshotsWorker.cs` | InventoryRisk after Inventory |
| `RefreshDashboardSnapshotsCommand.cs` | Single-domain refresh for InventoryRisk |
| `DashboardSnapshotOptions.cs` | `InventoryRiskIntervalMinutes = 60` |
| `ApplicationPortalExtensions.cs` | Register `DashboardInventoryRiskAggregator` |
| `InfrastructurePortalExtensions.cs` | Register new DALs |
| `btr.portal.worker/Program.cs` | `--domain InventoryRisk` |
| `appsettings.json` (api + worker) | `InventoryRiskIntervalMinutes` |

### Frontend

| File | Role |
| --- | --- |
| `InventoryRiskDashboardView.vue` | Main dashboard view (Proposal A layout) |
| `InventoryRiskAttentionList.vue` | Attention list DataTable |
| `InventoryRiskNavigationSection.vue` | Links to Inventory Dashboard + Report |
| `AgingPieChart.vue` | Extended with optional title/emptyMessage + inventory bucket colors |
| `router/index.ts` | Route `dashboard/inventory-risk` |
| `MainLayout.vue` | Sidebar **Inventory Risk** |
| `dashboard.ts` / `dashboardApi.ts` / `dashboardStore.ts` | Types, API, `loadInventoryRisk()` |
| `InventoryReportView.vue` | Reads `?q=` on mount for drill-down pre-filter |

### Tests

| File | Coverage |
| --- | --- |
| `DashboardInventoryRiskAggregatorTest.cs` | 12 tests: parity, boundaries, mutual exclusivity, at-risk math, In-Transit, Top 10 cap |
| `RefreshAllDashboardSnapshotsWorkerTest.cs` | Updated for InventoryRisk domain order |
| `RefreshDashboardSnapshotsHandlerTest.cs` | Updated constructor for InventoryRisk worker |

---

## 4. API Contract

**Endpoint:** `GET /api/dashboard/inventory-risk`  
**Auth:** JWT required  
**Response:** `ApiResponse<DashboardInventoryRiskResponse>`

Key fields: `IsAvailable`, `IsDataFresh`, `GeneratedAt`, `AttentionCards`, `AgingBuckets`, `CategoryRiskExposure`, `SupplierRiskExposure`, `AttentionList`, `Rankings.TopDead/TopSlow`, `Navigation`.

**Manual refresh:** `POST /api/admin/dashboard/refresh` with `{ "Domain": "InventoryRisk" }`  
**Worker CLI:** `btr.portal.worker --domain InventoryRisk --triggered-by Manual`

---

## 5. Verification Performed

| Check | Result |
| --- | --- |
| `btr.application` + `btr.infrastructure` + `btr.test` build (Release) | Pass |
| `DashboardInventoryRiskAggregatorTest` (12 cases) | Pass |
| Updated refresh worker/handler tests compile | Pass |
| Vue frontend `npm run build` | Pass |
| Full solution build via dotnet CLI | Partial — pre-existing VS/SSDT tooling gaps for `btr.portal.api`, `btr.sql`, `btr.distrib` (environment-specific) |

---

## 6. Deployment Checklist

1. Deploy six `BTR_PortalDashboardInventoryRisk*` tables to target database
2. Run initial refresh: `btr.portal.worker --domain InventoryRisk --triggered-by Manual`
3. Verify `GET /api/health/dashboard-snapshots` lists InventoryRisk domain
4. Navigate to `/dashboard/inventory-risk` and confirm all sections render
5. Spot-check `TotalInventoryValue` reconciles with Inventory Dashboard
6. Add Task Scheduler job for InventoryRisk domain (60-minute cadence)
7. **Phase 2 (separate delivery):** Executive Dashboard inventory risk KPI promotion

---

## 7. Out of Scope (Confirmed Unchanged)

- Executive Dashboard inventory section (Phase 2)
- Existing `BTR_PortalDashboardInventory*` tables and `GET /api/dashboard/inventory`
- Salesman dimension, ABC, warehouse breakdown, export, Kartu Stok drill-down
- Retur-adjusted demand, mutasi-based movement classification

---

## 8. Documentation Follow-up

Permanent feature docs (`docs/features/btr-portal/*`, `materialized-dashboard-domain.md`) should be updated in a separate docs pass per plan Section 11. This implementation summary serves as the handoff artifact for that update.

---

*End of M19 Phase 1 Implementation Summary*
