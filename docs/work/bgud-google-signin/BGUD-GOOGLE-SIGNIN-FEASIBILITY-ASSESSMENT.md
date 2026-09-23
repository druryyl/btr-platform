---
Title: BGud — Google Sign-In and Unauthenticated Cloud Endpoints — Feasibility Assessment
Code: BGUD-GOOGLE-SIGNIN-001
Artifact: FEASIBILITY-ASSESSMENT
Version: 1.3
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
- Superseded-for-this-feature decisions: `docs/work/barcode-registry/adrs/ADR-002-*.md`,
  `ADR-003-*.md`, `ADR-007-*.md` — **no longer authoritative for BGud** per the
  2026-09-23 authentication-direction override (§8 OQ-002); repository treatment of the ADR
  files is executed by the ADR owning process (request recorded in §9) and is **not** a
  prerequisite for architecture on this feature
- Precedent assessment `docs/work/btrade3-api-compatibility/FEASIBILITY-ASSESSMENT.md`
  (RD-001…RD-006) — BTrade3-scoped; unaffected by this override (§8 OQ-002 scope limitation)

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
| Subsequent requests | `network/AuthInterceptor.kt` attaches `Authorization: Bearer <token>` to every request when a token exists; it never adds a `ServerId` to any request (historical constraint ADR-007, P-06). |
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
  the `BTRADE_User` projection replicated from `BTR_User`. Under the OQ-001 decision (§8)
  this login path is removed for BGud entirely.

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

- **ADR-002 / ADR-003 / ADR-007 (files dated 2026-09-15; file status still reads
  `Accepted` as verified at this version):** required authenticated JWT identity on Cloud
  write endpoints, mobile authentication against the Cloud API, and server-side tenant
  resolution from authenticated identity.
  **Overridden for this feature on 2026-09-23** by the repository owner's BGud
  authentication-direction decision (§8 OQ-001/OQ-002): these ADRs are **no longer
  authoritative for BGud** and must not block its architecture or implementation. Their
  files must be removed or marked superseded/inactive per repository convention
  (treatment executed by the ADR owning process — request in §9). This is a deliberate
  architectural decision for BGud, **not** a temporary exception; no restoration path
  applies to BGud.
- **BTrade3 compatibility FEASIBILITY-ASSESSMENT (2026-09-17, RD-001/RD-002):** the
  BTrade3-side direction is for **BTrade3 to adopt BGud's JWT mechanism** before
  ADR-002-style enforcement extends to legacy write endpoints. This override is
  explicitly scoped to BGud and does **not** decide BTrade3's authentication direction
  (§8 OQ-002 scope limitation; see also the Important Clarification below §8 OQ-002).

## Existing Constraints

- Both BGud and BTrade3 target the same Cloud host over **cleartext HTTP**
  (`dev.smart-ics.com:8089`). After the OQ-001 decision (§8) BGud transmits **no
  credentials** to the Cloud API (Google Sign-In is a local UI gate only), so the
  token-transport constraint (ADR-003 / RD-004) no longer gates BGud; HTTPS remains a
  general platform concern (RISK-004).
- The JWT-tenant-resolved routes have no request body/path tenant field by design
  (P-06, historical ADR-007 §4); making them anonymous still requires a contract change
  on both sides (GAP-002, OQ-003).
- Tenant vocabularies diverge today: `BTR_WarehouseMapping` yields `JOGJA`/`MGL`,
  while BTrade3's legacy path selector supplies `JOG`/`MGL`.
- BGud's warehouse selector carries warehouse granularity (`GAMPING` and `CONCAT` are
  distinct `warehouseCode` values that share tenant `JOGJA`); queued local records are
  bound to `warehouseCode` and must never be silently re-homed (IR-09, ADR-RO-006).
  The OQ-001 decision (§8) requires this granularity to be preserved when BGud supplies
  tenant context explicitly.
- BTrade3's Google OAuth configuration is hard-coded to BTrade3's own
  `google-services.json` / web client id.
- JWT lifetime (480 minutes) is shorter than some field days and the current BGud design
  re-authenticates at login (no refresh flow exists). Current-state fact only — no longer
  applicable to the target design after OQ-001 (BGud holds no JWT).

---

# 3. Gap Analysis

