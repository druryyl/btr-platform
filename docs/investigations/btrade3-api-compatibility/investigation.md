# Investigation — BTrade3 Compatibility With BTrade-Api Since `d92e905f`

| Field | Value |
| --- | --- |
| Date | 2026-09-17 |
| Type | Compatibility / impact investigation |
| Consumer | `src/BTrade3` (Android, `com.elsasa.btrade3`) |
| Provider | `src/j06-pkl-btrade-api` (BTrade-Api / Cloud API) |
| Baseline commit | `d92e905f4e2d7f14096bb82c730605accdcb4e9f` ("Baseline BGud App Development") |
| Compared HEAD | `370ea930fefb514512c17f129acd5178f3ddc05a` |
| Method | Static diff (`git diff d92e905f..HEAD`), controller/contract inspection, ADR review |

## Executive Summary

**Compatibility Status: Compatible today — Partially Compatible against the API's committed security direction.**

No BTrade3 change is required for the application to keep working against the API as it stands at HEAD. The six endpoints BTrade3 consumes were not modified by any of the 15 API commits in the range, remain anonymous, and their request/response shapes are unchanged.

However, the ADRs accepted in this range (`ADR-002`, `ADR-003`) commit the API to requiring a JWT identity on **all** write endpoints, including the legacy order/check-in/customer-location writes BTrade3 uses. That enforcement is **not yet applied** to the legacy endpoints, so BTrade3 still works today, but it is a documented, coordinated breaking change that BTrade3 must absorb before enforcement goes live.

> No implementation was performed. This artifact is the Analyst + Architect investigation output.

## 1. API Endpoints Currently Used by BTrade3

| # | Verb / Route | BTrade3 call site | Purpose |
| --- | --- | --- | --- |
| 1 | `GET api/Brg/{serverId}` | `network/ApiService.kt:22` | Download item (Barang) master |
| 2 | `GET api/Customer/{serverId}` | `network/ApiService.kt:25` | Download customer master |
| 3 | `GET api/SalesPerson/{serverId}` | `network/ApiService.kt:28` | Download salesperson master |
| 4 | `POST api/Order` | `network/ApiService.kt:31` | Upload sales order |
| 5 | `PATCH api/Customer` | `network/ApiService.kt:34` | Upload customer GPS location |
| 6 | `POST api/CheckIn` | `network/ApiService.kt:37` | Upload visit check-in / check-out |

Base URL: `http://dev.smart-ics.com:8089/belajar-api/api/` (`network/NetworkModule.kt:10`).
Transport/auth: Retrofit + Gson + OkHttp logging interceptor only. **No `Authorization` header, no token.** Google Sign-In is a local gate only.

Provider-side mapping:

| # | Controller | Action | Auth today |
| --- | --- | --- | --- |
| 1 | `BrgController.cs:19` | `ListData` | anonymous |
| 2 | `CustomerController.cs:20` | `ListData` | anonymous |
| 3 | `SalesPersonController.cs:19` | `ListData` | anonymous |
| 4 | `OrderController.cs:20` | `UploadData` | anonymous |
| 5 | `CustomerController.cs:44` | `UpdateLocation` | anonymous |
| 6 | `CheckInController.cs:20` | `UploadData` | anonymous |

## 2. What Changed in `j06-pkl-btrade-api` Since `d92e905f`

Range: **15 commits, 67 files, +2473 / −1**. `git diff --name-status` reports **62 added, 5 modified** files; the only deletion is a single newline in `Program.cs`.

### 2.1 Modified existing files (complete list)

