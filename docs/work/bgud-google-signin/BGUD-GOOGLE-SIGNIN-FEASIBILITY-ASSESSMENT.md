---
Title: BGud — Google Sign-In and Unauthenticated Cloud Endpoints — Feasibility Assessment
Code: BGUD-GOOGLE-SIGNIN-001
Artifact: FEASIBILITY-ASSESSMENT
Version: 1.0
LastUpdated: 2026-09-23
Status: NOT-READY
---

# 1. Request Summary

Assessment of ISSUE `BGUD-GOOGLE-SIGNIN-001` (`docs/issues/BGUD-GOOGLE-SIGNIN-ISSUE.md`):
replace BGud's username/password/warehouse login with Google Sign-In (BTrade3-style) and
stop authenticating the Cloud endpoints BGud consumes.

Referenced artifacts:

- DOMAIN: `docs/foundation/DOMAIN.md`, `docs/work/barcode-registry/BARCODE-REGISTRY-DOMAIN.md`
  (no dedicated DOMAIN/FEATURE artifact exists for the BGud authentication flow — see GAP-007)
- FEATURE: none exists for this area (see GAP-007)
- Governing decisions: `docs/work/barcode-registry/adrs/ADR-002-*.md`, `ADR-003-*.md`,
  `ADR-007-*.md`; precedent assessment
  `docs/work/btrade3-api-compatibility/FEASIBILITY-ASSESSMENT.md` (RD-001…RD-006)

## Objective

Determine whether and how the requested change can be implemented within the current
system, and identify the gaps, questions, risks, and decisions that must be resolved
before the target architecture can be finalized.

---

# 2. Current State

This section contains facts only (verified 2026-09-23 at HEAD `2b73b7bc`).

## Existing Behavior

### BGud client (src/BGud)

| Area | Verified current behavior |
|---|---|
| Login UI | `ui/screen/LoginScreen.kt` collects Username, Password, and a Gudang selector (`Gudang Gamping` / `Gudang Concat` / `Gudang Magelang` → `locationId` `GAMPING` / `CONCAT` / `MAGELANG`). |
| Login request | `viewmodel/LoginViewModel.kt` posts `{userId, password, locationId}` to `POST api/Auth/login` (I-07) via an anonymous client (`tokenProvider = { null }`). |
| Session | The returned JWT, `userId`, `warehouseCode`, and `officeCode` (= login-returned `serverId`) are stored in `datastore/SessionPreferencesDataSource.kt` (keys `token`, `user_id`, `warehouse_code`, `office_code`). |
| Subsequent requests | `network/AuthInterceptor.kt` attaches `Authorization: Bearer <token>` to every request when a token exists; it never adds a `ServerId` to any request (ADR-007, P-06). |
| Login-time sync | Immediately after a successful login, `LoginViewModel.login()` runs `BarcodeSyncRepository.sync(serverId)` and `ReturnOrderSyncRepository.sync(serverId)` (DRAFT submission + reference downloads) using a client authenticated with the just-issued token. Barcode-sync failure blocks navigation; Return-Order failure does not. |
| Navigation gate | `ui/Navigation.kt` start destination is `home` only when a stored token exists, otherwise `login` (IR-M8). Settings logout clears the session. |
| Endpoints consumed | `network/BtradeApiService.kt`: I-07 `POST api/Auth/login` (anon), I-05 `GET api/barcodes/sync` (JWT), I-04 `POST /api/barcode-registration` (JWT), I-09 `GET api/BarcodeRegistration/status` (JWT), I-06 `GET /api/Brg/{serverId}`, I-RO-01 `POST api/return-order` (JWT), I-RO-03/04/05 `GET api/Customer|SalesPerson|Driver/{serverId}`. |
| Google configuration | BGud has **no** Google Sign-In configuration: no `google-services.json`, no `GoogleSignIn` usage, no OAuth client id anywhere under `src/BGud`. |

