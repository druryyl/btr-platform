---
Title: BGud — Google Sign-In and Unauthenticated Cloud Endpoints — Feasibility Assessment
Code: BGUD-GOOGLE-SIGNIN-001
Artifact: FEASIBILITY-ASSESSMENT
Version: 1.17
LastUpdated: 2026-09-23
Status: READY-FOR-PLANNING
---

# 1. Request Summary

Assessment of ISSUE `BGUD-GOOGLE-SIGNIN-001` (`docs/issues/BGUD-GOOGLE-SIGNIN-ISSUE.md`):
replace BGud's username/password/warehouse login with Google Sign-In (BTrade3-style) and
stop authenticating the Cloud endpoints BGud consumes.

Referenced artifacts:

- DOMAIN: `docs/foundation/DOMAIN.md`, `docs/work/barcode-registry/BARCODE-REGISTRY-DOMAIN.md`
  (no dedicated DOMAIN artifact exists for the BGud authentication flow; operator-account
  business knowledge is owned by Main Office and the OQ-004 decision — see GAP-007)
- FEATURE: `docs/features/bgud-operator-signin/feature.md` (`BGUD-OPERATOR-SIGNIN-001`) —
  created to close GAP-007
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
  on both sides (GAP-002; the target contract is decided in OQ-003 §8 — explicit
  session `locationId`, server-side resolution via `BTR_WarehouseMapping`).
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
| GAP-002 | CRITICAL | Tenant and actor resolution is currently supplied **by the JWT itself** for every BGud operational route except the legacy `{serverId}` reads: `api/barcodes/sync`, `api/barcode-registration`, `api/BarcodeRegistration/status`, `api/return-order`, and `api/User` read `User.GetServerId()`/`User.GetUserId()` (note: `api/User` is not in BGud's consumed-endpoint set — verified against `BtradeApiService.kt` — so it falls outside the BGud anonymous scope and keeps `[Authorize]`). Removing authentication without a replacement contract does not merely drop a header — it makes these endpoints inoperable and requires a server contract change BGud cannot make alone. **Posture decided by OQ-001 (§8)** and **tenant contract decided by OQ-003 (§8):** BGud carries the session `locationId` (`GAMPING`/`CONCAT`/`MAGELANG`) explicitly on every request, and the Cloud resolves it through `BTR_WarehouseMapping` to `ServerId` (no JWT-derived tenant). **Actor contract decided by OQ-005 (§8):** BGud carries the logged-in Google email as the actor input; the Cloud resolves it via the OQ-004 Main Office mapping to the authoritative BTR `UserId`. **Shared-controller scope is decided by OQ-010 (§8).** **Status: CLOSED (2026-09-23, §8 GAP-002)** — the target anonymous contract is fully decided; exact DTOs, carriers, controller changes, and implementation mechanics are architecture/implementation work, not feasibility blockers. |
| GAP-003 | CRITICAL | No Google-account-to-BTR-user mapping exists. `BTR_User`/`BTRADE_User` have no email/Google identifier column (verified schema), and nothing in the repository maps a Google account to a BTR `UserId`, `RoleId`, or operational location. "Which Google accounts may sign in" therefore has no answer in the current data model. **Status: CLOSED (2026-09-23, §8 GAP-003)** — resolved by OQ-004: only Google accounts mapped one-to-one to existing BTR users may sign in; the mapping is maintained in the Main Office **User** menu and Main Office is its authoritative source; the existing BTR `UserId`/`RoleId` remain authoritative. Required database, synchronization/projection, and User-menu UI changes are architecture/implementation work, not feasibility blockers. |
| GAP-004 | MAJOR | Login-UX swap and endpoint-auth removal are separable but entangled in BGud: the login response (`serverId`, `userId`, `warehouseCode`) drives navigation gating, login-time sync, session binding, and queued-record warehouse binding. Replacing the login call with a local-only Google gate requires re-sourcing every one of these values. **Status: CLOSED (2026-09-23, §8 GAP-004)** — resolved by OQ-008: login-time synchronization remains part of login; the JWT navigation gate is replaced by a local session gate (validated Google email + selected Gudang + persisted local session); logout and warehouse change clear the session while queued offline records retain their warehouse binding. Required client refactoring is architecture/implementation work, not a feasibility blocker. |
| GAP-005 | MAJOR | Warehouse/tenant selector semantics must be reconciled now that BGud supplies tenant context explicitly. BGud's selector (`GAMPING`/`CONCAT`/`MAGELANG` → `JOGJA`/`MGL`) preserves warehouse granularity required by IR-09/ADR-RO-006 — and the OQ-001 decision (§8) requires that granularity to be preserved — while a BTrade3-style tenant selector (`JOG`/`MGL`) collapses Gamping and Concat and uses a different vocabulary for the same tenant. **Status: CLOSED — resolved by OQ-003/OQ-006 (§8):** the existing three-Gudang selector is retained; BGud carries `locationId` (`GAMPING`/`CONCAT`/`MAGELANG`) as session context; `ServerId` stays a Cloud-side value derived via the existing warehouse mapping (BTrade3's `JOG` vocabulary is not on BGud's request path); offline queue binding is retained per the OQ-003 session rule. |
| GAP-006 | MAJOR | Removing endpoint authentication reopens the security properties ADR-002/ADR-007 were created to establish: unattributed writes (registration `RegisteredBy`, return-order actor), client-selectable tenant (cross-tenant read/write spoofing), and anonymous-writable Cloud state — while transport remains cleartext HTTP. **Status: CLOSED (2026-09-23, §8 GAP-006)** — accepted knowingly for BGud as deliberate policy by OQ-001/OQ-002 (not a temporary exception); the compensating business contracts are decided — tenant by OQ-003, actor by OQ-005, and shared-controller scope by OQ-010. Security hardening beyond the approved posture is not a feasibility prerequisite; remaining work is contract implementation and pre-deployment verification, not a restoration path. |
| GAP-007 | MINOR | Workflow-stage-1 knowledge is missing: there is no DOMAIN/FEATURE artifact for the BGud authentication/warehouse-login capability (the assessment was performed from ISSUE + ADRs + code). The requested business change has not been defined as a FEATURE artifact. **Status: CLOSED (2026-09-23, §8 GAP-007)** — resolved by the new FEATURE artifact `docs/features/bgud-operator-signin/feature.md` (`BGUD-OPERATOR-SIGNIN-001`), which defines the capability against the approved §8 decisions. |
| GAP-008 | MINOR | BGud has no Google OAuth client configuration in the repository (no `google-services.json`, no client id). Reusing BTrade3's hard-coded client id/project is an external-configuration dependency, not an in-code default. **Status: CLOSED (2026-09-23, §8 GAP-008)** — resolved by OQ-007: BGud reuses BTrade3's existing Google OAuth project and web client configuration, following BTrade3's already-implemented Sign-In pattern; no separate BGud OAuth registration. Adding the required configuration to BGud and reconciling Android package/fingerprint requirements are architecture/implementation tasks, not feasibility blockers. |

---

# 4. Open Questions

| ID | Question | Impact |
|------|------|------|
| OQ-001 | Does "not using authentication" mean only removing the BGud client bearer header, or also removing `[Authorize]` from the Cloud controllers (issue Notes 1–2)? | **CLOSED 2026-09-23 (override) — see §8.** Answer: **full removal as deliberate BGud policy** — no `api/Auth/login` call, no JWT stored or used, no `Authorization: Bearer` header, and no `[Authorize]` on Cloud endpoints consumed by BGud; not a temporary exception (GAP-001, GAP-002). |
| OQ-002 | Will the organization formally supersede or amend ADR-002/ADR-003/ADR-007, and does the same policy decision also reverse the planned BTrade3 remediation (RD-001)? | **CLOSED 2026-09-23 (override) — see §8.** Answer: **ADR-002/ADR-003/ADR-007 are no longer authoritative for this feature and must be removed or marked superseded/inactive per repository convention** — they may not remain `Accepted` while the approved BGud design intentionally deviates; no temporary-exception abstraction; ADR ratification is **not** required before architecture (GAP-001 resolved). Scope: BGud only — BTrade3's RD-001 direction is untouched. |
| OQ-003 | If authentication is removed, how must the Cloud identify the tenant? | **CLOSED 2026-09-23 — see §8.** Answer: **session-scoped Gudang location** — the selected `locationId` (`GAMPING`/`CONCAT`/`MAGELANG`) is the active BGud session context and is carried explicitly with every Cloud request; the Cloud resolves `locationId` → `BTR_WarehouseMapping` → `ServerId`. `api/Driver/{serverId}` keeps its route-based contract. Carries **`locationId`, not just `ServerId`**, so `GAMPING` and `CONCAT` stay distinct (GAP-002 tenant half; OQ-006, GAP-005 resolved). |
| OQ-004 | May any Google account sign in, or must accounts be allow-listed / mapped to existing `BTR_User` operators? If mapped, who creates and maintains the mapping, and which BTR `RoleId` applies? | **CLOSED 2026-09-23 — see §8.** Answer: **pre-registered mapping required** — a Google account may sign in only when it is mapped (one Google email per BTR user) to an existing BTR user; the mapping is created/modified/removed in the Main Office **User** menu by any BTR user with User-menu permission; the existing BTR `RoleId` remains the authoritative role; unmapped Google accounts are rejected; Main Office remains the authoritative source and no separate BGud user-management mechanism is introduced (GAP-003 direction decided). |
| OQ-005 | Who is recorded as the actor for barcode registrations and return orders after the JWT user id disappears, and is loss of per-operator attribution acceptable to the business? | **CLOSED 2026-09-23 — see §8.** Answer: **logged-in Google email as payload actor** — BGud sends the signed-in Google email with registration and return-order requests; the Cloud resolves it through the OQ-004 Main Office mapping to the authoritative BTR `UserId` and records that as actor / `RegisteredBy`. Per-operator attribution is preserved (no loss of attribution), and no API authentication/JWT is reintroduced (OQ-001) — GAP-002's actor residual and GAP-006's compensating actor contract are resolved. |
| OQ-006 | Should the warehouse selector (Gamping/Concat/Magelang) remain, and with which authoritative tenant vocabulary? | **CLOSED 2026-09-23 — resolved by the OQ-003 decision (§8).** Selector retained (three Gudang values); carried vocabulary is `locationId`; `ServerId` derived server-side (`JOGJA`/`MGL`) per existing mapping; BTrade3's `JOG` not used by BGud; session-fixed Gudang with re-selection after session end; offline queues retain location binding (IR-09, no re-homing) (GAP-005 closed). |
| OQ-007 | Should BGud reuse BTrade3's Google OAuth web client/project or obtain a separate BGud registration? | **CLOSED 2026-09-23 — see §8.** Answer: **reuse BTrade3's existing Google OAuth project and web client configuration** — BGud follows the same Google Sign-In configuration pattern already implemented in BTrade3; no separate BGud OAuth project/client is registered (GAP-008 direction decided). |
| OQ-008 | Should login-time master-data synchronization continue to run at login time (with whatever new identity step replaces it), and what replaces the token-based navigation gate (IR-M8)? | **CLOSED 2026-09-23 — see §8.** Answer: **Yes. Login-time synchronization remains part of the BGud login flow.** Navigation gate is replaced by local BGud session state (validated Google email + selected Gudang + persisted local session); no JWT required (GAP-004 closed). |
| OQ-009 | Should this remain one combined change request or be split into (a) login UX change and (b) endpoint-auth removal? | **CLOSED 2026-09-23 — see §8.** Answer: **keep `BGUD-GOOGLE-SIGNIN-001` as one combined change request** — Google Sign-In login, Main Office-mapped accounts, Gudang selection, local session, JWT/bearer removal, explicit session context on Cloud endpoints, retained login-time sync, and email→BTR `UserId` attribution ship together (supersedes the §7 Option D split recommendation). |
| OQ-010 | The BGud-consumed routes (`barcodes/sync`, `barcode-registration`, `BarcodeRegistration/status`, `return-order`, `Driver/{serverId}`) live on controllers shared with BTrade3, `j07-btrade-sync`, and other API consumers (ASM-002). Which non-BGud consumers, if any, depend on the current `[Authorize]` behavior of those shared routes, and is removing `[Authorize]` from those shared controllers acceptable instance-wide? (Formerly framed as scoping a "BGud-only exception"; that framing is withdrawn — the posture is now policy, so only the shared-consumer impact remains.) | **CLOSED 2026-09-23 — see §8.** Answer: **instance-wide `[Authorize]` removal on the five BGud-consumed shared routes** — no documented non-BGud consumer dependency on the current `[Authorize]` behavior was identified in the current repository artifacts; not a BGud-only exception; BTrade3/`j07-btrade-sync` continue against the revised contract; verify before deployment and raise a new ISSUE if an actual dependency is found (GAP-002 scope decided). |
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
| RISK-002 | Anonymous, client-selectable tenant on write endpoints | Cross-tenant data pollution/spoofing; forged registrations and return orders | **Accepted for BGud as deliberate policy (OQ-001/OQ-002, §8)** — not a time-boxed exception. Compensating design: tenant contract decided (OQ-003 — session `locationId` carried, `ServerId` resolved server-side); actor contract decided (OQ-005 — payload Google email, resolved server-side to BTR `UserId`; client-supplied and not cryptographically verified — accepted under the anonymous posture); shared-instance consumer scope decided by OQ-010. No restore-authorization path applies (OQ-011 obsolete). |
| RISK-003 | Loss of per-operator attribution on registrations/return orders | Audit and Main-Office validation expectations (ADR-002 context) break | **Answered by OQ-005 (§8):** attribution is preserved — BGud sends the logged-in Google email in the payload and the Cloud resolves it via the OQ-004 mapping to the BTR `UserId` recorded as actor/`RegisteredBy`; no API authentication is reintroduced |
| RISK-004 | Shared cleartext HTTP with any credential (Google ID token or JWT) in transit | Credential exposure | Pre-existing platform risk. After OQ-001, BGud sends no credentials to the Cloud API (Google Sign-In is a local gate), so this does not gate BGud's anonymous API traffic; HTTPS remains a general platform concern (RD-004 mirror) |
| RISK-005 | Reuse of BTrade3's Google OAuth client (ASM/ OQ-007) | Coupled app inventories; BTrade3 project changes can break BGud sign-in | **Approved by OQ-007 (§8):** reuse of BTrade3's OAuth project/client is the decided direction — coupling is knowingly accepted for the MVP; if a BTrade3 project change later breaks BGud sign-in, revisit with a BGud-specific OAuth client |
| RISK-006 | Duplicated/conflicting identity stores over time (Google allow-list vs `BTR_User`) | Off-boarded BTR users retain app access | **Answered by OQ-004 (§8):** Main Office is the single authoritative source for the one-to-one Google-email↔BTR-user mapping (maintained in the **User** menu); no parallel BGud identity store exists; access follows the BTR user's active/valid state, so revocation is handled by existing Main Office user management (email change/removal or user deactivation) — consistent with ASM-003 |
| RISK-007 | Tenant vocabulary drift (`JOG` vs `JOGJA`, collapsed warehouse granularity) | Wrong/failed tenant resolution on anonymous routes; queue re-homing violations (IR-09) | **Answered by OQ-003/OQ-006 (§8):** BGud carries `locationId` (`GAMPING`/`CONCAT`/`MAGELANG`) as session context; `ServerId` is derived server-side via `BTR_WarehouseMapping`; warehouse granularity and offline queue binding are preserved (IR-09). Shared-route consumer scope is decided by OQ-010; verification remains implementation/testing work. |
| RISK-008 | Reversing direction while BTrade3 remediation (RD-001: BTrade3 adopts JWT) is still planned | The two work items cancel each other; wasted effort | **Scoped by OQ-002 (§8):** the override applies to BGud only and explicitly does not decide BTrade3's direction; RD-001 remains as recorded in the BTrade3 compatibility assessment. Do not generalize the BGud decision to other mobile applications |
| RISK-009 | The anonymous posture applies to Cloud controllers shared with BTrade3 and `j07-btrade-sync` (ASM-002); removing `[Authorize]` from shared routes changes behavior for every consumer of those controllers | Cross-tenant and unattributed writes against live shared data; non-BGud consumers of the same controllers may be affected unexpectedly | Shared-controller scope is decided by OQ-010: no documented non-BGud dependency was identified, and no BGud-only exception is introduced. The compensating contract is fully designed — tenant by OQ-003 and actor by OQ-005. Verify known non-BGud clients before deployment; handle any discovered dependency through a new ISSUE/change. |

---

# 7. Recommendations

Alternative solution directions only. **No final decision is recorded here** — decisions
belong in §8 Gap Closure after stakeholder/architecture input.

*Status note (v1.11):* the following decisions are recorded in §8:

- **Authentication posture:** OQ-001 and OQ-002 — BGud operates anonymously by deliberate
  policy; no `api/Auth/login`, JWT, bearer header, or `[Authorize]` is used on BGud-consumed
  endpoints; ADR-002/003/007 are no longer authoritative for BGud.
- **Tenant and actor context:** OQ-003/OQ-006 — the selected Gudang `locationId` is carried
  as session context and resolved server-side; OQ-005 — Google email is resolved to the
  BTR `UserId` for attribution.
- **Identity and session flow:** OQ-004/OQ-007 — only Main Office-mapped Google accounts
  may sign in and BGud reuses BTrade3's Google OAuth project; OQ-008 — login-time
  synchronization is retained and navigation uses the local session.
- **Change boundary and shared routes:** OQ-009 — one combined change request; OQ-010 —
  instance-wide `[Authorize]` removal on the five BGud-consumed shared routes, with no
  BGud-only exception.

**Option A is the selected direction.** The options below remain the pre-decision
analysis record. Options B and C are ruled out by OQ-001, and Option D is superseded by
OQ-009; no replacement authentication mechanism is introduced unless required by the
actual BGud business flow.

## Option A — Literal BTrade3 parity (full removal) — SELECTED DIRECTION (§8 OQ-001)

Google Sign-In as a local gate; BGud sends no credentials; `[Authorize]` removed (or routes
duplicated) on the Cloud; tenant supplied explicitly by the client like BTrade3's
`{serverId}` convention, using BGud's existing warehouse model with granularity preserved.

### Advantages

- Exactly matches the requested outcome and BTrade3's current behavior.
- Simplest client: no token storage, no 401 handling, no login call.

### Disadvantages

- Requires server-side contract changes (GAP-002) — tenant contract decided (OQ-003:
  session `locationId` carried, resolved server-side); actor contract decided
  (OQ-005: payload Google email, resolved server-side to BTR `UserId`).
- Accepts anonymous, client-selectable-tenant writes knowingly for BGud (GAP-006,
  RISK-002/003); the compensating actor contract is decided (OQ-005) but the actor
  value is client-supplied and not cryptographically verified (accepted under the
  anonymous posture).
- Warehouse granularity is carried via `locationId` per OQ-003 (GAP-005, OQ-006 closed).

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

## Option D — Issue sequencing (historical analysis; superseded by §8 OQ-009)

Option D proposed splitting the ISSUE into (D1) login method change (Google Sign-In) and
(D2) endpoint authentication removal if release coordination required it. After the
OQ-001/OQ-002 decisions (§8), no policy conflict remained between D1 and D2; the split
was therefore considered optional for release coordination (RISK-001). OQ-009 subsequently
closed this question and selected one combined change request. This option remains only
as historical analysis. The FEATURE artifact for the BGud operator-authentication
capability (GAP-007) was subsequently created at
`docs/features/bgud-operator-signin/feature.md`. Option D answered issue Note 8.

### Advantages

- Unblocks the achievable part; keeps scope explicit and auditable; answers issue Note 8.

### Disadvantages

- Two issues to coordinate for any shared release (RISK-001).

---

# 8. Gap Closure

Ledger: **GAP-001 — CLOSED (2026-09-23)**, **GAP-002 — CLOSED (2026-09-23)**, **GAP-003 — CLOSED (2026-09-23)**, **GAP-004 — CLOSED (2026-09-23)**, **GAP-005 — CLOSED (2026-09-23)**, **GAP-006 — CLOSED (2026-09-23)**, **GAP-007 — CLOSED (2026-09-23)**, **GAP-008 — CLOSED (2026-09-23)**, **OQ-001 —
CLOSED (2026-09-23, override)**, **OQ-002 — CLOSED (2026-09-23, override)**, **OQ-003 —
CLOSED (2026-09-23)**, **OQ-004 — CLOSED (2026-09-23)**, **OQ-005 — CLOSED (2026-09-23)**, **OQ-006 — CLOSED (2026-09-23)**, **OQ-007 — CLOSED (2026-09-23)**, **OQ-008 — CLOSED (2026-09-23)**, **OQ-009 — CLOSED (2026-09-23)**, **OQ-010 — CLOSED (2026-09-23)**, **OQ-011 — OBSOLETE
(2026-09-23)**. All GAP and open-question entries are now **CLOSED** or **OBSOLETE**. When a further
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
  remaining open question OQ-010) without ratifying anything against
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