| ID | Severity | Gap |
|------|------|------|
| GAP-001 | CRITICAL | The request contradicted **accepted** decisions ADR-002 and ADR-003, which require JWT authentication on all Cloud write endpoints and state explicitly that Google Sign-In does not replace the API token, and it inverted the 2026-09-17 BTrade3 compatibility direction (RD-001). **Status: CLOSED — resolved by the authentication-direction override recorded under OQ-002 (§8, 2026-09-23):** ADR-002/ADR-003/ADR-007 are no longer authoritative for this feature and are to be removed/superseded per the treatment decided in OQ-002 (file status changes executed by the ADR owning process, requested in §9). No contradiction with any binding decision remains for BGud, and no temporary-exception abstraction is created. |
| GAP-002 | CRITICAL | Tenant and actor resolution is currently supplied **by the JWT itself** for every BGud operational route except the legacy `{serverId}` reads: `api/barcodes/sync`, `api/barcode-registration`, `api/BarcodeRegistration/status`, `api/return-order`, and `api/User` read `User.GetServerId()`/`User.GetUserId()` (note: `api/User` is not in BGud's consumed-endpoint set — verified against `BtradeApiService.kt` — so it falls outside the BGud anonymous scope and keeps `[Authorize]`). Removing authentication without a replacement contract does not merely drop a header — it makes these endpoints inoperable and requires a server contract change BGud cannot make alone. **Posture decided by OQ-001 (§8):** anonymous operation with BGud explicitly supplying tenant/warehouse context (no JWT-derived tenant); the per-route contract shape remains undecided (OQ-003). |
| GAP-003 | CRITICAL | No Google-account-to-BTR-user mapping exists. `BTR_User`/`BTRADE_User` have no email/Google identifier column (verified schema), and nothing in the repository maps a Google account to a BTR `UserId`, `RoleId`, or operational location. "Which Google accounts may sign in" therefore has no answer in the current data model. |
| GAP-004 | MAJOR | Login-UX swap and endpoint-auth removal are separable but entangled in BGud: the login response (`serverId`, `userId`, `warehouseCode`) drives navigation gating, login-time sync, session binding, and queued-record warehouse binding. Replacing the login call with a local-only Google gate requires re-sourcing every one of these values. |
| GAP-005 | MAJOR | Warehouse/tenant selector semantics must be reconciled now that BGud supplies tenant context explicitly. BGud's selector (`GAMPING`/`CONCAT`/`MAGELANG` → `JOGJA`/`MGL`) preserves warehouse granularity required by IR-09/ADR-RO-006 — and the OQ-001 decision (§8) requires that granularity to be preserved — while a BTrade3-style tenant selector (`JOG`/`MGL`) collapses Gamping and Concat and uses a different vocabulary for the same tenant. The contract vocabulary and selector flow must be chosen and reconciled (OQ-006, OQ-003). |
| GAP-006 | MAJOR | Removing endpoint authentication reopens the security properties ADR-002/ADR-007 were created to establish: unattributed writes (registration `RegisteredBy`, return-order actor), client-selectable tenant (cross-tenant read/write spoofing), and anonymous-writable Cloud state — while transport remains cleartext HTTP. **Accepted knowingly for BGud as deliberate policy by OQ-001/OQ-002 (§8), not a temporary exception.** Remaining work is the compensating tenant/actor contract design (OQ-003, OQ-005) and shared-consumer scoping (OQ-010), not a restoration path. |
| GAP-007 | MINOR | Workflow-stage-1 knowledge is missing: there is no DOMAIN/FEATURE artifact for the BGud authentication/warehouse-login capability (the assessment was performed from ISSUE + ADRs + code). The requested business change has not been defined as a FEATURE artifact. |
| GAP-008 | MINOR | BGud has no Google OAuth client configuration in the repository (no `google-services.json`, no client id). Reusing BTrade3's hard-coded client id/project is an external-configuration dependency, not an in-code default. |

---

# 4. Open Questions

| ID | Question | Impact |
|------|------|------|
| OQ-001 | Does "not using authentication" mean only removing the BGud client bearer header, or also removing `[Authorize]` from the Cloud controllers (issue Notes 1–2)? | **CLOSED 2026-09-23 (override) — see §8.** Answer: **full removal as deliberate BGud policy** — no `api/Auth/login` call, no JWT stored or used, no `Authorization: Bearer` header, and no `[Authorize]` on Cloud endpoints consumed by BGud; not a temporary exception (GAP-001, GAP-002). |
| OQ-002 | Will the organization formally supersede or amend ADR-002/ADR-003/ADR-007, and does the same policy decision also reverse the planned BTrade3 remediation (RD-001)? | **CLOSED 2026-09-23 (override) — see §8.** Answer: **ADR-002/ADR-003/ADR-007 are no longer authoritative for this feature and must be removed or marked superseded/inactive per repository convention** — they may not remain `Accepted` while the approved BGud design intentionally deviates; no temporary-exception abstraction; ADR ratification is **not** required before architecture (GAP-001 resolved). Scope: BGud only — BTrade3's RD-001 direction is untouched. |
| OQ-003 | BGud must now explicitly supply the tenant/warehouse context (decided direction under OQ-001 §8: existing warehouse model — Gamping/Concat → `JOGJA`, Magelang → `MGL`, warehouse granularity preserved). What is the per-route request contract for `api/barcodes/sync`, `api/barcode-registration`, `api/BarcodeRegistration/status`, and `api/return-order` — which today read `User.GetServerId()`/`User.GetUserId()` and carry no tenant field — and how are `ServerId`/`warehouseCode` carried? | BLOCKING — determines the required server contract (GAP-002). |
| OQ-004 | May any Google account sign in, or must accounts be allow-listed / mapped to existing `BTR_User` operators? If mapped, who creates and maintains the mapping, and which BTR `RoleId` applies? | BLOCKING — no data model answer exists (GAP-003, GAP-006). |
| OQ-005 | Who is recorded as the actor for barcode registrations and return orders after the JWT user id disappears, and is loss of per-operator attribution acceptable to the business? | BLOCKING — `RegisteredBy` and audit semantics depend on it (GAP-006). |
| OQ-006 | The OQ-001 direction preserves BGud's warehouse selector (Gamping/Concat/Magelang) and its granularity — which authoritative tenant vocabulary applies as BGud begins supplying tenant context explicitly (`JOGJA`/`MGL` vs BTrade3's `JOG`/`MGL`), and how does the selector state flow into the OQ-003 contract? | BLOCKING for flow definition (GAP-005); also determines whether offline queues keep warehouse binding (IR-09). |
| OQ-007 | Should BGud reuse BTrade3's Google OAuth web client/project or obtain a separate BGud registration? | Non-blocking for direction, blocking for any Google Sign-In realization detail (GAP-008). |
| OQ-008 | Should login-time master-data synchronization continue to run at login time (with whatever new identity step replaces it), and what replaces the token-based navigation gate (IR-M8)? | MAJOR — affects the target operational flow (GAP-004). |
| OQ-009 | Should this remain one combined change request or be split into (a) login UX change and (b) endpoint-auth removal? | MINOR — recommended split is documented in §7 Option D; affects ISSUE tracking, not feasibility. |
| OQ-010 | The BGud-consumed routes (`barcodes/sync`, `barcode-registration`, `BarcodeRegistration/status`, `return-order`, `Driver/{serverId}`) live on controllers shared with BTrade3, `j07-btrade-sync`, and other API consumers (ASM-002). Which non-BGud consumers, if any, depend on the current `[Authorize]` behavior of those shared routes, and is removing `[Authorize]` from those shared controllers acceptable instance-wide? (Formerly framed as scoping a "BGud-only exception"; that framing is withdrawn — the posture is now policy, so only the shared-consumer impact remains.) | BLOCKING for the Cloud change scope (GAP-002). |
| OQ-011 | What is the exit condition for the temporary exception — who owns restoring `[Authorize]`/tenant isolation after the demo, and against what trigger or date? | **OBSOLETE 2026-09-23 — retired by the OQ-001/OQ-002 override (§8):** it existed only for the temporary-exception/restore-after-demo concept, which the override removed. See §8. |

