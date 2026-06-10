# Portal Worker Console Progress — Example Output

## Implementation Summary

The dashboard snapshot worker (`btr.portal.worker`) now provides structured console feedback during long-running refreshes.

### Components

| Layer | Location | Purpose |
|-------|----------|---------|
| Progress abstraction | `btr.application/.../Progress/` | `IWorkerProgressReporter`, ambient `WorkerProgressScope`, no-op default for API/tests |
| Console visualization | `btr.portal.worker/Progress/` | Task checklist, phase progress bars, 30s heartbeat, optional colors |
| Orchestration | `WorkerRunCoordinator.cs` | Startup plan, config/DB validation, final summary, Manual-only exit prompt |

### Behavior

- **Startup plan** lists actual steps based on `--domain` (single domain or all 10 domains).
- **Phase hooks** in each snapshot worker report Initialize → Load → Aggregate → Save without changing business logic.
- **Multi-load domains** (Collection, Location, etc.) show sub-phase progress (`3 / 8`) with ASCII bar and ETA.
- **Heartbeat** prints every 30 seconds during in-progress steps so users know the process is alive.
- **NLog** file/console logging is unchanged; user-facing progress uses plain `Console.Out`.

### Usage

```text
btr.portal.worker.exe --domain Sales --triggered-by Manual
btr.portal.worker.exe --domain All --triggered-by Scheduler
```

---

## Example: Single Domain (Sales, Manual)

```text
==================================================
BTR Dashboard Snapshot Worker
==================================================
Domain: Sales | Triggered by: Manual

This process will perform:

[ ] Load Configuration
[ ] Validate Database Connection
[ ] Refresh Sales Snapshot
[ ] Generate Summary

Please wait...

[>] Load Configuration
[x] Load Configuration completed
    Using appsettings.json
    Duration: 00:00:00

[>] Validate Database Connection
[x] Validate Database Connection completed
    Server: JUDE7 | Database: BTR_Prod
    Duration: 00:00:01

[>] Refresh Sales Snapshot
    [>] Initialize refresh log
    [x] Initialize refresh log completed
    [>] Load source data
    [x] Load source data completed
        Records: 4,832
    [>] Aggregate metrics
    [x] Aggregate metrics completed
    [>] Save snapshot
    [x] Save snapshot completed
[x] Refresh Sales Snapshot completed
    Duration: 00:01:45

[>] Generate Summary
[x] Generate Summary completed
    Duration: 00:00:00


==================================================
PROCESS COMPLETED
==================================================

Load Configuration             OK    00:00:00
Validate Database Connection   OK    00:00:01
Refresh Sales Snapshot         OK    00:01:45
Generate Summary               OK    00:00:00

Total Duration : 00:01:46

Press any key to exit...
```

## Example: Long Operation Heartbeat

```text
[>] Refresh Collection Snapshot
    [>] Load source data
    Collection: Load open balances
    [####----------------] 20%
    1 / 5
    Estimated remaining: 00:02:40

Still processing Load source data...
Elapsed: 00:00:30

Still processing Load source data...
    Collection: Load pelunasan
    [############--------] 60%
    3 / 5
    Estimated remaining: 00:01:05
```

## Example: All Domains (abbreviated)

```text
==================================================
BTR Dashboard Snapshot Worker
==================================================
Domain: All | Triggered by: Scheduler

This process will perform:

[ ] Load Configuration
[ ] Validate Database Connection
[ ] Refresh Piutang Snapshot
[ ] Refresh Inventory Snapshot
[ ] Refresh InventoryRisk Snapshot
[ ] Refresh Sales Snapshot
[ ] Refresh Purchasing Snapshot
[ ] Refresh PurchasingManagement Snapshot
[ ] Refresh Customer Snapshot
[ ] Refresh Salesman Snapshot
[ ] Refresh Collection Snapshot
[ ] Refresh Location Snapshot
[ ] Generate Summary

Please wait...

... (each domain progresses through [>] / [x] states) ...

==================================================
PROCESS COMPLETED
==================================================

Load Configuration             OK    00:00:00
Validate Database Connection   OK    00:00:01
Refresh Piutang Snapshot       OK    00:01:23
Refresh Inventory Snapshot     OK    00:02:10
Refresh InventoryRisk Snapshot OK    00:01:55
Refresh Sales Snapshot         OK    00:01:45
Refresh Purchasing Snapshot    OK    00:01:20
Refresh PurchasingManagement Snapshot OK 00:00:55
Refresh Customer Snapshot      OK    00:02:30
Refresh Salesman Snapshot      OK    00:02:05
Refresh Collection Snapshot    OK    00:03:15
Refresh Location Snapshot      OK    00:02:50
Generate Summary               OK    00:00:00

Total Duration : 00:12:15
```
