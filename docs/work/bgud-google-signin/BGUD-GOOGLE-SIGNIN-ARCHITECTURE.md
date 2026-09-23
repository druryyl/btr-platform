---
Title: BGud — Google Sign-In and Anonymous Cloud Endpoints — Architecture
Code: BGUD-GOOGLE-SIGNIN-001
Artifact: ARCHITECTURE
Version: 1.0
LastUpdated: 2026-09-23
---

# 1. Overview

This architecture realizes FEATURE `BGUD-OPERATOR-SIGNIN-001`
(`docs/features/bgud-operator-signin/feature.md`): a warehouse operator signs in
to BGud with a Google account registered to them, selects the Gudang they are
working in, and continues in a local session fixed to that Gudang — with no BTR
password, no Cloud login call, and no JWT.

It realizes exactly one feature: BGud operator sign-in, session establishment,
sign-out, Gudang change, login-time data refresh, and the Cloud contract BGud
consumes anonymously. The originating request is ISSUE
`BGUD-GOOGLE-SIGNIN-001` (`docs/issues/BGUD-GOOGLE-SIGNIN-ISSUE.md`).

This architecture consumes the approved decisions of FEASIBILITY-ASSESSMENT
`BGUD-GOOGLE-SIGNIN-001`
(`docs/work/bgud-google-signin/BGUD-GOOGLE-SIGNIN-FEASIBILITY-ASSESSMENT.md`,
§8 OQ-001…OQ-010) and adds only the technical realization: carriers, component
responsibilities, resolution ownership, data changes, and implementation
boundaries.

---

# 2. Architectural Basis

## Business Context

- FEATURE `BGUD-OPERATOR-SIGNIN-001`
  (`docs/features/bgud-operator-signin/feature.md`) — the authoritative source
  for the business outcome, operational flow, domain orchestration, constraints,
  exceptions, and acceptance criteria realized here.
- DOMAIN `docs/foundation/DOMAIN.md` — operator accounts, the Gudang concept,
  and the product principle that Main Office is the single source of truth.
- DOMAIN `docs/work/barcode-registry/BARCODE-REGISTRY-DOMAIN.md` — the Warehouse
  Officer actor and the barcode-registry context that consumes the session.
- Referenced (historical, not authoritative for BGud): the barcode-registry
  login/authentication design, ADR-002/ADR-003/ADR-007 (no longer authoritative
  for this feature per OQ-002).

Business knowledge (who may sign in, Gudang granularity, offline queue binding,
attribution, login-time refresh) belongs to FEATURE/DOMAIN and is referenced,
not restated, here.

## Analysis Input

- FEASIBILITY-ASSESSMENT `BGUD-GOOGLE-SIGNIN-001` — current state, findings,
  risks, assumptions, and the approved decisions in §8 (OQ-001…OQ-010, with
  OQ-011 obsolete). This architecture performs no gap analysis, alternative
  evaluation, or decision resolution of its own.

## Existing Email Identity (survey)

Verified at the current HEAD. There is **no** email on the operator-account
store; the only Google-email identity that exists today is the **salesperson**
one used by BTrade3:

| Element | Fact |
| ------- | ---- |
| `BTR_User` (Main Office) | Columns `UserId, UserName, Password, Prefix, RoleId` — **no email column**. |
| `BTRADE_User` (Cloud projection) | `UserId, UserName, Password, RoleId, IsAktif, ServerId` — **no email column**. |
| `BTR_SalesPerson` (Main Office) | Has `Email VARCHAR(100) NULL` (`Tables/SalesContext/BTR_SalesPerson.sql`). |
| `BTRADE_SalesPerson` (Cloud) | Has `Email`, projected by `j07-btrade-sync`. |
| Cloud endpoint | `GET api/SalesPerson/exists?email=…` → `IsValidSalesEmailQuery` → `SalesPersonDal.GetByEmail` (anonymous). |
| Main Office reporting | Joins `UserEmail` on `BTR_SalesPerson.Email` (e.g. `CheckInDal`, `SalesOmzetSourceDal`, field-activity reports). |

