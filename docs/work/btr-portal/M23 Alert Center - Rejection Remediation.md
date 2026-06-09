# M23 Alert Center — Rejection Remediation

**Review:** [M23 Alert Center - Review Report.md](./M23%20Alert%20Center%20-%20Review%20Report.md)  
**Status:** REJECTED — 2026-06-09  
**Blocking items:** 2 Major findings

---

## Summary

M23 implementation is structurally complete (backend aggregator, API, frontend, unit tests, documentation). Rejection is due to **incomplete plan compliance**, not missing features.

| Priority | Finding | Effort |
| --- | --- | --- |
| **P1** | Unknown `SignalKey` — skip works, log warning missing | Small (~30 min) |
| **P1** | Manual verification checklist §7.3 not executed | Medium (deploy + 1–2 hours) |
| P2 | Registry test gap for M20 `Overdue` | Small |
| P2 | Missing `SalesAchievementWarning` unit test | Small |

---

## P1 — Add unknown SignalKey logging

### Requirement (Plan §5.3)

> Unknown `SignalKey` at runtime → **log warning**, skip row (do not invent category).

### Current behavior

`DashboardAlertCenterComposer.ApplyDeduplication` (and concentration collection) skips unregistered keys with no log output.

### Remediation steps

1. Add `ILogger<DashboardAlertCenterComposer>` to composer constructor (match logging pattern used in other portal composers/workers).
2. In `ApplyDeduplication`, before `continue` when `TryGetForProducer` fails:

   ```csharp
   _logger.LogWarning(
       "Alert Center skipped unknown signal. Source={Source} SignalKey={SignalKey} EntityType={EntityType} EntityName={EntityName}",
       row.Source, row.SignalKey, row.EntityType, row.EntityName);
   ```

3. Apply same pattern in `BuildConcentrations` producer loop if unknown concentration keys should also warn.
4. Register composer in DI if constructor signature changes (`InfrastructurePortalExtensions.cs` — `AddScoped` already present).
5. Extend `Compose_UnknownSignalKey_SkippedComposeSucceeds` test or add logging assertion per project conventions.

### Done when

- Warning emitted for unregistered producer keys at runtime.
- Existing unit test still passes; compose response unchanged.

---

## P1 — Execute manual verification (Plan §7.3)

### Prerequisite

- API and portal web running against environment with M17–M22 snapshot data.
- Authenticated portal user.

### Checklist

| # | Action | Expected |
| --- | --- | --- |
| 1 | `GET /api/dashboard/alerts` (authenticated) | 200; full `DashboardAlertCenterResponse` JSON |
| 2 | Navigate to `/alerts` | Title **Alert Center**; sections: Platform → Summary → Alerts → Inventory Risk → Concentrations → Navigation |
| 3 | Login | Lands on `/dashboard` (not `/alerts`) |
| 4 | M16 **Open Alert Center** button | Navigates to `/alerts` |
| 5 | Sidebar **Alert Center** | Navigates to `/alerts` |
| 6 | Compare platform banners vs M16 when stale/degraded | Same health semantics |
| 7 | Customer on M17 Overdue + M20 ChronicOverdue | One row (M20 Collection) |
| 8 | Salesman M18 HighOverdueExposure + M20 HighOverdueWorkload | One row (M20 Collection) |
| 9 | Collection category | ≤20 rows; summary badge shows true total + “showing 20” when HasMore |
| 10 | Inventory Risk section | KPI counts only — no SKU rows in Alerts |
| 11 | Concentrations | Concentration signals not in Alerts table |
| 12 | Click dashboard icon on alert row | Correct domain dashboard route |
| 13 | Click report icon (where present) | Report opens with `?q=` entity filter |
| 14 | **View Inventory Risk →** | Opens `/dashboard/inventory-risk` |
| 15 | Refresh button | Reloads without error |
| 16 | Confirm no new snapshot tables/workers | Schema/worker inventory unchanged |
| 17 | Domain dashboard attention lists | Unchanged after M23 deploy |

### Done when

- Update [Implementation Summary](./M23%20Alert%20Center%20-%20Implementation%20Summary.md) manual table with **Pass** and brief notes per row, or attach verification log.
- Request re-review.

---

## P2 — Recommended test improvements

### M20 `Overdue` registry disambiguation

Add to `AlertCenterRegistryTest.cs`:

```csharp
[Fact]
public void Registry_M20Overdue_ResolvesToCollectionCategory()
{
    AlertCenterRegistry.TryGetForProducer("M20", DashboardCollectionAggregator.SignalOverdue, out var entry)
        .Should().BeTrue();
    entry.Category.Should().Be(AlertCenterRegistry.CategoryCollection);
}
```

Remove duplicate `[InlineData(DashboardCollectionAggregator.SignalOverdue)]` if it duplicates M17 `Overdue` in the theory (xUnit currently skips one).

### Sales achievement Warning band

Add to `DashboardAlertCenterComposerTest.cs`:

```csharp
[Fact]
public void Compose_SalesWarningBand_CreatesSyntheticAlert()
{
    var input = FullInput();
    input.Sales.AchievementPercent = 92m; // adjust to Warning threshold per ExecutiveSalesAchievementBandResolver

    var result = Compose(input);
    GetCategoryAlerts(result, AlertCenterRegistry.CategorySales)
        .Should().ContainSingle(a => a.SignalKey == AlertCenterRegistry.SignalSalesAchievementWarning);
}
```

---

## Re-review submission

When P1 items are complete, notify reviewer with:

1. Link to logging change (file + line).
2. Manual verification table updated in Implementation Summary.
3. Confirmation unit tests still pass:

   ```text
   dotnet vstest btr.test/bin/Debug/btr.test.dll --TestCaseFilter:"FullyQualifiedName~AlertCenter"
   ```

Expected outcome after remediation: **APPROVED** (P2 items optional).
