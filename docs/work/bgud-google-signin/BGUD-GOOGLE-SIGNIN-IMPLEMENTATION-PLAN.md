---
Title: BGud — Google Sign-In and Anonymous Cloud Endpoints — Implementation Plan
Code: BGUD-GOOGLE-SIGNIN-001
Artifact: IMPLEMENTATION-PLAN
Version: 1.0
LastUpdated: 2026-09-23
Status: NOT-STARTED
Execution Approval: PENDING
---

# 1. Objective

Implement FEATURE `BGUD-OPERATOR-SIGNIN-001`: BGud operators sign in with a
Main-Office-registered Google account and a session-fixed Gudang; BGud
communicates with the Cloud anonymously (no `api/Auth/login`, no JWT, no
bearer header) under the explicit session-context contract.

Referenced artifacts:

- FEATURE: `docs/features/bgud-operator-signin/feature.md` (`BGUD-OPERATOR-SIGNIN-001`)
- ARCHITECTURE: `docs/work/bgud-google-signin/BGUD-GOOGLE-SIGNIN-ARCHITECTURE.md` (v1.0)
- FEASIBILITY-ASSESSMENT: `docs/work/bgud-google-signin/BGUD-GOOGLE-SIGNIN-FEASIBILITY-ASSESSMENT.md`
  (v1.17, `READY-FOR-PLANNING`; decisions OQ-001…OQ-010)

This plan realizes the ARCHITECTURE; it introduces no business or
architecture decisions. Where this plan cites `TD-nn`, `§n`, OQ, RISK, IR,
or P identifiers, the authoritative definition is in the referenced
artifacts.

---

# 2. Planning Scope

Target: the ARCHITECTURE (§3 Scope). Current: BGud username/password login
against `POST api/Auth/login` with JWT-tenant-resolved Cloud routes
(FEASIBILITY-ASSESSMENT §2). The implementation delta, by repository:

1. **Main Office** (`src/j05-btr-distrib`) — `BTR_User.Email` column with
   filtered unique index (TD-14, §8); User-menu create/modify/remove of the
   Google-email mapping with duplicate rejection.
2. **Cloud** (`src/j06-pkl-btrade-api`) — `BTRADE_User.Email` and
   `BTRADE_ReturnOrder.SubmittedBy` columns (§8); the `BTRADE_User` email
   read/write path on the user model and DAL; `SessionContextResolver`
   (TD-04) and anonymous `POST api/session/resolve` (TD-02); instance-wide
   `[Authorize]` removal and session-context resolution on the four
   BGud-consumed controllers (TD-03/TD-05/TD-06/TD-13); `SubmittedBy`
   persisted and returned on the incremental download (TD-15).
3. **Synchronization** (`src/j07-btrade-sync`) — `BTR_User → BTRADE_User`
   projection extended with `Email`; return-order relay carries `SubmittedBy`
   into the existing Main Office import (`BTR_ReturnOrder.CreatedBy`, TD-15).
4. **BGud** (`src/BGud`) — Google Sign-In configuration and helper following
   the BTrade3 pattern (TD-01/TD-07); local session model (TD-10);
   session-context network layer replacing the bearer interceptor (TD-12);
   sign-in flow with account resolution, Gudang selection, login-time
   synchronization (TD-09); local-session navigation gate (TD-11); 409
   session-ended handling (TD-13).

Out of scope (fixed by ARCHITECTURE §3, not re-decided here): BTrade3's
authentication direction; ADR file status treatment; HTTPS remediation; any
BGud role model or second identity store; barcode/Retur business rules; the
legacy `{serverId}` anonymous read routes; restoration of `[Authorize]`.

---

# 3. Dependencies

## External dependencies

- **EXT-01 — Google project configuration (blocks runtime verification of
  P6-S09):** BGud must be registered as an Android client (package
  `com.elsasa.bgud` + signing certificate fingerprint) inside the shared
  Google project `btrade3-663be`, and the resulting `google-services.json`
  delivered to the repository (TD-07). This is configuration delivery in an
  external console, not repository code.
