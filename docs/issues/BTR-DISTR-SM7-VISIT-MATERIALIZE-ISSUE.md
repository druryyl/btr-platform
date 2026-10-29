# ISSUE

## Metadata

ID: `BTR-DISTR-SM7-VISIT-MATERIALIZE-001`
Type: CHANGE-REQUEST
Status: New
Title: **SM7-Jadwal Kunjungan — Enable visit data materialization within menu**

## Source

Reported By: User
Reported Date: 2026-12-19

## Description

In `src/j05-btr-distrib` menu SM7-Visit Kunjungan, specifically `SM7JadwalKunjunganMenu` / `JadwalKunjunganForm` in `SalesContext\VisitPlanAgg`, visit data is not loaded automatically because the data requires prior materialization. Materialization is currently performed by a separate application, possibly `src/j05-btr-distrib/btr.visitplan.worker`. The request is to enable visit data materialization to be performed from within the SM7 menu itself.

## Desired Outcome

Users can trigger visit data materialization directly from SM7-Jadwal Kunjungan for the selected sales person and selected date. After materialization, the visit data is loaded and displayed in the SM7 UI, eliminating the need to use a separate application for materialization first.

## Current Situation

- SM7-Jadwal Kunjungan does not automatically load visit data.
- Data must be materialized first in a separate app before it becomes visible in SM7.
- No in-menu mechanism exists to materialize visit data.

## Evidence

No logs, screenshots, or error messages provided at intake.

## Notes

- Materialization application suspected to be `src/j05-btr-distrib/btr.visitplan.worker`, confirmation pending.
- Desired scope per reporter: materialize for selected sales person and selected date, with result shown in SM7.
