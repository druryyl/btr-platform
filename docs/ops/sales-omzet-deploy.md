# Sales Omzet Aggregate — Deploy & Backfill Runbook

This runbook covers first-time deployment of `BTR_SalesOmzet` and the production backfill reconcile. Normal day-to-day use: **RO2 → Materialisasi** when aggregate data must be refreshed; **Proses** loads the report from `BTR_SalesOmzet` only.

## Prerequisites

- SQL publish access for `btr.sql`
- Application deploy (`btr.distrib`, `btr.infrastructure`, `btr.application`, `btr.domain`)
- Dev/staging DB smoke-tested before production

## 1. Database publish

1. Publish or run scripts from `btr.sql`:
   - `Tables/SalesContext/BTR_SalesOmzet.sql` (table + indexes)
   - `Tables/SalesContext/BTR_SalesOmzetTarget.sql` (monthly omzet targets per sales person; RO2 KPI/chart)
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

### Option B — RO2 materialize dialog

1. Open **RO2 - Sales Omzet** (info form).
2. Click **Materialisasi**.
3. In the dialog, click **Rebuild Semua** (confirm warning).
4. Full reconcile runs (`Scope = Full`); status label shows row counts and duration.

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
3. Run **Materialisasi** twice for the same period — no duplicate `OrderId` / `FakturId` in `BTR_SalesOmzet`.

## 5. Normal operations

- **Proses** on RO2 loads the report for the selected window (max 122 days) from `BTR_SalesOmzet` — no reconcile on that click.
- When source data changed (new orders/faktur, Kembali Faktur): open **Materialisasi**, confirm dates, click **Materialisasi** for scoped reconcile, then **Proses** on the info form.
- Reconcile metrics appear in the materialize dialog status label (orders/fakturs processed, rows refreshed, duration).

## 6. Future work (out of scope)

- Scheduled reconcile in `btr.sync` (separate .NET 4.8 stack; no reference to `btr.application`)
- Event-driven reconcile on Save Faktur / Create Order
- Replacing `BTR_OrderMap` in production flows

## Rollback

1. Restore previous application build (legacy UNION read DAL).
2. `BTR_SalesOmzet` may be left in place or dropped after backup (product decision).
3. No automatic downgrade of aggregate data.
