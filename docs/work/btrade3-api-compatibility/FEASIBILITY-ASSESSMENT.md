# FEASIBILITY ASSESSMENT

| Field | Value |
| --- | --- |
| Title | BTrade3 compatibility with the current `j06-pkl-btrade-api` implementation |
| Date | 2026-09-17 |
| Type | Compatibility / enhancement feasibility |
| Consumer | `src/BTrade3` (Android, `com.elsasa.btrade3`) |
| Provider | `src/j06-pkl-btrade-api` (BTrade-Api / Cloud API) |
| Baseline commit | `d92e905f4e2d7f14096bb82c730605accdcb4e9f` |
| Assessed revision | `370ea930fefb514512c17f129acd5178f3ddc05a` (HEAD) |
| Assessment scope | Do API changes break BTrade3, and what must change to remain compatible |

---

## 1. Executive Summary

### Request

Determine whether `src/BTrade3` remains compatible with the current `src/j06-pkl-btrade-api` implementation after the API was extended to support a new mobile application (BGud) with Barcode Registry, Sales Return, Driver master, User credential projection, and JWT authentication.

### Recommendation

1. **No code change is required for BTrade3 to function against the API as it stands today.** All six consumed endpoints are unchanged, still anonymous, and structurally identical.
2. **Change is required before `ADR-002` enforcement is extended to the legacy write endpoints.** BTrade3 presents no API credential and will receive `401` on `POST api/Order`, `PATCH api/Customer`, and `POST api/CheckIn` once enforcement lands.
3. Planning for that change **cannot start yet**: two blocking questions (credential model and operational-location model for salespersons) must be resolved first. See §9.

### Compatibility Status

| Horizon | Status | Basis |
| --- | --- | --- |
| Current implementation (HEAD) | **Compatible** | Consumed contracts unchanged; legacy controllers remain anonymous |
| Committed target state (`ADR-002` enforced) | **Partially Compatible** | No API credential, no token storage, no bearer interceptor, no 401 handling in BTrade3 |

### Feasibility Result

```text
PARTIALLY FEASIBLE
```

Technically straightforward (an equivalent client was already built for BGud), but blocked on unresolved business/technical decisions about credentials and location selection.

---

## 2. Request Understanding

**Requested capability:** preserve BTrade3's operational flows (master downloads, sales-order upload, customer GPS upload, check-in upload) while `j06-pkl-btrade-api` evolves.

**Business objective:** safely enable a new mobile application on the shared Cloud API without regressing the legacy field-sales application used by salespersons.

**Expected outcome:** a compatibility determination plus an impact inventory detailed enough for a Planning Agent to scope any required BTrade3 work without further discovery.

**Affected users:** BTrade3 salespersons in the field (order capture, visit check-in/out, customer location tagging), and the BTR office that receives those records.

---

## 3. Current State Analysis

### 3.1 Existing Business Flow (BTrade3)

```text
Login (Google Sign-In, local gate)
    ↓
Master sync (Read):  GET api/Brg/{serverId}, api/Customer/{serverId}, api/SalesPerson/{serverId}
    ↓
Field work (offline, Room): orders, check-ins, customer GPS
    ↓
Write sync:          POST api/Order
                     PATCH api/Customer
                     POST api/CheckIn
```

### 3.2 Existing Components (BTrade3)