- **EXT-02 — Coordinated release order (deployment constraint, ARCHITECTURE
  §8):** Main Office `BTR_User` column → `j07-btrade-sync` projection →
  Cloud `BTRADE_User` column → Cloud API change → BGud app. The Cloud must
  not require the session headers before BGud ships, and BGud must not omit
  the bearer before the Cloud stops requiring `[Authorize]` (RISK-001).
  This order governs deployment actions, not build order; slices are
  designed to tolerate it at every intermediate step (see the notes on
  P3-S04 and P5-S07), and the P6-S10 edges keep the BGud bearer removal
  behind the Cloud contract change.
- **EXT-03 — Non-BGud consumer verification (pre-deployment):** the affected
  shared routes (`barcodes/sync`, `barcode-registration`,
  `BarcodeRegistration/status`, `return-order`, `Driver/{serverId}`) must be
  verified against `BTrade3` and `j07-btrade-sync` before deployment
  (OQ-010 Implementation Note). This is a testing activity; testing begins
  only after the plan is COMPLETED (§4). A discovered consumer dependency is
  raised as a new ISSUE — not handled inside this plan.
- **EXT-04 — Operator mapping data (pre-rollout):** operators must be mapped
  to Google emails in the Main Office User menu before BGud rollout; an
  unmapped operator cannot sign in (ARCHITECTURE §8 Migration
  Considerations). Operational data entry, not code.

## Slice dependencies

- `Depends On` declares implementation prerequisites.
- Dependencies reference Slice IDs only.
- Dependency satisfaction requires the referenced slice to have implementation
  status IMPLEMENTED.
- Dependency satisfaction does not require review status GO.
- Dependencies must represent real implementation prerequisites.

## Slice dependency graph

```text
P1-S01 ──► P2-S03
P1-S01 ──► P5-S07
P1-S02 ──► P3-S04 ──┬──► P4-S05 ──┐
                    ├──► P4-S06 ──┼──► P6-S10
                    │       │     │
                    │       └─────┼──► P5-S08
P6-S09 ───────────────────────────┘
```

Edges in detail: `P2-S03 ← P1-S01`; `P3-S04 ← P1-S02`; `P4-S05 ← P3-S04`;
`P4-S06 ← P3-S04`; `P5-S07 ← P1-S01`; `P5-S08 ← P4-S06`;
`P6-S10 ← P6-S09, P3-S04, P4-S05, P4-S06`.

## Execution order (derived)

| Wave | Slices (parallelizable) | Gate for next wave |
| ---- | ----------------------- | ------------------ |
| W1 | P1-S01, P1-S02, P6-S09 | schema + client config present |
| W2 | P2-S03, P3-S04, P5-S07 | resolution + projection paths exist |
| W3 | P4-S05, P4-S06 | Cloud serves BGud routes anonymously |
| W4 | P5-S08 | attribution relay functional |
| W5 | P6-S10 | BGud cutover (client/Cloud ship together, EXT-02) |

---

# 4. Progress Summary

Plan status values are:

- NOT-STARTED
- IN-PROGRESS
- BLOCKED
- COMPLETED

Execution Approval values are:

- PENDING
- APPROVED

Execution Approval is owned by the Architect. It is PENDING during Planning
and set to APPROVED when the plan is released for execution. Execution must
not begin while Execution Approval is PENDING.

COMPLETED is a plan-level status only. Set it only when every slice has
implementation status IMPLEMENTED and review status GO.

Testing and test-package creation must not begin until the plan is COMPLETED.
An individual slice with review status GO is not a testing entry condition.

Slice implementation status values are:

- NOT-STARTED
- IN-PROGRESS
- IMPLEMENTED
- BLOCKED

Slice review status values are:

