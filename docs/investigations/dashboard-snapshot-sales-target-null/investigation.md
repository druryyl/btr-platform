# Investigation: Dashboard Snapshot Refresh Fails on Sales and Salesman Domains

**Date:** 2026-06-10  
**Reporter:** Operations (via `btr.portal.worker` scheduler)  
**Environment:** Database `btr` on server `JUDE7`  
**Affected component:** `btr.portal.worker` — dashboard snapshot refresh (`Domain="All"`)

---

## Summary

Scheduled dashboard snapshot refresh fails for **Sales** and **Salesman** domains with `ArgumentNullException: Value cannot be null. Parameter name: source`. The failure is a **code defect**, not missing database objects or corrupt data. It occurs when `BTR_SalesOmzetTarget` has **no rows** for the current month while principal targets exist in `BTR_SalesPersonPrincipalTarget`.

---

## Expected Behavior

Per [Sales Person–Principal Target](../../features/sales-person-principal-target/feature.md) coexistence rules and [Materialized Dashboard](../../features/materialized-dashboard/materialized-dashboard-domain.md) semantics:

1. Dashboard snapshot refresh should complete successfully on schedule (Sales every 30 min, Salesman every 30 min).
2. When principal targets exist for a month, rep-level totals should use the **sum of principal targets**.
3. Legacy `BTR_SalesOmzetTarget` rows are a **fallback only** when no principal targets exist for a rep/month.
4. When no legacy rows exist at all, refresh should still succeed — treating legacy contribution as zero and resolving totals from principal targets alone.

---

## Actual Behavior

On 2026-06-10 at 16:00, `WorkerRunCoordinator` started a full dashboard refresh. After ~2 minutes, refresh failed with two inner exceptions (Sales and Salesman domains).

| Domain | Failing step | Error |
|--------|--------------|-------|
| Sales | `SumTargetAmountForMonth` → `ListLegacyTargetsForMonth` | `ArgumentNullException: source` |
| Salesman | `ListTargetsForMonth` → `ListLegacyTargetsForMonth` | `ArgumentNullException: source` |

Other domains in the same run were not reported as failed in the provided log excerpt.

---

## Reproduction

**Conditions (confirmed on JUDE7 / `btr`):**

```sql
-- Legacy targets for current month: 0
SELECT COUNT(*) FROM BTR_SalesOmzetTarget
WHERE TargetYear = 2026 AND TargetMonth = 6;
-- Result: 0

-- Principal targets for current month: present
SELECT COUNT(*) FROM BTR_SalesPersonPrincipalTarget
WHERE TargetYear = 2026 AND TargetMonth = 6;
-- Result: 4 (2 salespeople: SP003, SP04A)

-- Legacy table entirely empty
SELECT COUNT(*) FROM BTR_SalesOmzetTarget;
-- Result: 0
```

**Steps:**

1. Ensure `BTR_SalesPersonPrincipalTarget` has rows for the current year/month.
2. Ensure `BTR_SalesOmzetTarget` has **no** rows for that year/month (or is empty).
3. Run `btr.portal.worker` dashboard refresh for `Domain="All"` or trigger Sales/Salesman domains individually.

**Result:** Refresh throws `ArgumentNullException` at `SalesOmzetTargetDal.ListLegacyTargetsForMonth` line 134.

---

## Affected Workflow

| Workflow | Impact |
|----------|--------|
| Scheduled dashboard snapshot refresh (`btr.portal.worker`) | Sales and Salesman domains fail; `RefreshAllDashboardSnapshotsWorker` aggregates failures |
| Portal Sales dashboard (`/dashboard/sales`) | Snapshot not updated; stale or missing KPIs including Total Target |
| Portal Salesman dashboard (`/dashboard/salesman`) | Snapshot not updated; per-rep target resolution unavailable |
| Executive dashboard home (Layer A KPIs sourced from snapshots) | May show stale Sales/Salesman metrics |

Desktop RO2 and SM6 principal-target maintenance are **not** affected — only the portal worker read path fails.

---

## Root Cause

### Primary cause

`SalesOmzetTargetDal.ListLegacyTargetsForMonth` calls:

```csharp
var rows = conn.Read<TargetRowWithId>(sql, dp).ToList();
```

`DapperHelper.Read<T>` (`btr.nuna/Infrastructure/DapperHelper.cs`) returns **`null`** when the SQL query returns zero rows:

```csharp
if (result.Any())
    return result;
else
    return default;  // null for IEnumerable<T>
```

Calling `.ToList()` on `null` throws `ArgumentNullException` with parameter name `source`.

This is **not** a missing-table or connection error. Both tables exist on JUDE7. The query succeeds but returns no legacy rows.

### Why it surfaced now