| Component | Responsibility |
| --- | --- |
| `network/NetworkModule.kt` | Retrofit/OkHttp/Gson client; hard-coded base URL `http://dev.smart-ics.com:8089/belajar-api/api/` (line 10); logging interceptor only |
| `network/ApiService.kt` | The entire consumed API surface (6 methods, lines 22–38) |
| `repository/NetworkRepository.kt` | Read downloads; resolves `serverId` from `ServerHelper` |
| `repository/SyncRepository.kt` | Master-data sync orchestration |
| `repository/OrderSyncRepository.kt` | `apiService.syncOrder(...)` (line 120) |
| `repository/CustomerSyncRepository.kt` | `apiService.syncCustomerLocation(...)` (lines 59, 124) |
| `repository/CheckInSyncRepository.kt` | `apiService.syncCheckIn(...)` (line 105) |
| `util/ServerHelper.kt` | Returns locally selected `serverId` (`getSelectedServer`) |
| `datastore/ServerPreferencesDataSource.kt` | DataStore with only a `server_target` preference — **no token storage** |
| `util/GoogleSignInHelper.kt` / `ui/screen/LoginScreen.kt` | Google Sign-In for local identity only; result used as a UI gate, never sent as a credential |
| `ui/Navigation.kt` | Constructs repositories and injects the single `apiService` (lines 80–92) |

### 3.3 Existing Database (BTrade3 Room)

Local entities: `barang`, `customer`, `order`, `order_item`, `check_in`, `sales_person`. No token/session entity. No server-side schema is owned by BTrade3.

### 3.4 Existing Integrations (Cloud API, HEAD)

| # | Consumed route | Provider action | Auth at HEAD |
| --- | --- | --- | --- |
| 1 | `GET api/Brg/{serverId}` | `BrgController.ListData` | anonymous |
| 2 | `GET api/Customer/{serverId}` | `CustomerController.ListData` | anonymous |
| 3 | `GET api/SalesPerson/{serverId}` | `SalesPersonController.ListData` | anonymous |
| 4 | `POST api/Order` | `OrderController.UploadData` | anonymous |
| 5 | `PATCH api/Customer` | `CustomerController.UpdateLocation` | anonymous |
| 6 | `POST api/CheckIn` | `CheckInController.UploadData` | anonymous |

New (additive, not consumed by BTrade3): `POST api/Auth/login`; `[Authorize]`-protected `api/Barcode/sync`, `/api/barcodes/sync`, `api/BarcodeRegistration/{pending,ack,status}`, `/api/barcode-registration`, `api/Driver[/{serverId}]`, `/api/return-order`, `api/ReturnOrder/incremental/...`, `api/User`.

### 3.5 Existing Security Model

- **Provider:** JWT Bearer is configured (`PresentationService.cs:40-54`) and now wired to a `JsonWebTokenSecurityTokenValidator`; `[Authorize]` is applied only to new controllers. No global fallback policy. Legacy read/write controllers are anonymous.
- **Consumer:** none. BTrade3 sends no `Authorization` header. Google Sign-In is a local gate only.
- **Transport:** cleartext HTTP to the configured base URL (recorded as provider risk R-03).

### 3.6 What changed in the API since `d92e905f`

- 15 commits, 67 files, +2473 / −1; **62 added**, **5 modified** (`Program.cs`, `PresentationService.cs`, `ErrorHandlerMiddleware.cs`, `.sln`, `.sqlproj`).
- **No existing controller and no existing command/query contract was modified.** `git log` over `OrderController`, `CustomerController`, `CheckInController`, `BrgController`, `SalesPersonController` and their commands is empty for the range.
- Response envelope (`JSendOk` → `{status, code, data}`) unchanged.

---

## 4. Impact Analysis

### 4.1 Backend Impact

None. No `j06-pkl-btrade-api` component consumed by BTrade3 changed. New backend code is additive.

### 4.2 Database Impact

None. BTrade3 owns only its local Room database; the API's new SQL objects are additive and unrelated.

### 4.3 Frontend Impact (BTrade3)

**None at HEAD.** If JWT enforcement expands to legacy write endpoints, the following become impacted:

| Area | Files | Reason |
| --- | --- | --- |
| Network client | `network/NetworkModule.kt`, `network/ApiService.kt` | Add bearer header; add login call |
| Auth/session storage | `datastore/ServerPreferencesDataSource.kt` (or a new session store) | Persist/refresh token |
| Write repositories | `repository/OrderSyncRepository.kt`, `CustomerSyncRepository.kt`, `CheckInSyncRepository.kt` | 401 handling and retry/re-auth |
| Login UX | `ui/screen/LoginScreen.kt`, `util/GoogleSignInHelper.kt` | Obtain API credential/location, not just Google identity |
| Wiring | `ui/Navigation.kt` | Provide token-aware client |
| Manual sync UX | `ui/screen/SyncScreen.kt`, view models | Surface auth failures without losing queued records |

### 4.4 Integration Impact

| Integration | Impact |
| --- | --- |
| `POST api/Order` / `PATCH api/Customer` / `POST api/CheckIn` | Future `401` once `ADR-002` is enforced on legacy writes |
| `POST api/Auth/login` | New dependency required to obtain a JWT |
| Read endpoints | No impact (writes-only enforcement per `ADR-002` §1) |

### 4.5 Security Impact

- BTrade3 currently contributes to the anonymous-write exposure closed by `ADR-002`/`ADR-003`.
- No token/credential is stored at all today, so no secret-leak surface exists yet; adding auth introduces secure-storage obligations.
- Cleartext transport must be resolved before bearer tokens are introduced (`ADR-003` negative consequence; provider risk R-03).

---

## 5. Gap Analysis

### 5.1 Findings and Evidence (no-gap confirmations)

| Finding | Evidence |
| --- | --- |
| Consumed endpoints unchanged | `git diff --diff-filter=M d92e905f..HEAD -- src/j06-pkl-btrade-api/` lists no legacy controller |
| Consumed request/response schemas unchanged | `OrderUploadCommand.cs`, `CheckInUploadCommand.cs`, `CustomerLocationUpdateCommand.cs` identical to baseline; BTrade3 DTOs match field-for-field |
| Consumed routes still anonymous | No `[Authorize]` on `BrgController`/`CustomerController`/`SalesPersonController`/`OrderController`/`CheckInController`; no fallback policy in `PresentationService.cs` |
| JWT wiring cannot affect non-token callers | `SecurityTokenValidators` populated only when a bearer token is presented; BTrade3 sends none |
| No routing collisions | New routes do not overlap `api/Brg`, `api/Customer`, `api/SalesPerson`, `api/Order`, `api/CheckIn` |
| BTrade3 itself unchanged | `git diff --stat d92e905f..HEAD -- src/BTrade3/` is empty |

**Conclusion:** there are **no contract mismatches or breaking changes at HEAD**. The gaps below are forward-looking against the API's committed security direction.

### 5.2 Gap Register

| Gap ID | Type | Description | Blocking |
| --- | --- | --- | --- |
| GAP-001 | Integration | BTrade3 has no API credential and sends no `Authorization` header; `ADR-002` requires JWT on all write endpoints including legacy ones | No (current) / Yes (target) |
| GAP-002 | Functional | BTrade3 identity is a Google email with no password; `POST api/Auth/login` requires `UserId`, `Password`, `LocationId` (`IssueTokenCommand.cs:10-13`) verified against `BTRADE_User` | Yes (for auth plan) |
| GAP-003 | UX | No operational-location selection for salespersons; login resolves `ServerId` from `BTR_WarehouseMapping` via `LocationId` (`IssueTokenCommand.cs:54-62`), a warehouse-oriented mapping | Yes (for auth plan) |
| GAP-004 | Technical | No token storage, refresh, or expiry handling; JWT default lifetime is 480 minutes (`JwtOptions.cs:10`), shorter than some offline field periods | No |
| GAP-005 | Technical | No `401` handling; a failed write never falls back to a defined re-authenticate path, risking loss of queued records (`ADR-003` negative consequence) | No |
| GAP-006 | Integration | Base URL points at a cleartext legacy host (`NetworkModule.kt:10`); bearer tokens must not traverse cleartext (`ADR-003`) | No |
| GAP-007 | Technical | BTrade3 still sends `serverId` from `ServerHelper`; permitted for legacy routes by `ADR-007` §8, but incompatible if those routes later adopt token-derived tenancy | No |
| GAP-008 | Documentation | `ADR-002` §4 requires enforcement on existing write endpoints, but the implementation applies `[Authorize]` only to new controllers; decision and code diverge | No (dependency) |