## GAP-002

**Status: CLOSED**

### Decision

The feasibility question is fully resolved by OQ-001, OQ-003, OQ-005, and OQ-010.

The target contract is:

- BGud sends no JWT or bearer token.
- Tenant context is supplied using the session `locationId`.
- Cloud resolves `locationId` to `ServerId` through `BTR_WarehouseMapping`.
- Barcode registration and return order carry the logged-in Google email for actor
  attribution.
- Cloud resolves the email to the authoritative BTR `UserId`.
- The five BGud-consumed routes are anonymous instance-wide.

Exact DTOs, carriers, controller changes, and implementation mechanics are
architecture/implementation work and are not feasibility blockers.

### Rationale

The gap existed because four of the five BGud-consumed routes derived tenant context —
and, for barcode registration, actor identity — from the JWT itself and carried no
client-supplied tenant field (P-06), so removing authentication without a replacement
contract would have made those endpoints inoperable (GAP-002). Each element of that
replacement contract has since been decided: OQ-001 fixes the anonymous posture (no JWT,
no bearer header, no `[Authorize]`), OQ-003 fixes the tenant contract (session
`locationId` → `BTR_WarehouseMapping` → `ServerId`, warehouse granularity preserved),
OQ-005 fixes the actor contract (logged-in Google email in the payload → BTR `UserId`),
and OQ-010 fixes the shared-controller scope (instance-wide `[Authorize]` removal, no
BGud-only exception). With posture, tenant, actor, and scope all decided, no unanswered
feasibility question remains; only the contract-change realization is left, which
belongs to architecture/implementation rather than feasibility.