| File | Change | Impact on BTrade3 |
| --- | --- | --- |
| `btrade.webapi/Program.cs` | Added `public partial class Program { }` | None — test host hook only |
| `btrade.webapi/Configurations/PresentationService.cs` | Added `JwtOptions` binding + `IJwtTokenService` DI; `SecurityTokenValidators.Clear()` + `JsonWebTokenSecurityTokenValidator` | None without a bearer token; changes only JWT validation path |
| `btrade.webapi/Middlewares/ErrorHandlerMiddleware.cs` | Added `UnauthorizedAccessException` → `401` mapping | Only error path; no legacy endpoint throws this |
| `btrade.sqldb/btrade.sqldb.sqlproj` | Registered new schema objects | None (additive) |
| `j06-pkl-btrade-api.sln` | Added `btrade.webapi.Test` project | None |

**No existing application/domain/infrastructure contract and no existing controller was modified.** `git log` over `OrderController`, `CustomerController`, `CheckInController`, `BrgController`, `SalesPersonController` and their commands (`OrderUploadCommand`, `CheckInUploadCommand`, `CustomerLocationUpdateCommand`) is empty for the range.

### 2.2 New endpoints added (all additive)

| Verb / Route | Controller | Auth |
| --- | --- | --- |
| `POST api/Auth/login` | `AuthController.cs:20` | `[AllowAnonymous]` |
| `POST api/Barcode/sync` | `BarcodeController.cs:23` | `[Authorize]` |
| `GET /api/barcodes/sync` | `BarcodeController.cs:35` | `[Authorize]` |
| `GET api/BarcodeRegistration/pending`, `POST .../ack`, `GET .../status` | `BarcodeRegistrationController.cs` | `[Authorize]` |
| `POST /api/barcode-registration` | `BarcodeRegistrationController.cs:44` | `[Authorize]` |
| `GET api/Driver/{serverId}`, `POST api/Driver` | `DriverController.cs` | `[Authorize]` |
| `POST /api/return-order`, `GET api/ReturnOrder/incremental/{tgl1}/{tgl2}/{serverId}` | `ReturnOrderController.cs` | `[Authorize]` |
| `POST api/User` | `UserController.cs:23` | `[Authorize]` |

These are new-feature endpoints. BTrade3 does not call any of them (`grep` for `Barcode|ReturnOrder|Driver|Auth|User` in `src/BTrade3` returns no matches).

### 2.3 Authentication changes

- `[Authorize]` is applied **only to the new controllers**. Legacy write controllers remain anonymous.
- There is **no** `AddAuthorization` fallback policy and no global filter, so anonymity of legacy routes is unaffected.
- `SecurityTokenValidators.Clear()` / custom validator only execute when a bearer token is presented. BTrade3 presents none.

## 3. Compatibility Verification

| Concern | Finding | Evidence |
| --- | --- | --- |
| Endpoint changes | None for the 6 consumed routes | `git diff --diff-filter=M` lists no controller/command for them |
| Request schema | Unchanged | `OrderUploadCommand`, `CheckInUploadCommand`, `CustomerLocationUpdateCommand` identical to baseline; BTrade3 DTOs match field-for-field |
| Response schema | Unchanged | `JSendOk` envelope `{status, code, data}` still emitted; BTrade3 `*Response.kt` models still align |
| Authentication | No new requirement on consumed routes | No `[Authorize]` on legacy controllers; no fallback policy in `PresentationService.cs`/`Program.cs` |
| Headers/params/payload | Unchanged | Consumed routes and command records untouched; `serverId` still route/body-supplied for legacy APIs (permitted by `ADR-007` §8) |
| Behavioral | Only error-path change (`UnauthorizedAccessException` → 401) | `ErrorHandlerMiddleware.cs`; no legacy endpoint raises it |
| Routing | No conflicts | New routes (`api/Barcode*`, `api/Driver`, `api/User`, `/api/return-order`) do not collide with `api/Brg`, `api/Customer`, `api/SalesPerson`, `api/Order`, `api/CheckIn` |
| Build/compile | BTrade3 untouched | `git diff d92e905f..HEAD -- src/BTrade3/` is empty |

## 4. Impacted Features / Screens

**Today: none.** BTrade3 behavior is unaffected.