The business has adopted **principal-level targets** (SM6 / `BTR_SalesPersonPrincipalTarget`) without populating legacy `BTR_SalesOmzetTarget`. On JUDE7, the legacy table is completely empty. This is valid per coexistence rules but exposes a latent null-handling bug in `SalesOmzetTargetDal`.

### Call chain

**Sales domain:**

```
RefreshDashboardSalesSnapshotWorker.Execute (line 78)
  → ISalesOmzetTargetDal.SumTargetAmountForMonth
    → SalesOmzetTargetDal.SumTargetAmountForMonth (line 31)
      → ListLegacyTargetsForMonth (line 134) ← throws
```

**Salesman domain:**

```
RefreshDashboardSalesmanSnapshotWorker.Execute (line 104)
  → ISalesOmzetTargetDal.ListTargetsForMonth
    → SalesOmzetTargetDal.ListTargetsForMonth (line 65)
      → ListLegacyTargetsForMonth (line 134) ← throws
```

### Secondary latent defect (same pattern)

| Location | Risk |
|----------|------|
| `SalesOmzetTargetDal.GetLegacyTargetAmount` (line 115) | `conn.Read<TargetRow>(...).FirstOrDefault()` — would throw if called with no matching legacy row |
| `SalesPersonPrincipalTargetDal.SumByPeriod` (line 140) | `conn.Read<SumRow>(...).ToList()` — would throw if **no** principal targets exist for the month (masked today because 4 rows exist) |

The immediate production failure is driven by empty legacy targets while principal targets exist. The principal DAL has the same pattern but was not hit in this incident.

---

## Severity

| Dimension | Assessment |
|-----------|------------|
| **Business impact** | **High** — Sales and Salesman portal dashboards stop refreshing |
| **Data integrity** | **None** — no data corruption; read-only failure |
| **Frequency** | **Every refresh** while legacy table has no rows for the active month |
| **User visibility** | Portal shows stale snapshots; refresh log marks Sales/Salesman as failed |

---

## Recommended Fix

Hand to **Architect / Implementer**. Business rule is unchanged; this is defensive null handling aligned with existing worker patterns.

### 1. Fix `SalesOmzetTargetDal` (required)

Treat empty legacy query results as an empty list, consistent with other dashboard workers:

```csharp
// ListLegacyTargetsForMonth
var rows = conn.Read<TargetRowWithId>(sql, dp)?.ToList()
    ?? new List<TargetRowWithId>();

// GetLegacyTargetAmount
var row = conn.Read<TargetRow>(sql, dp)?.FirstOrDefault();
```

After fix, `SumTargetAmountForMonth` for June 2026 on JUDE7 should return **1,450,000** (SP003: 700,000 + SP04A: 750,000 from principal sums).

### 2. Fix `SalesPersonPrincipalTargetDal.SumByPeriod` (recommended, same defect class)

```csharp
var rows = conn.Read<SumRow>(sql, dp)?.ToList()
    ?? new List<SumRow>();
```

Prevents failure when a month has zero principal targets (legacy-only or no targets at all).

### 3. Add unit test (recommended)

Extend `SalesOmzetTargetResolutionTest` or add integration test covering:

- Principal targets present, legacy empty → `SumTargetAmountForMonth` and `ListTargetsForMonth` succeed
- Both empty → return 0 / empty dictionary
- Legacy only (stub principal dal returning empty) → legacy values used

No database migration or data fix is required on JUDE7.

---

## Verification Plan

| Check | Expected |
|-------|----------|
| Run worker refresh for Sales domain on JUDE7 | Completes without error |
| Run worker refresh for Salesman domain on JUDE7 | Completes without error |
| `BTRPD_DashboardSalesSnapshot` (or equivalent CURRENT row) | `TotalTarget` ≈ 1,450,000 for June 2026 |
| `BTRPD_DashboardSalesmanSnapshot` | Per-rep targets resolved for SP003, SP04A |
| Refresh log | Sales and Salesman marked Success |
| Month with no targets in either table | Refresh succeeds; Total Target = 0 |

---

## Data Notes (JUDE7 / `btr` at time of investigation)

| Table | June 2026 state |
|-------|-----------------|
| `BTR_SalesOmzetTarget` | 0 rows (entire table empty) |
| `BTR_SalesPersonPrincipalTarget` | 4 rows, 2 salespeople (SP003: 700,000; SP04A: 750,000 aggregated) |

This state is **expected** after principal-target rollout without legacy migration.

---

## References

- Log: `btr-portal-worker-2026-10.log` (2026-06-10 16:02:24)
- Code: `btr.infrastructure/SalesContext/SalesOmzetAgg/SalesOmzetTargetDal.cs`
- Code: `btr.nuna/Infrastructure/DapperHelper.cs`
- Feature: `docs/features/sales-person-principal-target/feature.md` (coexistence rules)
- Feature: `docs/features/materialized-dashboard/materialized-dashboard-domain.md`