**How BTrade3 maps the user:** Google Sign-In yields the account email; the app
stores it locally and stamps it as `UserEmail` on orders/check-ins; the Cloud
and Main Office match that email to `BTR_SalesPerson.Email`. BTrade3's own
`ApiService` never calls the `exists` endpoint and made no server login call in
the version assessed — the email only becomes meaningful downstream. So
BTrade3's email↔identity mapping is **salesperson-based**, not BTR-user-based.

**Consequence for BGud:** an operator email↔BTR-user mapping does **not** already
exist anywhere. The approved OQ-004 decision fixes the mapping target as the
**BTR user**, maintained in the Main Office **User** menu, with the existing BTR
`RoleId` authoritative — so this architecture realizes that by adding the email
to `BTR_User` (and its Cloud projection). The existing salesperson email
mechanism is not reused (see TD-14).

## Realized Decisions

| Analysis decision | Architectural realization |
| ----------------- | ------------------------- |
| OQ-001/OQ-002 — no login, JWT, bearer, or `[Authorize]` on BGud's path; deliberate policy | §4 TD-01, TD-02, TD-06, TD-10; §5 BGud network components; §10 constraints |
| OQ-003/OQ-006 — session `locationId` carried, `ServerId` resolved server-side via `BTR_WarehouseMapping`; selector and offline binding retained | §4 TD-03, TD-04, TD-08, TD-09; §6; §7 |
| OQ-004 — only Main Office-mapped Google accounts may sign in; mapping maintained in the User menu; BTR `RoleId` authoritative | §4 TD-02, TD-07; §5 Main Office components; §8 |
| OQ-005 — logged-in Google email as actor input, resolved server-side to BTR `UserId`; attribution preserved | §4 TD-05; §6; §8 |
| OQ-007 — reuse BTrade3's Google OAuth project/web client; follow its Sign-In pattern | §4 TD-07; §10 constraints |
| OQ-008 — login-time synchronization retained; local session replaces the JWT navigation gate; logout/warehouse change clears the session | §4 TD-09, TD-10, TD-11; §5; §11 |
| OQ-009 — one combined change request | §3 scope; §10 coordinated-delivery constraint |
| OQ-010 — instance-wide `[Authorize]` removal on the five BGud-consumed shared routes; no BGud-only exception | §4 TD-06; §5 Cloud components; §10 constraints |

---

# 3. Scope

## Included

- BGud Google Sign-In gate and the replacement of the username/password form.
- BGud local session: establishment, persistence, navigation gate, logout, and
  Gudang change.
- Cloud account resolution: Google email → authoritative BTR user, including the
  session location→`ServerId` mapping the device needs for legacy read routes.
- Cloud session-context contract for the four JWT-derived BGud routes: carried
  `locationId` and actor email, resolved server-side.
- Instance-wide removal of `[Authorize]` from the controllers serving BGud's
  consumed routes (`BarcodeController`, `BarcodeRegistrationController`,
  `ReturnOrderController`, `DriverController`).
- Main Office `BTR_User` Google-email mapping and its maintenance in the User
  menu.
- `j07-btrade-sync` credential-projection extension (`BTR_User` → `BTRADE_User`
  incl. email) and return-order attribution relay.
- Database changes required by the above, and their migration order.
- Login-time barcode-registry and return-order synchronization under the new
  session context.

## Excluded

- BTrade3's authentication direction (RD-001 of the BTrade3 compatibility
  assessment) — explicitly untouched by OQ-002.
- ADR file status treatment — owned by the ADR owning process (OQ-002/§9 of the
  assessment); not a prerequisite here.
- HTTPS/token-transport remediation (RISK-004) — general platform concern;
  BGud sends no credentials after this change.
- A BGud role model, BGud account provisioning, or any second identity store
  (OQ-004).
- Barcode-registry and Retur business rules — owned by their domains and
  referenced, not restated.
- Restoration/re-authentication path for BGud (OQ-011 retired).
- Redesign of the legacy anonymous read routes (`api/Brg`, `api/Customer`,
  `api/SalesPerson`) — they keep their existing client-supplied `ServerId`
  contract.