---

## 6. Solution Options

### GAP-001 / GAP-002 / GAP-003 — Obtaining and presenting an API identity

**Option A — Authenticate BTrade3 against `pkl.btrade.api` with BTR user accounts + operational location (recommended).**
- Advantages: aligns with `ADR-002`/`ADR-003`; per-user attribution and audit; consistent with BGud; reuses an already proven mechanism.
- Disadvantages: requires credential provisioning and a location-selection step for salespersons; changes the login UX.
- Risk: medium — depends on unresolved credential/location model (OQ-001, OQ-002).
- Recommendation: **preferred.**

**Option B — Defer until the provider enforces `ADR-002` on legacy routes.**
- Advantages: zero immediate work; no UX change today.
- Disadvantages: leaves BTrade3 on the critical path of a coordinated release; risk of unplanned outage; implementation compressed into the enforcement window.
- Risk: high operational risk.
- Recommendation: acceptable only with an agreed enforcement date and a funded work item.

**Option C — Per-device static credential / API key.**
- Advantages: no user UX change.
- Disadvantages: rejected by `ADR-002` alternatives (weaker identity semantics, no per-user audit).
- Recommendation: rejected.

### GAP-004 / GAP-005 — Token lifecycle and failure handling

**Option A — Store token in DataStore + OkHttp bearer interceptor + re-authenticate-and-retry on `401`, preserving the existing local DRAFT queue (recommended).**
- Advantages: matches BGud's `AuthInterceptor`/`SessionBinding` pattern; offline records survive token loss; `ADR-003` explicitly requires "queue locally, authenticate at send time".
- Disadvantages: introduces secure-storage and 401-state handling.
- Risk: low-medium.
- Recommendation: **preferred.**

**Option B — Re-authenticate on every launch only.**
- Disadvantages: writes fail after expiry within a long field day; poor UX.
- Recommendation: rejected.

### GAP-006 — Transport

**Option A — Repoint the client to an HTTPS endpoint before shipping tokens (recommended).**
- Risk: low; configuration-driven.
- Recommendation: **preferred** and a prerequisite for token use.

### GAP-007 — `serverId` semantics

**Option A — Keep sending `serverId` for legacy routes until the provider migrates them (recommended).**
- Rationale: `ADR-007` §8 explicitly permits legacy routes to keep `ServerId` in the route; removing it now would break them.
- Recommendation: **preferred.**

### GAP-008 — Decision/implementation divergence

**Option A — Treat as a provider-side dependency and track the enforcement date (recommended).**
- Rationale: outside BTrade3's control; document as an open question and planning dependency.

---

## 7. Recommended Approach

Preserve BTrade3's current behavior and make no change while legacy write endpoints remain anonymous, but prepare a scoped BTrade3 enhancement to adopt API authentication **before** `ADR-002` enforcement is extended to the legacy write endpoints.

The preferred solution is solution-level, not architectural:

1. Introduce an API authentication step in BTrade3 that obtains a JWT from `POST api/Auth/login` (GAP-001, GAP-002, GAP-003).
2. Persist the token securely and attach it to API requests via an OkHttp interceptor (GAP-004).
3. On `401`, re-authenticate and retry once; never discard locally queued orders/check-ins (GAP-005).
4. Move the client to HTTPS before tokens are introduced (GAP-006).
5. Retain `serverId` in legacy write payloads/routes until the provider migrates them (GAP-007).
6. Coordinate release timing with the provider's enforcement date (GAP-008).