### Impact

GAP-002 no longer blocks architecture.

### Architecture Impact

- The Architect designs the anonymous Cloud contract with the explicit session
  `locationId` carried on the four JWT-derived routes (`GET api/barcodes/sync`,
  `POST api/barcode-registration`, `GET api/BarcodeRegistration/status`,
  `POST api/return-order`) and resolves it server-side via `BTR_WarehouseMapping`;
  `GET api/Driver/{serverId}` keeps its existing route-based contract.
- `POST api/barcode-registration` (for `RegisteredBy`) and `POST api/return-order`
  carry the logged-in Google email for actor attribution, resolved server-side to the
  BTR `UserId`.
- `[Authorize]` is removed instance-wide from the five BGud-consumed routes (OQ-010);
  no BGud-only exception, dual-mode, or separate compatibility mechanism is introduced.
- No JWT-claim reads (`User.GetServerId()`, `User.GetUserId()`) remain on BGud's request
  path.
- The exact carriers (header, query/route parameter, or payload field), Dto/envelope
  changes, and controller edits are target-architecture decisions for the Architect.

### Resolved By

Repository owner (issue reporter) via the OQ-001/OQ-003/OQ-005/OQ-010 decisions (§8);
recorded by the Analyst.

### Resolved Date

2026-09-23

---

## GAP-003

**Status: CLOSED**

### Decision

The business and authority rules for Google-account mapping are fully resolved by OQ-004.

- Only mapped Google accounts may access BGud.
- Each BTR user maps to at most one Google email.
- Main Office is the authoritative source.
- Mapping is maintained through the existing BTR User menu.
- Existing BTR `UserId` and `RoleId` remain authoritative.

The required database changes, synchronization/projection changes, and User-menu UI
changes are implementation work.

### Rationale

The gap existed because neither `BTR_User` nor its `BTRADE_User` projection has an
email/Google-identifier column and nothing in the repository maps a Google account to a
BTR `UserId`, `RoleId`, or operational location (verified schema), so "which Google
accounts may sign in" had no answer in the current data model. OQ-004 supplies that
answer at the business/authority level: sign-in is restricted to Google accounts mapped
one-to-one to existing BTR users, the mapping is created/modified/removed in the Main
Office **User** menu, Main Office stays the single authoritative source, the existing BTR
`UserId`/`RoleId` remain authoritative, and unmapped accounts are rejected. With the
policy and authority decided, the residual work is the data-model/projection/UI
realization, which belongs to architecture/implementation rather than feasibility. The
decision also keeps the existing single-source-of-truth model (ASM-003) and avoids a
parallel BGud identity store.

### Impact

GAP-003 no longer blocks architecture.

### Architecture Impact

- The Google email ↔ BTR user mapping is stored and maintained where the Main Office
  system manages users; no separate BGud account store and no Cloud-side independent
  credential authority (ASM-003).
- The mapping must become readable where identity resolution occurs — for the BGud local
  sign-in gate and for Cloud-side actor resolution (OQ-005) — so database and
  synchronization/projection changes (e.g., extending the existing `j07-btrade-sync`
  `BTRADE_User` projection) are architecture/implementation work.
- Resolution yields the authoritative BTR `UserId`; the user's existing BTR `RoleId`
  remains authoritative and no new BGud role model is designed.
- The Main Office Desktop change (Google email field plus create/modify/remove in the
  **User** menu) enters scope as a work item for the plan.
- Uniqueness enforcement (one Google email per BTR user, one BTR user per email) is a
  design point for the Architect.

### Resolved By

Repository owner (issue reporter) via the OQ-004 decision (§8); recorded by the Analyst.

### Resolved Date

2026-09-23

---

## GAP-004

**Status: CLOSED**

### Decision

The login/session behavior is fully resolved by OQ-008.

Google Sign-In → validate mapped account → select Gudang → create local session →
login-time sync → Home.

The JWT navigation gate is replaced by the local session gate.

Required client refactoring is implementation work.

### Rationale

The gap existed because the old login response (`serverId`, `userId`, `warehouseCode`)
drove navigation gating, login-time synchronization, session binding, and queued-record
warehouse binding, so replacing the login call with a local-only Google gate required
re-sourcing every one of those values. OQ-008 resolves how each is re-sourced: Google
Sign-In plus the validated mapped account establishes identity; Gudang selection
establishes the operational/location context; a persisted local session replaces the JWT
session; login-time synchronization (Barcode Registry and Return Order data) remains part
of login and runs under the selected location; navigation is gated on local session state;
and logout and warehouse change clear/rotate the session while queued offline records keep
their original warehouse binding. With the flow fixed, the residual work is client
refactoring (session storage, navigation gate, sync triggers), which belongs to
implementation rather than feasibility.

### Impact

GAP-004 no longer blocks architecture.

### Architecture Impact

- BGud navigation changes from a stored-token existence check to a local-session
  existence check (validated Google email + selected `locationId` + persisted local
  session).
- Session storage changes from JWT-based keys (`token`, `user_id`, `warehouse_code`,
  `office_code`) to local session keys (Google email, `locationId`, etc.); no JWT or
  Cloud token is stored or used.
