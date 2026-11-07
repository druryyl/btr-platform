# Investigation — SM7 Jadwal Kunjungan throws missing `ROUTE_CYCLE_ANCHOR_DATE`

**Type:** Bug investigation (Analyst + Architect)  
**Surface:** Opening menu `SM7 - Jadwal Kunjungan` in BTR Desktop (`btr.distrib`)  
**Status:** Root cause confirmed  
**Related artifacts:** `docs/features/visit-plan/feature.md`, `docs/ops/visit-plan-deploy.md`, `docs/investigations/field-activity-overview-plan-always-zero.md`

---

## 1. Executive Summary

When opening SM7 Jadwal Kunjungan, the form initializes and calls `JadwalKunjunganForm.RefreshPlans()` which calls `_calendar.ResolveHariRuteId(SelectedVisitDate)`. `RuteCycleCalendar.ResolveHariRuteId` calls `GetAnchorDate()`, which queries `BTR_ParamSistem` for parameter code `ROUTE_CYCLE_ANCHOR_DATE`. If the parameter does not exist or has an empty value, `GetAnchorDate()` throws `System.InvalidOperationException: System parameter 'ROUTE_CYCLE_ANCHOR_DATE' is not configured.` This matches the reported error exactly.

The visit-plan feature requires two system parameters: `ROUTE_CYCLE_ANCHOR_DATE` (required for cycle calculation) and `VISIT_PLAN_HORIZON_DAYS` (optional, defaults to 90). The codebase has seeds for both in `btr.sql/DataSeeds/BTR_ParamSistem_VisitPlan.sql`, but they may not have been applied to the target database.

---

## 2. Reproduction Path

1. User opens BTR Desktop and clicks menu `SM7 - Jadwal Kunjungan` (registered in `MainForm` / menu seeds).
2. DI resolves `JadwalKunjunganForm` with `IRuteCycleCalendar` (`RuteCycleCalendar`).
3. `JadwalKunjunganForm` constructor calls `RefreshPlans()` at the end (line ~67).
4. `RefreshPlans()` sets `CycleLabel` via `_calendar.ResolveHariRuteId(SelectedVisitDate)` and `_calendar.GetCycleWeekLabel(...)` (lines ~156-160). On non-Sunday dates, these call `GetAnchorDate()`.
5. `RuteCycleCalendar.GetAnchorDate()` (lines ~54-70 in `RuteCycleCalendar.cs`):
   - Checks cache (empty initially)
   - Calls `_paramSistemDal.GetData(new ParamSistemModel(AnchorDateParamCode))`
   - If param is null or empty → throws `InvalidOperationException` with message `"System parameter 'ROUTE_CYCLE_ANCHOR_DATE' is not configured."`
6. Exception propagates up and is displayed as the reported error.

**Note:** If the selected visit date is a Sunday, `ResolveHariRuteId` returns `null` early (line ~21-22) and does not call `GetAnchorDate()`. However the form defaults `VisitDatePicker.Value = DateTime.Today` and calls `RefreshPlans()` immediately - if today is not Sunday, it will throw. Also even if today is Sunday, changing the date picker triggers `RefreshPlans()` via event handler (line ~74) and would throw on non-Sunday dates.

---

## 3. Evidence

### Code references
- `src/j05-btr-distrib/btr.application/SalesContext/VisitPlanAgg/Services/RuteCycleCalendar.cs:9,24,40,54-70` - throws if param missing/invalid
- `src/j05-btr-distrib/btr.distrib/SalesContext/VisitPlanAgg/JadwalKunjunganForm.cs:36-68,151-180` - constructor calls RefreshPlans() which uses calendar
- `src/j05-btr-distrib/btr.sql/DataSeeds/BTR_ParamSistem_VisitPlan.sql:1-9` - seeds both required params
- `docs/ops/visit-plan-deploy.md:22,55-61` - deployment instructions mention SM7 and params
- `docs/investigations/field-activity-overview-plan-always-zero.md:18,86,98,126,139,169,181` - same root cause documented

### Expected parameter values
- `ROUTE_CYCLE_ANCHOR_DATE`: Monday anchor in `yyyy-MM-dd` format (e.g., `2026-01-05` as per seeds/docs)
- `VISIT_PLAN_HORIZON_DAYS`: integer days for horizon (e.g., `90` as per seeds; optional with default 90 in `RegenerateVisitPlanWorker`)

---

## 4. Root Cause

**Primary cause:** System parameter `ROUTE_CYCLE_ANCHOR_DATE` is not configured in `BTR_ParamSistem` table for the database the BTR Desktop application connects to.

**Contributing factors:**
1. Visit-plan feature was added recently (SM7/Jadwal Kunjungan) but database seeds were not applied during deployment/migration
2. The calendar service fails fast with a clear exception, but this blocks the entire SM7 form from opening
3. Similar to the portal issue documented in `field-activity-overview-plan-always-zero.md` where missing params prevented visit plan materialization

---

## 5. Recommended Solution

### Immediate fix (unblock users)
1. **Add the missing parameter to the database**. Run the seed script or insert directly:
```sql
IF NOT EXISTS (SELECT 1 FROM BTR_ParamSistem WHERE ParamCode = 'ROUTE_CYCLE_ANCHOR_DATE')
	INSERT INTO BTR_ParamSistem (ParamCode, ParamValue)
	VALUES ('ROUTE_CYCLE_ANCHOR_DATE', '2026-01-05');
GO

IF NOT EXISTS (SELECT 1 FROM BTR_ParamSistem WHERE ParamCode = 'VISIT_PLAN_HORIZON_DAYS')
	INSERT INTO BTR_ParamSistem (ParamCode, ParamValue)
	VALUES ('VISIT_PLAN_HORIZON_DAYS', '90');
GO
```
2. **Verify the anchor date is correct** for the business cycle. The default `2026-01-05` (first Monday of 2026) is used in seeds/docs - confirm with business if a different anchor is needed.
3. **Restart/reopen BTR Desktop** and test opening SM7 again.

### Long-term improvements (prevent recurrence)
1. **Make the error more user-friendly**: In `JadwalKunjunganForm`, catch the exception from calendar initialization/refresh and show a clear message directing to configuration, or disable the form gracefully.
2. **Add validation on startup/feature access**: Check for required parameters when entering visit-plan features and provide actionable guidance.
3. **Improve deployment checklist**: Ensure `BTR_ParamSistem_VisitPlan.sql` is applied as part of visit-plan feature deployment (as noted in `visit-plan-deploy.md`).
4. **Consider fallback/defaults**: For `ROUTE_CYCLE_ANCHOR_DATE`, could have a sensible default, but the business logic requires a specific anchor - better to fail fast with clear message than use wrong cycle.

---

## 6. Verification

After applying the fix:
1. Query: `SELECT * FROM BTR_ParamSistem WHERE ParamCode IN ('ROUTE_CYCLE_ANCHOR_DATE','VISIT_PLAN_HORIZON_DAYS')` - both rows should exist
2. Open BTR Desktop → Menu SM7 Jadwal Kunjungan - form should open without exception
3. Change visit date to various days (including non-Sunday) - cycle label should display correctly (e.g., "Minggu1 / H11" etc.)
4. Base plan, effective plan, and exceptions grids should load without errors (may be empty if no data)

---

## 7. Notes

- This is the same issue as documented in the field activity overview investigation - the visit-plan subsystem requires these parameters to function.
- The form is properly implemented; the issue is purely missing configuration/data.
- No code changes are required for the immediate fix - only database configuration.