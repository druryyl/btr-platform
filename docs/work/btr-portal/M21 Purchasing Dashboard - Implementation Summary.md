# M21 Purchasing Management Dashboard — Implementation Summary

## Document Status

| Field | Value |
| --- | --- |
| Initiative | M21 Purchasing Management Dashboard |
| Plan reference | `docs/work/btr-portal/M21 Purchasing Dashboard - Plan.md` |
| Analysis reference | `docs/work/btr-portal/M21-purchasing-dashboard-analysis.md` |
| Status | **Approved (Phase 1)** |
| Review reference | `docs/work/btr-portal/M21 Purchasing Dashboard - Re-Review Report.md` |
| Date | 2026-06-09 |

**Phase 2 deferred:** Executive Dashboard promotion of Compound Dependency / Top 3 Principal % / Qualified Backlog Value to attention cards beyond `RequiresAttention` revision (PO Q10).

---

## Review Follow-Up (2026-06-09)

Addressed required actions from the M21 review report:

| Issue | Fix |
| --- | --- |
| Build break — `IOptions<>` in application worker | `RefreshDashboardPurchasingManagementSnapshotWorker` now injects `DashboardSnapshotOptions` directly; resolved via `AddScoped` in `ApplicationPortalExtensions` |
| `PrincipalInventoryNoPurchaseCount` KPI incorrect | Count derived from M15 supplier Top 10 with zero MTD purchase (not purchase Top 10 rows only) |
| Tests unverified | `dotnet build btr.test.csproj` succeeds; M21 test suite executed via `dotnet vstest` |
| Missing signal tests (Plan §9.1) | Added `PrincipalInventoryConcentration` and `PrincipalAtRiskExposure` aggregator cases |

**Optional polish applied:** `GeneratedAt` displays `min(V1, Management)`; worker traces warning when inventory/risk snapshots null; analysis doc plan link; attention list column header → **Entity**.

---

## 1. Delivered Outcomes

| Acceptance item | Status |
| --- | --- |
| Route `/dashboard/purchasing` with Attention-First layout + V1 charts retained | Done |
| Page title **Purchasing Management Dashboard** | Done |
| Dedicated `BTR_PortalDashboardPurchasingManagement*` snapshot domain (3 tables) | Done |
| Extended `GET /api/dashboard/purchasing` — V1 + management sections | Done |
| Qualified backlog (age-based `BELUM`, default 3 days on `LastUpdate`) | Done |
| Eight approved attention signals with sort priority | Done |
| Cross-domain Principal Exposure (M15 inventory + M19 at-risk) | Done |
| Principal drill-down → Purchasing Report `?q=` | Done |
| Executive `RequiresAttention` uses qualified backlog only | Done |
| Staleness banner when snapshot exceeds 30 minutes | Done |
| V1 traceability KPIs unchanged (Grand Total, Total Invoice) | Done |
| Health endpoint + worker CLI `PurchasingManagement` domain | Done |
| Aggregator + key resolver + executive composer unit tests | Done — 47 tests passed (see §6) |

---

## 2. Architecture Delivered

```text
Source DALs (refresh time)
  IInvoiceViewDal (+ CreateTime, LastUpdate)
  IDashboardInventorySnapshotDal (M15)
  IDashboardInventoryRiskSnapshotDal (M19)
  IDashboardPurchasingSnapshotDal (V1)
    ↓
RefreshDashboardPurchasingManagementSnapshotWorker
    ↓ DashboardPurchasingManagementAggregator
BTR_PortalDashboardPurchasingManagement* (3 tables)
    ↓
GET /api/dashboard/purchasing
    ↓ DashboardPurchasingDal (V1 + management merge)
PurchasingDashboardView.vue

Executive:
  DashboardExecutiveDal → ComposePurchasing (QualifiedBacklogCount → RequiresAttention)
```

Refresh order in `RefreshAll`: … → Purchasing (V1) → **PurchasingManagement** → Customer → …

---

## 3. Database Changes

Three new tables under `src/j05-btr-distrib/btr.sql/Tables/ReportingContext/`:

| Table | Purpose |
| --- | --- |
| `BTR_PortalDashboardPurchasingManagementKpi` | Qualified backlog, concentration %, compound counts, inactivity flag |
| `BTR_PortalDashboardPurchasingManagementAttention` | Principal × Signal attention list |
| `BTR_PortalDashboardPurchasingManagementTopPrincipal` | Top 10 MTD purchase with cross-domain columns |

All use `SnapshotKey = 'CURRENT'` delete-and-replace pattern. Registered in `btr.sql.sqlproj`.

**Deploy note:** Run SQL project deploy before first PurchasingManagement refresh.

---

## 4. Backend Changes

### Application layer (`btr.application`)