- NOT-REVIEWED
- GO
- NO-GO

| Phase | Implementation Status | Review Status | Progress |
|-------|-----------------------|---------------|----------|
| P1 | NOT-STARTED | NOT-REVIEWED | 0/2 |
| P2 | NOT-STARTED | NOT-REVIEWED | 0/1 |
| P3 | NOT-STARTED | NOT-REVIEWED | 0/1 |
| P4 | NOT-STARTED | NOT-REVIEWED | 0/2 |
| P5 | NOT-STARTED | NOT-REVIEWED | 0/2 |
| P6 | NOT-STARTED | NOT-REVIEWED | 0/2 |

---

# 5. Phases

## P1 - Database Foundation

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

Additive-only schema changes (ARCHITECTURE §8): both slices are
non-breaking for existing consumers and are deployed first (EXT-02).

### P1-S01

Title: Main Office `BTR_User` Google-email column and filtered unique index

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

Objective: Add the operator Google-email mapping storage to the authoritative
Main Office account table per TD-14 and ARCHITECTURE §8:
`Email VARCHAR(100) NOT NULL` with constraint `DF_BTR_User_Email DEFAULT('')`
and filtered unique index `UX_BTR_User_Email` on `Email` where `Email <> ''`.

Depends On: None

Repository: `src/j05-btr-distrib` (`btr.sql` database project)

Completion Criteria:

- `btr.sql/Tables/Helper/BTR_User.sql` table definition contains the `Email`
  column and the filtered unique index exactly as ARCHITECTURE §8 specifies.
- An idempotent `Scripts/Upgrade_*.sql` script (existing repository upgrade
  convention) applies the column, default constraint, and index to an
  existing database and is safe to re-run.
- The new/modified objects are registered in the `.sqlproj`.
- Existing rows receive `Email = ''` and remain unaffected.

Notes: No duplicate pre-existing data can exist (the column is new); the
duplicate-resolution caveat of ARCHITECTURE §8 applies to future User-menu
saves (P2-S03).

---

### P1-S02

Title: Cloud `BTRADE_User.Email` and `BTRADE_ReturnOrder.SubmittedBy` columns

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

Objective: Add the Cloud-side columns required by account resolution and
return-order attribution per ARCHITECTURE §8: `BTRADE_User.Email
VARCHAR(100) NOT NULL DEFAULT('')` with index `IX_BTRADE_User_Email`;
`BTRADE_ReturnOrder.SubmittedBy VARCHAR(50) NOT NULL DEFAULT('')`.

Depends On: None

Repository: `src/j06-pkl-btrade-api` (`btrade.sqldb` database project)

Completion Criteria:

- `BarcodeContext/BTRADE_User.sql` and
  `ReturnOrderContext/BTRADE_ReturnOrder.sql` table definitions contain the
  new columns and index as specified.
- An idempotent `Scripts/Upgrade_*.sql` script applies the changes to an
  existing database and is safe to re-run (existing
  `Scripts/Upgrade_Barcode_Registry.sql` is the convention precedent).
- The objects are registered in the `.sqlproj`.
- No existing consumer breaks: additions are defaulted and additive.

Notes: `BTRADE_BarcodeRegistrationRequest` and `BTR_ReturnOrder` require no
schema change (ARCHITECTURE §8); do not touch them.

---

## P2 - Main Office Account Mapping

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

### P2-S03

Title: User-menu maintenance of the Google-email mapping

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

Objective: Realize OQ-004/TD-14 in the Main Office desktop application: an
authorized user can create, modify, and remove a BTR user's Google email in
the existing **User** menu. Emails are trimmed and compared
case-insensitively; a save that would duplicate a non-empty email already
registered to another user is rejected.

Depends On: P1-S01

Repository: `src/j05-btr-distrib`

Completion Criteria:

- `btr.domain/SupportContext/UserAgg/UserModel.cs` carries `Email`.
- `btr.application/SupportContext/UserAgg` (`IUserDal`, `UserBuilder`,
  `UserValidator`, `UserWriter`) round-trips `Email`, normalizes it (trim,
  case-insensitive comparison), and rejects a duplicate non-empty email on
  save with an operator-visible validation message.
- `btr.infrastructure/SupportContext/UserAgg/UserDal.cs` reads and writes the
  `Email` column.
- `btr.distrib/SharedForm/UserForm.cs` presents and edits the Google email
  field; empty email (unmapped) remains a valid state for non-BGud users.
- Existing user maintenance behavior (password, role, prefix) is unchanged.

Notes: Empty `Email` rows are simply not eligible for BGud sign-in
(ARCHITECTURE §8). Permission model is unchanged — User-menu access is the
existing gate (OQ-004).

---

## P3 - Cloud Account-Mapping and Resolution Base

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

### P3-S04

Title: Cloud user email path, `SessionContextResolver`, and `POST api/session/resolve`

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

Objective: Build the Cloud's complete `BTRADE_User.Email` capability in one
base slice: carry `Email` on the user model/DAL (read by email for
resolution, write through the `POST api/User` projection ingest); add the
single context owner `SessionContextResolver` (TD-04) —
`ResolveAccount(email)`, `ResolveTenant(locationId)`, `ListWarehouses()`; and
expose the anonymous resolution endpoint `POST api/session/resolve` (TD-02)
returning JSendOk `{ UserId, UserName, RoleId, Warehouses[ { LocationId,
ServerId } ] }`.

Depends On: P1-S02

Repository: `src/j06-pkl-btrade-api`

Completion Criteria:

- `btrade.domain/BarcodeFeature/UserType.cs` carries `Email` (PascalCase JSON
  payload field per §10), and `btrade.infrastructure/BarcodeFeature/UserDal.cs`
  reads and writes the column, including an email-based lookup honoring
  TD-14 normalization (trim, case-insensitive) against
  `IX_BTRADE_User_Email`.
- `SessionContextResolver` exists as an application-layer component
  (DI-registered in `btrade.webapi/Configurations`), reuses the existing
  `IUserDal` and `IWarehouseMappingDal`, and introduces no new tenant table
  or vocabulary.
- `ResolveTenant` fails explicitly for an unmapped `locationId` — no
  fallback — consistent with the existing `IssueTokenCommand` mapping
  behavior; `ResolveAccount` rejects an unmapped or invalid account.
- `SessionController` serves `POST api/session/resolve` anonymously, issues
  no token, and stores no server-side session state; unmapped/invalid Google
  account → HTTP 400 (JSend failure) per §9 Error Semantics.
- A `POST api/User` payload without `Email` (legacy `j07-btrade-sync` still
  deploying) is ingested without error — the field defaults to empty
  (EXT-02 tolerance).
- Tests in `btrade.webapi.Test` cover: resolve success, unmapped email,
  inactive account, warehouse list content (GAMPING/CONCAT distinct), and
  email-based projection round-trip.
- `UserController` keeps `[Authorize]` and its current behavior otherwise
  (TD-06).

Notes: The response `Warehouses` list is what BGud later uses to supply
`ServerId` on legacy `{serverId}` routes (TD-08). Model/DAL email read+write
and the resolver are one slice deliberately: they share `UserType.cs`,
`IUserDal.cs`, and `UserDal.cs` and cannot be implemented concurrently
without conflict.

---

## P4 - Cloud Anonymous Session Contract

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

Both slices serve BGud-consumed routes on disjoint controllers/use-cases and
may run in parallel; together with P6-S10 they form the coordinated release
(RISK-001, EXT-02). Neither may be deployed ahead of the release order in
ARCHITECTURE §8.

### P4-S05

Title: Barcode routes — instance-wide `[Authorize]` removal and session-context resolution

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

