# BTR Portal — Deploy & Backfill Runbook

This runbook covers first-time deployment and upgrades of **BTR Portal** (`btr.portal.api`, `btr.portal.web`, `btr.portal.worker`) and the materialized dashboard tables (`BTRPD_*`). Normal day-to-day use: scheduled worker jobs refresh snapshots; the API reads snapshot tables only (live reports query core `BTR_*` tables directly).

Portal shares the **same SQL Server database** as BTR Desktop. There is no portal-only SQL project — snapshot objects ship inside `btr.sql`.

## Prerequisites

- SQL publish access for `btr.sql` (SSDT / SqlPackage / Visual Studio Publish)
- IIS (or equivalent) for API and static frontend
- Windows Task Scheduler access for snapshot worker jobs
- Dev/staging smoke-tested before production
- BTR Desktop database already exists with core transactional tables (`BTR_User`, `BTR_Faktur`, `BTR_Piutang`, etc.)

## 1. Database publish

### Fresh deploy (new or empty snapshot schema)

**Option A — single script (portal tables only)**

Run `btr.sql/Scripts/Create_BTRPD_PortalDashboard_Tables.sql` against the target database. This creates all 46 `BTRPD_*` tables, their indexes, and `IX_BTR_Piutang_OpenBalance`. The script is idempotent (skips objects that already exist). Requires `BTR_Piutang` to already exist.

```powershell
sqlcmd -S "OFFICE-SQL01\SQLEXPRESS" -d btr -i "src\j05-btr-distrib\btr.sql\Scripts\Create_BTRPD_PortalDashboard_Tables.sql"
```

**Option B — full SSDT publish**

Publish the full `btr.sql` project. All portal snapshot objects are registered in `btr.sql.sqlproj` under `Tables/ReportingContext/`.

**Portal-specific objects (46 tables + 1 index):**

| Domain | Tables |
| ------ | ------ |
| Shared | `BTRPD_RefreshLog` |
| Piutang | `BTRPD_PiutangKpi`, `BTRPD_PiutangAging`, `BTRPD_PiutangCustomerAging`, `BTRPD_PiutangTopCustomerRisk` (`BTRPD_PiutangTopCustomer` deprecated — no longer written after M14 V2) |
| Inventory | `BTRPD_InventoryKpi`, `BTRPD_InventoryBreakdown` |
| Inventory Risk | `BTRPD_InventoryRiskKpi`, `BTRPD_InventoryRiskAging`, `BTRPD_InventoryRiskAttention`, `BTRPD_InventoryRiskTopDead`, `BTRPD_InventoryRiskTopSlow`, `BTRPD_InventoryRiskBreakdown` |
| Sales | `BTRPD_SalesKpi`, `BTRPD_SalesWeekTrend`, `BTRPD_SalesTopSalesman` |
| Purchasing | `BTRPD_PurchasingKpi`, `BTRPD_PurchasingWeekTrend`, `BTRPD_PurchasingPostingStatus`, `BTRPD_PurchasingTopPrincipal` |
| Purchasing Management | `BTRPD_PurchasingManagementKpi`, `BTRPD_PurchasingManagementAttention`, `BTRPD_PurchasingManagementTopPrincipal` |
| Customer | `BTRPD_CustomerKpi`, `BTRPD_CustomerTopOmzet`, `BTRPD_CustomerTopPiutang`, `BTRPD_CustomerAttention`, `BTRPD_CustomerSegmentation` |
| Salesman | `BTRPD_SalesmanKpi`, `BTRPD_SalesmanTopOmzet`, `BTRPD_SalesmanTopAchievement`, `BTRPD_SalesmanTopPiutang`, `BTRPD_SalesmanAttention`, `BTRPD_SalesmanSegmentation` |
| Collection | `BTRPD_CollectionKpi`, `BTRPD_CollectionAging`, `BTRPD_CollectionAttention`, `BTRPD_CollectionTopOverdueCustomer`, `BTRPD_CollectionTopOverdueSalesman`, `BTRPD_CollectionTopOverdueWilayah` |
| Location | `BTRPD_LocationKpi`, `BTRPD_LocationTopWarehouseInventory`, `BTRPD_LocationTopWarehouseAtRisk`, `BTRPD_LocationTopWarehouseSales`, `BTRPD_LocationTopWarehousePurchasing`, `BTRPD_LocationTopWilayahSales`, `BTRPD_LocationAttention` |
| Index | `IX_BTR_Piutang_OpenBalance` on `BTR_Piutang` (accelerates Piutang/Collection refresh) |

