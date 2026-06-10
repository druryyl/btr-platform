# Staging Smoke Test: Territory Execution Plan

Manual sign-off checklist for Phase 4 exit criteria.

**Environment:** Staging  
**Date:** _pending operator execution_  
**Executed by:** _pending_

## Prerequisites

- [ ] Schema deployed (`BTR_VisitPlan`, `BTR_VisitPlanException`)
- [ ] Seeds applied (`BTR_ParamSistem_VisitPlan`, `BTR_Menu` SM7, `BTR_RoleMenu_VisitPlan`)
- [ ] `ROUTE_CYCLE_ANCHOR_DATE` confirmed with business
- [ ] Desktop and `btr.visitplan.worker` published

## Automated Pre-check (dev/staging build machine)

```text
dotnet msbuild btr.test\btr.test.csproj /p:Configuration=Release
dotnet vstest btr.test\bin\Release\btr.test.dll ^
  --TestCaseFilter:"FullyQualifiedName~RuteCycleCalendarTest|FullyQualifiedName~EffectiveVisitPlanResolverTest|FullyQualifiedName~RegenerateVisitPlanWorkerTest|FullyQualifiedName~SalesRuteVisitPlanMaterializationTest|FullyQualifiedName~VisitPlanDalDeleteFutureTest"
```

Expected: **19 passed, 0 failed**

## Manual Scenarios

| # | Scenario | Steps | Expected | Pass |
| - | -------- | ----- | -------- | ---- |
| T1 | Initial materialization | Run `btr.visitplan.worker.exe --triggered-by Manual` | Rows exist for `[today, today+horizon]` | [ ] |
| T2 | Template change | Edit SM4 route for one salesman; save | Future `BTR_VisitPlan` rows update; past unchanged | [ ] |
| T3 | Add exception | SM7: add customer on tomorrow | Effective plan includes added customer | [ ] |
| T4 | Replace exception | SM7: replace customer on future date | Effective plan shows replacement | [ ] |
| T5 | Remove exception | SM7: remove customer on future date | Effective plan excludes customer | [ ] |
| T6 | Past-date guard | SM7: attempt exception on past date | Rejected (UI + server) | [ ] |
| T7 | Sunday | Query materialized plan for next Sunday | No base rows | [ ] |
| T8 | SALES role | Login as SALES user | SM7 menu hidden/disabled | [ ] |
| T9 | LEADR role | Login as LEADR user | SM7 accessible | [ ] |
| T10 | Worker recovery | Skip worker 3 days; run manual | Horizon filled; no duplicate rows | [ ] |

## Sign-off

| Role | Name | Date | Approved |
| ---- | ---- | ---- | -------- |
| Implementer | AI Agent | 2026-06-11 | Automated tests pass |
| Operator / QA | | | Pending manual T1–T10 |
