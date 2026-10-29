# Investigation Report: SM7-Jadwal Kunjungan Error `ROUTE_CYCLE_ANCHOR_DATE is not configured`

**Date:** 2026-10-26  
**Artifact:** INVESTIGATION  
**Target System:** `src/j05-btr-distrib` (`btr.distrib`)  
**Feature / Menu:** `SM7 - Jadwal Kunjungan`  
**Reported Error:**  
`System.InvalidOperationException: System parameter 'ROUTE_CYCLE_ANCHOR_DATE' is not configured.`

---

## 1. Executive Summary

When opening the menu **`SM7 - Jadwal Kunjungan`** in `btr.distrib`, the application throws an unhandled `System.InvalidOperationException` stating that `ROUTE_CYCLE_ANCHOR_DATE` is not configured.

Investigation into the source code and database architecture reveals:
1. **Trigger during instantiation:** In `MainForm.SM7JadwalKunjunganButton_Click`, resolving `JadwalKunjunganForm` via DI triggers its constructor. The constructor binds the sales dropdown and immediately calls `RefreshPlans()`.
2. **Calendar cycle calculation:** `RefreshPlans()` calls `IRuteCycleCalendar.ResolveHariRuteId(DateTime visitDate)` to determine the 2-week cycle code (`H11`–`H16` for Minggu-1, `H21`–`H26` for Minggu-2).
3. **Missing system parameter:** `RuteCycleCalendar.GetAnchorDate()` queries the database table `BTR_ParamSistem` for `ParamCode = 'ROUTE_CYCLE_ANCHOR_DATE'`. Because this record does not exist in the connected database, it throws the `InvalidOperationException`.
4. **Seed omission:** The seed script `src/j05-btr-distrib/btr.sql/DataSeeds/BTR_ParamSistem_VisitPlan.sql` was provided during visit plan implementation but was not applied to the running database, even though the menu role seed (`BTR_RoleMenu_VisitPlan.sql`) was applied.
5. **UI constraint:** In `XX2 - Parameter Sistem` (`ParamSistemForm`), `AllowUserToAddRows` is set to `false`, preventing administrators from adding missing parameters through the UI if they do not already exist in the database table.

---

## 2. Concrete Evidence & Call Chain Analysis

### 2.1 Error Call Stack

The error originates along this call path:

```text
MainForm.SM7JadwalKunjunganButton_Click(object sender, EventArgs e)
  │ (btr.distrib/SharedForm/MainForm.cs:243)
  ▼
ServiceProvider.GetRequiredService<JadwalKunjunganForm>()
  │ (btr.distrib/SalesContext/VisitPlanAgg/JadwalKunjunganForm.cs:67)
  ▼
JadwalKunjunganForm..ctor(...)
  │ (calls InitSalesComboBox() which sets SelectedSalesPersonId, then calls RefreshPlans())
  ▼
JadwalKunjunganForm.RefreshPlans()
  │ (btr.distrib/SalesContext/VisitPlanAgg/JadwalKunjunganForm.cs:156)
  ▼
RuteCycleCalendar.ResolveHariRuteId(SelectedVisitDate)
  │ (btr.application/SalesContext/VisitPlanAgg/Services/RuteCycleCalendar.cs:24)
  ▼
RuteCycleCalendar.GetAnchorDate()
  │ (btr.application/SalesContext/VisitPlanAgg/Services/RuteCycleCalendar.cs:59-63)
  ▼
ParamSistemDal.GetData(new ParamSistemModel("ROUTE_CYCLE_ANCHOR_DATE"))
  │
  ▼
SELECT ParamCode, ParamValue FROM BTR_ParamSistem WHERE ParamCode = 'ROUTE_CYCLE_ANCHOR_DATE'
  --> Returns NULL / 0 rows!
  --> throws InvalidOperationException("System parameter 'ROUTE_CYCLE_ANCHOR_DATE' is not configured.")
```

### 2.2 Source Code Evidence

#### `RuteCycleCalendar.cs` (lines 55–70)
```csharp
public DateTime GetAnchorDate()
{
    if (_cachedAnchorDate.HasValue)
        return _cachedAnchorDate.Value;

    var param = _paramSistemDal.GetData(new ParamSistemModel(AnchorDateParamCode));
    if (param == null || string.IsNullOrWhiteSpace(param.ParamValue))
        throw new InvalidOperationException(
            $"System parameter '{AnchorDateParamCode}' is not configured.");

    if (!DateTime.TryParse(param.ParamValue, out var anchorDate))
        throw new InvalidOperationException(
            $"System parameter '{AnchorDateParamCode}' has invalid date value '{param.ParamValue}'.");

    _cachedAnchorDate = anchorDate.Date;
    return _cachedAnchorDate.Value;
}
```