**Also required from `btr.sql` (shared with Desktop, not portal-specific):**

| Purpose | Tables |
| ------- | ------ |
| Authentication | `BTR_User`, `BTR_Role` |
| Live reports | `BTR_Faktur`, `BTR_Piutang`, `BTR_Customer`, `BTR_Invoice`, `BTR_Stok`, `BTR_Warehouse`, `BTR_SalesPerson`, `BTR_Wilayah`, and related core tables |
| Sales report aggregate (optional path) | `BTR_SalesOmzet` — see [sales-omzet-deploy.md](./sales-omzet-deploy.md) if RO2 aggregate is not yet deployed |

**Alert Center (M23)** has no separate tables — it composes alerts from the snapshot tables above.

**ID generation:** snapshot workers generate row IDs with `Ulid.NewUlid()`. No `BTR_ParamNo` seed is required for portal dashboards.

### Field Activity demo seed (M18.5 — dev/staging only)

For environments with sparse check-in data, optionally run:

```powershell
# Edit script first: set @EnableFieldActivityDemoSeed = 1
sqlcmd -S "OFFICE-SQL01\SQLEXPRESS" -d btr -i "src\j05-btr-distrib\btr.sql\Scripts\Seed_FieldActivity_Demo.sql"
```

**Never run in production.** The script seeds `FA_DEMO_*` customers, visit plan rows, check-ins, and orders for yesterday against the first salesperson with a configured `Email`.

Optional performance index for check-in lookups:

```powershell
sqlcmd -S "OFFICE-SQL01\SQLEXPRESS" -d btr -i "src\j05-btr-distrib\btr.sql\Scripts\Upgrade_M18_5_FieldActivity_Index.sql"
```

Portal API config: `FieldActivity:VisitPlanGoLiveDate` (default `2026-03-01`) controls planned-visit availability for dates before visit-plan materialization go-live.

### Publish steps

1. Open `j05-btr-distrib.sln` in Visual Studio 2022.
2. Right-click `btr.sql` → **Publish** → select target connection / publish profile.
3. Review the deployment plan; confirm all `BTRPD_*` tables and `IX_BTR_Piutang_OpenBalance` are included.
4. Publish.

Alternative (CI / command line): build the DACPAC from `btr.sql` and deploy with `SqlPackage.exe /Action:Publish`.

### Post-publish verification (SQL)

```sql
-- Expect 46 BTRPD_* user tables (+ BTR_Piutang is pre-existing)
SELECT COUNT(*) AS BtrpdTableCount
FROM sys.tables
WHERE name LIKE 'BTRPD[_]%';

-- Index for refresh performance
SELECT name FROM sys.indexes
WHERE name = 'IX_BTR_Piutang_OpenBalance';

-- Tables should exist but be empty before first backfill
SELECT 'Piutang' AS Domain, COUNT(*) AS KpiRows FROM BTRPD_PiutangKpi
UNION ALL SELECT 'Inventory', COUNT(*) FROM BTRPD_InventoryKpi
UNION ALL SELECT 'Sales', COUNT(*) FROM BTRPD_SalesKpi;
```

## 2. Upgrade path (existing databases)

Run these **manual scripts** only when upgrading from an older portal schema. They are **not** executed automatically by `btr.sql` publish (`<None Include>` in the project).

| Script | When to run |
| ------ | ----------- |
| `btr.sql/Scripts/Rename_PortalDashboard_Tables_To_BTRPD.sql` | Database still has `BTR_PortalDashboard*` table names |
| `btr.sql/Scripts/Upgrade_PortalDashboard_GeneratedIds_ToUlid.sql` | Snapshot ID columns are still `VARCHAR(13)` (pre-ULID) |