---

# 4. Technical Decisions

The decisions below realize the approved analysis decisions. They do not
re-evaluate them.

## TD-01 — Google Sign-In is a local identity gate only

BGud obtains the operator's Google account email on-device. The Google ID token
is **not** sent to the Cloud and is not verified by the Cloud; the email is
identity data, not a credential. This realizes OQ-001 (no credentials in
transit, no Cloud authentication) and OQ-007 (same Sign-In pattern as BTrade3).

## TD-02 — Cloud account resolution endpoint

A new anonymous Cloud endpoint resolves the Google email to an existing, active
BTR user and returns the BTR identity plus the session location→`ServerId`
mapping:

```text
POST api/session/resolve
Request  : { "Email": "operator@gmail.com" }
Success  : JSendOk data { UserId, UserName, RoleId, Warehouses[ { LocationId, ServerId } ] }
Rejected : HTTP 400 (unmapped or invalid Google account)
```

- The endpoint is anonymous (no `[Authorize]`, no token issuance). It is the
  technical realization of the OQ-004 business rule and of the OQ-003 statement
  that `ServerId` stays a Cloud-side value derived from `BTR_WarehouseMapping`.
- Returning the location→`ServerId` mapping is deliberate: BGud must supply the
  resolved `ServerId` on the legacy path routes (`api/Brg`, `api/Customer`,
  `api/SalesPerson`, `api/Driver`), which keep their existing contract. This
  avoids duplicating tenant vocabulary on the device (OQ-003/OQ-006) and keeps
  `BTR_WarehouseMapping` authoritative.
- The endpoint never issues a token and never stores server-side session state;
  BGud remains stateless towards the Cloud between requests.

## TD-03 — Session context is carried in dedicated request headers

The four JWT-derived BGud routes carry the session context explicitly:

| Header | Value | Routes |
| ------ | ----- | ------ |
| `X-Session-Location` | `locationId` (`GAMPING`/`CONCAT`/`MAGELANG`) | `GET api/barcodes/sync`, `POST api/barcode-registration`, `GET api/BarcodeRegistration/status`, `POST api/return-order` |
| `X-Session-Actor` | logged-in Google email | `POST api/barcode-registration`, `GET api/BarcodeRegistration/status`, `POST api/return-order` |

- Headers were chosen because they apply uniformly to GET and POST without
  changing the existing JSend request DTOs, and because they replace the
  `Authorization` header BGud no longer sends — the simplest possible client
  change (one interceptor) and a single server read point.
- `X-Session-Location` always carries `locationId`, **never** `ServerId`, so
  Gamping and Concat stay distinct (OQ-003, IR-09).
- The values are plain context data, not an authentication mechanism (OQ-003's
  explicit statement); they confer no authority beyond selecting operational
  context.
- BGud continues to supply `ServerId` only where the existing route contract
  requires it (the legacy `{serverId}` path routes), using the mapping returned
  by TD-02.

## TD-04 — Server-side context resolution has one owner

A Cloud application-layer component, **`SessionContextResolver`**, owns all
context resolution:

- `ResolveAccount(email)` → authoritative BTR `UserId`/`UserName`/`RoleId`, or a
  rejection when the email is unmapped or the account is not valid.
- `ResolveTenant(locationId)` → `ServerId` via `BTR_WarehouseMapping`, failing
  explicitly for an unmapped code (no fallback — consistent with the existing
  `IssueTokenCommand` behavior).
- `ListWarehouses()` → the location→`ServerId` mapping for TD-02.

Both the resolution endpoint (TD-02) and the four BGud routes (TD-03) use this
one resolver; no controller reads `User.GetServerId()`, `User.GetUserId()`, or
JWT claims on BGud's path (OQ-001/OQ-002). The resolver reuses the existing
`IUserDal` and `IWarehouseMappingDal`; no new tenant table or vocabulary is
introduced.

## TD-05 — Actor attribution

- `POST api/barcode-registration`: the resolver resolves `X-Session-Actor` to a
  BTR `UserId`, passed as `RequestedBy` — stored in the existing
  `BTRADE_BarcodeRegistrationRequest.RequestedBy` column, preserving the current
  identifier shape.