- The Login ViewModel continues to trigger `BarcodeSyncRepository.sync` and
  `ReturnOrderSyncRepository.sync` after session creation; sync calls carry `locationId`
  (OQ-003) and actor email (OQ-005) with no Authorization header.
- Logout and warehouse change clear the session before a new selection; queued offline
  records retain their original warehouse/location binding and are never silently
  re-homed (IR-09, ASM-005).
- The exact session model, storage keys, and navigation/refactor mechanics are
  target-architecture decisions for the Architect.

### Resolved By

Repository owner (issue reporter) via the OQ-008 decision (§8); recorded by the Analyst.

### Resolved Date

2026-09-23

---

## GAP-005

**Status: CLOSED**

### Decision

The existing BGud warehouse/selector model is retained as the contract vocabulary:

- The login selector keeps the three Gudang values: `GAMPING`, `CONCAT`, `MAGELANG`.
- BGud carries the session `locationId` (`GAMPING`/`CONCAT`/`MAGELANG`) as its client-side
  context — **`locationId`, not `ServerId`**, so `GAMPING` and `CONCAT` remain distinct
  and are never collapsed to `JOGJA` on the client.
- `ServerId` (`JOGJA`/`MGL`) remains a Cloud-side tenant identifier, derived from the
  selected location via the existing `BTR_WarehouseMapping`.
- BTrade3's `JOG`/`MGL` selector vocabulary is a BTrade3 concern and is not on BGud's
  request path.

### Rationale

Fixed by the OQ-003 decision (§8): BGud users already understand and select Gudang
locations; `GAMPING` and `CONCAT` are distinct operational locations even though both
resolve to `JOGJA`; the existing BGud warehouse model is preserved; tenant implementation
details are not exposed as the primary login concept; offline/queued records retain their
warehouse binding and are never silently re-homed (IR-09, ASM-005).

### Impact

- GAP-005 is resolved; no selector-model or vocabulary reconciliation remains for BGud.
- RISK-007 (vocabulary drift / collapsed granularity) is mitigated by carrying
  `locationId` end-to-end.
- The `JOG` vs `JOGJA` divergence persists only as a cross-app observation (BTrade3's
  own path), not as a BGud contract question.

### Architecture Impact

- Contracts carry `locationId` (warehouse granularity preserved); the server resolves it
  through the existing mapping data; `GET api/Driver/{serverId}` continues to take the
  mapped `ServerId` in its route.
- Offline queue records store the location/`warehouseCode` binding per the OQ-003
  session rule.

### Resolved By

Repository owner (issue reporter) via the OQ-003 decision (§8); recorded by the Analyst.

### Resolved Date

2026-09-23

---

## GAP-006

**Status: CLOSED**

### Decision

The loss of API authentication is an accepted architectural policy for BGud, established by OQ-001/OQ-002.

The resulting risks are knowingly accepted.

Compensating business contracts are already decided:

- tenant context → OQ-003
- actor attribution → OQ-005
- shared-controller scope → OQ-010

Security hardening beyond the approved BGud posture is not a feasibility prerequisite.

### Rationale

The gap described the security properties that ADR-002/ADR-007 had established and that
BGud's deliberate anonymous posture reopens — unattributed writes, client-selectable
tenant (cross-tenant read/write spoofing), and anonymous-writable Cloud state over
cleartext HTTP. This is not an unresolved feasibility question: OQ-001/OQ-002 establish
anonymous operation as deliberate BGud policy (not a temporary exception) with the named
risks knowingly accepted, and each compensating business contract is already decided —
tenant by OQ-003, actor by OQ-005, and shared-controller scope by OQ-010. What remains is
realization of those contracts and the documented pre-deployment verification, which are
architecture/implementation/testing activities rather than feasibility blockers. Security
hardening beyond the approved posture is explicitly not a prerequisite.

### Impact

GAP-006 no longer blocks architecture.

### Architecture Impact

- The Architect designs the anonymous Cloud contract with the OQ-003 tenant contract
  (session `locationId` → `BTR_WarehouseMapping` → `ServerId`) and the OQ-005 actor
  contract (logged-in Google email → BTR `UserId`) as the compensating controls;
  `[Authorize]` is removed instance-wide from the five BGud-consumed routes (OQ-010).
- The client-supplied tenant and actor values are not cryptographically verified —
  knowingly accepted under the approved anonymous posture (OQ-001/OQ-002); no
  restoration/authentication path is designed and OQ-011 remains obsolete.
- Pre-deployment verification of the shared routes against known non-BGud clients and
  integration tests is implementation/testing work; any discovered dependency is handled
  as a new ISSUE/change, not as a BGud-specific authorization exception.
- Cleartext HTTP remains a general platform concern; BGud sends no credentials to the
  Cloud under the approved posture, so it does not gate this feature.
- Security hardening beyond the approved BGud posture is out of scope for this feature
  and does not block architecture.

### Resolved By

Repository owner (issue reporter) via the OQ-001/OQ-002 policy decisions and the
OQ-003/OQ-005/OQ-010 compensating contracts (§8); recorded by the Analyst.

### Resolved Date

2026-09-23

---

## GAP-007

**Status: CLOSED**

### Decision

The missing workflow-stage-1 knowledge is resolved: the BGud sign-in capability is now
defined as a FEATURE artifact, `docs/features/bgud-operator-signin/feature.md`
(`BGUD-OPERATOR-SIGNIN-001`, v1.0, 2026-09-23).

The FEATURE covers purpose, business outcome, participating domains, trigger,
preconditions, operational flow, domain orchestration, constraints, exceptions, and
acceptance criteria for operator sign-in and warehouse session establishment, written
against the approved §8 decisions (OQ-001…OQ-010).

### Rationale

GAP-007 existed because the assessment had been performed from the ISSUE, ADRs, and code,
with no FEATURE artifact defining the requested business change. The new FEATURE artifact
supplies that definition and is the single authoritative owner of the feature's business
outcome, operational flow, and acceptance criteria; barcode and Retur business rules remain
owned by their domains and are referenced rather than restated. Work-stage knowledge (the
login UX blueprint, the login/authentication architecture, and the implementation plan) was
used only as source material and remains owned by its own artifact types.

### Impact

GAP-007 no longer blocks architecture. All GAP and open-question entries in this
assessment are now closed or obsolete.

### Architecture Impact

- The Architect can design the target architecture against the FEATURE artifact
  `docs/features/bgud-operator-signin/feature.md` together with the §8 decisions, rather
  than reconstructing business intent from the ISSUE and code.
- The barcode-registry login/authentication artifacts remain historical sources; their
  pre-Google login design is not authoritative for BGud.