**Order:** rename tables first, then widen ID columns, then deploy application binaries that write ULIDs.

Skip both scripts on a **fresh** deploy that creates `BTRPD_*` tables directly.

## 3. Application deploy

Deploy all three portal components from the same branch / build.

### API (`btr.portal.api`)

1. Visual Studio: right-click `btr.portal.api` → **Publish** → **FolderProfile**.
2. Output: `src/j05-btr-distrib/publish/btr-portal-api/`
3. Copy to IIS physical path (e.g. `C:\inetpub\btr-portal-api`).

MSBuild alternative:

```powershell
msbuild src\j05-btr-distrib\btr.portal.api\btr.portal.api.csproj `
  /p:DeployOnBuild=true /p:PublishProfile=FolderProfile /p:Configuration=Release
```

### Frontend (`btr.portal.web`)

```powershell
cd src\j05-btr-distrib\btr.portal.web
npm install
# Use the hostname clients use in the browser, not localhost, unless users browse via localhost.
# Production IIS: include the API application path (e.g. http://server:8080/api).
$env:VITE_API_BASE_URL = "http://server:8080/api"
npm run build
```

After build, confirm the bundle does not contain `localhost`:

```powershell
Select-String -Path dist\assets\*.js -Pattern 'localhost' -SimpleMatch
# Expect no matches
```

Copy `dist/` contents to the IIS static site (e.g. `C:\inetpub\btr-portal`).

### Worker (`btr.portal.worker`)

Build Release and copy output to a dedicated folder (not under the API site):

```powershell
msbuild src\j05-btr-distrib\btr.portal.worker\btr.portal.worker.csproj /p:Configuration=Release
# Copy bin\Release\* to C:\inetpub\btr-portal-worker\
```

The worker folder must contain `btr.portal.worker.exe`, `appsettings.json`, `NLog.config`, and all dependency DLLs.

## 4. Configuration

Portal does **not** use the BTR Desktop registry. Create `appsettings.{MACHINE_NAME}.json` in **both** the API folder and the worker folder (`{MACHINE_NAME}` must match `Environment.MachineName` on that server).

```json
{
  "Database": {
    "ServerName": "OFFICE-SQL01\\SQLEXPRESS",
    "DbName": "btr",
    "IsTest": false
  },
  "Jwt": {
    "Issuer": "btr-portal-api",
    "Audience": "btr-portal-vue",
    "Key": "REPLACE-WITH-STRONG-SECRET-256-BITS-MINIMUM",
    "ExpiryMinutes": 480
  },
  "Cors": {
    "AllowedOrigins": [ "https://your-server/btr-portal" ]
  },
  "Presentation": {
    "Enabled": false,
    "BusinessDate": "2026-06-05"
  }
}
```

- **Presentation mode:** Set `"Presentation": { "Enabled": true, "BusinessDate": "<snapshot-date>" }` on the API (and worker `appsettings.json` if separate) before demos. `BusinessDate` is required when enabled (ISO `yyyy-MM-dd`). Re-run `btr.portal.worker.exe --domain All` after enabling so materialized snapshots use the business date. Revert `Enabled` to `false` afterward and re-run the worker. Hides platform freshness/worker UI warnings; business calculations use `BusinessDate`. See `docs/features/btr-portal/presentation-mode/feature.md`.
- IIS app pool identity must reach SQL Server (embedded `btrLogin` credentials in `ConnectionStringFactory`).
- Ensure `logs/` under the API folder is writable.
- Worker only needs the `Database` section (JWT/CORS are API-only).

## 5. First-time backfill (required once)

Run **before** users access dashboards. Empty `BTRPD_*` tables cause dashboard endpoints to return 503 or "data not yet available".

```powershell
cd C:\inetpub\btr-portal-worker
.\btr.portal.worker.exe --domain All --triggered-by Manual
```

`--domain All` runs domains in this order: Piutang → Inventory → InventoryRisk → Sales → Purchasing → PurchasingManagement → Customer → Salesman → Collection → Location.

Exit code `0` = success. On failure, check `logs\btr-portal-worker-{date}.log` and re-run failed domains individually.

### Verify refresh log

```sql
SELECT Domain, Status, CompletedAt, DurationMs, ErrorMessage
FROM BTRPD_RefreshLog
ORDER BY CompletedAt DESC;
```

All ten domains should show `Status = 'Success'`.

### Verify KPI rows

```sql
SELECT 'Piutang' AS Domain, GeneratedAt FROM BTRPD_PiutangKpi WHERE SnapshotKey = 'CURRENT'
UNION ALL SELECT 'Inventory', GeneratedAt FROM BTRPD_InventoryKpi WHERE SnapshotKey = 'CURRENT'
UNION ALL SELECT 'InventoryRisk', GeneratedAt FROM BTRPD_InventoryRiskKpi WHERE SnapshotKey = 'CURRENT'
UNION ALL SELECT 'Sales', GeneratedAt FROM BTRPD_SalesKpi WHERE SnapshotKey = 'CURRENT'
UNION ALL SELECT 'Purchasing', GeneratedAt FROM BTRPD_PurchasingKpi WHERE SnapshotKey = 'CURRENT'
UNION ALL SELECT 'PurchasingManagement', GeneratedAt FROM BTRPD_PurchasingManagementKpi WHERE SnapshotKey = 'CURRENT'
UNION ALL SELECT 'Customer', GeneratedAt FROM BTRPD_CustomerKpi WHERE SnapshotKey = 'CURRENT'
UNION ALL SELECT 'Salesman', GeneratedAt FROM BTRPD_SalesmanKpi WHERE SnapshotKey = 'CURRENT'
UNION ALL SELECT 'Collection', GeneratedAt FROM BTRPD_CollectionKpi WHERE SnapshotKey = 'CURRENT'
UNION ALL SELECT 'Location', GeneratedAt FROM BTRPD_LocationKpi WHERE SnapshotKey = 'CURRENT';
```

## 6. Scheduled tasks (normal operations)

Create **separate** Windows Task Scheduler jobs per domain:

| Task name | Interval | Command |
| --------- | -------- | ------- |
| `BTR-Portal-Dashboard-Piutang` | Every **15 min** | `btr.portal.worker.exe --domain Piutang --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Sales` | Every **30 min** | `btr.portal.worker.exe --domain Sales --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Purchasing` | Every **30 min** | `btr.portal.worker.exe --domain Purchasing --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-PurchasingManagement` | Every **30 min** | `btr.portal.worker.exe --domain PurchasingManagement --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Customer` | Every **30 min** | `btr.portal.worker.exe --domain Customer --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Salesman` | Every **30 min** | `btr.portal.worker.exe --domain Salesman --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Collection` | Every **30 min** | `btr.portal.worker.exe --domain Collection --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Inventory` | Every **60 min** | `btr.portal.worker.exe --domain Inventory --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-InventoryRisk` | Every **60 min** | `btr.portal.worker.exe --domain InventoryRisk --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Location` | Every **60 min** | `btr.portal.worker.exe --domain Location --triggered-by Scheduler` |

**Task checklist:**

- Run whether user is logged on or not — service account with SQL access
- **Start in:** worker folder
- Stop if running longer than 30 minutes
- On failure: restart every 5 minutes, up to 3 attempts

**Sample registration (adjust paths and account):**

```powershell
$worker = "C:\inetpub\btr-portal-worker\btr.portal.worker.exe"
$workDir = "C:\inetpub\btr-portal-worker"