**Constraints:** must not alter existing offline capture behavior; must not remove `serverId` from legacy payloads; must not change the existing `JSend` envelope handling; must remain consistent with BGud's established token mechanism.

---

## 8. Risks

| Risk | Impact | Probability | Mitigation |
| --- | --- | --- | --- |
| `ADR-002` enforcement lands before BTrade3 is JWT-capable | High — order/check-in/upload outage in the field | Medium | Treat as release-gating; agree an enforcement date (OQ-003) |
| Credential model for salespersons cannot be resolved | High — auth work cannot be planned or built | Medium | Resolve OQ-001 before planning |
| Location mapping does not cover sales/office locations | High — login cannot resolve `ServerId` for salespersons | Medium | Resolve OQ-002 before planning |
| Token expiry during a long field day causes write failures | Medium — lost/delayed uploads | Medium | Re-auth-and-retry with local queue preservation (GAP-005) |
| Bearer token exposed over cleartext HTTP | Critical | Medium | HTTPS before token rollout (GAP-006) |
| `serverId` route migration silently breaks payloads | High | Low | Keep `serverId` for legacy routes; monitor provider changes (GAP-007) |
| Offline queue loses records on auth failure | High — data loss | Low-Medium | Preserve existing DRAFT semantics; authenticate at send time (GAP-005) |
| Assumption A-001 (BTrade3 remains supported) is wrong | Medium — wasted effort | Low | Confirm product stance (OQ-005) |

---

## 9. Open Questions

### Business Questions

| ID | Question | Blocking |
| --- | --- | --- |
| OQ-001 | What credential will a salesperson use to obtain a JWT? BTR user accounts require `UserId`+`Password`; BTrade3 currently knows only a Google email. How are accounts provisioned and passwords communicated? | **BLOCKING** |
| OQ-005 | Is BTrade3 confirmed as a supported product with ongoing investment (vs. planned retirement)? | **BLOCKING** for committing to remediation |

### Technical Questions

| ID | Question | Blocking |
| --- | --- | --- |
| OQ-002 | What is the operational-location (`LocationId`) model for salespersons? `POST api/Auth/login` resolves `ServerId` via `BTR_WarehouseMapping`, which is warehouse-oriented. Does it (or a companion mapping) include sales/office locations, and how does a salesperson select one? | **BLOCKING** |
| OQ-004 | Will the legacy `serverId`-in-route write endpoints be migrated to token-derived tenancy? If so, when, and will payload `serverId` be rejected or ignored? | No |
| OQ-006 | Should BTrade3 reuse the exact BGud token/session mechanism, or is an independent implementation required? | No |

### Operational Questions

| ID | Question | Blocking |
| --- | --- | --- |
| OQ-003 | What is the target date/release for extending `ADR-002` enforcement to the legacy write endpoints? | No (planning dependency) |
| OQ-007 | Is HTTPS termination available for the BTrade3 deployment host, and can `BASE_URL` be repointed without release risk? | No |

---

## 10. Assumptions

> Explicitly recorded (not silently resolved). Each requires confirmation.

| ID | Assumption | Basis | Status |
| --- | --- | --- | --- |
| A-001 | BTrade3 remains a supported application | `docs/foundation/LANDSCAPE.md` lists BTrade3 as active Mobile Sales Operations | **Unconfirmed** — see OQ-005 |
| A-002 | The six routes in `ApiService.kt` are the complete API surface consumed by BTrade3 | Single Retrofit service; repository grep found no other call sites | High confidence |
| A-003 | `ADR-002` enforcement will eventually extend to `POST api/Order`, `PATCH api/Customer`, `POST api/CheckIn` | `ADR-002` Accepted, §4 | High confidence |
| A-004 | The `POST api/Auth/login` request/response contract and claim names remain stable | `IssueTokenCommand.cs` / `JwtTokenService.cs` at HEAD | Medium confidence |
| A-005 | Provider will not remove `serverId` from legacy routes without a migration window | `ADR-007` §8 | Medium confidence |