- Operator-account/registration business knowledge remains with Main Office (OQ-004 and the
  FEATURE's participating-domain and constraint statements); if the domain process later
  chooses a dedicated operator-identity DOMAIN artifact, that is a knowledge-organization
  follow-up, not a feasibility blocker.

### Resolved By

Analyst (via the `feature-creation` activity) on the repository owner's instruction;
recorded by the Analyst.

### Resolved Date

2026-09-23

---

## GAP-008

**Status: CLOSED**

### Decision

OQ-007 resolves the OAuth configuration decision: BGud reuses the existing BTrade3 Google OAuth project and web client configuration.

Adding the required configuration to BGud and reconciling Android package/fingerprint requirements are implementation tasks.

### Rationale

The gap existed because BGud has no Google OAuth client configuration in the repository
(no `google-services.json`, no client id), so Google Sign-In depended on an external
configuration that had not been selected or committed. OQ-007 removes that uncertainty by
deciding that BGud reuses BTrade3's existing Google OAuth project and web client
configuration, following BTrade3's already-implemented Sign-In pattern, with no separate
BGud OAuth registration. The residual work — bringing the configuration into the BGud app
and reconciling Android package/fingerprint requirements with the shared client — is
configuration/implementation work rather than an open feasibility question. The coupling
with the BTrade3 project is knowingly accepted for the MVP (RISK-005).

### Impact

GAP-008 no longer blocks architecture.

### Architecture Impact

- BGud carries the shared OAuth configuration (web client id / `google-services.json`
  pattern as in BTrade3); the delivery mechanism into the BGud build is an
  implementation detail under the plan.
- The BGud Google Sign-In setup targets the same OAuth project/client as BTrade3; Android
  application identity constraints imposed by the shared client (package name, SHA
  certificate/fingerprint requirements) must be reconciled by the Architect/implementer —
  adding BGud as an allowed client within the existing project is realization, not a new
  registration decision.
- No separate BGud OAuth project/client is registered; the coupling to BTrade3's project
  is knowingly accepted (RISK-005), with a BGud-specific client as a future fallback only.
- Google Sign-In establishes only the Google account; BTR identity resolution remains per
  OQ-004/OQ-005 (email → BTR user → `UserId`).

### Resolved By

Repository owner (issue reporter) via the OQ-007 decision (§8); recorded by the Analyst.

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
`User.GetServerId()`, `User.GetUserId()`, or JWT claims for BGud operations. The
request-contract shape is decided in OQ-003 (§8): BGud carries the session `locationId`,
and `ServerId` is derived server-side.

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
- Did not close GAP-002 at the time of this decision (posture, tenant — OQ-003, and
  actor — OQ-005 now all decided; contract implementation remains); **GAP-002 was
  subsequently CLOSED once OQ-010 fixed shared-controller scope (§8)**. GAP-003 was
  subsequently **CLOSED after OQ-004 decided the account-authority rules (§8)**; its
  data-model/UI realization remains. GAP-004 was subsequently **CLOSED after OQ-008
  fixed the login/session flow (§8)**; its client refactoring remains. GAP-006 was
  subsequently **CLOSED as knowingly accepted policy once OQ-010 fixed shared-controller
  scope (§8)**; its contract implementation remains. GAP-008 was subsequently
  **CLOSED after OQ-007 decided the OAuth-client reuse (§8)**; its configuration
  delivery remains. GAP-007 was subsequently **CLOSED by the new FEATURE artifact (§8)**.
  (GAP-005 is closed by OQ-003/OQ-006.)
- GAP-001 is closed via OQ-002 (§8).
- OQ-010 is closed as an instance-wide shared-controller decision, not a BGud-only
  exception; OQ-011 is retired as obsolete because no exception exit/restoration condition
  exists.
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
  explicit tenant contract is decided in OQ-003 (§8) — session `locationId` carried,
  resolved server-side via the map above — and the submitting-actor input is decided in
  OQ-005 (§8) — logged-in Google email carried, resolved server-side to the BTR
  `UserId`. Only `api/Driver/{serverId}` is already
  tenant-explicit in the route.
- Google Sign-In remains part of the request (ISSUE part 1) and is **not** resolved by
  this decision: OQ-008 (login-time
  sync and navigation gate) still stands — account authority was subsequently decided
  under OQ-004 (§8), the actor contract under OQ-005 (§8), and the OAuth client under
  OQ-007 (§8); the local gate replaces the token-based
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
  question (OQ-010) is resolved — without construing
  ADR-002/003/007 as binding.
- The BGud warehouse model table (Gamping/Concat → `JOGJA`, Magelang → `MGL`) is the
  server-side resolution basis realized by the OQ-003 decision (§8): session `locationId`
  → `BTR_WarehouseMapping` → `ServerId`, regardless of ADR-007's file status; the
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

## OQ-003

**Status: CLOSED** — Session-Scoped Gudang Location

### Decision

BGud uses the **selected Gudang location as the session context**.

The user selects a Gudang on the BGud login screen. The selected `locationId` becomes
part of the active BGud session and is carried with every Cloud API request made during
that session.

#### Login flow

```text
BGud Login
   ↓
Select Gudang
   ├── GAMPING
   ├── CONCAT
   └── MAGELANG
   ↓
Create local session
   ↓
Every Cloud request carries the selected Location
```

#### Location values

| Gudang | locationId | ServerId |
| --- | --- | --- |
| Gudang Gamping | `GAMPING` | `JOGJA` |
| Gudang Concat | `CONCAT` | `JOGJA` |
| Gudang Magelang | `MAGELANG` | `MGL` |

`locationId` is the session context used by BGud. `ServerId` remains a Cloud-side tenant
identifier and is derived from the selected location according to the existing warehouse
mapping.

#### Request contract

For endpoints that previously obtained tenant context from JWT claims, BGud provides the
session's `locationId` explicitly. The Cloud resolves:

```text
locationId → warehouse mapping → ServerId
```

The affected endpoints are:

- `GET api/barcodes/sync`
- `POST api/barcode-registration`
- `GET api/BarcodeRegistration/status`
- `POST api/return-order`

`GET api/Driver/{serverId}` remains compatible with its existing route-based contract.

**Implementation insistence:** carry **`locationId`, not just `ServerId`**, in the BGud
session. The repository already treats `GAMPING` and `CONCAT` as distinct locations, and
collapsing them to `JOGJA` too early would lose that distinction.

#### Session behavior

- The selected Gudang is fixed for the active session. To operate from another Gudang,
  the user ends the current session and selects another Gudang when starting the next
  session.
- The selected location is retained for offline data and queued operations, so records
  created under one warehouse are not silently re-homed to another warehouse (IR-09,
  ASM-005).

#### Authentication relationship

The location/session context is **not an authentication mechanism**. BGud remains
unauthenticated against the Cloud API as decided by OQ-001/OQ-002 (§8). The selected
Gudang only identifies the operational context in which the application is working.

### Rationale

This approach is preferred over making `ServerId` the primary client-side concept
because:

- BGud users already understand and select Gudang locations.
- `GAMPING` and `CONCAT` are distinct operational locations even though both resolve to
  `JOGJA`.
- It preserves the existing BGud warehouse model.
- It avoids exposing tenant implementation details as the primary login concept.
- It provides one consistent session context that can be reused by every endpoint.
- It is simpler than recreating JWT-based authentication solely to carry location
  context.

### Impact

- OQ-003 is closed: the per-route tenant contract for the four JWT-derived BGud routes is
  decided — explicit `locationId`, server-side resolution (GAP-002 tenant half
  resolved).
- Consequential closures recorded with this decision: **OQ-006** (selector retained,
  `locationId` vocabulary, offline binding fixed) and **GAP-005** (selector/vocabulary
  reconciliation) — both fully answered by this decision's content.
- GAP-002's direction is now fully decided — tenant (this decision), actor (OQ-005), and
  shared-controller scope (OQ-010); GAP-006's compensating tenant and actor design are
  both in place.
- RISK-007 is mitigated (`locationId` end-to-end, mapping server-side); RISK-002's
  tenant-compensating control is decided.
- Does not address Google Sign-In details; account authority, actor contract, OAuth client,
  login-time synchronization, and shared-controller scope were subsequently closed under
  OQ-004, OQ-005, OQ-007, OQ-008, and OQ-010 respectively.

### Architecture Impact

- The Cloud contract change is now specified at the WHAT level: add explicit `locationId`
  to the four affected routes and resolve it through the existing, already-seeded
  `BTR_WarehouseMapping`; no new tenant table or vocabulary is needed. The exact carrier
  (header, query/route parameter, or payload field) and any Dto/envelope changes are
  target-architecture decisions for the Architect.
- The server must reuse the existing mapping resolution (the login path already resolves
  `LocationId` → `ServerId` today) rather than reintroduce JWT-claim reads.
- The actor input for barcode registrations and return orders is **not** covered by this
  decision; it was subsequently decided by OQ-005 (§8) — logged-in Google email in the
  payload, resolved server-side to the BTR `UserId`.
- `GET api/Driver/{serverId}` needs no contract change; the client supplies the mapped
  `ServerId` (`JOGJA`/`MGL`) as it effectively does today via `officeCode`.

### Resolved By

Repository owner (issue reporter); recorded by the Analyst.

### Resolved Date

2026-09-23

---

## OQ-006

**Status: CLOSED** — Resolved by the OQ-003 decision (§8)

### Decision

Fixed by the OQ-003 decision: the existing three-Gudang selector (Gamping / Concat /
Magelang) is retained; the carried vocabulary is `locationId` (`GAMPING`/`CONCAT`/
`MAGELANG`); `ServerId` (`JOGJA`/`MGL`) is derived server-side via the existing
warehouse mapping and BTrade3's `JOG` value is not used by BGud; the Gudang is fixed for
the session and re-selected when a new session starts; offline/queued records retain
their location binding and are never re-homed (IR-09).

### Rationale

The OQ-003 decision (§8) explicitly fixes all three elements this question covered —
selector model, authoritative vocabulary, and offline binding — on the grounds that BGud
users already understand Gudang locations, `GAMPING`/`CONCAT` stay distinct, the existing
warehouse model is preserved, and one consistent session context serves every endpoint.

### Impact

- OQ-006 is closed; GAP-005 is closed with it (§8).
- RISK-007 is mitigated; no `JOG`-vs-`JOGJA` reconciliation remains on BGud's request
  path.
- The BGud flow definition (GAP-004 / OQ-008) can now be specified against the retained
  selector.

### Architecture Impact

- BGud contracts carry `locationId`; the Driver legacy route carries the mapped
  `ServerId`; no client-side collapse of warehouse granularity occurs.

### Resolved By

Repository owner (issue reporter) via the OQ-003 decision (§8); recorded by the Analyst.

### Resolved Date

2026-09-23

---

## OQ-004

**Status: CLOSED** — Google Account Must Be Pre-Registered in Main Office

### Decision

Only Google accounts that are **registered in the Main Office BTR system** are allowed to
sign in to BGud.

A BGud user must therefore already exist as a BTR user before the user can access BGud.

Each BTR user may be mapped to **one Google email account**.

```text
BTR User
   ↓
Google Email Mapping
   ↓
Google Sign-In
   ↓
BGud access
```

A Google account that has no mapping to a BTR user is rejected.

#### Account Registration and Maintenance

The Google email mapping is maintained in the **Main Office BTR Desktop application**.

Any BTR user who has permission to access the **User** menu may create, modify, or remove
the Google email mapping.

No separate BGud user-management mechanism is required.

The Main Office system remains the authoritative source for the mapping.

#### Role

BGud does not define a separate role system.

The user's existing BTR `RoleId` remains the authoritative role associated with the BTR
user.

The Google account only identifies the user during sign-in; it does not create or assign
a new BTR role.

Whether a user may use BGud is therefore determined by:

1. The Google account matches the email registered for a BTR user.
2. The corresponding BTR user is active/valid according to the existing Main Office
   user management rules.
3. The user's existing BTR `RoleId` and permissions apply.

#### Sign-In Rule

```text
Google account
      ↓
Find matching BTR User by registered email
      ↓
Match found?
   ├── No  → Reject login
   └── Yes → Continue with that BTR user
```

Google email is an **identity mapping**, not an independent user account.

### Rationale

This keeps BGud aligned with the existing Main Office user-management model:

- No new user database is introduced.
- No separate BGud account provisioning is required.
- User access is controlled from the existing BTR Desktop application.
- The company can revoke or change access by changing the user's registered email in
  the Main Office system.
- Existing BTR `RoleId` remains the source of the user's role.

### Impact

- OQ-004 is closed: which Google accounts may sign in is decided — only Google accounts
  mapped one-to-one to existing BTR users; unmapped Google accounts are rejected.
- GAP-003's policy question ("which Google accounts may sign in", "who creates and
  maintains the mapping", "which `RoleId` applies") is answered — direction
  **decided by this decision**, and GAP-003 is **CLOSED (§8)**. The data-model/UI
  realization of the mapping (mapping storage + maintenance in the Main Office **User**
  menu) remains as architecture/implementation work rather than an unanswered
  feasibility question.
- RISK-006's mitigation is decided: Main Office is the single authoritative source for
  the mapping — no parallel BGud identity store; access follows the BTR user's
  active/valid state, so revocation flows through existing Main Office user management
  (consistent with ASM-003, PRODUCT "Single Source of Truth").
- Consistent with OQ-001 (§8): Google Sign-In remains a **local UI gate** — this
  decision governs which accounts that gate admits, not API authentication; BGud still
  sends no credentials to the Cloud (no JWT, no bearer header, no `[Authorize]`).
- Does not decide the login-time sync/navigation gate or shared-consumer scope; those
  questions were subsequently closed under OQ-008 and OQ-010. The actor/attribution model
  was decided under OQ-005 (§8), and the OAuth client under OQ-007 (§8).

### Architecture Impact

- The Google email ↔ BTR user mapping must be stored and maintained where the Main
  Office system manages users (Main Office `BTR_User`-side data + Desktop **User** menu
  UI); no separate BGud account store and no Cloud-side independent credential authority
  (ASM-003).
- Cardinality: each BTR user maps to at most one Google email (stated), and the sign-in
  rule implies a registered email resolves to a single BTR user — uniqueness enforcement
  on both sides is a design point for the Architect.
- BGud's local gate must resolve the signed-in Google email to a BTR user and reject
  when no mapping exists. How that lookup reaches the device (and who serves it) under
  the anonymous Cloud contract (OQ-001/OQ-003) is a target-architecture decision — this
  decision fixes the rule, not the mechanism.
- The user's existing BTR `RoleId` remains the authoritative role; no new BGud role
  model is designed, and the Google account never assigns a role.
- The Main Office Desktop change (Google email field + create/modify/remove in the
  **User** menu) enters scope as a work item for the plan.

### Resolved By

Repository owner (issue reporter); recorded by the Analyst.

### Resolved Date

2026-09-23

---

## OQ-005

**Status: CLOSED** — Actor Is the Logged-In Google Email, Resolved to BTR `UserId`

### Decision

BGud will send the **logged-in Google email** as the actor identity in the endpoint
payload.

The Cloud uses the email to find the corresponding BTR user from the existing
Main Office user mapping and obtains the authoritative `UserId`.

```text
BGud Google Sign-In
      ↓
Logged-in Google Email
      ↓
Endpoint payload
      ↓
Cloud resolves BTR User by email
      ↓
BTR UserId
      ↓
Record actor / RegisteredBy
```

This applies to the actor-bearing write endpoints BGud consumes — barcode registration
(`RegisteredBy`) and return order — after the JWT-derived `User.GetUserId()` is removed
by the OQ-001 decision (§8).

### Rationale

- Restores per-operator attribution without reintroducing API authentication: the
  Google email is identity attribution in the payload, not a credential, so the OQ-001
  anonymous posture (no JWT, no bearer header) is preserved — this is exactly the
  "Google-email-based attribution as a business choice without reintroducing API
  authentication" path anticipated by RISK-003.
- Reuses the OQ-004 mapping (§8): Main Office remains the single authoritative source
  for Google-email↔BTR-user (ASM-003); no new identity store and no Cloud-side
  independent credential authority.
- The Google email is already the local sign-in state on the device after the OQ-004
  gate, so BGud can supply it without any login call to the Cloud.
- Recording the resolved BTR `UserId` keeps `RegisteredBy`/actor values in the same
  authoritative shape as today's JWT-claim-based values, so downstream Main-Office
  validation expectations are not changed by a new identifier vocabulary.

### Impact

- OQ-005 is closed: the actor for barcode registrations and return orders is decided —
  logged-in Google email in the payload, resolved by the Cloud to the BTR `UserId`.
  Per-operator attribution is preserved; loss of attribution is **not** accepted as
  unavoidable — it is compensated by this contract.
- GAP-002's direction is now fully decided: posture (OQ-001), tenant (OQ-003), and
  actor (OQ-005); shared-controller scope was subsequently fixed by OQ-010, after which
  GAP-002 was **CLOSED (§8)** — only the contract-change implementation (architecture
  work) remains.
- GAP-006's compensating actor contract is decided (tenant + actor both in place); shared-
  consumer scope was subsequently closed by OQ-010, after which GAP-006 is **CLOSED
  (§8)**.
- RISK-003 is answered (attribution preserved via email → `UserId` resolution);
  RISK-002's and RISK-009's actor-compensating controls are decided.
- Consistent with OQ-001: no API authentication mechanism is reintroduced — the actor
  field is plain identity data on an anonymous endpoint.
- Does not decide the login-time sync/navigation gate or shared-consumer scope; those
  questions were subsequently closed under OQ-008 and OQ-010. The OAuth client was
  decided under OQ-007 (§8).

### Architecture Impact

- `POST api/barcode-registration` (for `RegisteredBy`) and `POST api/return-order`
  gain an explicit actor-email input in the request contract; the exact carrier (payload
  field, header, or envelope — alongside the OQ-003 `locationId` carrier) and any
  Dto changes are target-architecture decisions for the Architect.
- The Cloud must be able to resolve the email to a BTR user using the OQ-004 mapping.
  Today neither `BTR_User` nor its `BTRADE_User` projection has an email column, so the
  mapping must become readable Cloud-side (e.g., extending the existing
  `j07-btrade-sync` `BTRADE_User` projection) while Main Office stays the authoritative
  source (ASM-003, OQ-004).
- Write-time behavior for an email with no mapping (reject vs. fallback) is a design
  point for the Architect; under the OQ-004 sign-in gate a mapped email is guaranteed at
  session start, so this is an edge case (mapping removed mid-session), not the normal
  path.
- The actor value is client-supplied and not cryptographically verified — accepted
  knowingly under the anonymous posture (OQ-001, GAP-006, RISK-002); it is not a
  restoration of authentication and confers no tenant authority (tenant remains the
  OQ-003 session `locationId`).
- Stored actor values remain BTR `UserId`s (resolved server-side), so existing audit
  consumers keep the current identifier shape.

### Resolved By

Repository owner (issue reporter); recorded by the Analyst.

### Resolved Date

2026-09-23

---

## OQ-007

**Status: CLOSED** — Reuse BTrade3's Google OAuth Project and Web Client

### Decision

BGud will **reuse BTrade3's existing Google OAuth project and web client configuration**.

BGud will follow the same Google Sign-In configuration pattern already implemented in
BTrade3.

### Rationale

A separate Google OAuth project/client would add configuration and maintenance overhead
without providing a current business benefit.

Reusing the existing configuration:

- avoids creating and maintaining another Google OAuth project;
- allows BGud to follow the already-working BTrade3 Google Sign-In implementation;
- reduces implementation effort for the BGud MVP;
- keeps Google account identity compatible with the existing BTR email mapping defined
  in OQ-004.

### Identity Mapping

Google Sign-In only establishes the user's Google account.

After successful sign-in, BGud uses the signed-in Google email to resolve the corresponding
BTR user according to OQ-004.

```text
Google Sign-In
      ↓
Google Email
      ↓
BTR User email mapping
      ↓
BTR UserId
```

### Impact

- OQ-007 is closed: BGud reuses BTrade3's Google OAuth project/web client — no
  separate BGud OAuth registration is required.
- GAP-008 is **CLOSED (§8)**: BGud reuses BTrade3's configuration; delivering that
  configuration into the BGud app (config file / client id) and reconciling
  package/fingerprint requirements are implementation work.
- RISK-005's mitigation is decided: the reuse that RISK-005 flagged is now the
  explicitly approved direction — the coupling (BTrade3 project changes can affect BGud
  sign-in) is knowingly accepted for the MVP rather than avoided.
- The identity flow is fixed as two independent steps: Google Sign-In establishes only
  the Google account; the BTR user comes from the OQ-004 email mapping (this decision
  does not change account authority, the actor contract, or the anonymous Cloud
  posture).
- Does not decide login-time sync/navigation gate (OQ-008) or shared-consumer scope
  (OQ-010).

### Architecture Impact

- BGud must carry the shared OAuth configuration (web client id / `google-services.json`
  pattern as in BTrade3) — the delivery mechanism into the BGud app build is an
  implementation detail under the plan (GAP-008 closed; implementation residual).
- The BGud Google Sign-In setup must target the same OAuth project/client as BTrade3:
  package/fingerprint requirements imposed by that shared client (e.g., package name,
  SHA certificate requirements) are constraints the Architect must reconcile with BGud's
  application identity — if the shared client restricts callers, adding BGud as an
  allowed client within the existing project is part of realization, not a new
  registration decision.
- Known coupling (RISK-005) is accepted: configuration or project-level changes made
  for BTrade3 may affect BGud sign-in; a future BGud-specific client is a fallback, not
  part of this decision.
- Identity resolution remains per OQ-004 (email → BTR user → `UserId`); this decision
  supplies only how the Google account is authenticated client-side.

### Resolved By

Repository owner (issue reporter); recorded by the Analyst.

### Resolved Date

2026-09-23

---

## OQ-008

**Status: CLOSED** — Keep Login-Time Synchronization; Replace JWT Gate with Local Session Gate

### Decision

**Yes. Login-time synchronization remains part of the BGud login flow.**

After the user successfully signs in with Google and selects a Gudang, BGud creates its local session and performs the required synchronization before entering the Home screen.

**Login Flow**

Google Sign-In → Validate Google email against BTR user mapping → Select Gudang → Create local BGud session → Login-time synchronization (Barcode Registry master data, Return Order reference/master data) → Home

**Login-Time Synchronization**

The existing login-time synchronization behavior is retained. BGud is designed as an offline/cache-first application. The Barcode Registry architecture explicitly defines synchronization at login and manual request, while Return Order uses locally cached Customer, Salesman, Driver, Item, and Barcode data for offline operation. Therefore login-time synchronization remains the primary mechanism to refresh the device's operational data before the user starts working.

**Synchronization Context**

Synchronization uses the selected Gudang Location from the BGud session. The selected location determines which Office/Tenant data is synchronized. The location must remain consistent with the session and must not be silently changed while offline data is pending.

**Navigation Gate**

The existing JWT-based navigation gate is removed. The new navigation gate is based entirely on local BGud session state. A session is considered valid when BGud has: 1) A successfully validated Google email. 2) A selected Gudang Location. 3) The corresponding local session data persisted on the device. Conceptually: Has valid local session? No → Login, Yes → Home. No JWT or Cloud authentication token is required to enter the Home screen.