$action = New-ScheduledTaskAction -Execute $worker -Argument "--domain Piutang --triggered-by Scheduler" -WorkingDirectory $workDir
$trigger = New-ScheduledTaskTrigger -Once -At (Get-Date) -RepetitionInterval (New-TimeSpan -Minutes 15) -RepetitionDuration ([TimeSpan]::MaxValue)
Register-ScheduledTask -TaskName "BTR-Portal-Dashboard-Piutang" -Action $action -Trigger $trigger -User "DOMAIN\svc-btr-portal" -RunLevel Highest
```

Repeat for each domain with the interval from the table above.

### Worker CLI reference

| Argument | Values | Default |
| -------- | ------ | ------- |
| `--domain` | `All`, `Piutang`, `Inventory`, `InventoryRisk`, `Sales`, `Purchasing`, `PurchasingManagement`, `Customer`, `Salesman`, `Collection`, `Location` | `All` |
| `--triggered-by` | `Scheduler`, `Manual` | `Scheduler` |

## 7. Manual refresh (ad-hoc)

### Option A — Worker CLI (recommended for full rebuild)

```powershell
.\btr.portal.worker.exe --domain All --triggered-by Manual
.\btr.portal.worker.exe --domain Piutang --triggered-by Manual
```

No HTTP timeout. Use for `--domain All`, initial backfill, and recovery.

### Option B — Portal API (single domain)

```http
POST /api/admin/dashboard/refresh
Authorization: Bearer <token>
Content-Type: application/json