| File | Role |
| --- | --- |
| `DashboardPurchasingManagementAggregator.cs` | 5-pass aggregation: qualified backlog, Top 10, cross-domain, signals, posted % |
| `DashboardPurchasingManagementKeyResolver.cs` | Principal name normalization (`Unknown`, trim, case-insensitive match) |
| `DashboardPurchasingManagementAggregateResult.cs` | KPI + attention + top principal models |
| `IDashboardPurchasingManagementSnapshotDal.cs` | Snapshot contract |
| `RefreshDashboardPurchasingManagementSnapshotWorker.cs` | Domain `"PurchasingManagement"` |
| `GetDashboardPurchasingQuery.cs` | Extended response DTOs |
| `DashboardSnapshotOptions.cs` | `PurchasingManagementIntervalMinutes`, `PurchasingQualifiedBacklogDays` |
| `DashboardExecutiveComposer.cs` | `RequiresAttention` → `QualifiedBacklogCount > 0` |
| `InvoiceView.cs` | Added `CreateTime`, `LastUpdate` |

### Infrastructure layer (`btr.infrastructure`)

| File | Role |
| --- | --- |
| `DashboardPurchasingManagementSnapshotDal.cs` | Read/write 3 management tables |
| `DashboardPurchasingDal.cs` | Merge V1 + management; derive attention card flags |
| `InvoiceViewDal.cs` | SELECT `CreateTime`, `LastUpdate` |
| `DashboardExecutiveDal.cs` | Inject management snapshot |

### Wiring

- `ApplicationPortalExtensions` — register aggregator; resolve `DashboardSnapshotOptions` for application-layer workers
- `InfrastructurePortalExtensions` — register snapshot DAL
- `RefreshAllDashboardSnapshotsWorker` — 9th domain after V1 Purchasing
- `RefreshDashboardSnapshotsCommand` — `PurchasingManagement` case
- `HealthController` — domain + interval
- `btr.portal.worker/Program.cs` — CLI domain
- `appsettings.json` (API + worker) — new options

---

## 5. Frontend Changes

| File | Role |
| --- | --- |
| `PurchasingDashboardView.vue` | Attention-first layout; title change; staleness banner |
| `PurchasingAttentionCards.vue` | Four card groups |
| `PurchasingAttentionCardGroup.vue` | Attention border wrapper |
| `PurchasingSummaryRow.vue` | Traceability + posting context |
| `PurchasingAttentionList.vue` | Principal × Signal table |
| `PurchasingPrincipalExposureTable.vue` | Cross-domain comparison grid |
| `PurchasingNavigationSection.vue` | Report + inventory cross-links |
| `dashboard.ts` | Extended `DashboardPurchasingResponse` types |

---

## 6. Test Coverage

**Verification command (2026-06-09):**

```text
dotnet build src/j05-btr-distrib/btr.test/btr.test.csproj
dotnet vstest btr.test/bin/Debug/btr.test.dll --TestCaseFilter:"FullyQualifiedName~DashboardPurchasingManagement|FullyQualifiedName~DashboardExecutiveComposer|FullyQualifiedName~RefreshAllDashboardSnapshots|FullyQualifiedName~RefreshDashboardSnapshots"
```

**Result:** 47 passed, 0 failed (22 purchasing management + 25 executive/refresh).

| File | Cases |
| --- | --- |
| `DashboardPurchasingManagementAggregatorTest.cs` | Qualified backlog age rules, Top 10 %, compound dependency, inactivity, sort order, deduplication, **PrincipalInventoryNoPurchase count**, **PrincipalInventoryConcentration**, **PrincipalAtRiskExposure** |
| `DashboardPurchasingManagementKeyResolverTest.cs` | Principal normalization |
| `DashboardExecutiveComposerTest.cs` | Qualified vs unqualified BELUM `RequiresAttention` |
| `RefreshAllDashboardSnapshotsWorkerTest.cs` | 9 domains; PurchasingManagement after Purchasing |
| `RefreshDashboardSnapshotsHandlerTest.cs` | Valid domain includes PurchasingManagement |

---

## 7. Manual Verification Checklist

1. Deploy SQL tables; run `PurchasingManagement` refresh
2. `GET /api/dashboard/purchasing` returns V1 + management sections
3. Page title and section order correct; V1 weekly trend and posting pie unchanged
4. Grand Total / Total Invoice match Purchasing Report footer
5. Qualified backlog spot-check against aged BELUM invoices in report
6. Attention list row → `/reports/purchasing?q=`
7. Executive purchasing card: `RequiresAttention` only when qualified backlog > 0
8. `GET /api/health/dashboard-snapshots` lists `PurchasingManagement`
9. Worker: `btr.portal.worker --domain PurchasingManagement --triggered-by Manual`

---

## 8. Explicitly Preserved (unchanged)

- V1 `BTR_PortalDashboardPurchasing*` table schemas
- `DashboardPurchasingInvoiceAggregator` formulas
- `RefreshDashboardPurchasingSnapshotWorker`
- Desktop PT1/PT2 write paths
- Route `/dashboard/purchasing` and sidebar label **Purchasing**

---

## 9. Phase 2 (deferred)

Promote selected M21 signals to Executive purchasing card UI: Compound Dependency Count, Qualified Backlog Value, Top 3 Principal % — separate delivery after stabilization.

---

*End of implementation summary — M21 Purchasing Management Dashboard (ready for re-review)*