**Logout**

Logout clears the local BGud session: Google email, selected Gudang Location, other session-bound values. After logout, BGud returns to the Login screen. The user must sign in again and select a Gudang to establish a new session.

**Warehouse Change**

Changing Gudang is treated as a new session. The user must leave the current session and select another Gudang. Queued offline records must retain the original warehouse/location context and must never be silently re-homed to the newly selected Gudang.

The key distinction is that Google Sign-In establishes the local user identity, while the Gudang selection establishes the operational context. Together they replace the old JWT session for BGud.

### Rationale

Keeping login-time synchronization is simpler and more consistent with the existing BGud design than introducing a new synchronization trigger just because JWT authentication has been removed. The JWT was serving both authentication and session gating in the previous design. Authentication is now removed, so only the local session concept is retained. The resulting design is: Google Identity + Gudang Selection → Local BGud Session → Login-Time Sync → Home.

### Impact

- OQ-008 is closed: login-time master-data synchronization continues at login time, and the token-based navigation gate is replaced by a local session gate.
- GAP-004 is **CLOSED (§8)** — the login UX / navigation gating and login-time sync aspects are resolved and the flow re-sources the session values required by login-time sync and navigation.
- No JWT or Cloud authentication token is required to enter the Home screen.
- Session state drives navigation; offline behavior and warehouse binding are preserved.

