# Deploy Runbook: BTR Portal Dashboard Snapshot Worker

Operations guide for scheduling materialized dashboard snapshot refresh jobs on the BTR Portal server.

**Related:** [implementation-plan.md](./implementation-plan.md) Phase 4

---

## Prerequisites

1. Deploy `btr.sql` schema including all `BTR_PortalDashboard*` tables and `IX_BTR_Piutang_OpenBalance`.
2. Deploy `btr.portal.api` and `btr.portal.worker` to the server.
3. Configure `appsettings.json` (or `appsettings.{MachineName}.json`) in **both** the portal API folder and the worker folder with correct `Database` connection settings.
4. Run an initial full refresh before disabling live fallback:

```powershell
cd C:\path\to\btr.portal.worker\bin\Release
.\btr.portal.worker.exe --domain All --triggered-by Manual
```

5. Verify `BTR_PortalDashboardRefreshLog` shows `Success` for Piutang, Inventory, and Sales.

---

## Worker CLI

| Argument | Values | Default |
| --- | --- | --- |
| `--domain` | `All`, `Piutang`, `Inventory`, `Sales` | `All` |
| `--triggered-by` | `Scheduler`, `Manual` | `Scheduler` |

**Exit codes:** `0` = success, non-zero = failure (check `logs/btr-portal-worker-*.log`).

**Examples:**

```powershell
# Scheduled per-domain jobs (production)
.\btr.portal.worker.exe --domain Piutang --triggered-by Scheduler
.\btr.portal.worker.exe --domain Sales --triggered-by Scheduler
.\btr.portal.worker.exe --domain Inventory --triggered-by Scheduler

# Manual ops / initial backfill
.\btr.portal.worker.exe --domain All --triggered-by Manual
```

---

## Windows Task Scheduler Jobs

Create **three separate scheduled tasks** so each domain respects its maximum staleness cadence.

| Task name | Schedule | Command |
| --- | --- | --- |
| `BTR-Portal-Dashboard-Piutang` | Every **15 minutes** | `btr.portal.worker.exe --domain Piutang --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Sales` | Every **30 minutes** | `btr.portal.worker.exe --domain Sales --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Inventory` | Every **60 minutes** | `btr.portal.worker.exe --domain Inventory --triggered-by Scheduler` |

### Task configuration checklist

- **Run whether user is logged on or not** — use a service account with SQL access matching portal API credentials.
- **Start in:** worker executable directory (must contain `appsettings.json`, `NLog.config`, and dependent DLLs).
- **Stop task if runs longer than:** 30 minutes (adjust if data volume requires).
- **If task fails, restart every:** 5 minutes, up to 3 attempts.
- **Run with highest privileges:** only if required by SQL/network policy.

### Sample PowerShell registration (adjust paths)

```powershell
$worker = "C:\inetpub\btr-portal-worker\btr.portal.worker.exe"
$workDir = "C:\inetpub\btr-portal-worker"

$action = New-ScheduledTaskAction -Execute $worker -Argument "--domain Piutang --triggered-by Scheduler" -WorkingDirectory $workDir
$trigger = New-ScheduledTaskTrigger -Once -At (Get-Date) -RepetitionInterval (New-TimeSpan -Minutes 15) -RepetitionDuration ([TimeSpan]::MaxValue)
Register-ScheduledTask -TaskName "BTR-Portal-Dashboard-Piutang" -Action $action -Trigger $trigger -User "DOMAIN\svc-btr-portal" -RunLevel Highest
```

Repeat for Sales (30 min) and Inventory (60 min) with matching `--domain` values.

---

## Monitoring

| Check | How |
| --- | --- |
| Last refresh per domain | `SELECT TOP 1 * FROM BTR_PortalDashboardRefreshLog WHERE Domain = 'Piutang' ORDER BY CompletedAt DESC` |
| Worker log | `{worker-folder}/logs/btr-portal-worker-{date}.log` |
| Task Scheduler history | Task Scheduler → task → History tab |
| Portal overview staleness | Dashboard home cards show per-domain `GeneratedAt` |
| Snapshot populated | `SELECT GeneratedAt FROM BTR_PortalDashboardPiutangKpi WHERE SnapshotKey = 'CURRENT'` |

---

## Rollback

If snapshot data is stale or incorrect:

1. Re-run manual refresh: `btr.portal.worker.exe --domain All --triggered-by Manual`
2. Investigate `BTR_PortalDashboardRefreshLog.ErrorMessage` for failed runs
3. Detail pages return HTTP 503 when snapshot is empty — ensure worker jobs are running

---

## Configuration reference

`appsettings.json` → `DashboardSnapshot` section documents intended cadence (informational; Task Scheduler enforces schedule):

```json
{
  "DashboardSnapshot": {
    "PiutangIntervalMinutes": 15,
    "InventoryIntervalMinutes": 60,
    "SalesIntervalMinutes": 30
  }
}
```

Live aggregation fallback has been **removed** from the portal API read path. Dashboard endpoints require populated snapshot tables.