---

# 5. Assumptions

| ID | Assumption |
|------|------|
| ASM-001 | "Following BTrade3's approach" means BTrade3's **currently implemented** behavior (local Google gate, email as login state, anonymous APIs, explicit `serverId`), not an as-yet undocumented target design. |
| ASM-002 | BGud and BTrade3 consume the same Cloud database/host today (`dev.smart-ics.com:8089`), so any BGud endpoint change affects data shared with BTrade3 and `j07-btrade-sync`. |
| ASM-003 | `BTR_User` remains the operator store of record at the Main Office; any new identifier mapping must not turn the Cloud into an independent credential authority (PRODUCT principle "Single Source of Truth"). |
| ASM-004 | ADR-002/ADR-003/ADR-007 are **not** treated as binding for this feature: the 2026-09-23 authentication-direction override (§8 OQ-001/OQ-002) removed their authority for BGud and approved their removal/supersession in the repository. The BGud anonymous posture is deliberate policy, not a temporary exception. Their ADR files still read `Accepted` pending status treatment by the ADR owning process (request in §9); that treatment is a knowledge-synchronization follow-up, not an architecture prerequisite. |
| ASM-005 | There are (or may be) field-deployed BGud devices with queued local records bound to `warehouseCode`; the change must not silently re-home or orphan them. |

---

# 6. Risks

| ID | Risk | Impact | Mitigation |
|------|------|------|------|
| RISK-001 | BGud client stops sending tokens while the Cloud still enforces `[Authorize]` (partial change) | Complete BGud operational outage (401 on every route) | **Answered by OQ-001 (§8):** both sides change as deliberate policy — client and Cloud must ship as one coordinated release |
| RISK-002 | Anonymous, client-selectable tenant on write endpoints | Cross-tenant data pollution/spoofing; forged registrations and return orders | **Accepted for BGud as deliberate policy (OQ-001/OQ-002, §8)** — not a time-boxed exception. Compensating design is the explicit tenant contract (OQ-003) and the actor model (OQ-005); shared-instance consumer impact is scoped by OQ-010. No restore-authorization path applies (OQ-011 obsolete). |
| RISK-003 | Loss of per-operator attribution on registrations/return orders | Audit and Main-Office validation expectations (ADR-002 context) break | Decide actor model (OQ-004/005); Google-email-based attribution is still possible as a business choice without reintroducing API authentication |
| RISK-004 | Shared cleartext HTTP with any credential (Google ID token or JWT) in transit | Credential exposure | Pre-existing platform risk. After OQ-001, BGud sends no credentials to the Cloud API (Google Sign-In is a local gate), so this does not gate BGud's anonymous API traffic; HTTPS remains a general platform concern (RD-004 mirror) |
| RISK-005 | Reuse of BTrade3's Google OAuth client (ASM/ OQ-007) | Coupled app inventories; BTrade3 project changes can break BGud sign-in | Register a BGud-specific OAuth client unless reuse is explicitly approved |
| RISK-006 | Duplicated/conflicting identity stores over time (Google allow-list vs `BTR_User`) | Off-boarded BTR users retain app access | If Google login is adopted, define the authoritative mapping and provisioning owner (OQ-004) |
| RISK-007 | Tenant vocabulary drift (`JOG` vs `JOGJA`, collapsed warehouse granularity) | Wrong/failed tenant resolution on anonymous routes; queue re-homing violations (IR-09) | Resolve selector and vocabulary decision first (OQ-006, GAP-005) |
| RISK-008 | Reversing direction while BTrade3 remediation (RD-001: BTrade3 adopts JWT) is still planned | The two work items cancel each other; wasted effort | **Scoped by OQ-002 (§8):** the override applies to BGud only and explicitly does not decide BTrade3's direction; RD-001 remains as recorded in the BTrade3 compatibility assessment. Do not generalize the BGud decision to other mobile applications |
| RISK-009 | The anonymous posture applies to Cloud controllers shared with BTrade3 and `j07-btrade-sync` (ASM-002); removing `[Authorize]` from shared routes changes behavior for every consumer of those controllers, and compensating tenant/actor controls are not yet designed | Cross-tenant and unattributed writes against live shared data; non-BGud consumers of the same controllers may be affected unexpectedly | Scope the consumer impact per OQ-010 and design the compensating tenant/actor contract per OQ-003/OQ-005; verify no non-BGud consumer depends on `[Authorize]` for the BGud-consumed routes. (The former exit-plan mitigation via OQ-011 is retired — the posture is no longer an exception.) |