### Architecture Impact

- BGud client navigation logic changes from token existence check to local session existence check (validated Google email + selected location persisted).
- Login ViewModel remains responsible for triggering BarcodeSyncRepository.sync and ReturnOrderSyncRepository.sync after session creation.
- Session storage changes from JWT-based keys to local session keys (Google email, locationId, etc.).
- Sync calls carry locationId and actor email per OQ-003/OQ-005; no Authorization header is required.
- Warehouse change must clear existing session before allowing new selection to prevent silent re-homing of queued offline records.

### Resolved By

Repository owner (issue reporter); recorded by the Analyst.

### Resolved Date

2026-09-23

---

## OQ-009

**Status: CLOSED** — Keep `BGUD-GOOGLE-SIGNIN-001` as One Combined Change Request

### Decision

Keep `BGUD-GOOGLE-SIGNIN-001` as **one combined change request**.

The change includes:

1. Replace the current username/password login with Google Sign-In.
2. Allow only Google accounts registered in the Main Office BTR system.
3. Let the user select a Gudang during login.
4. Create a local BGud session containing the logged-in Google email and selected Gudang.
5. Remove BGud's JWT authentication and bearer-token usage.
6. Update the Cloud endpoints consumed by BGud to accept the session context explicitly.
7. Continue login-time master-data synchronization using the selected Gudang context.
8. Resolve the Google email to the existing BTR `UserId` for operator attribution.

### Rationale

These changes are part of one coherent login/session redesign.

The existing login currently provides several values that are subsequently required by BGud:

- User identity
- Warehouse/location
- Server/tenant context
- JWT session state
- Login-time synchronization context

Removing the old login mechanism therefore requires changing the way those values are established and carried through the application.

Splitting the change would create an unnecessary intermediate state such as:

```text
New Google Login
      ↓
Old JWT API Authentication
```

### Impact

- OQ-009 is closed: no split into (a) login UX change and (b) endpoint-auth removal; `BGUD-GOOGLE-SIGNIN-001` ships as a single coordinated change.
- Supersedes the §7 Option D split recommendation: Option D remains as pre-decision analysis record only (recommended split for release coordination) and does not create a second ISSUE track.
- The eight scope items above consolidate the already-closed §8 decisions (OQ-001/OQ-002 anonymous posture and ADR treatment, OQ-003/OQ-006 session `locationId` and selector, OQ-004 account authority, OQ-005 actor contract, OQ-007 OAuth reuse, OQ-008 login-time sync and local-session gate) into one delivery boundary.
- ISSUE tracking impact only (OQ-009 was MINOR, not a feasibility gate): no separate D1/D2 release coordination is required; client and Cloud still ship together per RISK-001 as decided under OQ-001.
- OQ-010 shared-controller scope is closed; its instance-wide contract decision is
  included in the combined change boundary.

### Architecture Impact

- The Architect designs one target architecture covering the BGud login/session redesign and the anonymous Cloud contract together, rather than two sequenced architectures.
- No intermediate architecture (new Google login over old JWT authentication, or vice versa) needs to be designed, migrated through, or rolled back.
- §7 Option D is retained unchanged as historical analysis; the §8 OQ-009 decision governs and the Architect works against the combined scope above.

### Resolved By

Repository owner (issue reporter); recorded by the Analyst.

### Resolved Date

2026-09-23

---

## OQ-010

**Status: CLOSED** — Instance-Wide `[Authorize]` Removal on BGud-Consumed Shared Routes; No BGud-Only Exception

### Decision

No documented non-BGud consumer dependency on the current `[Authorize]` behavior of the affected shared routes was identified in the current repository artifacts.

The BGud change therefore applies instance-wide to the affected shared routes:

- `barcodes/sync`
- `barcode-registration`
- `BarcodeRegistration/status`
- `return-order`
- `Driver/{serverId}`

Their `[Authorize]` requirement may be removed as part of the BGud authentication change.

This is **not** a BGud-only exception. The new policy is that these routes do not require JWT authentication.

Existing consumers such as BTrade3 or `j07-btrade-sync` must continue to function with the revised contract. No separate compatibility mechanism is introduced unless an actual non-BGud consumer dependency is discovered during implementation/testing.

### Rationale

The repository currently provides no evidence that another consumer requires the existing authorization behavior. Maintaining `[Authorize]` solely because the controllers are shared would preserve the old authentication architecture and conflict with the accepted BGud authentication direction (OQ-001/OQ-002).

### Impact

- OQ-010 is closed: the shared-controller scope gate for the Cloud change is decided — instance-wide `[Authorize]` removal on the five listed routes, not a BGud-only exception.
- GAP-002's scope question is answered (posture OQ-001, tenant OQ-003, actor OQ-005, shared-controller scope OQ-010) and GAP-002 is **CLOSED (§8)**; only the contract-change implementation, which is architecture work, remains.
- GAP-006's remaining shared-consumer work is answered at policy level and GAP-006 is **CLOSED (§8)**; the compensating tenant (OQ-003) and actor (OQ-005) contracts stand unchanged.
- RISK-002's shared-instance residual and RISK-009's consumer-impact scope are decided at policy level; pre-deployment verification against known non-BGud clients and integration tests becomes implementation/testing work, not a feasibility question.
- If an actual non-BGud consumer dependency is discovered, a new ISSUE/change is raised by its owning process rather than reintroducing a BGud-specific authorization exception (OQ-011 stays obsolete; no restoration framing).

### Architecture Impact