### Cloud API server side (src/j06-pkl-btrade-api)

| Controller | Attribute | Tenant / actor source |
|---|---|---|
| `AuthController` (`POST api/Auth/login`) | `[AllowAnonymous]` | `LocationId` → `BTR_WarehouseMapping` → `ServerId` |
| `BarcodeController` (`api/barcodes/sync`, pending) | `[Authorize]` | `User.GetServerId()` (JWT claim) |
| `BarcodeRegistrationController` (submit/pending/ack/status) | `[Authorize]` | `User.GetServerId()` + `User.GetUserId()` (JWT claims) |
| `ReturnOrderController.Submit` (`POST api/return-order`) | `[Authorize]` | `User.GetServerId()` (JWT claim); body carries **no** ServerId (P-06) |
| `UserController` | `[Authorize]` | `User.GetServerId()` (JWT claim) |
| `DriverController` (`GET api/Driver/{serverId}`) | `[Authorize]` | route `{serverId}` |
| `BrgController`, `CustomerController`, `SalesPersonController`, `OrderController`, `CheckInController`, `PackingOrderController`, `KategoriController`, `WilayahController` | anonymous | client-supplied route/body `ServerId` (legacy convention, ADR-007 §8) |

- JWT claims issued (`Infrastructure/JwtTokenService.cs`): name identifier (userId), role,
  `locationId` (WarehouseCode), `serverId` (consumed via
  `Infrastructure/ClaimsPrincipalExtensions.cs` → `GetServerId()` / `GetUserId()`).
- `IssueTokenCommandHandler` verifies `SHA-256(password)` against the `BTRADE_User`
  projection, then resolves the tenant through `BTR_WarehouseMapping`
  (seeded `GAMPING`→`JOGJA`, `CONCAT`→`JOGJA`, `MAGELANG`→`MGL`); an unmapped
  warehouse code fails explicitly, no fallback (ADR-RO-008 / IR-RO-04).
- JWT default lifetime 480 minutes (`JwtOptions.cs`).
- There is **no** anonymous variant of the JWT-tenant-resolved routes
  (`api/barcodes/sync`, `api/barcode-registration`, `api/BarcodeRegistration/status`,
  `api/return-order`, `api/User`): the tenant and, for registrations, the submitting
  user id are read from the token itself.

### Credential stores (BTR_User confirmation)

- Store of record: Main Office `BTR_User`
  (`src/j05-btr-distrib/btr.sql/Tables/Helper/BTR_User.sql`) with columns
  `UserId, UserName, Password (64-char SHA-256 hex), Prefix, RoleId`.
- Replicated to the Cloud as `BTRADE_User` by `j07-btrade-sync`
  (`Repository/UserDal.cs`: `SELECT UserId, UserName, Password, RoleId, … '' AS ServerId
  FROM BTR_User` — "replicates BTR_User verbatim"); Cloud schema
  (`btrade.sqldb/BarcodeContext/BTRADE_User.sql`) adds `IsAktif` and `ServerId`
  (unused by the login path; the comment states ServerId is assigned by the Cloud from
  the JWT, ADR-007).
- **Neither table has an email column, and no Google-account-to-BTR-user mapping exists
  anywhere in the repository.**
- **Confirmation for the reporter:** yes — BGud login is ultimately based on `BTR_User`
  accounts: BGud sends credentials to the Cloud, which verifies the SHA-256 hash against
  the `BTRADE_User` projection replicated from `BTR_User`.

### BTrade3 reference behavior (verified)

- `util/GoogleSignInHelper.kt`: Google Sign-In with `requestIdToken` using BTrade3's
  **hard-coded web client id** (`405920502340-odieer196drj8fd5jinppg7hnj8s8bpa…`,
  `app/google-services.json`).
- `ui/screen/LoginScreen.kt`: on sign-in the account **email** is passed to
  `onUserSignedIn`; a JOG/MGL server selector is shown. No server login call is made.