---

# 7. Recommendations

Alternative solution directions only. **No final decision is recorded here** — decisions
belong in §8 Gap Closure after stakeholder/architecture input.

*Status note (v1.3):* the authentication posture is decided as **deliberate BGud policy**
(§8 OQ-001 CLOSED — full removal: no `api/Auth/login`, no JWT, no bearer header, no
`[Authorize]` on BGud-consumed endpoints; **not** a temporary exception), and the ADR
treatment is decided (§8 OQ-002 CLOSED — ADR-002/003/007 no longer authoritative for
BGud, to be removed/superseded; no ratification prerequisite; no temporary-exception
abstraction). **Option A is the selected direction.** The options below remain the
pre-decision analysis record; Option A's remaining unknowns are open as
OQ-003/OQ-004/OQ-005/OQ-006/OQ-010. Options B and C are ruled out for BGud by OQ-001 —
no replacement authentication mechanism is to be introduced unless it is required by the
actual BGud business flow.

## Option A — Literal BTrade3 parity (full removal) — SELECTED DIRECTION (§8 OQ-001)

Google Sign-In as a local gate; BGud sends no credentials; `[Authorize]` removed (or routes
duplicated) on the Cloud; tenant supplied explicitly by the client like BTrade3's
`{serverId}` convention, using BGud's existing warehouse model with granularity preserved.

### Advantages

- Exactly matches the requested outcome and BTrade3's current behavior.
- Simplest client: no token storage, no 401 handling, no login call.

### Disadvantages

- Requires server-side contract changes (GAP-002) — direction decided, contract shape
  still open (OQ-003).
- Accepts anonymous, client-selectable-tenant writes knowingly for BGud (GAP-006,
  RISK-002/003); the actor and tenant contracts remain to be designed (OQ-003/OQ-005).
- Warehouse granularity must be carried through the explicit tenant contract
  (GAP-005, OQ-006).

## Option B — Google identity UX with Cloud-issued JWT (federated login) — RULED OUT for BGud (§8 OQ-001)

**Ruled out for BGud by the OQ-001 decision:** BGud must not store or use a JWT, must not
send an `Authorization: Bearer` header, and the endpoints it consumes must not require
`[Authorize]`; a federated login that issues an API token reintroduces exactly the
authentication infrastructure the decision removes. Retained below as historical analysis
only.

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

## Option C — Partial removal (anonymous reads, authenticated writes) — RULED OUT for BGud (§8 OQ-001)

**Ruled out for BGud by the OQ-001 decision:** Cloud endpoints consumed by BGud must not
require `[Authorize]`; a split anonymous-read/authenticated-write model contradicts the
decision. Retained below as historical analysis only.

Google-gated login for UX plus (optionally) anonymous tenant-parameterized reads; the
write endpoints (`barcode-registration`, `return-order`, ack) keep JWT per ADR-002.

### Advantages

- Reduces the authenticated surface and demonstrates "unauthenticated consumption" for
  the read half without abandoning write attribution.

### Disadvantages

- Splits the platform policy again (two models coexist); the read routes still need a
  tenant parameter replacement (GAP-002 variant).
- Half-meets both parts of the request.

## Option D — Issue sequencing (applies to any option)

Split the ISSUE into (D1) login method change (Google Sign-In) and (D2) endpoint
authentication removal if release coordination requires it. After the OQ-001/OQ-002
decisions (§8) no policy conflict remains between D1 and D2 — both proceed under the
same anonymous posture — so the split is now optional, recommended only for release
coordination (RISK-001). Remaining gates for D2 are OQ-003/OQ-010; D1's gates are
OQ-004/OQ-007/OQ-008. Also create the missing FEATURE artifact for the BGud
operator-authentication capability (GAP-007) before architecture. Answers issue Note 8.