- The Architect designs the Cloud contract without `[Authorize]` on the five listed routes, instance-wide; no BGud-only exception, dual-mode, or separate compatibility mechanism is introduced.
- The explicit session-context carriers decided under OQ-003 (`locationId`, resolved server-side via `BTR_WarehouseMapping`) and OQ-005 (payload Google email, resolved server-side to the BTR `UserId`) apply under the revised contract.
- Verification of the affected shared routes against known non-BGud clients and integration tests is implementation/testing work under the plan; any newly discovered dependency is handled as a new ISSUE/change, not as a BGud-specific exception.

### Implementation Note

Before deployment, verify the affected shared routes against known non-BGud clients and integration tests. If an actual dependency is discovered, raise a new ISSUE/change rather than reintroducing a BGud-specific authorization exception.

### Resolved By

Repository owner (issue reporter); recorded by the Analyst.

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

- RISK-009's mitigation no longer depends on OQ-011; shared-consumer scope is decided by
  OQ-010 and verification remains implementation/testing work.
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

- [ ] All critical gaps resolved (GAP-001 CLOSED by OQ-002, GAP-002 CLOSED by
      OQ-001/OQ-003/OQ-005/OQ-010, GAP-003 CLOSED by OQ-004, GAP-004 CLOSED by OQ-008,
      GAP-005 CLOSED by OQ-003, GAP-006 CLOSED by OQ-001/OQ-002/OQ-003/OQ-005/OQ-010,
      GAP-007 CLOSED by the new FEATURE artifact `docs/features/bgud-operator-signin/
      feature.md`, GAP-008 CLOSED by OQ-007; no open gaps remain)
- [ ] All required decisions recorded (§8 holds the authentication-direction decisions —
      OQ-001 full anonymous posture, OQ-002 ADR non-authority and treatment approval,
      OQ-003 tenant contract as session-scoped `locationId` with OQ-006/GAP-005 closed,
      OQ-004 Google-account authority as a pre-registered one-to-one Google-email↔BTR-user
      mapping maintained in the Main Office **User** menu with the existing BTR `RoleId`
      authoritative (GAP-003 direction decided and CLOSED), OQ-005 actor contract as the logged-in
      Google email in the payload resolved server-side to the BTR `UserId` for actor/
      `RegisteredBy` (GAP-002 actor residual and GAP-006 actor half decided and CLOSED),
      OQ-007 OAuth client as reuse of BTrade3's existing Google OAuth project/web client
      (GAP-008 direction decided and CLOSED), OQ-008 login-time synchronization retained and navigation
      gate replaced by local session (GAP-004 closed), OQ-009 single combined change request
      (supersedes the §7 Option D split recommendation), OQ-010 shared-controller
      scope as instance-wide `[Authorize]` removal with no BGud-only exception,
      GAP-001 closed, GAP-002 closed, GAP-003 closed, GAP-004 closed, GAP-006 closed,
      GAP-007 closed, GAP-008 closed, OQ-011 retired)
- [ ] All blocking open questions resolved (OQ-001, OQ-002, OQ-003, OQ-004, OQ-005,
      OQ-006, OQ-007, OQ-008, OQ-009, OQ-010 closed; OQ-011 obsolete)
- [ ] Architecture can be finalized or updated

## Status

READY-FOR-PLANNING

## Notes

- The direction is now fixed: BGud operates **anonymously by deliberate policy** (no
  `api/Auth/login`, no JWT, no bearer header, no `[Authorize]` on BGud-consumed
  endpoints; tenant context supplied explicitly as the session `locationId`
  (`GAMPING`/`CONCAT`/`MAGELANG`) and resolved server-side to `ServerId` via
  `BTR_WarehouseMapping` — §8 OQ-003, refining OQ-001), and ADR-002/003/007 are
  **no longer authoritative for this feature** (§8 OQ-002). There is no
  temporary-exception framing and no ADR-ratification prerequisite anywhere in this
  assessment.
- The tenant/selector contract is decided (OQ-003 and OQ-006 closed 2026-09-23 —
  session-scoped `locationId`, server-side mapping, selector and offline binding
  retained), the Google-account authority is decided (OQ-004 closed 2026-09-23 —
  only Google accounts mapped one-to-one to existing BTR users may sign in; mapping
  created/modified/removed in the Main Office **User** menu; existing BTR `RoleId`
  remains authoritative; unmapped accounts rejected; no separate BGud user management),
  and the actor contract is decided (OQ-005 closed 2026-09-23 — logged-in Google email
  sent in the payload; Cloud resolves it via the OQ-004 mapping to the BTR `UserId`
  recorded as actor/`RegisteredBy`; attribution preserved; no API authentication
  reintroduced), and the OAuth client is decided (OQ-007 closed 2026-09-23 — BGud
  reuses BTrade3's existing Google OAuth project and web client configuration, following
  BTrade3's implemented Sign-In pattern; no separate BGud OAuth registration; coupling
  with the BTrade3 project knowingly accepted for the MVP). The shared-controller scope
  is decided (OQ-010 closed 2026-09-23 — instance-wide `[Authorize]` removal on the
  five BGud-consumed shared routes, no BGud-only exception; pre-deployment verification
  against known non-BGud clients and integration tests is implementation/testing work).
  GAP-002 is **CLOSED** (2026-09-23 — the target anonymous contract is fully decided by
  OQ-001/OQ-003/OQ-005/OQ-010; only the contract-change implementation remains, which is
  architecture work). GAP-003 is **CLOSED** (2026-09-23 — the Google-account
  mapping/authority rules are fully decided by OQ-004; the database, synchronization/
  projection, and User-menu UI realization remains, which is implementation work).
  GAP-004 is **CLOSED** (2026-09-23 — the login/session flow is fully decided by OQ-008:
  Google Sign-In plus the validated mapped account plus Gudang selection plus a persisted
  local session, with login-time synchronization retained and the JWT navigation gate
  replaced by the local session gate; required client refactoring remains implementation
  work). GAP-006 is **CLOSED** (2026-09-23 — the loss of API authentication is knowingly
  accepted policy under OQ-001/OQ-002, with the compensating business contracts decided
  by OQ-003 (tenant), OQ-005 (actor), and OQ-010 (shared-controller scope); security
  hardening beyond the approved posture is not a prerequisite, and only contract
  implementation and pre-deployment verification remain). GAP-008 is **CLOSED**
  (2026-09-23 — the OAuth configuration decision is resolved by OQ-007: BGud reuses
  BTrade3's existing Google OAuth project and web client configuration; adding the
  configuration to BGud and reconciling package/fingerprint requirements remain
  implementation tasks). GAP-007 is **CLOSED** (2026-09-23 — the missing workflow-stage-1
  knowledge is supplied by the new FEATURE artifact
  `docs/features/bgud-operator-signin/feature.md`, `BGUD-OPERATOR-SIGNIN-001`, written
  against the approved §8 decisions). All GAP entries are now closed.
  OQ-008 is closed;
  OQ-009 is closed — single combined change request, superseding the §7 Option D
  split recommendation; OQ-011 is obsolete. No open OQs remain; remaining work is the
  contract-change implementation and verification, which is architecture work.
- **Requests to owning roles (outside Analyst authority — recorded here, not executed):**
  1. *ISSUE update (owner: Issue Intake / ISSUE owning process).* Update
     `docs/issues/BGUD-GOOGLE-SIGNIN-ISSUE.md` so its authentication requirement matches
     the §8 decisions: Google Sign-In may be used as a local UI gate only; no API
     authentication is required; Cloud endpoints used by BGud are anonymous; tenant
     context is explicitly supplied by BGud as the session `locationId`
     (`GAMPING`/`CONCAT`/`MAGELANG`), resolved server-side to `ServerId` via the existing
     warehouse mapping (§8 OQ-003 — carry `locationId`, not `ServerId`; granularity
     preserved); no JWT-based user/tenant session is required; and anonymous access must
      **not** be described as a temporary workaround. Issue Notes 1–2 are answered by
      OQ-001; Note 3 (tenant resolution) is answered by OQ-003; Note 4 (warehouse
      selector) is answered by OQ-003/OQ-006; account authority is answered by OQ-004
      (only Google accounts pre-registered as a one-to-one mapping to an existing BTR
      user in the Main Office **User** menu may sign in; unmapped accounts rejected;
      existing BTR `RoleId` authoritative); actor attribution is answered by OQ-005
      (BGud sends the logged-in Google email in the endpoint payload; the Cloud
      resolves it via the Main Office mapping to the BTR `UserId` recorded as actor/
      `RegisteredBy`); OAuth client choice is answered by OQ-007 (BGud reuses BTrade3's
      existing Google OAuth project and web client configuration, following BTrade3's
      implemented Sign-In pattern — no separate BGud registration).
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
  stay synchronized; the ISSUE must be aligned (request 1 above). The FEATURE artifact for
  operator authentication now exists at `docs/features/bgud-operator-signin/feature.md`
  (GAP-007 closed), and the ARCHITECTURE work for the confirmed Cloud contract change
  belongs to the Architect now that all blocking gaps and OQs are closed.

---

# 10. References

Referenced artifacts:

- `docs/issues/BGUD-GOOGLE-SIGNIN-ISSUE.md` (ISSUE, input — alignment request in §9)
- `docs/features/bgud-operator-signin/feature.md` (`BGUD-OPERATOR-SIGNIN-001`) — the FEATURE
  artifact for this capability (GAP-007 closed)
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