**If/when ADR-002 enforcement is extended to legacy write endpoints, the affected surface is:**

| Feature | Screen / component | Endpoint at risk |
| --- | --- | --- |
| Sales Order sync | `OrderSyncScreen` / `OrderSyncViewModel` / `OrderSyncRepository` | `POST api/Order` |
| Customer GPS capture | `LocationCaptureScreen` / `CustomerSelectionScreen` | `PATCH api/Customer` |
| Visit check-in/out | `CheckInScreen` / `CheckInViewModel` | `POST api/CheckIn` |
| Manual sync | `SyncScreen` / `SyncViewModel` | all of the above |

Read downloads (`Brg`, `Customer`, `SalesPerson`) are not covered by `ADR-002` (write-only) and are not at risk.

## 5. Required Remediation

### 5.1 Required now
None. BTrade3 compiles and runs against the current API.

### 5.2 Required before ADR-002 enforcement on legacy write endpoints
1. **Add API authentication to BTrade3** — call `POST api/Auth/login` (user/password) and store the returned JWT securely (DataStore/Encrypted storage).
2. **Attach the token** — OkHttp `Interceptor` adding `Authorization: Bearer <token>` to every request (pattern already used by `BGud` in `network/AuthInterceptor.kt`).
3. **Handle 401** — re-authenticate, retry once, then fall back to the existing local DRAFT queue so offline-captured orders/check-ins are not lost (mirrors `ADR-003` consequence).
4. **Reconcile `serverId` semantics** — legacy routes still require `serverId` (`ADR-007` §8 explicitly keeps them); do not remove it from `OrderSyncRequest`/`CustomerSyncRequest`/`CheckInRequest` until the API migrates those routes to token-derived tenancy.
5. **Align deployment config** — confirm the production host/base path; current `belajar-api` cleartext URL (`NetworkModule.kt:10`) is flagged as risk R-03 (transport security) and must move to HTTPS for bearer tokens.
6. **Coordinate release** — `ADR-002` and Risk R-04 require clients to be JWT-capable before the protected write path goes live.

## 6. Risk Assessment

| Risk | Likelihood | Impact | Notes |
| --- | --- | --- | --- |
| BTrade3 breaks at runtime on current HEAD | Very low | High | All 6 consumed contracts unchanged and anonymous |
| Future 401s when legacy write endpoints are protected (`ADR-002` §4) | High (committed direction) | High | Requires client token flow before enforcement |
| Token/credential exposure via cleartext HTTP | Medium | Critical | R-03; out of scope of the API commit but blocks safe JWT rollout |
| `SecurityTokenValidators` change affecting non-token callers | Very low | Medium | Validation runs only when a bearer token is present |
| UnauthorizedAccessException now returns 401 instead of 500 | Low | Low | Error-path only; no legacy endpoint throws it |

**Residual risk: Medium** — driven entirely by the planned (not yet applied) JWT enforcement on legacy write endpoints, not by any change that already breaks BTrade3.

## 7. Conclusion

- **Current compatibility: Compatible.** No BTrade3 changes are required as a result of the `j06-pkl-btrade-api` changes in `d92e905f..HEAD`.
- **Forward compatibility: Partially Compatible.** BTrade3 lacks any API credential and will fail the write endpoints once `ADR-002`/`ADR-003` are enforced on legacy routes. Remediation described in §5.2 should be scheduled and coordinated with the API enforcement.

## Appendix — Evidence Commands

```text
git diff --shortstat d92e905f..HEAD -- src/j06-pkl-btrade-api/     # 67 files, +2473 / -1
git diff --name-status d92e905f..HEAD -- src/j06-pkl-btrade-api/   # 62 A, 5 M
git diff --diff-filter=M --name-only d92e905f..HEAD -- src/j06-pkl-btrade-api/
git log --oneline d92e905f..HEAD -- <legacy controllers/commands>  # empty
git diff --stat d92e905f..HEAD -- src/BTrade3/                     # empty
```