#### `JadwalKunjunganForm.cs` (lines 63–68, 151–160)
```csharp
InitSalesComboBox();
InitGrids();
VisitDatePicker.MinDate = DateTime.Today;
VisitDatePicker.Value = DateTime.Today;
RefreshPlans(); // Called directly in the constructor!
```
```csharp
private void RefreshPlans()
{
    if (string.IsNullOrWhiteSpace(SelectedSalesPersonId))
        return;

    var hariRuteId = _calendar.ResolveHariRuteId(SelectedVisitDate);
    CycleLabel.Text = string.IsNullOrWhiteSpace(hariRuteId)
        ? "Minggu (tidak ada rute)"
        : $"{_calendar.GetCycleWeekLabel(SelectedVisitDate)} / {hariRuteId}";
    ...
```

#### `ParamSistemForm.cs` (lines 54–56)
```csharp
ParamGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
ParamGrid.Columns["ParamCode"].ReadOnly = true;
ParamGrid.AllowUserToAddRows = false; // Admin cannot add new keys via UI
```

### 2.3 Existing Seed Script

In `src/j05-btr-distrib/btr.sql/DataSeeds/BTR_ParamSistem_VisitPlan.sql`:
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

---

## 3. Business & Operational Background

### Why `ROUTE_CYCLE_ANCHOR_DATE` is Required
- In BTR, salesman sales routes run on a bi-weekly cycle (14 calendar days):
  - **Week 1 (`Minggu1`):** Days `H11` (Monday) through `H16` (Saturday).
  - **Week 2 (`Minggu2`):** Days `H21` (Monday) through `H26` (Saturday).
  - Sunday has no route (`null`).
- To calculate which week/day code any calendar date falls on, `RuteCycleCalendar` requires a reference epoch (`AnchorDate`) that is a known **Monday**.
- Calculating days elapsed:
  ```text
  daysSinceAnchor = (visitDate - anchorDate).Days
  cycleDayIndex = daysSinceAnchor % 14
  weekInCycle = cycleDayIndex < 7 ? 1 : 2
  ```
- If the anchor date is not set, the calendar engine cannot reliably evaluate whether a given date is `Minggu1` or `Minggu2`. An arbitrary fallback is not used in `RuteCycleCalendar` to avoid inverting route schedules if the application guess differs from operational reality.

---

## 4. Root Causes

1. **Database Seed Not Executed:** The data seed `BTR_ParamSistem_VisitPlan.sql` was not run on the database environment hosting the application.
2. **Absence of User-Facing Parameter Insertion:** The parameter management UI (`XX2 - Parameter Sistem`) intentionally forbids adding rows (`AllowUserToAddRows = false`), which prevented users/administrators from self-resolving the missing parameter in the GUI.
3. **No Graceful UI Fallback / Error Handling in SM7 Form Constructor:** Calling `RefreshPlans()` inside the form constructor without handling configuration exceptions crashes the form opening workflow, preventing the UI from rendering an actionable guidance message.

---

## 5. Recommended Solutions

### 5.1 Immediate Operational Resolution (Database Configuration)
Execute the seed script against the active database (`btr` / active database instance):

```sql
IF NOT EXISTS (SELECT 1 FROM BTR_ParamSistem WHERE ParamCode = 'ROUTE_CYCLE_ANCHOR_DATE')
BEGIN
    INSERT INTO BTR_ParamSistem (ParamCode, ParamValue)
    VALUES ('ROUTE_CYCLE_ANCHOR_DATE', '2026-01-05');
END;

IF NOT EXISTS (SELECT 1 FROM BTR_ParamSistem WHERE ParamCode = 'VISIT_PLAN_HORIZON_DAYS')
BEGIN
    INSERT INTO BTR_ParamSistem (ParamCode, ParamValue)
    VALUES ('VISIT_PLAN_HORIZON_DAYS', '90');
END;
```

> **Note on Value:** `'2026-01-05'` is the first Monday of 2026 and matches the established system standard for the 2-week route calendar cycle. Once inserted, it can be viewed and adjusted via `XX2 - Parameter Sistem`.

---

### 5.2 Application Resilience Improvements (Recommended for Code Base)

1. **Defensive Error Handling in `JadwalKunjunganForm`:**
   - Move or wrap `RefreshPlans()` in a `try...catch (InvalidOperationException ex)` block.
   - Display a clear user message (e.g. `MessageBox.Show("Parameter sistem 'ROUTE_CYCLE_ANCHOR_DATE' belum dikonfigurasi. Hubungi administrator sistem atau atur di menu XX2.")`) rather than letting an unhandled exception crash the menu action.
2. **Defensive Validation in `MainForm.SM7JadwalKunjunganButton_Click`:**
   - Catch configuration errors cleanly during form creation so the MDI workspace remains stable.