### Advantages

- Unblocks the achievable part; keeps scope explicit and auditable; answers issue Note 8.

### Disadvantages

- Two issues to coordinate for any shared release (RISK-001).

---

# 8. Gap Closure

Ledger: **GAP-001 — CLOSED (2026-09-23)**, **OQ-001 — CLOSED (2026-09-23, override)**,
**OQ-002 — CLOSED (2026-09-23, override)**, **OQ-011 — OBSOLETE (2026-09-23)**. All other
entries (GAP-002…GAP-008, OQ-003…OQ-010) remain **Status: OPEN**. When a further
resolution is approved it will be recorded here in place (Decision, Rationale, Impact,
Architecture Impact, Resolved By, Resolved Date) without renumbering, and Planning
Readiness will be updated accordingly.

## GAP-001

**Status: CLOSED**

### Decision

Resolved by the BGud authentication-direction override recorded under OQ-002 (§8).

ADR-002, ADR-003, and ADR-007 are **no longer authoritative for this feature**. The
contradiction between the requested change and those ADRs is removed by superseding or
inactivating the ADRs for BGud (treatment decided in OQ-002) — not by creating a
temporary exception and not by an ADR-ratification prerequisite. No binding decision now
opposes BGud's anonymous authentication posture.

### Rationale

The ADRs were created for a different security posture. BGud is an internal, limited-user
application; the repository owner has deliberately chosen anonymous operation for BGud to
keep the implementation simple and the demo/MVP operable. Contradictory ADRs must not
remain `Accepted` while the approved architecture intentionally deviates from them, and
they must no longer block BGud architecture or implementation.

### Impact

- GAP-001 is resolved and no longer blocks architecture.
- The BTrade3 compatibility direction (RD-001, its own assessment) is unaffected — this
  closure is scoped to BGud (see OQ-002 scope limitation).
- ADR file status changes and downstream reference clean-up are follow-ups owned by the
  ADR/architecture processes (inventory in §9) and are explicitly **not** prerequisites
  for architecture work on this feature.

### Architecture Impact

- BGud architecture may be designed against the anonymous Cloud contract (subject to the
  remaining open questions OQ-003…OQ-006, OQ-010) without ratifying anything against
  ADR-002/003/007.
- Barcode-registry and return-order artifacts that still cite these ADRs as the basis for
  JWT enforcement on the shared controllers need consistency updates by their owning
  processes (inventory in §9); this assessment does not rewrite them.

### Resolved By

Repository owner (issue reporter) via the authentication-direction override; recorded by
the Analyst.

### Resolved Date

2026-09-23

---

## OQ-001

**Status: CLOSED** — Accepted (BGud Authentication Direction Override)

_Supersedes the earlier OQ-001 decision recorded in v1.2 of this assessment (temporary
demo exception), which is no longer valid._

### Decision

"Not using authentication" for BGud means, as a **deliberate architectural decision for
this feature — not a temporary or demo-only exception**:

1. BGud does not authenticate against `api/Auth/login`.
2. BGud does not store or use a JWT for Cloud API communication.
3. BGud does not send an `Authorization: Bearer` header.
4. Cloud endpoints consumed by BGud must not require `[Authorize]`.

The Cloud API contract must therefore support anonymous BGud operation. JWT is **not**
preserved merely because the current implementation already uses it.

**Tenant consequence.** JWT-based tenant resolution must not be reproduced. BGud
explicitly provides the tenant/warehouse context required by the Cloud API, using the
existing BGud warehouse model:

| Warehouse | ServerId |
|---|---|
| Gudang Gamping | `JOGJA` |
| Gudang Concat | `JOGJA` |
| Gudang Magelang | `MGL` |

Warehouse granularity is preserved in BGud even when multiple warehouses map to the same
`ServerId`. Affected Cloud API contracts must be updated so they no longer depend on
`User.GetServerId()`, `User.GetUserId()`, or JWT claims for BGud operations.

**Scope limitation.** This decision means only that **BGud's current authentication
requirement is intentionally removed** because BGud is a controlled, limited-user
application and simplicity is the current priority. It does **not** mean that
authentication is never needed for BTR, that all future mobile applications must be
anonymous, or that security should never be implemented. No replacement authentication
mechanism is to be introduced unless it is required by the actual BGud business flow.

### Rationale

BGud is an internal, limited-user Android application, not a public application
distributed through Google Play. For the current product stage, authentication/security
is not the primary concern. The stated priorities are:

1. Get BGud working.
2. Keep the implementation simple.
3. Avoid introducing authentication infrastructure that adds complexity.
4. Avoid architecture that will make the demo/MVP unnecessarily difficult to operate.

The existing authentication architecture is therefore unnecessary for BGud at this stage.
The current implementation's JWT flow is not worth preserving for its own sake: removing
it client-side alone would 401 against the current `[Authorize]` controllers, so both
client and Cloud change together as one deliberate posture.

### Impact

- Resolves issue Notes 1–2: the endpoint scope is the full BGud-consumed set, and client
  **and** server change together (RISK-001 is answered as "change both", not avoided).
