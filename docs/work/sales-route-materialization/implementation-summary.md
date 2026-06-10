# Implementation Summary: Sales Route Materialization

## Status

Remediation complete per reviewer findings (2026-06-11).

## Remediation Changes

| Reviewer action | Resolution |
| --------------- | ---------- |
| Add `RegenerateVisitPlanWorkerTest` | Added `btr.test/SalesContext/RegenerateVisitPlanWorkerTest.cs` (4 tests) |
| Add SM4 → visit plan integration test | Added `btr.test/SalesContext/SalesRuteVisitPlanMaterializationTest.cs` (2 tests) |
| DAL past-date guard on `DeleteFuture` | `VisitPlanDal` now injects `ITglJamDal`, clamps `fromDate` to today, and adds `VisitDate >= @Today` in SQL |
| Demonstrate passing tests | 19 visit-plan tests pass (see Test Results below) |
| Phase 4 staging smoke test | Checklist in `staging-smoke-test.md` |

### Additional fix

`VisitPlanDal` date parameters changed from unsupported `SqlDbType.Date` to `SqlDbType.DateTime` (required by `TypeConvertor`).

## Test Results

Executed 2026-06-11:

```text
dotnet vstest btr.test\bin\Release\btr.test.dll
  --TestCaseFilter:"FullyQualifiedName~RuteCycleCalendarTest|
    FullyQualifiedName~EffectiveVisitPlanResolverTest|
    FullyQualifiedName~RegenerateVisitPlanWorkerTest|
    FullyQualifiedName~SalesRuteVisitPlanMaterializationTest|
    FullyQualifiedName~VisitPlanDalDeleteFutureTest"
```

| Result | Count |
| ------ | ----- |
| Passed | 19 |
| Failed | 0 |

### Test classes

| Class | Tests | Type |
| ----- | ----- | ---- |
| `RuteCycleCalendarTest` | 7 | Unit |
| `EffectiveVisitPlanResolverTest` | 5 | Unit |
| `RegenerateVisitPlanWorkerTest` | 4 | Unit |
| `SalesRuteVisitPlanMaterializationTest` | 2 | Integration (stub pipeline) |
| `VisitPlanDalDeleteFutureTest` | 1 | Integration (SQL) |

## Delivered Artifacts

- Source: `VisitPlanDal.cs` (past-date guard)
- Tests: 3 new test files registered in `btr.test.csproj`
- Ops: existing `src/j05-btr-distrib/docs/ops/visit-plan-deploy.md`
- Feature: `docs/features/visit-plan/feature.md`
- Staging: `docs/work/sales-route-materialization/staging-smoke-test.md`
