# Materialized Dashboard Data — Operational Guide

**Audience:** End Users, Administrators, Support Team  
**Purpose:** How to use materialized dashboards day to day and how to operate the snapshot worker.

**Related docs:** [Domain (WHY)](./materialized-dashboard-domain.md) · [Architecture (WHAT)](./materialized-dashboard-architecture.md) · [BTR Portal operational](../btr-portal/btr-portal-operational.md)

For login, navigation, and report usage, see [btr-portal-operational.md](../btr-portal/btr-portal-operational.md). This guide focuses on **snapshot behavior**, **freshness**, and **administrator tasks**.

---

## For End Users

### What Changed

Dashboard numbers are **pre-calculated** on a schedule and stored in the database. Opening a dashboard reads the latest stored snapshot — it does not recalculate from all operational tables on every visit.

### Reading the Generated-At Timestamp

Every dashboard page shows when data was last rebuilt (e.g. "Generated at 08:45").

| What it means | What it does **not** mean |
| ------------- | ------------------------ |
| When the background job last finished updating that domain | When you clicked Refresh |
| How old the numbers may be (up to 15 / 30 / 30 / 30 / 30 / 60 min) | That BTR Desktop was updated at that exact second |

**Home page:** Executive view uses consolidated freshness (`Min(GeneratedAt)`). Domain detail pages may show different generated-at times per domain refresh schedule.

### Refresh Button

Click **Refresh** on any dashboard page to reload the latest stored snapshot from the server. It does **not** force an immediate recalculation. If numbers look stale, wait for the next scheduled refresh or ask an administrator to trigger a manual rebuild.

### Scheduled Freshness

| Domain | Updates at most every |
| ------ | --------------------- |
| Piutang | 15 minutes |
| Sales | 30 minutes |
| Purchasing | 30 minutes |
| Customer | 30 minutes |
| Salesman | 30 minutes |
| Inventory | 60 minutes |
| Inventory Risk | 60 minutes |

Piutang refreshes most often because receivable balances change throughout the day (payments, new invoices). Inventory and Inventory Risk share the 60-minute cadence; Inventory Risk runs immediately after Inventory in `--domain All`.

### When Dashboards Are Unavailable

| Symptom | Cause | What to do |
| ------- | ----- | ---------- |
| Home warning: data not yet available | Snapshot tables empty (new deploy) | Contact administrator — worker must run initial backfill |
| Detail page error / 503 | Snapshot missing for that domain | Contact administrator — check scheduled tasks |
| Numbers seem old | Normal within refresh interval | Wait or request manual refresh |
| Sales differs from Sales Report | Different refresh times or month boundary | Refresh both pages; if still wrong, escalate |

### Sales Dashboard Note

Sales analytics show **invoiced sales (Faktur)** for the current month — not pipeline or order-based omzet. Dashboard Total Omzet should match the sum of **Total** on the Sales Report for the same month.

---

## For Administrators and IT

### Prerequisites (New Deploy)

1. Deploy `btr.sql` schema (all `BTR_PortalDashboard*` tables + `IX_BTR_Piutang_OpenBalance`).
2. Run `DataSeeds/BTR_ParamNo_PortalDashboard.sql` if ID prefixes missing.
3. Deploy `btr.portal.api`, `btr.portal.web`, and `btr.portal.worker`.
4. Configure `appsettings.{MACHINE_NAME}.json` in **both** API and worker folders (same `Database` settings).
5. Run initial backfill **before** users access dashboards (see below).

### Initial Backfill (Required Once)

```powershell
cd C:\path\to\btr.portal.worker
.\btr.portal.worker.exe --domain All --triggered-by Manual
```

Verify success:

```sql
SELECT Domain, Status, CompletedAt, DurationMs
FROM BTR_PortalDashboardRefreshLog
ORDER BY CompletedAt DESC;
```

All seven domains (Piutang, Inventory, InventoryRisk, Sales, Purchasing, Customer, Salesman) should show `Status = 'Success'`.