---

## 11. Recommended Decisions

| ID | Decision | Rationale |
| --- | --- | --- |
| RD-001 | Adopt API (JWT) authentication in BTrade3 as a planned enhancement, gated on the provider's legacy-enforcement date | `ADR-002`/`ADR-003` commit the platform to authenticated writes |
| RD-002 | Reuse the BGud token mechanism (`AuthInterceptor` + session storage) rather than inventing a new one | Consistency and proven implementation; reduces risk and review surface |
| RD-003 | Retain `serverId` in legacy write payloads/routes until the provider migrates them | `ADR-007` §8 permits legacy routes; removal would break them now |
| RD-004 | Move BTrade3's base URL to HTTPS before any token is introduced | `ADR-003` prohibits cleartext for the token endpoint/transport |
| RD-005 | Preserve the offline DRAFT queue; authenticate at send time; no silent record loss | `ADR-003` rejected offline anonymous queueing but requires local queueing at capture |
| RD-006 | Track provider-side `ADR-002` enforcement as an external release dependency | GAP-008 divergence between decision and implementation |

---

## 12. Required BTrade3 Changes (Scope Only — No Slices)

> This section identifies **what must change**, not how. The Planning Agent owns slicing.

| Change Area | Required Change | Trigger | Gaps |
| --- | --- | --- | --- |
| API authentication | Add `POST api/Auth/login` call and session acquisition | Before legacy enforcement | GAP-001, GAP-002, GAP-003 |
| Token storage | Persist token (and expiry/bound `ServerId`) in secure app storage | With auth | GAP-004 |
| Request auth | Attach `Authorization: Bearer <token>` to API requests | With auth | GAP-001 |
| Failure handling | Handle `401`: re-authenticate, retry once, preserve local queue | With auth | GAP-005 |
| Transport | Repoint base URL to HTTPS | Before token rollout | GAP-006 |
| Login UX | Introduce credential/location selection consistent with `IssueTokenCommand` | With auth | GAP-002, GAP-003 |
| Legacy payloads | Keep `serverId`; do not remove until provider migration | Provider migration | GAP-007 |

**No change is required** to: read downloads, Room schema, the `JSend` response models, or any endpoint contract.

---

## 13. Implementation Impact Inventory

### Backend

None (BTrade3-internal assessment). Provider change only if/when `ADR-002` enforcement expands (GAP-008).

### Database

None. (No BTrade3 schema change required; optional token storage is app-local.)

### Frontend

| Component | File | Change Type |
| --- | --- | --- |
| Network client | `network/NetworkModule.kt` | Add interceptor; HTTPS base URL |
| API definition | `network/ApiService.kt` | Add login call |
| Session storage | `datastore/ServerPreferencesDataSource.kt` or new session store | Add token persistence |
| Write repos | `repository/OrderSyncRepository.kt`, `CustomerSyncRepository.kt`, `CheckInSyncRepository.kt` | 401 handling |
| Login | `ui/screen/LoginScreen.kt`, `util/GoogleSignInHelper.kt` | API credential/location flow |
| Wiring | `ui/Navigation.kt` | Token-aware client wiring |
| Sync UX | `ui/screen/SyncScreen.kt` (+ view models) | Auth-failure messaging |

### Integration

| Integration | Change |
| --- | --- |
| `POST api/Auth/login` | New outbound dependency |
| `POST api/Order`, `PATCH api/Customer`, `POST api/CheckIn` | Add auth header + 401 handling |
| `GET api/Brg\|Customer\|SalesPerson/{serverId}` | No contract change (optional header) |

### Security

| Element | Change |
| --- | --- |
| API credential | Introduced; provisioning policy required |
| JWT storage | New secure-storage requirement |
| Transport | Cleartext → HTTPS |
| Tenant identity | Bound `ServerId` may come from token for new flows (`ADR-007`) |

