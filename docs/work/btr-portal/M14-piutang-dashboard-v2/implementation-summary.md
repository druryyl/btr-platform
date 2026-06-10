# Implementation Summary: M14 Piutang Dashboard V2

**Date:** 2026-06-10  
**Plan:** `docs/work/btr-portal/M14-piutang-dashboard-v2/M14-piutang-dashboard-v2-plan.md`  
**Review:** `docs/work/btr-portal/M14-piutang-dashboard-v2/M12-piutang-dashboard-v2-review.md`  
**Scope:** Piutang snapshot worker, API, portal `/dashboard/piutang`

---

## Remediation (2026-06-10)

Review rejection addressed:

| Finding | Resolution |
| ------- | ---------- |
| **Critical** — duplicate `DashboardPiutangTopCustomerRiskRow` types caused compile failure | Removed `DashboardPiutangAgg.Queries` import from aggregate layer. Added `DashboardPiutangAgingBucket` to `DashboardSnapshotAgg/Models`. API DAL maps aggregate models to API DTOs via type aliases. |
| **Major** — integration verification missing | Added `SnapshotRoundTrip_PreservesCustomerAgingAndTopRiskRowCounts` in `DashboardPiutangSnapshotVerificationTest` (in-memory snapshot DAL round-trip with row-count and KPI reconciliation assertions). |
| **Major** — tie-break not unit-tested | Added `Aggregate_TieBreaksEqualBalance_ByCustomerNameAscending` in `DashboardPiutangAggregatorTest`. |
| **Minor** — deploy doc stale | Updated `docs/ops/btr-portal-deploy.md` Piutang table list for V2. |
| **Major** — manual V1–V7 not evidenced | Checklist documented below; requires dev/staging environment (not executed in automated remediation pass). |

---

## Delivered

### Database

| Artifact | Purpose |
| -------- | ------- |
| `btr.sql/Tables/ReportingContext/BTRPD_PiutangKpi.sql` | Extended with Overdue Piutang, >90 amount/%, Top 10/20 concentration columns |
| `BTRPD_PiutangCustomerAging.sql` | Full per-customer aging snapshot (`CustomerId` key) |
| `BTRPD_PiutangTopCustomerRisk.sql` | Top 20 denormalized rows for dashboard table + Executive rank-1 |
| `Scripts/Upgrade_M14_PiutangDashboard_V2.sql` | Idempotent ALTER + CREATE for existing deployments |
| `Scripts/Create_BTRPD_PortalDashboard_Tables.sql` | Greenfield blocks updated |

### Backend

| File | Changes |
| ---- | ------- |
| `PiutangAgingBucketResolver.cs` | Shared 5-bucket resolver extracted from Piutang aggregator |
| `DashboardPiutangAgingBucket.cs` | Aggregate-layer aging bucket model (decoupled from API DTOs) |
| `PiutangOpenBalanceDto` + `PiutangOpenBalanceDal` | `CustomerId` on source rows |
| `DashboardPiutangAggregator` | Single-pass customer aging, Top 20 risk, concentration KPIs |
| `DashboardPiutangSnapshotDal` | Read/write new tables; stop `BTRPD_PiutangTopCustomer` |
| `GetDashboardPiutangQuery` / `DashboardPiutangDal` | V2 API fields; `TopCustomerRisk` replaces `TopCustomers` |
| `DashboardExecutiveComposer` | Rank-1 % and critical exposures from `TopCustomerRisk` |
| `DashboardAlertCenterComposer` | Top customer % from `TopCustomerRisk` rank 1 |
| `RefreshDashboardPiutangSnapshotWorker` | Logs skipped `CustomerId` count in progress detail |

### Frontend

| File | Changes |
| ---- | ------- |
| `dashboard.ts` | V2 response types |
| `PiutangDashboardView.vue` | 5 KPI tiles, concentration row, Top 20 risk table, updated subtitle |
| `PiutangCustomerRiskTable.vue` | New aging-breakdown DataTable with investigation drill-down |

### Tests

Extended: `DashboardPiutangAggregatorTest`, `DashboardPiutangSnapshotVerificationTest`, `DashboardPiutangDalTest`, `DashboardExecutiveComposerTest`, `DashboardAlertCenterComposerTest`.

### Documentation

| File | Purpose |
| ---- | ------- |
| `docs/features/btr-portal/btr-portal-domain.md` | Piutang dashboard section V2 |
| `docs/features/materialized-dashboard/materialized-dashboard-domain.md` | Piutang KPI rules + snapshot table list |
| `docs/ops/btr-portal-deploy.md` | Piutang V2 snapshot table list |

### Unchanged (by design)

- `GET /api/dashboard/overview` home card (Total Piutang, Total Customer only)
- Collection, Customer, Salesman snapshot workers
- M10 Piutang Report API
- BTR Desktop FF1
- `BTRPD_PiutangTopCustomer` table (deprecated writes only)

---

## Deployment

1. Run `Scripts/Upgrade_M14_PiutangDashboard_V2.sql` on target database.
2. Deploy API + worker; run `btr.portal.worker --domain Piutang`.
3. Deploy portal SPA (breaking: `TopCustomers` removed from piutang API).

---

## Verification

### Automated (2026-06-10)

**Backend build:**

```text
dotnet build btr.test/btr.test.csproj
Build succeeded. 0 Warning(s), 0 Error(s)
```

**Reporting tests** (62 passed):

```text
dotnet vstest btr.test/bin/Debug/btr.test.dll \
  --TestCaseFilter:"FullyQualifiedName~DashboardPiutangAggregatorTest|FullyQualifiedName~DashboardPiutangSnapshotVerificationTest|FullyQualifiedName~DashboardPiutangDalTest|FullyQualifiedName~DashboardExecutiveComposerTest|FullyQualifiedName~DashboardAlertCenterComposerTest"

Passed!  - Failed: 0, Passed: 62, Skipped: 0, Total: 62
```

**Frontend build** (from initial implementation, re-verified in review):

```text
npm run build  — portal SPA builds successfully
```

### Manual checklist (plan §8.5 — pending ops sign-off)

Requires dev/staging environment with deployed V2 schema and worker access. Not executed during automated remediation.

| # | Step | Status |
| - | ---- | ------ |
| V1 | Run `btr.portal.worker --domain Piutang`; confirm refresh success in `BTRPD_RefreshLog` | Pending |
| V2 | Open `/dashboard/piutang` — 5 KPIs + 2 concentration metrics populated | Pending |
| V3 | Aging pie unchanged; bucket sum = Total Piutang | Pending |
| V4 | Top 20 table shows aging columns; row click opens Piutang Report | Pending |
| V5 | Compare Total Piutang to M10 report footer (all open) | Pending |
| V6 | Confirm Collection Dashboard has no duplicate Top 20 total-balance table | Pending |
| V7 | `GeneratedAt` displayed (layout component) | Pending |

Automated coverage substitutes for partial verification: reconciliation invariants (§2.2), tie-break ranking, snapshot round-trip row counts, API field mapping, Executive/Alert Center regression.

---

*Ready for re-review after manual V1–V7 sign-off in deployment environment.*