Confirm KPI rows exist:

```sql
SELECT 'Piutang' AS Domain, GeneratedAt FROM BTR_PortalDashboardPiutangKpi WHERE SnapshotKey = 'CURRENT'
UNION ALL
SELECT 'Inventory', GeneratedAt FROM BTR_PortalDashboardInventoryKpi WHERE SnapshotKey = 'CURRENT'
UNION ALL
SELECT 'Sales', GeneratedAt FROM BTR_PortalDashboardSalesKpi WHERE SnapshotKey = 'CURRENT'
UNION ALL
SELECT 'Purchasing', GeneratedAt FROM BTR_PortalDashboardPurchasingKpi WHERE SnapshotKey = 'CURRENT'
UNION ALL
SELECT 'Customer', GeneratedAt FROM BTR_PortalDashboardCustomerKpi WHERE SnapshotKey = 'CURRENT'
UNION ALL
SELECT 'Salesman', GeneratedAt FROM BTR_PortalDashboardSalesmanKpi WHERE SnapshotKey = 'CURRENT'
UNION ALL
SELECT 'InventoryRisk', GeneratedAt FROM BTR_PortalDashboardInventoryRiskKpi WHERE SnapshotKey = 'CURRENT';
```

### Scheduled Tasks

Create **seven separate** Windows Task Scheduler jobs:

| Task name | Interval | Command |
| --------- | -------- | ------- |
| `BTR-Portal-Dashboard-Piutang` | Every **15 min** | `btr.portal.worker.exe --domain Piutang --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Sales` | Every **30 min** | `btr.portal.worker.exe --domain Sales --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Purchasing` | Every **30 min** | `btr.portal.worker.exe --domain Purchasing --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Inventory` | Every **60 min** | `btr.portal.worker.exe --domain Inventory --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-InventoryRisk` | Every **60 min** | `btr.portal.worker.exe --domain InventoryRisk --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Customer` | Every **30 min** | `btr.portal.worker.exe --domain Customer --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Salesman` | Every **30 min** | `btr.portal.worker.exe --domain Salesman --triggered-by Scheduler` |

**Task checklist:**

- Run whether user is logged on or not — service account with SQL access
- **Start in:** worker folder (must contain `appsettings.json`, `NLog.config`, DLLs)
- Stop if running longer than 30 minutes
- On failure: restart every 5 minutes, up to 3 attempts
- Exit code `0` = success; check `logs/btr-portal-worker-{date}.log` on failure

**Sample registration (adjust paths and account):**

```powershell
$worker = "C:\inetpub\btr-portal-worker\btr.portal.worker.exe"
$workDir = "C:\inetpub\btr-portal-worker"

$action = New-ScheduledTaskAction -Execute $worker -Argument "--domain Piutang --triggered-by Scheduler" -WorkingDirectory $workDir
$trigger = New-ScheduledTaskTrigger -Once -At (Get-Date) -RepetitionInterval (New-TimeSpan -Minutes 15) -RepetitionDuration ([TimeSpan]::MaxValue)
Register-ScheduledTask -TaskName "BTR-Portal-Dashboard-Piutang" -Action $action -Trigger $trigger -User "DOMAIN\svc-btr-portal" -RunLevel Highest
```

Repeat for Sales (30 min), Purchasing (30 min), Customer (30 min), Salesman (30 min), Inventory (60 min), and InventoryRisk (60 min).

### Manual Refresh Options

#### Option A — Worker CLI (recommended for full rebuild)

```powershell
# All domains (initial backfill, recovery)
.\btr.portal.worker.exe --domain All --triggered-by Manual

# Single domain
.\btr.portal.worker.exe --domain Piutang --triggered-by Manual
```

No HTTP timeout. Use for `--domain All` and large databases.

#### Option B — Portal API (single-domain ad-hoc)

```http
POST /api/admin/dashboard/refresh
Authorization: Bearer <token>
Content-Type: application/json

{ "domain": "Piutang" }
```