Objective: Per TD-03/TD-05/TD-06/TD-13, remove `[Authorize]` from
`BarcodeController` and `BarcodeRegistrationController`, and convert BGud's
three barcode routes to header-resolved context: `GET api/barcodes/sync`,
`POST api/barcode-registration`, and `GET api/BarcodeRegistration/status`
resolve `ServerId` from `X-Session-Location` and (where applicable) the
actor from `X-Session-Actor` through `SessionContextResolver`. No JWT-claim
reads (`User.GetServerId()`/`User.GetUserId()`) remain on those routes.

Depends On: P3-S04

Repository: `src/j06-pkl-btrade-api`

Completion Criteria:

- Both controllers are anonymous instance-wide; no BGud-only exception,
  dual-mode, or compatibility shim.
- Registration submit records the resolver's BTR `UserId` as `RequestedBy`
  in the existing `BTRADE_BarcodeRegistrationRequest.RequestedBy` column
  (identifier shape unchanged).
- `GET status` keeps returning only the caller's own requests via the
  resolved `RequestedBy` filter (TD-05).
- `pending`/`ack` keep their current claim-based behavior for token-bearing
  existing consumers (`j07-btrade-sync` still sends a bearer; removing the
  attribute does not strip claims from a supplied token).
- Error semantics per TD-13: missing/unmapped `X-Session-Location` → HTTP
  400; unresolvable `X-Session-Actor` → HTTP 409 (JSend failure), mapped via
  `ErrorHandlerMiddleware`.
- Header names are exactly `X-Session-Location` / `X-Session-Actor` (§10);
  route DTOs and the JSend envelope are unchanged (TD-03).
- Existing JWT tests in `btrade.webapi.Test` are updated to the new posture;
  new tests cover header resolution, 400/409 outcomes, and submit
  attribution.
- Session-context header values are never logged at informational level
  (§9 Logging).

Notes: Only the three BGud-consumed routes lose JWT-claim reads;
`POST api/Barcode/sync` (j07 publish path) and `pending`/`ack` behavior for
token-bearing consumers is otherwise unchanged.

---

### P4-S06

Title: Return-order and driver routes — `[Authorize]` removal and `SubmittedBy` attribution

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

Objective: Per TD-03/TD-05/TD-06/TD-13/TD-15, remove `[Authorize]` from
`ReturnOrderController` and `DriverController`; resolve
`POST api/return-order` context from the session headers, persist the
resolved operator `UserId` as `BTRADE_ReturnOrder.SubmittedBy`, and include
`SubmittedBy` in the incremental download output. `GET
api/Driver/{serverId}` keeps its existing route contract, now anonymous.

Depends On: P3-S04

Repository: `src/j06-pkl-btrade-api`

Completion Criteria:

- Both controllers are anonymous instance-wide; `ReturnOrderController.Submit`
  no longer calls `User.GetServerId()`; no JWT-claim reads remain on BGud's
  path.
- `ReturnOrderUploadCommand` (and its handler/DAL) carries and persists the
  resolved `SubmittedBy`; the request body keeps no ServerId/actor field
  (P-06 preserved — context travels in headers only).
- `GET api/ReturnOrder/incremental/...` output includes `SubmittedBy`
  (domain `ReturnOrderType` extended) so `j07-btrade-sync` can relay it
  (TD-15).
- Error semantics per TD-13: unmapped `X-Session-Location` → 400;
  unresolvable `X-Session-Actor` → 409.
- Existing JWT/enforcement tests updated; new tests cover submit
  attribution, incremental output inclusion, and 400/409 outcomes.
- `POST api/Driver` (sync path used by `j07-btrade-sync`) keeps its current
  behavior for token-bearing consumers.

Notes: Legacy Cloud rows without `SubmittedBy` are handled by the relay
slice (P5-S08), not here.

---

## P5 - Synchronization Adjustments

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

### P5-S07

Title: `j07-btrade-sync` user projection carries `Email`

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