- `POST api/return-order`: the resolver resolves `X-Session-Actor` to a BTR
  `UserId`, recorded on the relay as `SubmittedBy` (new column, §8).
- `GET api/BarcodeRegistration/status`: the resolver resolves the actor so the
  query continues to return only the caller's own requests (`RequestedBy` filter
  preserved); omitting this would change existing behavior.
- The actor value is client-supplied and not cryptographically verified — knowingly
  accepted under the approved anonymous posture (OQ-001/OQ-005); Tenant authority
  remains the separately resolved `locationId`.

## TD-06 — `[Authorize]` removal scope

`[Authorize]` is removed from the controllers serving BGud's consumed routes,
instance-wide:

- `BarcodeController` (`GET api/barcodes/sync`, `POST api/Barcode/sync`)
- `BarcodeRegistrationController` (`submit`, `status`, `pending`, `ack`)
- `ReturnOrderController` (`POST api/return-order`, `incremental`)
- `DriverController` (`GET api/Driver/{serverId}`, `POST api/Driver`)

`UserController` (`POST api/User`) keeps `[Authorize]` — it is not consumed by
BGud and `j07-btrade-sync` continues to authenticate for it. Because `[Authorize]`
is a controller-level attribute, removal is instance-wide on those four
controllers: no BGud-only exception, dual-mode, or compatibility shim is
introduced (OQ-010). Existing consumers that still send a bearer token remain
functional; the token is simply no longer required.

## TD-07 — Google OAuth configuration reuse

BGud reuses BTrade3's existing Google OAuth project and web client configuration
(OQ-007):

- BGud is registered as an Android client inside the same Google project
  (`btrade3-663be`) with BGud's `applicationId` (`com.elsasa.bgud`) and signing
  certificate fingerprint; the shared web client id
  (`405920502340-…apps.googleusercontent.com`) is used for the Sign-In options.
- `src/BGud/app/google-services.json` is added for BGud (its own Android client
  entry within the shared project); the Google Services Gradle plugin is applied
  to the BGud app module.
- This is configuration delivery within the existing project, not a new OAuth
  registration.

## TD-08 — Legacy read routes keep the `{serverId}` contract

`GET api/Brg/{serverId}`, `GET api/Customer/{serverId}`,
`GET api/SalesPerson/{serverId}`, and `GET api/Driver/{serverId}` are unchanged.
BGud supplies the `ServerId` resolved at session establishment (TD-02) and held
in the local session. No header-based tenant contract is added to them.

## TD-09 — Login-time synchronization under the session context

Login-time synchronization remains part of the sign-in flow (OQ-008). After the
session is established, BGud runs the existing ordered sync runs
(`BarcodeSyncRepository.sync`, `ReturnOrderSyncRepository.sync`) with a client
that attaches the session headers (TD-03) and **no** `Authorization` header. The
`ServerId` for legacy read routes comes from the local session. Sync failures
retain their current blocking semantics (barcode sync blocks navigation;
return-order reference failures do not).

## TD-10 — Local session model

The local session replaces the JWT-based session and stores
(`SessionPreferencesDataSource`):

| Key | Value |
| --- | ----- |
| `google_email` | signed-in Google email (session identity) |
| `user_id` | resolved BTR `UserId` |
| `user_name` | resolved BTR `UserName` |
| `role_id` | resolved BTR `RoleId` |
| `location_id` | selected Gudang `locationId` |
| `server_id` | resolved `ServerId` for the selected Gudang |

`token` and `office_code` are removed; existing sync timestamps are retained.
Session validity = `google_email` present. Old installs holding a legacy `token`
key are therefore treated as signed out and are routed to sign-in. Logout and
Gudang change clear the session; queued offline records keep their original
`locationId` binding and are never re-homed (IR-09). `SessionBinding` semantics
are preserved, comparing the queued record's `locationId` against the session's
`locationId`.

## TD-11 — Navigation gate on local session state

