# Sales Omzet Aggregate — Deploy & Backfill Runbook

This runbook covers first-time deployment of `BTR_SalesOmzet` and the production backfill reconcile. Normal day-to-day use remains **RO2 → Proses** (scoped reconcile for the selected period).

## Prerequisites

- SQL publish access for `btr.sql`
- Application deploy (`btr.distrib`, `btr.infrastructure`, `btr.application`, `btr.domain`)
- Dev/staging DB smoke-tested before production

## 1. Database publish

1. Publish or run scripts from `btr.sql`:
   - `Tables/SalesContext/BTR_SalesOmzet.sql` (table + indexes)
   - `DataSeeds/BTR_ParamNo_SalesOmzet.sql` (prefix `SO` for `SalesOmzetWriter`)
2. Verify:

   ```sql
   SELECT * FROM BTR_ParamNo WHERE Prefix = 'SO';
   SELECT COUNT(*) FROM BTR_SalesOmzet;
   ```

## 2. Application deploy

Deploy binaries built from the branch that includes Phases 1–5 (aggregate read path, reconcile worker, RO2 integration).

Legacy RO2 **UNION** read SQL is removed; emergency rollback requires restoring the previous `SalesOmzetDal` from git. The `BTR_SalesOmzet` table can remain after rollback.

## 3. First-time backfill (Full reconcile)

Run **one** full reconcile after deploy to populate/refresh all aggregate rows.

### Option A — Integration test (recommended for ops with test harness)

From `btr.test`, run:

- `SalesOmzetReconcileTest.UT3_FullReconcile_CompletesWithoutError`

Configure connection like `SalesOmzetEntityDalTest` / existing `SalesOmzetReconcileTest` constructor (`DatabaseOptions`).

Request shape:

```csharp
_worker.Execute(new ReconcileSalesOmzetRequest
{
    Periode = new Periode(DateTime.Today.AddYears(-10), DateTime.Today), // ignored for source lists; required on request
    Scope = ReconcileSalesOmzetScope.Full,
    UserId = "deploy-backfill"
});
```

Inspect `request.Result` for counts and duration.

### Option B — RO2 admin button

1. Open **Sales Omzet Info** (RO2).
2. Click **Rebuild Semua** (confirm warning).
3. Full reconcile runs (`Scope = Full`); status label shows row counts and duration.

### Transaction / timeout note

Full reconcile uses a **single** database transaction. On very large databases, if timeout occurs:

- Run during a maintenance window, or
- Split backfill by calendar year (temporary code or repeated scoped runs over historical periods — slower but safer), or
- Increase command timeout at SQL connection level (ops decision).

Document any batching approach used in your environment.

## 4. Post-backfill verification

1. Open RO2, set **Periode Omzet** (checkbox unchecked), select current quarter, click **Proses**.
2. Compare sample rows to [Testing checklist](../plans/sales-omzet-aggregate-implementation.md#testing-checklist):
   - Order in Jan without KembaliFaktur: hidden in Omzet Period, visible in Sales Period
   - Order Jan + KembaliFaktur Feb: Feb omzet period, Jan sales period
3. Run **Proses** twice on the same period — no duplicate `OrderId` / `FakturId` in `BTR_SalesOmzet`.

## 5. Normal operations

- Users run **Proses** on RO2 for their report window (max 122 days).
- Each Proses: **scoped** reconcile (`PeriodeScoped`) for selected dates, then list with Omzet Period or Sales Period mode.
- Reconcile metrics appear in the form status label (orders/fakturs processed, rows refreshed, duration).

## 6. Future work (out of scope)

- Scheduled reconcile in `btr.sync` (separate .NET 4.8 stack; no reference to `btr.application`)
- Event-driven reconcile on Save Faktur / Create Order
- Replacing `BTR_OrderMap` in production flows

## Rollback

1. Restore previous application build (legacy UNION read DAL).
2. `BTR_SalesOmzet` may be left in place or dropped after backup (product decision).
3. No automatic downgrade of aggregate data.