Objective: Extend the `BTR_User → BTRADE_User` credential projection
(IR-05 "replicates BTR_User verbatim") so the Google email flows to the
Cloud: read `Email` from `BTR_User`, carry it in the sync model, and include
it in the `POST api/User` payload.

Depends On: P1-S01

Repository: `src/j07-btrade-sync`

Completion Criteria:

- `j07-btrade-sync/Repository/UserDal.cs` `ListData()` selects `Email`
  alongside the existing columns.
- `j07-btrade-sync/Model/UserType.cs` carries `Email`.
- `j07-btrade-sync/Service/UserSyncService.cs` posts the extended payload to
  the still-authenticated `POST api/User` (this service keeps its login/JWT —
  OQ-001's anonymous posture is BGud-only).
- The stale "ServerId is assigned by the Cloud from the JWT" comment is
  corrected to reflect the current contract.

Notes: The Cloud tolerates this payload before and after P3-S04's ingest
change deploys (unknown-field tolerance per EXT-02), so the ARCHITECTURE §8
order (Main Office column → sync → Cloud column) is safe.

---

### P5-S08

Title: Return-order attribution relay to Main Office

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

Objective: Per TD-15, relay the Cloud-resolved operator attribution into the
Main Office: `SubmittedBy` from the incremental download reaches the
existing import path so `BTR_ReturnOrder.CreatedBy` holds the operator
`UserId`; legacy rows fall back to the existing service-account behavior.

Depends On: P4-S06

Repository: `src/j07-btrade-sync`

Completion Criteria:

- `j07-btrade-sync/Model/ReturnOrderModel.cs` carries `SubmittedBy`
  (case-insensitive deserialization from the incremental response).
- The import path (`SyncForm.ProcessReturnOrder` →
  `Repository/ReturnOrderDal.cs` insert) writes `CreatedBy` from
  `SubmittedBy` when non-empty, preserving the current identifier shape.
- A row with empty/absent `SubmittedBy` keeps the current pre-relay behavior
  unchanged (legacy fallback).
- Return-order numbering and validation remain office-side; no Main Office
  schema change.

Notes: The download service already authenticates with its service JWT
(`BtradeAuthService`); removal of `[Authorize]` on the Cloud route does not
change the sync client's call — verify it still succeeds, do not "clean up"
its token header.

---

## P6 - BGud Client Cutover

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

### P6-S09

Title: Google Sign-In configuration and helper

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

Objective: Per TD-01/TD-07 (OQ-007), give BGud Google Sign-In capability
following BTrade3's implemented pattern: add `app/google-services.json` for
BGud's Android client inside the shared Google project (EXT-01), apply the
Google Services Gradle plugin to the app module, add the Play Services Auth
dependency, and introduce `util/GoogleSignInHelper.kt` acquiring the
signed-in account email with `requestIdToken` on the shared web client id.

Depends On: None

Repository: `src/BGud`

Completion Criteria:

- `src/BGud/app/google-services.json` exists (currently absent) and the
  Google Services plugin is applied in `app/build.gradle.kts` with the Play
  Services Auth dependency.
- The Sign-In helper yields the account email and exposes sign-out of the
  Google account, mirroring `src/BTrade3/util/GoogleSignInHelper.kt`.
- The app compiles and its existing behavior (username/password login) is
  untouched by this slice — no sign-in path is rewired yet.
- The shared web client id is used per TD-07; no separate BGud OAuth
  registration appears anywhere.

Notes: Runtime verification of the Google flow requires EXT-01; if the
console registration is not yet delivered, compile-level completion is still
reviewable and this must be recorded as the residual check.

---

### P6-S10

Title: Local-session sign-in flow and bearer-free network layer

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

Objective: Execute the BGud cutover realizing TD-02/TD-03/TD-08/TD-09/TD-10/
TD-11/TD-12/TD-13 and FEATURE §6: replace the username/password
`api/Auth/login` flow with Google Sign-In → `POST api/session/resolve` →
Gudang selection → persisted local session → login-time synchronization →
Home; replace the bearer interceptor with the session-context interceptor;
gate navigation on local session state; logout and Gudang change clear the
session.

Depends On: P6-S09, P3-S04, P4-S05, P4-S06

Repository: `src/BGud`

Completion Criteria:

- **Sign-in flow:** `ui/screen/LoginScreen.kt` +
  `viewmodel/LoginViewModel.kt` present the Google button and the retained
  three-Gudang selector; the signed-in email is resolved through a new
  `SessionResolverRepository` calling `POST api/session/resolve`; an unmapped
  account (HTTP 400) shows the administrator message and creates no session;
  cancellation/failure leaves the operator signed out.
- **Local session:** `datastore/SessionPreferencesDataSource.kt` stores
  `google_email`, `user_id`, `user_name`, `role_id`, `location_id`,
  `server_id` (TD-10); `token` and `office_code` are removed; sync
  timestamps retained; session validity = `google_email` present, so legacy
  installs route to sign-in; `datastore/SessionBinding.kt` compares the
  queued record's `locationId` against the session's (IR-09 — queued records
  are never re-homed).
- **Network layer:** `network/AuthInterceptor.kt` is replaced by a
  `SessionContextInterceptor` attaching `X-Session-Location` and
  `X-Session-Actor` when a session exists; `network/ApiClient.kt` no longer
  accepts a token provider; no `Authorization` header is ever sent; the
  resolution call carries only the body email (TD-12).
- **API surface:** `network/BtradeApiService.kt` removes the
  `api/Auth/login`/`LoginRequest`/`LoginResult` usage and declares
  `POST api/session/resolve`; legacy `{serverId}` reads keep their route
  contract using the session's resolved `server_id` (TD-08).
- **Login-time sync:** `BarcodeSyncRepository.sync` and
  `ReturnOrderSyncRepository.sync` still run immediately after session
  creation with the session-context client; barcode-sync failure blocks
  navigation, return-order failure does not (TD-09, current semantics);
  `sync/BarcodeSyncWorker.kt` and `sync/ReturnOrderSyncWorker.kt` build
  their clients from the local session, not a token.
- **Gate and lifecycle:** `ui/Navigation.kt` start destination is `home`
  only on a valid local session, never a token (TD-11);
  `viewmodel/SettingsViewModel.kt` logout and Gudang change clear the local
  session; a Cloud 409 clears the session and returns the operator to
  sign-in (TD-13).
- **Removal:** no code path stores, reads, or sends a JWT or password.
- Unit tests cover the session store/validity, interceptor header
  attachment, and 409 session-clear behavior; the app builds.

Notes: Largest slice of the plan, intentionally: session keys, network
layer, and the login flow are compile-entangled and cannot ship separately
(RISK-001). Execute strictly against ARCHITECTURE TD-02…TD-13; §10
constraints apply verbatim (no `ServerId` on the four context routes, fixed
header names, single composition-root base URL unchanged, non-destructive
DataStore/Room handling). The edges to P3-S04/P4-S05/P4-S06 encode EXT-02:
the Cloud must serve the new contract before BGud omits the bearer. Sign-in
data prerequisites (mapped operators, projected emails) are EXT-01/EXT-04,
not code dependencies.

---

# 6. Change Log

- v1.0 (2026-09-23): Plan created from ARCHITECTURE v1.0 and
  FEASIBILITY-ASSESSMENT v1.17 (READY-FOR-PLANNING); grounded in current
  codebase (BGud network/session/login/sync sources, Cloud controllers,
  user/barcode/return-order DALs, `j05` User-menu components, `j07`
  projection/relay services, BTrade3 Sign-In precedent). 6 phases, 10
  slices. Cloud user-model/DAL email read+write deliberately unified into
  P3-S04 (shared-file conflict prevention with the resolver slice).