The navigation start destination is `home` when a valid local session exists and
`login` otherwise. No JWT or Cloud token is consulted (OQ-008).

## TD-12 — Network layer without bearer

`AuthInterceptor` is replaced by a **`SessionContextInterceptor`** that attaches
`X-Session-Location` and `X-Session-Actor` (when a session exists). The account
resolution call (TD-02) carries the email in its body, so no session headers are
required for it. `ApiClient` no longer accepts a token provider.

## TD-13 — Invalid session-context signalling

A request whose `X-Session-Actor` no longer resolves to a valid BTR user (mapping
removed mid-session) is rejected with a dedicated outcome — HTTP **409 Conflict**
with a JSend failure — distinct from a malformed request (HTTP 400 for a
missing/unmapped `locationId`). BGud treats 409 as session-ended, clears the
local session, and returns the operator to sign-in (FEATURE exception:
"Registration is removed while a session is active").

## TD-14 — Google email normalization and uniqueness

- Emails are trimmed and compared case-insensitively.
- Uniqueness is enforced twice: one `Email` column per `BTR_User` (one Google
  email per BTR user), and a filtered unique index over non-empty values (one
  BTR user per Google email). This technical enforcement realizes the OQ-004
  cardinality rule; the design point OQ-004 left to the Architect is thereby
  fixed.
- The mapping target is the **BTR user** (`BTR_User.Email`), **not** the
  existing `BTR_SalesPerson.Email` used by BTrade3. This is required by OQ-004
  (one email per BTR user; maintained in the Main Office User menu; the BTR
  `RoleId` stays authoritative) and keeps warehouse operators — who are not
  necessarily salespersons — separate from the salesperson identity model. The
  Cloud's existing `SalesPerson/exists` email lookup is BTrade3's mechanism and
  is not part of BGud's path.

## TD-15 — Return-order attribution relay

The resolved operator `UserId` (`BTRADE_ReturnOrder.SubmittedBy`) is returned by
the incremental download and relayed by `j07-btrade-sync`; the relay passes the
operator `UserId` to the existing Main Office import path so
`BTR_ReturnOrder.CreatedBy` holds the operator (identifier shape unchanged).
Legacy rows with no `SubmittedBy` fall back to the existing sync service-account
behavior. Numbering and validation remain office-side; no Main Office schema
change is required.

---

# 5. Component Responsibilities

| Component | Responsibility |
| --------- | -------------- |
| BGud `GoogleSignInGate` (`util/GoogleSignInHelper`) | Acquire the signed-in Google account email; expose sign-out of the Google account. |
| BGud `LoginScreen` / `LoginViewModel` | Present the Google Sign-In gate and Gudang selector; drive session resolution, session creation, and login-time sync; surface refusal. |
| BGud `SessionResolverRepository` (new) | Call `POST api/session/resolve`; return resolved BTR identity + warehouse mapping or a refusal. |
| BGud `SessionPreferencesDataSource` | Persist and clear the local session (TD-10). |
| BGud `SessionContextInterceptor` | Attach `X-Session-Location` / `X-Session-Actor` to Cloud requests when a session exists. |
| BGud `ApiClient` | Build the Retrofit/OkHttp client with the session-context interceptor (no token provider). |
| BGud `Navigation` | Gate the start destination on local session state. |
| BGud `SettingsViewModel` | Logout and Gudang change: clear the local session; never touch queued records. |
| BGud `BarcodeSyncRepository` / `ReturnOrderSyncRepository` / `ReturnOrderReferenceSyncRepository` | Run login-time/manual synchronization under the session context; supply actor email and `ServerId` as required. |
| Cloud `SessionController` (`POST api/session/resolve`) | Expose anonymous account resolution and warehouse mapping (TD-02). |
| Cloud `SessionContextResolver` (application) | Own account-by-email resolution, tenant-by-location resolution, and warehouse listing (TD-04). |
| Cloud `BarcodeController` / `BarcodeRegistrationController` / `ReturnOrderController` / `DriverController` | Serve the BGud-consumed routes anonymously; resolve context via `SessionContextResolver`; pass resolved values to existing commands/queries. |
| Cloud `UserController` | Continue serving the authenticated `BTRADE_User` projection for `j07-btrade-sync` (unchanged). |
| Cloud `ErrorHandlerMiddleware` | Map invalid-session-context to 409 and malformed context to 400 (TD-13). |
| Main Office `UserForm` / `UserModel` / `UserDal` / `UserBuilder` / `UserValidator` | Maintain and persist the `BTR_User.Email` Google-email mapping in the User menu. |
| `j07-btrade-sync` `UserDal` / `UserType` / `UserSyncService` | Project `BTR_User` including `Email` into `BTRADE_User`. |
| `j07-btrade-sync` return-order relay (`ReturnOrderIncrementalDownloadService`, `SyncForm`, `ReturnOrderModel`) | Carry `SubmittedBy` and pass operator attribution into the Main Office import (TD-15). |