| `domain` value | Behavior |
| -------------- | -------- |
| `All` (default) | Piutang → Inventory → InventoryRisk → Sales → Purchasing → Customer → Salesman |
| `Piutang`, `Inventory`, `InventoryRisk`, `Sales`, `Purchasing`, `Customer`, `Salesman` | Single domain only |

Domain values are case-insensitive. Logged as `TriggeredBy = Manual` in refresh log.

**Timeout warning:** API runs refresh synchronously on the HTTP thread. IIS default execution timeout is ~110 seconds. Full `--domain All` may return 502/504 even if work continues — check refresh log. Prefer worker CLI for full rebuilds.

### Monitoring

| Check | How |
| ----- | --- |
| Snapshot health (API) | `GET /api/health/dashboard-snapshots` |
| Overall status | `unknown` (no history), `ok`, `refreshing`, or `degraded` |
| Last refresh (SQL) | `SELECT TOP 1 * FROM BTR_PortalDashboardRefreshLog WHERE Domain = 'Piutang' ORDER BY CompletedAt DESC` |
| Worker log | `{worker-folder}/logs/btr-portal-worker-{date}.log` |
| Task Scheduler | History tab on each task |
| User-visible staleness | Dashboard home cards — per-domain generated-at |

**Health endpoint example:**

```http
GET /api/health/dashboard-snapshots
```

Returns latest refresh per domain plus configured interval minutes. No authentication required.

### Troubleshooting

| Symptom | Likely cause | Action |
| ------- | ------------ | ------ |
| Dashboard 503 / unavailable | Empty snapshot tables | Run `--domain All --triggered-by Manual` |
| Home warning banner | One or more KPI tables empty | Check refresh log; re-run failed domain |
| Stale data | Scheduler not running | Task Scheduler history; fix task; manual refresh |
| Refresh log `Failed` | SQL error, timeout, data issue | Read `ErrorMessage` column; check worker log |
| API refresh 502/504 | Request timeout on full rebuild | Use worker CLI instead |
| Piutang vs report mismatch | Stale snapshot or payment just posted | Wait 15 min or refresh Piutang domain |
| Sales vs report mismatch | Month rollover not yet refreshed | Run Sales worker; check `PeriodYear/Month` on KPI row |

### Rollback / Recovery

1. Re-run manual refresh: `btr.portal.worker.exe --domain All --triggered-by Manual`
2. Investigate `BTR_PortalDashboardRefreshLog.ErrorMessage`
3. Fix Task Scheduler if jobs stopped
4. Detail pages return 503 until snapshots repopulated — no live fallback in production

### Post-Deploy Smoke Test

```text
GET  /api/health                         → 200
GET  /api/health/dashboard-snapshots     → 200 (status ok after first refresh)
POST /api/auth/login                     → 200 with JWT
GET  /api/dashboard/overview             → 200 with KPI data
GET  /api/dashboard/sales                → 200 with token
GET  /api/dashboard/piutang            → 200 with token
GET  /api/dashboard/inventory            → 200 with token
GET  /api/dashboard/purchasing           → 200 with token
GET  /api/dashboard/customers            → 200 with token
GET  /api/dashboard/salesmen             → 200 with token
GET  /api/dashboard/inventory-risk       → 200 with token
```

---

## Quick Reference Card

```text
INITIAL SETUP     btr.portal.worker.exe --domain All --triggered-by Manual
SCHEDULE          7 Task Scheduler jobs: 15 / 30 / 30 / 30 / 30 / 60 / 60 min
MANUAL (FULL)     Worker CLI --domain All
MANUAL (ONE)      API POST { "domain": "Piutang" }  OR  worker --domain Piutang
MONITOR           GET /api/health/dashboard-snapshots
LOGS              worker-folder/logs/btr-portal-worker-*.log
STALE DATA        Wait for schedule OR manual refresh — user Refresh button re-reads only
```