{ "domain": "Piutang" }
```

`domain` accepts the same values as the worker CLI. Prefer the worker for `All` — the API runs refresh synchronously and may hit IIS request timeout (~110 seconds) on large databases.

## 8. Post-deploy verification (HTTP)

```text
GET  /api/health                         → 200
GET  /api/health/dashboard-snapshots     → 200, domains show ok
POST /api/auth/login                     → 200 with token
GET  /api/dashboard/executive            → 200 with token (after backfill)
GET  /api/dashboard/sales                → 200 with token
GET  /api/dashboard/purchasing           → 200 with token
GET  /api/dashboard/customers            → 200 with token
GET  /api/dashboard/salesmen             → 200 with token
GET  /api/dashboard/field-activity/salesmen → 200 with token
GET  /api/dashboard/field-activity?salesPersonId=&visitDate= → 200 with token
GET  /api/dashboard/collection           → 200 with token
GET  /api/dashboard/location             → 200 with token
GET  /api/dashboard/inventory-risk       → 200 with token
GET  /api/reports/sales                  → 200 with token
GET  /api/reports/purchasing             → 200 with token
```

## 9. Monitoring

| Check | How |
| ----- | --- |
| API health | `GET /api/health` → 200 |
| Snapshot health | `GET /api/health/dashboard-snapshots` — per-domain `LastRefresh.Status` |
| Last refresh (SQL) | `SELECT TOP 1 * FROM BTRPD_RefreshLog WHERE Domain = 'Sales' ORDER BY CompletedAt DESC` |
| Worker log | `{worker-folder}/logs/btr-portal-worker-{date}.log` |
| Task Scheduler | History tab on each scheduled task |

## 10. Troubleshooting

| Symptom | Likely cause | Action |
| ------- | ------------ | ------ |
| Login fails with SQL error | Missing or wrong `appsettings.{MACHINE}.json` | Fix `Database` section; recycle app pool |
| Dashboard 503 / unavailable | Empty snapshot tables | Run worker `--domain All --triggered-by Manual` |
| Stale dashboard data | Scheduler not running | Check Task Scheduler history; re-run worker |
| API refresh times out | Full rebuild too slow for IIS | Use worker CLI instead |
| CORS error in browser | Origin not in `Cors:AllowedOrigins` | Add frontend URL to API appsettings |
| `Invalid object name 'BTRPD_*'` | SQL schema not published | Publish `btr.sql` or run upgrade scripts |
| Refresh slow on Piutang/Collection | Missing index | Confirm `IX_BTR_Piutang_OpenBalance` exists |

## 11. Rollback

1. Restore previous application binaries (API, web, worker).
2. `BTRPD_*` tables can remain in place — older portal builds that expect `BTR_PortalDashboard*` names require the rename script to be reversed manually (not automated).
3. Snapshot data does not affect BTR Desktop transactional data; dropping `BTRPD_*` tables is safe if dashboards are decommissioned (backup first).

## Related docs

- [btr-portal-operational.md](../../../../docs/features/btr-portal/btr-portal-operational.md) — user workflows and admin overview
- [materialized-dashboard-operational.md](../../../../docs/features/materialized-dashboard/materialized-dashboard-operational.md) — snapshot refresh intervals and architecture context
- [sales-omzet-deploy.md](./sales-omzet-deploy.md) — `BTR_SalesOmzet` aggregate (separate from portal snapshots)