- Selects §7 **Option A** as the standing direction and rules out **Option B**
  (federated login keeping JWT) and **Option C** (anonymous reads / authenticated writes)
  for BGud — no replacement authentication mechanism without an actual business-flow
  requirement.
- Does not close GAP-002 (per-route contract shape undecided — OQ-003), GAP-003
  (Google-account authority — OQ-004), GAP-004 (OQ-008), GAP-005 (OQ-006), GAP-006
  (accepted knowingly as policy; compensating contracts OQ-003/OQ-005), GAP-007,
  GAP-008.
- GAP-001 is closed via OQ-002 (§8).
- OQ-010 is refocused from "scoping a BGud-only exception" to shared-consumer impact;
  OQ-011 is retired as obsolete because no exception exit/restoration condition exists.
- Reconciles ASM-004: the former ADRs are no longer binding for this feature.

### Architecture Impact

- The Cloud is in scope: `[Authorize]` removal on BGud-required endpoints is a contract
  change that the Architect designs against this decision — **no ADR ratification is
  required first** (OQ-002).
- Evidence-derived endpoint scope (BGud's authenticated surface, verified in
  `BtradeApiService.kt` and the controller attributes in §2) is five routes:
  `GET api/barcodes/sync`, `POST /api/barcode-registration`,
  `GET api/BarcodeRegistration/status`, `POST api/return-order`, and
  `GET api/Driver/{serverId}`. `api/User` is `[Authorize]` but is not consumed by BGud
  and keeps its attribute. BGud also ceases calling `POST api/Auth/login` entirely.
- Four of those five routes currently derive tenant (and, for registration, actor)
  **from the JWT itself** and carry no client-supplied tenant field (P-06, historical
  ADR-007 §4), so anonymous exposure cannot be an attribute-only edit for them: the
  explicit tenant contract is exactly the OQ-003/OQ-005/OQ-006 work, with the warehouse
  map above as the decided vocabulary input. Only `api/Driver/{serverId}` is already
  tenant-explicit in the route.
- Google Sign-In remains part of the request (ISSUE part 1) and is **not** resolved by
  this decision: OQ-004 (account authority), OQ-007 (OAuth client), OQ-008 (login-time
  sync and navigation gate) still stand; the local gate replaces the token-based
  navigation gate (IR-M8) as a flow question, not an authentication mechanism.
- Cleartext-HTTP constraints (RISK-004) are no longer a BGud blocker (no credentials in
  transit), and the queued-record warehouse binding constraint (IR-09, ASM-005) is
  unchanged and must be honored by the explicit tenant/warehouse contract.

### Resolved By

Repository owner (issue reporter) via the authentication-direction override; recorded by
the Analyst.

### Resolved Date

2026-09-23

---

## OQ-002

**Status: CLOSED** — Accepted (ADR Non-Authority for BGud; ADR Treatment Approved)

_Supersedes the earlier OQ-002 decision recorded in v1.2 of this assessment (temporary
exception ratified; ADRs remain `Accepted`), which is no longer valid._

### Decision

**The existing authentication ADRs that directly block the BGud design are no longer
authoritative for this feature.** Specifically ADR-002
(`authenticated-jwt-write-endpoints`), ADR-003 (`mobile-authentication-against-cloud-api`),
and ADR-007 (`tenant-isolation-from-authenticated-identity`).

Required repository treatment (decided):

- Do **not** keep contradictory ADRs marked `Accepted` while the BGud architecture
  intentionally violates them.
- Do **not** create another "temporary exception" abstraction.
- Remove the obsolete ADRs, **or** mark them `Superseded` / `Rejected` / otherwise
  inactive according to the repository's established ADR lifecycle — whichever fits the
  convention — preserving historical traceability where appropriate.
- After treatment, they must no longer block BGud architecture or implementation.

ADR ratification is **not** required before architecture can proceed on this feature, and
this assessment no longer treats the ADRs as binding for BGud.

**Scope limitation (Important Clarification).** This decision does **not** mean that
authentication is never needed for BTR, that all future mobile applications must be
anonymous, or that security should never be implemented. It means only that BGud's
current authentication requirement is intentionally removed because BGud is a controlled,
limited-user application and simplicity is the current priority. The BTrade3 remediation
direction (RD-001 in `docs/work/btrade3-api-compatibility/FEASIBILITY-ASSESSMENT.md`) is
not reversed or decided by this question and remains as recorded in its own assessment.

### Rationale

These ADRs were created for a different security posture and now conflict with the
approved BGud direction. Leaving them `Accepted` while the approved architecture
deliberately violates them would make the repository's knowledge base self-
contradictory. Marking them as a temporary exception would contradict the owner's
instruction to treat the BGud posture as a deliberate architectural decision and would
create an exception abstraction with no purpose. Superseding/inactivating them preserves
historical traceability (why the barcode-registry era required JWT) without letting them
block BGud.

### Impact

- GAP-001 is **CLOSED** by this decision (§8 GAP-001 block).
- No ADR ratification, deferral schedule, or restoration owner is needed before
  architecture; the assessment no longer describes the posture as a temporary demo
  exception anywhere.
