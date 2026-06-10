# M22 Branch / Warehouse Performance Dashboard — Implementation Summary



**Status:** Review follow-up complete — pending dev deploy verification  

**Date:** 2026-06-09



## Delivered



- **Route:** `/dashboard/locations` — Branch / Warehouse Performance Dashboard (sidebar: **Locations**)

- **API:** `GET /api/dashboard/locations`

- **Snapshot domain:** `Location` — 7 tables `BTRPD_Location*`, 60-minute refresh cadence

- **Refresh order:** After Collection in `RefreshAll` (last domain)

- **Worker CLI:** `btr.portal.worker --domain Location --triggered-by Manual`

- **Admin refresh:** `POST /api/admin/dashboard/refresh` with `{ "Domain": "Location" }`



## Architecture



Materialized location concentration dashboard composing:



- Source DALs at refresh: stok balance, faktur, invoice, warehouse master, brg last faktur

- Denominator snapshots (read-only): Inventory, InventoryRisk, Sales, Purchasing KPIs

- Shared M19 classification via `DashboardInventoryRiskClassifier`

- Warehouse × Signal attention list (6 signals, M23-compatible keys)



## Key files



| Layer | Path |

| --- | --- |

| SQL | `btr.sql/Tables/ReportingContext/BTRPD_Location*.sql` |

| Aggregator | `DashboardLocationAggregator.cs` |

| Worker | `RefreshDashboardLocationSnapshotWorker.cs` |

| Read API | `GetDashboardLocationQuery.cs`, `LocationDashboardController.cs` |

| Frontend | `LocationDashboardView.vue`, `Location*.vue` components |



## Review Follow-Up (2026-06-09)



Reviewer approved M22 with minor gaps. Three follow-up actions:



| Action | Status |

| --- | --- |

| Add remaining Plan §9.1 unit tests (reconciliation + concentration signals) | **Complete** |

| Add implementation plan link to analysis document header | **Complete** |

| Execute manual checklist §9.4 after SQL deploy and first Location refresh | **Pending** — no dev stack available; checklist documented below |



## Verification



### Unit test verification



```text

dotnet build src/j05-btr-distrib/btr.test/btr.test.csproj

dotnet vstest btr.test/bin/Debug/btr.test.dll --TestCaseFilter:"FullyQualifiedName~DashboardLocation"

```



**Result:** 19 passed, 0 failed.



| File | Cases |

| --- | --- |

| `DashboardLocationAggregatorTest.cs` | In-Transit exclusion, special/inactive ranking exclusion, inactive + no-sales signals, **inventory/at-risk concentration**, **Top 1/Top 3 % KPI**, **reconciliation** (omzet, purchase, inventory fallback), **wilayah grouping**, **deduplication**, **missing purchasing snapshot fallback**, attention sort order |

| `DashboardLocationKeyResolverTest.cs` | Warehouse name trim, In-Transit case sensitivity, eligibility checks, wilayah normalization |

| `DashboardInventoryRiskClassifierTest.cs` | M19 at-risk BrgId parity (in same file namespace) |



Refresh orchestration (`RefreshAllDashboardSnapshotsWorkerTest`, `RefreshDashboardSnapshotsHandlerTest`) confirms Location domain registration and RefreshAll order separately.



## Manual Verification Checklist (Plan §9.4)



Execute on first dev/staging deploy. No live environment was available during review follow-up.



| # | Check | Status | Notes |

| --- | --- | --- | --- |

| 1 | Deploy 7 SQL tables; run Location refresh | **Pending** | Requires dev SQL deploy |

| 2 | `GET /api/dashboard/locations` returns full response | **Pending** | Requires API + snapshot data |

| 3 | Page title **Branch / Warehouse Performance Dashboard**; sections in fixed order | **Pending** | Requires portal web |

| 4 | Sum warehouse inventory matches Inventory Report footer (excl. In-Transit) | **Pending** | Requires live snapshots + Inventory Report |

| 5 | Sum warehouse MTD sales matches Sales Dashboard `TotalOmzet` | **Pending** | Requires live snapshots + Sales Dashboard |

| 6 | Sum warehouse MTD purchase matches Purchasing Dashboard `GrandTotalPurchase` | **Pending** | Requires live snapshots + Purchasing Dashboard |

| 7 | Warehouse at-risk total ≤ Inventory Risk `AtRiskInventoryValue` | **Pending** | Requires live snapshots + Inventory Risk Dashboard |

| 8 | Click inventory ranking row → Inventory Report with `?q=` pre-filled | **Pending** | Requires portal web |

| 9 | Click wilayah sales row → Collection Dashboard (not Piutang Report) | **Pending** | Requires portal web |

| 10 | Navigation links reach all seven sibling dashboards | **Pending** | Requires portal web |

| 11 | Inactive warehouse with stock on attention list but not in Top 10 rankings | **Pending** | Requires seed data |

| 12 | **"⚠ Dashboard Data Not Fresh"** when snapshot exceeds 60-minute interval | **Pending** | Requires stale snapshot or clock mock |

| 13 | `GET /api/health/dashboard-snapshots` includes `Location` | **Pending** | Requires running API |

| 14 | Manual refresh: `POST /api/admin/dashboard/refresh` with `{ "Domain": "Location" }` | **Pending** | Requires API + auth |

| 15 | Worker CLI: `btr.portal.worker --domain Location --triggered-by Manual` | **Pending** | Requires worker + DB |

| 16 | RefreshAll order: Location runs after Collection | **Pass (unit test)** | `RefreshAllDashboardSnapshotsWorkerTest` asserts domain index 9 = Location |

| 17 | Desktop SF1 spot-check: warehouse and wilayah MTD omzet grouping | **Pending** | Manual Desktop validation |



## Phase 2 (deferred)



Promote selected location concentration KPIs to Management Attention Center (`/dashboard`) — separate PR after M22 validated in production.



---



*End of implementation summary — M22 Branch / Warehouse Performance Dashboard (review follow-up complete)*