- `ui/Navigation.kt`: email stored in SharedPreferences (`user_email`); presence of the
  email is the logged-in state.
- `network/NetworkModule.kt`: no authentication interceptor; hard-coded **cleartext
  HTTP** base URL `http://dev.smart-ics.com:8089/belajar-api/api/`.
- `network/ApiService.kt`: consumes `Brg/{serverId}`, `Customer/{serverId}`,
  `SalesPerson/{serverId}`, `POST Order`, `PATCH Customer`, `POST CheckIn` — all
  **anonymous**, tenant supplied explicitly by the client (`ServerHelper`, values
  `JOG` / `MGL`).

### Committed platform direction (knowledge artifacts)

- **ADR-002 (Accepted 2026-09-15):** all Cloud **write** endpoints must require an
  authenticated JWT identity; anonymous write operations are prohibited; enforcement
  applies to existing and new endpoints.
- **ADR-003 (Accepted 2026-09-15):** mobile applications must authenticate against
  `pkl.btrade.api` before submitting operational commands; §4 states explicitly that
  "Google Sign-In (or any local sign-in) does not by itself satisfy this requirement;
  … it does not replace the API token."
- **ADR-007 (Accepted 2026-09-15):** `ServerId` is derived server-side from the
  authenticated identity plus the location selected during authentication; write
  operations must not accept `ServerId` from client payloads (legacy in-route `ServerId`
  routes are explicitly out of scope and may be migrated later).
- **BTrade3 compatibility FEASIBILITY-ASSESSMENT (2026-09-17, RD-001/RD-002):** the
  planned platform direction is for **BTrade3 to adopt BGud's JWT mechanism** before
  ADR-002 enforcement extends to legacy write endpoints — i.e. the two apps are planned
  to converge toward authentication, not away from it.

## Existing Constraints

- Both BGud and BTrade3 target the same Cloud host over **cleartext HTTP**
  (`dev.smart-ics.com:8089`); ADR-003 prohibits cleartext transport for tokens, and the
  BTrade3 assessment made HTTPS a prerequisite for token rollout (RD-004).
- The JWT-tenant-resolved routes have no request body/path tenant field by design
  (P-06, ADR-007 §4); making them anonymous requires a contract change on both sides.
- Tenant vocabularies diverge today: `BTR_WarehouseMapping` yields `JOGJA`/`MGL`,
  while BTrade3's legacy path selector supplies `JOG`/`MGL`.
- BGud's warehouse selector carries warehouse granularity (`GAMPING` and `CONCAT` are
  distinct `warehouseCode` values that share tenant `JOGJA`); queued local records are
  bound to `warehouseCode` and must never be silently re-homed (IR-09, ADR-RO-006).
- BTrade3's Google OAuth configuration is hard-coded to BTrade3's own
  `google-services.json` / web client id.
- JWT lifetime (480 min) is shorter than some field days; the current BGud design
  re-authenticates at login (no refresh flow exists).

---

# 3. Gap Analysis