- OQ-011 (exception exit condition) is retired as obsolete.
- The BGud decision does not generalize: BTrade3's RD-001 direction and barcode-registry's
  own historical decision records stand as their own artifacts' content, subject to the
  consistency follow-ups in §9.

### Architecture Impact

- BGud architecture may proceed against the anonymous contract once the remaining open
  questions (OQ-003, OQ-004, OQ-005, OQ-006, OQ-010) are resolved — without construing
  ADR-002/003/007 as binding.
- The BGud warehouse model table (Gamping/Concat → `JOGJA`, Magelang → `MGL`) remains the
  tenant vocabulary input for OQ-003/OQ-006 regardless of ADR-007's file status; the
  authoritative copy of that mapping is the `BTR_WarehouseMapping` data, not the ADR.
- Execution of the ADR status treatment itself belongs to the ADR owning process
  (Architect / ADR decision process) — request recorded in §9. As of this version the
  ADR files still read `Accepted`; that pending file edit is a knowledge-synchronization
  follow-up and is **not** a prerequisite for architecture work on this feature.
- Downstream artifacts that cite these ADRs as binding (barcode-registry architecture,
  implementation plan, and feasibility assessment; return-order artifacts; BTrade3
  compatibility assessment and investigation; a BGud investigation note; source-code
  comments) are inventoried in §9 for their owning processes — this assessment does not
  rewrite unrelated barcode-registry architecture.

### Resolved By

Repository owner (issue reporter) via the authentication-direction override; recorded by
the Analyst. ADR file status treatment to be executed by the ADR owning process (§9).

### Resolved Date

2026-09-23

---

## OQ-011

**Status: OBSOLETE** — retired 2026-09-23

### Decision

Entry retired. The temporary-exception / restore-`[Authorize]`-after-demo concept it
addressed no longer exists: the OQ-001/OQ-002 authentication-direction override (§8)
establishes anonymous BGud operation as deliberate policy. No restoration owner, trigger,
or date is required for BGud.

### Rationale

The question existed only because of the temporary-exception framing, which the override
explicitly withdrew. Keeping it open would reintroduce the abolished exception
abstraction.

### Impact

- RISK-009's mitigation no longer depends on OQ-011; shared-consumer scope remains open
  as OQ-010.
- The "restore `[Authorize]` after the demo" follow-up ISSUE previously recommended in
  v1.2 §9 is withdrawn — it belongs to the abolished exception concept.

### Architecture Impact

- None — no restoration path or exception expiry needs to be designed for BGud.

### Resolved By

Repository owner (issue reporter) via the authentication-direction override; recorded by
the Analyst.

### Resolved Date

2026-09-23

---

# 9. Planning Readiness

## Readiness Checklist

- [ ] All critical gaps resolved (GAP-001 CLOSED by OQ-002; GAP-002, GAP-003 open)
- [ ] All required decisions recorded (§8 holds the authentication-direction decisions —
      OQ-001 full anonymous posture with explicit tenant supply, OQ-002 ADR non-authority
      and treatment approval, GAP-001 closed, OQ-011 retired; the anonymous
      tenant/actor/selector contract decisions are still outstanding)
- [ ] All blocking open questions resolved (OQ-001, OQ-002 closed; OQ-003, OQ-004,
      OQ-005, OQ-006, OQ-010 open)
- [ ] Architecture can be finalized or updated

## Status

NOT-READY

## Notes

- The direction is now fixed: BGud operates **anonymously by deliberate policy** (no
  `api/Auth/login`, no JWT, no bearer header, no `[Authorize]` on BGud-consumed
  endpoints; tenant/warehouse context supplied explicitly per the warehouse model in
  §8 OQ-001), and ADR-002/003/007 are **no longer authoritative for this feature**
  (§8 OQ-002). There is no temporary-exception framing and no ADR-ratification
  prerequisite anywhere in this assessment.
- The remaining blocking core is the **explicit tenant/actor/selector contract**:
  dependency order OQ-003 (per-route tenant contract), OQ-005 (actor model for
  registrations and return orders), OQ-006 (selector and tenant-vocabulary authority,
  `JOG` vs `JOGJA`), OQ-004 (Google-account authority for ISSUE part 1), then OQ-010
  (shared-controller consumer impact for the Cloud change). OQ-007 and OQ-008 gate
  realization details; OQ-009 remains minor; OQ-011 is obsolete.