Every implementation responsibility above has exactly one owner.

---

# 6. Integration Design

| Source | Target | Purpose |
| ------ | ------ | ------- |
| BGud `LoginScreen` | Google Sign-In (external) | Acquire the operator's Google account email. |
| BGud `SessionResolverRepository` | Cloud `SessionController` `POST api/session/resolve` | Resolve email → BTR identity; obtain warehouse mapping. |
| Cloud `SessionController` | Cloud `SessionContextResolver` | Apply the OQ-004 sign-in rule and OQ-003 mapping rule. |
| Cloud `SessionContextResolver` | Cloud `IUserDal` (`BTRADE_User.Email`) | Read the authoritative BTR user for an email. |
| Cloud `SessionContextResolver` | Cloud `IWarehouseMappingDal` (`BTR_WarehouseMapping`) | Resolve `locationId` → `ServerId`; list Gudang mappings. |
| BGud `LoginViewModel` | BGud `SessionPreferencesDataSource` | Persist the established local session. |
| BGud `SessionContextInterceptor` | Cloud four BGud routes | Carry `locationId` and actor email explicitly. |
| Cloud four BGud routes | Cloud `SessionContextResolver` | Resolve tenant and actor per request. |
| Cloud `BarcodeRegistrationSubmitCommand` | `BTRADE_BarcodeRegistrationRequest` | Record resolved `RequestedBy`. |
| Cloud `ReturnOrderUploadCommand` | `BTRADE_ReturnOrder` | Record resolved `SubmittedBy`. |
| BGud sync repositories | Cloud four routes + legacy read routes | Login-time/manual synchronization under the session context (no bearer). |
| Main Office `UserForm` | `BTR_User` | Create/modify/remove the Google-email mapping. |
| `j07-btrade-sync` `UserSyncService` | Cloud `UserController` `POST api/User` | Project `BTR_User` incl. `Email` into `BTRADE_User` (authenticated). |
| Cloud `ReturnOrderController` (incremental) | `j07-btrade-sync` | Deliver return orders including `SubmittedBy`. |
| `j07-btrade-sync` `SyncForm` | Main Office `BTR_ReturnOrder` | Relay return orders and record operator attribution. |

No business flow is described here; see FEATURE §6.

---

# 7. Data Ownership

| Data | Owner |
| ---- | ----- |
| Google email ↔ BTR user mapping (authoritative) | Main Office `BTR_User` |
| Cloud copy of the mapping (for resolution) | Cloud `BTRADE_User` (projection owned by `j07-btrade-sync`) |
| Location (`locationId`) → `ServerId` mapping | `BTR_WarehouseMapping` (Cloud resolution basis) |
| BGud local session (email, BTR identity, Gudang, `ServerId`) | BGud device (`session_preferences`) |
| Queued offline registrations / return orders and their `locationId` binding | BGud device (Room) |
| Barcode registration actor (`RequestedBy`) | Cloud `BTRADE_BarcodeRegistrationRequest` |
| Return-order actor (`SubmittedBy`) and Main Office return-order audit | Cloud `BTRADE_ReturnOrder` (relay) → Main Office `BTR_ReturnOrder` |
| Google OAuth project / client configuration | Existing Google project owned outside the repository (shared with BTrade3) |