| ID | Severity | Gap |
|------|------|------|
| GAP-001 | CRITICAL | The request contradicts **accepted** decisions ADR-002 and ADR-003, which require JWT authentication on all Cloud write endpoints and state explicitly that Google Sign-In does not replace the API token. The request also inverts the 2026-09-17 BTrade3 compatibility direction (RD-001: BTrade3 to adopt BGud's JWT mechanism). No superseding decision exists; the policy direction must be decided (and the ADRs superseded) before any architecture work. |
| GAP-002 | CRITICAL | Tenant and actor resolution is currently supplied **by the JWT itself** for every BGud operational route except the legacy `{serverId}` reads: `api/barcodes/sync`, `api/barcode-registration`, `api/BarcodeRegistration/status`, `api/return-order`, and `api/User` read `User.GetServerId()`/`User.GetUserId()`. Removing authentication without a replacement mechanism does not merely drop a header — it makes these endpoints inoperable and requires a server contract change BGud cannot make alone. |
| GAP-003 | CRITICAL | No Google-account-to-BTR-user mapping exists. `BTR_User`/`BTRADE_User` have no email/Google identifier column (verified schema), and nothing in the repository maps a Google account to a BTR `UserId`, `RoleId`, or operational location. "Which Google accounts may sign in" therefore has no answer in the current data model. |
| GAP-004 | MAJOR | Login-UX swap and endpoint-auth removal are separable but entangled in BGud: the login response (`serverId`, `userId`, `warehouseCode`) drives navigation gating, login-time sync, session binding, and queued-record warehouse binding. Replacing the login call with a local-only Google gate requires re-sourcing every one of these values. |
| GAP-005 | MAJOR | Warehouse/tenant selector semantics conflict. BGud's selector (`GAMPING`/`CONCAT`/`MAGELANG` → server-side mapping to `JOGJA`/`MGL`) preserves warehouse granularity required by IR-09/ADR-RO-006; a BTrade3-style tenant selector (`JOG`/`MGL`) collapses Gamping and Concat, and its `JOG` value differs from the mapped `JOGJA`. The desired selector model must be chosen and its data vocabulary reconciled. |
| GAP-006 | MAJOR | Removing endpoint authentication reopens the security properties ADR-002/ADR-007 were created to establish: unattributed writes (registration `RegisteredBy`, return-order actor), client-selectable tenant (cross-tenant read/write spoofing), and anonymous-writable Cloud state — while transport remains cleartext HTTP. |
| GAP-007 | MINOR | Workflow-stage-1 knowledge is missing: there is no DOMAIN/FEATURE artifact for the BGud authentication/warehouse-login capability (the assessment was performed from ISSUE + ADRs + code). The requested business change has not been defined as a FEATURE artifact. |
| GAP-008 | MINOR | BGud has no Google OAuth client configuration in the repository (no `google-services.json`, no client id). Reusing BTrade3's hard-coded client id/project is an external-configuration dependency, not an in-code default. |

---

# 4. Open Questions

| ID | Question | Impact |
|------|------|------|
| OQ-001 | Does "not using authentication" mean only removing the BGud client bearer header, or also removing `[Authorize]` from the Cloud controllers (issue Notes 1–2)? | BLOCKING — decides whether this is a client-only change or a Cloud API contract/policy change (GAP-001, GAP-002). |
| OQ-002 | Will the organization formally supersede or amend ADR-002/ADR-003/ADR-007, and does the same policy decision also reverse the planned BTrade3 remediation (RD-001)? | BLOCKING — architecture cannot be finalized while two accepted, contradictory decisions are both standing (GAP-001). |
| OQ-003 | If authentication is removed, how must the Cloud identify the tenant for `api/barcodes/sync`, `api/barcode-registration`, `api/BarcodeRegistration/status`, and `api/return-order` — client-supplied `ServerId` (BTrade3 style, contradicting ADR-007 §4) or another mechanism? | BLOCKING — determines the required server contract (GAP-002). |
| OQ-004 | May any Google account sign in, or must accounts be allow-listed / mapped to existing `BTR_User` operators? If mapped, who creates and maintains the mapping, and which BTR `RoleId` applies? | BLOCKING — no data model answer exists (GAP-003, GAP-006). |
| OQ-005 | Who is recorded as the actor for barcode registrations and return orders after the JWT user id disappears, and is loss of per-operator attribution acceptable to the business? | BLOCKING — `RegisteredBy` and audit semantics depend on it (GAP-006). |
| OQ-006 | Should the warehouse selector (Gamping/Concat/Magelang) remain, or follow BTrade3's JOG/MGL server selector — and with which authoritative tenant vocabulary given `JOG` vs `JOGJA`? | BLOCKING for flow definition (GAP-005); also determines whether offline queues keep warehouse binding (IR-09). |
| OQ-007 | Should BGud reuse BTrade3's Google OAuth web client/project or obtain a separate BGud registration? | Non-blocking for direction, blocking for any Google Sign-In realization detail (GAP-008). |
| OQ-008 | Should login-time master-data synchronization continue to run at login time (with whatever new identity step replaces it), and what replaces the token-based navigation gate (IR-M8)? | MAJOR — affects the target operational flow (GAP-004). |
| OQ-009 | Should this remain one combined change request or be split into (a) login UX change and (b) endpoint-auth removal? | MINOR — recommended split is documented in §7 Option D; affects ISSUE tracking, not feasibility. |

---

# 5. Assumptions

| ID | Assumption |
|------|------|
| ASM-001 | "Following BTrade3's approach" means BTrade3's **currently implemented** behavior (local Google gate, email as login state, anonymous APIs, explicit `serverId`), not an as-yet undocumented target design. |
| ASM-002 | BGud and BTrade3 consume the same Cloud database/host today (`dev.smart-ics.com:8089`), so any BGud endpoint change affects data shared with BTrade3 and `j07-btrade-sync`. |
| ASM-003 | `BTR_User` remains the operator store of record at the Main Office; any new identifier mapping must not turn the Cloud into an independent credential authority (PRODUCT principle "Single Source of Truth"). |
| ASM-004 | Existing ADRs remain in force until explicitly superseded by the owning decision process; this assessment treats them as binding constraints on the current state. |
| ASM-005 | There are (or may be) field-deployed BGud devices with queued local records bound to `warehouseCode`; the change must not silently re-home or orphan them. |

---

# 6. Risks

| ID | Risk | Impact | Mitigation |
|------|------|------|------|
| RISK-001 | BGud client stops sending tokens while the Cloud still enforces `[Authorize]` (partial change) | Complete BGud operational outage (401 on every route) | OQ-001 must be answered before implementation; changes must be coordinated as one release (BTrade3 assessment notes the same dependency in reverse) |
| RISK-002 | Anonymous, client-selectable tenant on write endpoints | Cross-tenant data pollution/spoofing; forged registrations and return orders | Keep writes authenticated (Option B/C) or accept explicit policy reversal with compensating controls (OQ-002/003) |
| RISK-003 | Loss of per-operator attribution on registrations/return orders | Audit and Main-Office validation expectations (ADR-002 context) break | Decide actor model (OQ-004/005); Google-email-to-user mapping preserves attribution if desired |
| RISK-004 | Shared cleartext HTTP with any credential (Google ID token or JWT) in transit | Credential exposure | Pre-existing platform risk; any option needs HTTPS before introducing tokens of any kind (mirror of RD-004/GAP-006 in BTrade3 assessment) |
| RISK-005 | Reuse of BTrade3's Google OAuth client (ASM/ OQ-007) | Coupled app inventories; BTrade3 project changes can break BGud sign-in | Register a BGud-specific OAuth client unless reuse is explicitly approved |
| RISK-006 | Duplicated/conflicting identity stores over time (Google allow-list vs `BTR_User`) | Off-boarded BTR users retain app access | If Google login is adopted, define the authoritative mapping and provisioning owner (OQ-004) |
| RISK-007 | Tenant vocabulary drift (`JOG` vs `JOGJA`, collapsed warehouse granularity) | Wrong/failed tenant resolution on anonymous routes; queue re-homing violations (IR-09) | Resolve selector and vocabulary decision first (OQ-006, GAP-005) |
| RISK-008 | Reversing direction while BTrade3 remediation (RD-001: BTrade3 adopts JWT) is still planned | The two work items cancel each other; wasted effort | Handle both apps in one platform-level authentication policy decision (OQ-002) |

---

# 7. Recommendations

Alternative solution directions only. **No final decision is recorded here** — decisions
belong in §8 Gap Closure after stakeholder/architecture input.

## Option A — Literal BTrade3 parity (full removal)

Google Sign-In as a local gate; BGud sends no credentials; `[Authorize]` removed (or routes
duplicated) on the Cloud; tenant supplied explicitly by the client like BTrade3's
`{serverId}` convention; selector becomes tenant-level.

### Advantages

- Exactly matches the requested outcome and BTrade3's current behavior.
- Simplest client: no token storage, no 401 handling, no login call.

### Disadvantages

- Requires formally superseding ADR-002/ADR-003/ADR-007 (OQ-002) and reversing the
  BTrade3 RD-001 remediation direction (RISK-008).
- Requires server-side contract changes (GAP-002) and reintroduces anonymous,
  client-selectable-tenant writes (GAP-006, RISK-002/003).
- Loses warehouse granularity or requires a new tenant/warehouse parameter on JWT-only
  routes (GAP-005).

## Option B — Google identity UX with Cloud-issued JWT (federated login) — smallest policy-compatible change

BGud replaces the username/password form with Google Sign-In; the Cloud verifies the
Google ID token and a Google-account-to-`BTR_User` mapping (allow-list), then issues the
**existing JWT** with the same claims; endpoints stay unchanged and ADR-003 §4's intent
(Google identity informs local identity; the API token remains) is preserved.

### Advantages

- Satisfies the business desire (no BTR password for operators) without reopening the
  write boundary; tenant + attribution are untouched.
- Zero contract change for the already-`[Authorize]` routes; warehouse selector and
  login-time sync semantics (IR-09, IR-M8) are preserved as-is.

### Disadvantages

- Requires the new mapping/allow-list data model (GAP-003, OQ-004) owned at the Main
  Office, plus Google OAuth client configuration (GAP-008).
- ID-token verification on cleartext transport must be resolved (HTTPS prerequisite,
  RISK-004).
- Does **not** satisfy part 2 of the request (no endpoint authentication).

## Option C — Partial removal (anonymous reads, authenticated writes)

Google-gated login for UX plus (optionally) anonymous tenant-parameterized reads; the
write endpoints (`barcode-registration`, `return-order`, ack) keep JWT per ADR-002.

### Advantages

- Reduces the authenticated surface and demonstrates "unauthenticated consumption" for
  the read half without abandoning write attribution.

### Disadvantages

- Splits the platform policy again (two models coexist); the read routes still need a
  tenant parameter replacement (GAP-002 variant).
- Half-meet both parts of the request; likely rework once OQ-002 is answered.

## Option D — Issue sequencing (applies to any option)

Split the ISSUE into (D1) login method change (Google Sign-In) and (D2) endpoint
authentication removal, because D2 is gated on an ADR-level policy decision (OQ-002)
while D1 has a policy-compatible path (Option B). Also create the missing FEATURE
artifact for the BGud operator-authentication capability (GAP-007) before architecture.

### Advantages

- Unblocks the achievable part; keeps the security-policy question explicit and
  auditable; answers issue Note 8.

### Disadvantages

- Two issues to coordinate for any shared release (RISK-001).

---

# 8. Gap Closure

No gap or open question has been resolved yet. Every GAP-00x and OQ-00x entry above is
**Status: OPEN**. When a resolution is approved, it will be recorded here in place
(Decision, Rationale, Impact, Architecture Impact, Resolved By, Resolved Date) without
renumbering, and Planning Readiness will be updated accordingly.

---

# 9. Planning Readiness

## Readiness Checklist

- [ ] All critical gaps resolved (GAP-001, GAP-002, GAP-003 open)
- [ ] All required decisions recorded (§8 empty; ADR-supersession decision outstanding)
- [ ] All blocking open questions resolved (OQ-001…OQ-006 open)
- [ ] Architecture can be finalized or updated

## Status

NOT-READY

## Notes

- The blocking core is a **policy contradiction**, not a technical unknown: the request
  reverses ADR-002/ADR-003/ADR-007 (all Accepted 2026-09-15) and the BTrade3 RD-001
  direction (2026-09-17). Until the owner of those decisions explicitly supersedes or
  reaffirms them (OQ-001/OQ-002), the target architecture cannot be finalized in either
  direction.
- If the reaffirmed direction is Option B (or A/C after supersession), the
  Google-account-to-BTR-user mapping decision (OQ-004/OQ-005) and the selector/tenant
  vocabulary decision (OQ-006) are the next required gap-closure inputs.
- READY-FOR-PLANNING means that feasibility is sufficiently resolved for the Architecture
  skill to finalize or update the target architecture. It does not mean that architecture
  is complete or that the architecture step may be skipped. Planning begins only after the
  required architecture work is complete. Only the Architect grants the gate; this
  artifact remains NOT-READY while blocking gaps remain.
- Knowledge propagation: once the policy decision is made, `ADR-002`, `ADR-003`,
  `ADR-007`, and the BTrade3 compatibility recommendations (RD-001/RD-003) require review
  by their owning process; a FEATURE artifact for operator authentication should be created
  (GAP-007, stage Discovery).

---

# 10. References

Referenced artifacts:

- `docs/issues/BGUD-GOOGLE-SIGNIN-ISSUE.md` (ISSUE, input)
- `docs/foundation/DOMAIN.md`, `docs/foundation/LANDSCAPE.md`, `docs/foundation/PRODUCT.md`, `docs/foundation/WORKFLOW.md`
- `docs/work/barcode-registry/adrs/ADR-002-authenticated-jwt-write-endpoints.md`
- `docs/work/barcode-registry/adrs/ADR-003-mobile-authentication-against-cloud-api.md`
- `docs/work/barcode-registry/adrs/ADR-007-tenant-isolation-from-authenticated-identity.md`
- `docs/work/btrade3-api-compatibility/FEASIBILITY-ASSESSMENT.md` (RD-001…RD-006)
- `docs/work/return-order/RETURN-ORDER-IMPL-PLAN.md` (S2.4 warehouse mapping; ADR-RO-008)

Referenced codebase locations:

- `src/BGud/app/src/main/java/com/elsasa/bgud/viewmodel/LoginViewModel.kt`
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/LoginScreen.kt`
- `src/BGud/app/src/main/java/com/elsasa/bgud/network/AuthInterceptor.kt`, `ApiClient.kt`, `BtradeApiService.kt`
- `src/BGud/app/src/main/java/com/elsasa/bgud/datastore/SessionPreferencesDataSource.kt`
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/Navigation.kt` (token gate, `CLOUD_BASE_URL`)
- `src/BTrade3/app/src/main/java/com/elsasa/btrade3/util/GoogleSignInHelper.kt`, `ui/screen/LoginScreen.kt`, `ui/Navigation.kt`, `network/NetworkModule.kt`, `network/ApiService.kt`, `app/google-services.json`
- `src/j06-pkl-btrade-api/btrade.webapi/Controllers/AuthController.cs`, `BarcodeController.cs`, `BarcodeRegistrationController.cs`, `ReturnOrderController.cs`, `DriverController.cs`, `UserController.cs`
- `src/j06-pkl-btrade-api/btrade.webapi/Infrastructure/JwtTokenService.cs`, `ClaimsPrincipalExtensions.cs`
- `src/j06-pkl-btrade-api/btrade.application/UseCase/IssueTokenCommand.cs`
- `src/j06-pkl-btrade-api/btrade.sqldb/BarcodeContext/BTRADE_User.sql`, `Scripts/Create_BTR_WarehouseMapping.sql`
- `src/j07-btrade-sync/j07-btrade-sync/Repository/UserDal.cs`
- `src/j05-btr-distrib/btr.sql/Tables/Helper/BTR_User.sql`

Referenced documents:

- `docs/work/barcode-registry/BARCODE-REGISTRY-ARCHITECTURE.md` (IR-05 credential projection, §9 auth)