- **Requests to owning roles (outside Analyst authority — recorded here, not executed):**
  1. *ISSUE update (owner: Issue Intake / ISSUE owning process).* Update
     `docs/issues/BGUD-GOOGLE-SIGNIN-ISSUE.md` so its authentication requirement matches
     the §8 decisions: Google Sign-In may be used as a local UI gate only; no API
     authentication is required; Cloud endpoints used by BGud are anonymous; tenant
     context is explicitly supplied by BGud using the warehouse model (Gamping/Concat →
     `JOGJA`, Magelang → `MGL`, granularity preserved); no JWT-based user/tenant session
     is required; and anonymous access must **not** be described as a temporary workaround.
     Issue Notes 1–2 are answered by OQ-001; Note 3 is reframed by OQ-001/OQ-003.
  2. *ADR treatment (owner: ADR owning process / Architect).* Execute the OQ-002 decision
     on the ADR files: remove or mark `Superseded`/`Rejected`/inactive
     `docs/work/barcode-registry/adrs/ADR-002-authenticated-jwt-write-endpoints.md`,
     `ADR-003-mobile-authentication-against-cloud-api.md`, and
     `ADR-007-tenant-isolation-from-authenticated-identity.md` per the repository's ADR
     lifecycle; no contradictory ADR may remain `Accepted` while the approved BGud
     design deviates. (Repository check found no pre-existing `Superseded`/`Rejected` ADR
     precedent; IMPL-PLAN convention retains superseded outcomes as historical evidence —
     status marking with retained history is the closest fit.)
  3. *ADR reference consistency (owner: each artifact's owning process; inventory only —
     no broad rewrite performed):*
     - `docs/work/barcode-registry/BARCODE-REGISTRY-ARCHITECTURE.md` — ADR rows
       (≈L59, L208–213) plus **directly BGud-contradicting lines** ≈L1069 ("BGud …
       Authenticates against `pkl.btrade.api` … Presents the JWT") and L1710 (IR-M8
       "no valid JWT → Login"); barcode-registry's own JWT rows remain its owner's scope.
     - `docs/work/barcode-registry/BARCODE-REGISTRY-IMPL-PLAN.md` (≈L350, L363, L383,
       L403, L551, L588) and `BARCODE-REGISTRY-FEASIBILITY-ASSESMENT.md` (GAP-008
       closure, decision-record lists) — historical decision records citing the ADRs as
       the enforcement basis.
     - `docs/work/return-order/*` (`RETURN-ORDER-ARCHITECTURE.md` L125,
       `RETURN-ORDER-FEASIBILITY-ASSESSMENT.md` L228/L277/L731,
       `RETURN-ORDER-IMPL-PLAN.md` L420) — `api/return-order` is BGud-consumed and will
       become anonymous; these artifacts still assume the JWT-tenant model.
     - `docs/work/btrade3-api-compatibility/FEASIBILITY-ASSESSMENT.md` and
       `docs/investigations/btrade3-api-compatibility/investigation.md` — extensive
       ADR-002/003/007 dependency for BTrade3's RD-001; BTrade3-scoped, not reversed by
       this decision, but the cited ADR statuses change under OQ-002.
     - `docs/investigations/bgud-first-install-infinite-spinner.md` L139 — "tenant is
       JWT-resolved (ADR-007)" guidance becomes stale for BGud.
     - Source comments (implementation-time cleanup under the approved plan):
       `src/j07-btrade-sync/.../UserDal.cs` L18 and BGud files
       (`AuthInterceptor.kt`, `ApiClient.kt`, `ApiModels.kt`, `LoginViewModel.kt`,
       `Navigation.kt`, `BtradeApiService.kt`, `SessionBinding.kt`, `BarcodeSyncRepository.kt`,
       `BarcodeSyncWorker.kt`, `SettingsScreen.kt`) citing ADR-002/003/007.
- READY-FOR-PLANNING means that feasibility is sufficiently resolved for the Architecture
  skill to finalize or update the target architecture. It does not mean that architecture
  is complete or that the architecture step may be skipped. Planning begins only after the
  required architecture work is complete. Only the Architect grants the gate; this
  artifact remains NOT-READY while blocking gaps remain.
- Knowledge propagation: the OQ-001/OQ-002 decisions must be reflected in the ADR files'
  own status records by their owning process (request 2 above) so the knowledge artifacts
  stay synchronized; the ISSUE must be aligned (request 1 above). A FEATURE artifact for
  operator authentication is still required (GAP-007, stage Discovery), and the
  ARCHITECTURE work for the confirmed Cloud contract change belongs to the Architect once
  the blocking OQs are closed.

---

# 10. References

Referenced artifacts:

- `docs/issues/BGUD-GOOGLE-SIGNIN-ISSUE.md` (ISSUE, input — alignment request in §9)
- `docs/foundation/DOMAIN.md`, `docs/foundation/LANDSCAPE.md`, `docs/foundation/PRODUCT.md`, `docs/foundation/WORKFLOW.md`
- `docs/work/barcode-registry/adrs/ADR-002-authenticated-jwt-write-endpoints.md`
  — no longer authoritative for BGud (§8 OQ-002); status treatment requested (§9)
- `docs/work/barcode-registry/adrs/ADR-003-mobile-authentication-against-cloud-api.md`
  — no longer authoritative for BGud (§8 OQ-002); status treatment requested (§9)
- `docs/work/barcode-registry/adrs/ADR-007-tenant-isolation-from-authenticated-identity.md`
  — no longer authoritative for BGud (§8 OQ-002); status treatment requested (§9);
  warehouse mapping table content superseded as knowledge by `BTR_WarehouseMapping` data
  and the §8 OQ-001 decision
- `docs/work/btrade3-api-compatibility/FEASIBILITY-ASSESSMENT.md` (RD-001…RD-006 —
  BTrade3-scoped; unaffected by this override)
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