No data is jointly owned; the Main Office remains the single source of truth for
accounts and their Google registrations.

---

# 8. Database Design

## New Tables

None.

## Modified Tables

| Table | Change |
| ----- | ------ |
| `BTR_User` (Main Office) | Add `Email VARCHAR(100) NOT NULL CONSTRAINT DF_BTR_User_Email DEFAULT('')`; add filtered unique index `UX_BTR_User_Email` on `Email` where `Email <> ''`. |
| `BTRADE_User` (Cloud) | Add `Email VARCHAR(100) NOT NULL DEFAULT('')`; add index `IX_BTRADE_User_Email`. |
| `BTRADE_ReturnOrder` (Cloud) | Add `SubmittedBy VARCHAR(50) NOT NULL DEFAULT('')` (resolved BTR `UserId` of the submitting operator). |
| `BTRADE_BarcodeRegistrationRequest` (Cloud) | No schema change — `RequestedBy` is reused for the resolved `UserId`. |
| `BTR_ReturnOrder` (Main Office) | No schema change — existing `CreatedBy` receives the operator `UserId` through the import path (TD-15). |

## Relationships

- `BTR_User.Email` is projected one-to-one into `BTRADE_User.Email`; `Email` is
  unique among non-empty values on both sides.
- `BTRADE_User.UserId` (resolved from `Email`) is recorded as
  `BTRADE_BarcodeRegistrationRequest.RequestedBy` and
  `BTRADE_ReturnOrder.SubmittedBy`.
- `BTR_WarehouseMapping.WarehouseCode` → `BTR_WarehouseMapping.ServerId` is
  reused unchanged as the resolution basis for `X-Session-Location` and for the
  resolution endpoint's warehouse list.

## Migration Considerations

- All column additions are additive with defaults and are non-breaking for
  existing consumers.
- The filtered unique index requires duplicate non-empty emails to be resolved
  before it is created; the User menu save path must reject a duplicate email
  (TD-14).
- Operators must be mapped (non-empty `Email`) before BGud rollout; an unmapped
  operator cannot sign in. Existing `BTR_User` rows receive `Email = ''` and are
  therefore simply not eligible until mapped.
- Coordinated deployment order (one release, per RISK-001): Main Office
  `BTR_User` column → `j07-btrade-sync` projection change → Cloud `BTRADE_User`
  column → Cloud API change → BGud app. The Cloud API must not require the
  headers before BGud ships (and BGud must not omit the bearer before the Cloud
  stops requiring `[Authorize]`).
- Existing BGud installs: the local session key change routes them to sign-in on
  first launch after upgrade; queued offline records and caches are untouched
  and keep their `locationId` binding.

---

# 9. Cross-Cutting Concerns

## Security

- BGud operates anonymously by deliberate policy (OQ-001/OQ-002): no login call,
  no JWT, no bearer header, and no `[Authorize]` on the BGud-consumed routes.
  No credentials (BTR password or Google ID token) are transmitted to the Cloud.
- The carried tenant (`locationId`) and actor (Google email) are client-supplied
  and not cryptographically verified — knowingly accepted; they are the
  compensating contracts for OQ-003 and OQ-005. No authorization/restoration
  path is designed (OQ-011 remains obsolete).
- `[Authorize]` removal is instance-wide on the four shared controllers; no
  BGud-only exception or dual-mode exists. `UserController` remains
  authenticated.
- Cleartext HTTP remains a general platform concern; because BGud sends no
  credentials, it does not gate this feature.

## Audit

- Actor values remain BTR `UserId`s resolved server-side
  (`RequestedBy`, `SubmittedBy`, and the Main Office return-order audit), so
  existing audit consumers keep the current identifier shape.
- Main Office return-order numbering and validation remain office-side
  unchanged.

## Logging and Observability

- The session-context headers and the resolution request contain a personal
  email; logging must not add these values at informational level, consistent
  with the existing rule that the `Authorization` header is never logged.
- Resolution failures (unmapped location, unknown actor) are logged as
  `Bad Request` / `Conflict` outcomes by the existing error middleware.

