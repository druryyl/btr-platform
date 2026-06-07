# Implementation Summary: Materialized Dashboard Data — Phase 5 (Operations Enhancements)

## Status

Phase 5 is complete. Authenticated users can trigger on-demand dashboard snapshot refresh via the portal API. Operations and monitoring can inspect per-domain refresh health without SQL access.

**Authoritative plan:** [implementation-plan.md](./implementation-plan.md)  
**Prior phase:** [implementation-summary-phase-4.md](./implementation-summary-phase-4.md)

---

## Goal Delivered

Add deferred operational enhancements: manual refresh API and refresh-status observability.

**Exit criterion met:** `POST /api/admin/dashboard/refresh` triggers snapshot workers with `TriggeredBy = Manual`; `GET /api/health/dashboard-snapshots` exposes last refresh attempt per domain.

---

## Scope Delivered vs Deferred

| Item | Status | Notes |
| --- | --- | --- |
| 5.1 `POST /api/admin/dashboard/refresh` | **Done** | JWT required; domain `All\|Piutang\|Inventory\|Sales` |
| 5.1 BTR Desktop trigger | **Deferred** | No existing portal HTTP integration in `btr.distrib`; worker CLI + API cover ops |
| 5.2 `HealthController` refresh status | **Done** | `GET /api/health/dashboard-snapshots` (no auth) |
| 5.3 Layer C piutang open fact | **Deferred** | Not required — shadow reconciliation passed without persisted row-level fact |
| 5.4 Historical monthly snapshot retention | **Deferred** | `CURRENT` row pattern retained per product decision |

---

## API Endpoints

### Manual refresh (authenticated)

```
POST /api/admin/dashboard/refresh
Authorization: Bearer <token>
Content-Type: application/json

{ "domain": "All" }
```

| Domain value | Behavior |
| --- | --- |
| `All` (default) | Piutang → Inventory → Sales via orchestrator |
| `Piutang` | Single-domain refresh |
| `Inventory` | Single-domain refresh |
| `Sales` | Single-domain refresh |

Domain values are **case-insensitive** (`piutang`, `ALL`, etc.). Prefer the worker CLI for `--domain All` when refresh may exceed IIS request timeout (~110 s); see [dashboard-snapshot-worker-runbook.md](./dashboard-snapshot-worker-runbook.md).

**Response:** `RefreshDashboardSnapshotsResponse` with per-domain `RefreshLogId` and `DurationMs`.

**Errors:** Invalid domain → HTTP 400; worker failure → HTTP 500 (logged in `BTR_PortalDashboardRefreshLog`).

### Dashboard snapshot health (public)

```
GET /api/health/dashboard-snapshots
```

Returns latest refresh log row per domain (`Piutang`, `Inventory`, `Sales`) plus configured interval minutes. Overall `status`: `unknown` (no refresh logged for any domain), `ok`, `refreshing` (any domain `Running`), or `degraded` (any domain `Failed`).

---

## Post-Review Fixes

| Reviewer action | Change |
| --- | --- |
| Case-insensitive domain parameter | `NormalizeDomain` canonicalizes `All`, `Piutang`, `Inventory`, `Sales` |
| Health when no refresh history | Overall status `unknown` when all domains have null `LastRefresh` |
| IIS timeout guidance | Documented in runbook — API sync refresh vs worker CLI |

---

## Files Added

| File | Purpose |
| --- | --- |
| `Commands/RefreshDashboardSnapshotsCommand.cs` | MediatR command + handler dispatching to snapshot workers |
| `Models/DashboardSnapshotRefreshStatusModel.cs` | Read model for health status |
| `Controllers/Admin/AdminDashboardRefreshController.cs` | `POST /api/admin/dashboard/refresh` |
| `Models/DashboardRefreshRequest.cs` | Request body DTO |
| `RefreshDashboardSnapshotsHandlerTest.cs` | Handler unit tests (3) |

## Files Modified

| File | Change |
| --- | --- |
| `IDashboardSnapshotRefreshLogDal.cs` | Added `GetLatestPerDomain()` |
| `DashboardSnapshotRefreshLogDal.cs` | SQL query for latest row per domain |
| `HealthController.cs` | Added `GET /api/health/dashboard-snapshots` |
| `PortalPresentationExtensions.cs` | Registered admin controller |
| `btr.application.csproj`, `btr.portal.api.csproj`, `btr.test.csproj` | Registered new sources |
| `btr-portal-domain.md` | Documented manual refresh + health endpoint |
| `dashboard-snapshot-worker-runbook.md` | Added API-based manual refresh and health monitoring |

---

## Test Results

| Suite | Tests | Result |
| --- | --- | --- |
| Phase 5 new tests | 3 | Pass (handler) |
| `btr.application` + `btr.test` build | — | Pass |

---

## Verification Checklist (Phase 5)

| # | Criterion | Status |
| --- | --- | --- |
| 1 | Manual refresh API triggers workers with `TriggeredBy = Manual` | Done |
| 2 | Single-domain and All-domain refresh supported | Done |
| 3 | Health endpoint returns per-domain last refresh status | Done |
| 4 | Domain documentation updated | Done |
| 5 | Layer C / historical retention deferred with rationale | Done |