---

## 14. Planning Readiness

### Status

```text
NOT READY
```

The **compatibility conclusion is settled** (no change required at HEAD), but the **remediation plan** cannot be produced until the blocking open questions are resolved.

### Blocking Issues

| ID | Issue |
| --- | --- |
| OQ-001 | Salesperson credential model for `POST api/Auth/login` is undefined |
| OQ-002 | Operational-location (`LocationId`) model for salespersons is undefined |
| OQ-005 | Support commitment for BTrade3 is unconfirmed |

### Planner Guidance

- **Implementation scope (if remediation is approved):** BTrade3 client only — login/credential flow, token storage, bearer interceptor, 401 handling, HTTPS base URL. No API, database, or domain changes.
- **Major dependencies:** provider `ADR-002` enforcement timeline (OQ-003); credential provisioning (OQ-001); location mapping coverage (OQ-002); HTTPS availability (OQ-007).
- **Sequencing concerns (not slices):** credential/location decisions must precede any auth work; HTTPS must precede token rollout; release must be coordinated so BTrade3 is JWT-capable before legacy writes are protected.
- **Review concerns:** consistency with BGud's token mechanism; no regression to offline DRAFT queue; no removal of `serverId` from legacy payloads; secure token storage; no cleartext token transport.
- **Reference implementation available:** BGud already ships a compatible client (`network/AuthInterceptor.kt`, `network/ApiClient.kt`, `datastore/SessionBinding.kt`, `datastore/SessionPreferencesDataSource.kt`) and can serve as a proven reference.

---

## Appendix A — Evidence Index

| Claim | Evidence |
| --- | --- |
| BTrade3's complete consumed API surface | `src/BTrade3/.../network/ApiService.kt:22-38` |
| Base URL / transport | `src/BTrade3/.../network/NetworkModule.kt:10` |
| No token storage | `src/BTrade3/.../datastore/ServerPreferencesDataSource.kt:18-19` |
| Google local-only identity | `src/BTrade3/.../util/GoogleSignInHelper.kt`; `ui/screen/LoginScreen.kt:107-109` |
| Legacy endpoints unchanged | `git diff --diff-filter=M d92e905f..HEAD -- src/j06-pkl-btrade-api/` (5 files, all presentation/config) |
| Legacy controllers anonymous | `BrgController.cs`, `CustomerController.cs`, `SalesPersonController.cs`, `OrderController.cs`, `CheckInController.cs` (no `[Authorize]`) |
| New endpoints `[Authorize]` | `BarcodeController.cs:11`, `BarcodeRegistrationController.cs:10`, `DriverController.cs:9`, `ReturnOrderController.cs:11`, `UserController.cs:11` |
| JWT login contract | `IssueTokenCommand.cs:10-22,45-62` |
| JWT lifetime default | `JwtOptions.cs:10` (480 min) |
| JWT token claims | `JwtTokenService.cs:23-29` |
| ADR-002 scope/negative consequence | `docs/work/barcode-registry/adrs/ADR-002-authenticated-jwt-write-endpoints.md:26-35,48-49` |
| ADR-003 mobile auth requirement | `docs/work/barcode-registry/adrs/ADR-003-mobile-authentication-against-cloud-api.md:25-32` |
| ADR-007 legacy route allowance | `docs/work/barcode-registry/adrs/ADR-007-tenant-isolation-from-authenticated-identity.md:42-44` |
| Provider risk R-04 (coordinated client updates) | `docs/work/barcode-registry/BARCODE-REGISTRY-ARCHITECTURE.md:1995-2005` |
| BTrade3 unchanged since baseline | `git diff --stat d92e905f..HEAD -- src/BTrade3/` (empty) |
| BGud reference client | `src/BGud/.../network/AuthInterceptor.kt`, `ApiClient.kt`, `datastore/SessionBinding.kt` |