## Performance and Concurrency

- Resolution adds one indexed lookup per BGud request
  (`BTRADE_User.Email`, `BTR_WarehouseMapping.WarehouseCode`).
- Existing sync concurrency rules are unchanged (one run at a time,
  `ExistingWorkPolicy.KEEP`; no periodic work).

## Error Semantics

- Missing or unmapped `X-Session-Location` → HTTP 400 (malformed context).
- Unresolvable `X-Session-Actor` (mapping removed/validity lost) → HTTP 409
  (session invalid); BGud clears the session and returns to sign-in.
- Unmapped Google account at `POST api/session/resolve` → HTTP 400 (sign-in
  refused; operator advised to contact an administrator).

---

# 10. Implementation Constraints

- No JWT, bearer header, login call, or `[Authorize]` may be reintroduced on
  BGud's request path (OQ-001/OQ-002).
- BGud must never send `ServerId` as a command input on the four session-context
  routes; it sends `locationId` in `X-Session-Location`. `ServerId` is supplied
  only in the existing legacy `{serverId}` path routes.
- Header names are fixed: `X-Session-Location`, `X-Session-Actor`.
- The Cloud must resolve tenant and actor through the single
  `SessionContextResolver`; controllers must not read JWT claims or
  `User.GetServerId()`/`User.GetUserId()` on BGud's path.
- `BTR_WarehouseMapping` remains the authoritative `locationId`→`ServerId`
  source; no new tenant table or client-side tenant vocabulary is introduced.
- Main Office remains the single source of truth for accounts and Google-email
  registration; no second identity store is created.
- Persistence stack is unchanged: Dapper/SQL Server only, no Entity Framework.
- HTTP contract conventions are unchanged: REST, `JSendOk`/`JSend` envelope,
  PascalCase payload fields, 30-second timeouts.
- The Cloud base URL remains a single composition-root value in BGud; no
  environment URL is hardcoded in the network layer.
- Google Sign-In follows BTrade3's implemented pattern and reuses the shared
  OAuth project via `google-services.json` + Play Services Auth; no separate
  OAuth registration.
- Room/DataStore migrations must be non-destructive; queued records retain their
  original `locationId` and are never re-homed.
- Client and Cloud ship as one coordinated release; the affected shared routes
  must be verified against known non-BGud consumers (`BTrade3`,
  `j07-btrade-sync`) before deployment; a discovered dependency is handled as a
  new ISSUE, not as a BGud-specific authorization exception.

---

# 11. Acceptance Conditions

Implementation is architecturally complete when:

1. A BGud operator can sign in with a Google account registered to an active BTR
   user, select one Gudang (Gamping/Concat/Magelang), and reach Home; an
   unregistered Google account is refused with an administrator message.
2. No `api/Auth/login` call, JWT, `Authorization` header, or server-side session
   token is used by BGud.
3. The local session stores the Google email, resolved BTR `UserId`/`UserName`/
   `RoleId`, `locationId`, and resolved `ServerId`; the navigation gate is based
   on local session state; logout and Gudang change clear the session.
4. The four BGud-consumed routes carry `X-Session-Location` and (where
   applicable) `X-Session-Actor`, resolve tenant/actor server-side, and require
   no authentication; `api/User` remains authenticated.
5. Barcode registrations record the resolved BTR `UserId` as `RequestedBy`;
   return orders record it as `SubmittedBy` and it reaches the Main Office
   return-order audit; `GET api/BarcodeRegistration/status` still returns only
   the caller's own requests.
6. Login-time barcode-registry and return-order synchronization run under the
   session context without a bearer header; legacy `{serverId}` read routes
   continue to work with the session's resolved `ServerId`.
7. The Main Office User menu can create/modify/remove a Google email for a BTR
   user; the mapping is projected to the Cloud; uniqueness is enforced.
8. A session whose actor mapping disappears is rejected (409) and BGud returns
   the operator to sign-in; queued records keep their original `locationId`.
9. Existing non-BGud consumers (BTrade3, `j07-btrade-sync`) continue to function
   against the revised contract.
