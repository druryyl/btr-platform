---
Title: BGud Google Sign-In — Deployment Guideline
Code: BGUD-GOOGLE-SIGNIN-001
Artifact: DEPLOYMENT-GUIDELINE
Version: 1.0
LastUpdated: 2026-09-24
---

# 1. Purpose

This document is a step-by-step deployment guide for the BGud Google Sign-In
feature implemented under ISSUE `BGUD-GOOGLE-SIGNIN-001`
(`docs/issues/BGUD-GOOGLE-SIGNIN-ISSUE.md`).

It is written for a deployment team with **no prior knowledge of the
implementation**. Each section lists what must be done, in what order, and how to
verify it.

## Referenced Documents

| Document | Purpose |
|----------|---------|
| `docs/work/bgud-google-signin/BGUD-GOOGLE-SIGNIN-ARCHITECTURE.md` | Technical design, data model, component responsibilities, error semantics. |
| `docs/work/bgud-google-signin/BGUD-GOOGLE-SIGNIN-IMPLEMENTATION-PLAN.md` | Slice-level implementation notes and file lists. |
| `docs/work/bgud-google-signin/BGUD-GOOGLE-SIGNIN-FEASIBILITY-ASSESSMENT.md` | Decision rationale and risk context. |
| `docs/features/bgud-operator-signin/feature.md` | Business acceptance criteria. |

---

# 2. Overview of the Change

The BGud mobile app's login changes from a username/password form that calls
`POST api/Auth/login` (JWT returned) to a **Google Sign-In gate that operates
locally on the device**. BGud no longer transmits credentials to the Cloud API.

The Cloud endpoints BGud consumes (`barcodes/sync`, `barcode-registration`,
`BarcodeRegistration/status`, `return-order`, `Driver`) are now **anonymous** —
`[Authorize]` is removed instance-wide. Instead, each BGud request carries two
explicit headers:

| Header | Value | Purpose |
|--------|-------|---------|
| `X-Session-Location` | `locationId` (`GAMPING`/`CONCAT`/`MAGELANG`) | Identifies the Gudang tenant. Resolved server-side to `ServerId`. |
| `X-Session-Actor` | Google account email | Identifies the operator. Resolved server-side to BTR `UserId` for attribution. |

This is a **single coordinated release** across Main Office, Cloud API,
synchronization service, and BGud app. Partial deployment (e.g., Cloud still
requires `[Authorize]` while BGud stops sending bearer tokens) will cause a
**complete operational outage** (RISK-001). The deployment order in §7
encodes the safe sequence.

---

# 3. Database Changes

## 3.1 New Tables

**None.** No new tables are created.

## 3.2 Modified Tables

### 3.2.1 Main Office — `BTR_User`

Add a Google-email column and a filtered unique index.

| Object | SQL |
|--------|-----|
| Column | `Email VARCHAR(100) NOT NULL CONSTRAINT DF_BTR_User_Email DEFAULT('')` |
| Index | `UX_BTR_User_Email` — filtered unique index on `Email` where `Email <> ''` |

**File:** `src/j05-btr-distrib/btr.sql/Tables/Helper/BTR_User.sql`

**Idempotent upgrade script** (applies to an existing database, safe to re-run):
`src/j05-btr-distrib/Scripts/Upgrade_*.sql` (naming follows the existing
repository convention; the implementation plan P1-S01 used this pattern).

**Effect on existing rows:** All existing `BTR_User` rows receive `Email = ''`
(empty string). Existing users are unaffected; they simply are not eligible for
BGud sign-in until an email is mapped.

### 3.2.2 Cloud — `BTRADE_User`

| Object | SQL |
|--------|-----|
| Column | `Email VARCHAR(100) NOT NULL CONSTRAINT DF_BTRADE_User_Email DEFAULT('')` |
| Index | `IX_BTRADE_User_Email` — non-unique index on `Email` |

**File:** `src/j06-pkl-btrade-api/btrade.sqldb/BarcodeContext/BTRADE_User.sql`

**Idempotent upgrade script:**
`src/j06-pkl-btrade-api/Scripts/Upgrade_Google_SignIn.sql` (registered in
`btrade.sqldb.sqlproj`).

### 3.2.3 Cloud — `BTRADE_ReturnOrder`

| Object | SQL |
|--------|-----|
| Column | `SubmittedBy VARCHAR(50) NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_SubmittedBy DEFAULT('')` |

**File:** `src/j06-pkl-btrade-api/btrade.sqldb/ReturnOrderContext/BTRADE_ReturnOrder.sql`

The same `Scripts/Upgrade_Google_SignIn.sql` script applies this column.

## 3.3 Required Indexes, Constraints, or Migrations

| Constraint | Where | Enforcement |
|------------|-------|-------------|
| Filtered unique index `UX_BTR_User_Email` | Main Office `BTR_User` | Database-level — prevents two BTR users from sharing the same non-empty Google email. |
| Index `IX_BTRADE_User_Email` | Cloud `BTRADE_User` | Database-level — supports fast email-lookup at session resolution. |
| Default constraint `DF_BTR_User_Email` | Main Office `BTR_User` | Application-level — ensures new rows get `Email = ''`. |
| Default constraint `DF_BTRADE_User_Email` | Cloud `BTRADE_User` | Application-level — ensures new rows get `Email = ''`. |
| Default constraint `DF_BTRADE_ReturnOrder_SubmittedBy` | Cloud `BTRADE_ReturnOrder` | Application-level — ensures legacy rows get `SubmittedBy = ''`. |
| Application-level duplicate check | Main Office User menu | The `UserValidator` rejects a save that would duplicate a non-empty email on another user. |

**Migrations to run before or during deployment:**

1. **`btr.sql` upgrade script** — adds the `Email` column, default constraint, and
   `UX_BTR_User_Email` index to the Main Office database. This must run **before**
   the User menu UI is used to map operators.

2. **`btrade.sqldb` upgrade script** — adds `Email` + `IX_BTRADE_User_Email` to
   `BTRADE_User`, and `SubmittedBy` + default constraint to `BTRADE_ReturnOrder`.
   This must run **before** the Cloud API is deployed with the new resolution
   endpoint and anonymous routes.

---

# 4. Data Seeding

## 4.1 Mandatory Seed Data

**None is auto-seeded.** The feature relies on two pre-existing data sources that
require no new seed records:

| Source | Existing State | Action Required |
|--------|----------------|-----------------|
| `BTR_WarehouseMapping` (Cloud) | Already seeded: `GAMPING`→`JOGJA`, `CONCAT`→`JOGJA`, `MAGELANG`→`MGL` | None — reused as-is. |
| `BTR_DEPT` / `BTR_UserRole` | Already seeded | None. |

## 4.2 Required Manual Data Entry (Pre-Rollout)

### 4.2.1 Operator-to-Google Email Mapping

**This is a mandatory pre-deployment step.** Warehouse operators must be mapped to
their Google account emails in the Main Office before BGud rollout. An unmapped
Google account is **rejected** at sign-in (HTTP 403/400 on
`POST api/session/resolve`).

**Who performs it:** Any authorized BTR user with access to the Main Office
**User** menu.

**How:**
1. Open the Main Office application.
2. Navigate to **User** → **User Management**.
3. For each warehouse operator:
   - Open the operator's user record.
   - Enter the operator's Google account email in the new **Google Email** field
     (e.g., `operator@example.com`).
   - Save.
4. The duplicate-check (§3.3) prevents two users from sharing the same email.
   Empty email (unmapped) is valid for non-BGud users.

**Verification:** After mapping, the email should appear in the User form. The
sync service (`j07-btrade-sync`) must run once to project the email to the Cloud
`BTRADE_User` table.

## 4.3 Default Configuration Values

| Component | Setting | Default |
|-----------|---------|---------|
| Cloud session resolution | HTTP status for unmapped account | `400 Bad Request` (JSend failure) |
| Cloud error mapping | Missing/unmapped `X-Session-Location` | `400 Bad Request` |
| Cloud error mapping | Unresolvable `X-Session-Actor` | `409 Conflict` (session-ended) |
| BGud session validity | Determined by | `google_email` key present and non-blank |

---

# 5. API Deployment

## 5.1 New APIs

### 5.1.1 `POST api/session/resolve`

**Purpose:** Anonymous endpoint that resolves a Google account email to the
authoritative BTR user and returns the warehouse mapping.

**Controller:** `SessionController` (`src/j06-pkl-btrade-api/btrade.webapi/Controllers/SessionController.cs`)

**Contract:**
```
POST api/session/resolve
Request:  { "Email": "operator@gmail.com" }
Success (200): JSendOk data {
    "UserId": "USR001",
    "UserName": "Operator Name",
    "RoleId": "WH",
    "Warehouses": [
        { "LocationId": "GAMPING", "ServerId": "JOGJA" },
        { "LocationId": "CONCAT",  "ServerId": "JOGJA" },
        { "LocationId": "MAGELANG", "ServerId": "MGL" }
    ]
}
Rejected (400): { "status": "error", "message": "Bad Request", ... }
  — returned for unmapped or inactive Google accounts.
```

**Attributes:** No `[Authorize]`. No token issued. No server-side session state.

**Depends on:** `BTRADE_User.Email` column (P1-S02) and `BTR_WarehouseMapping`
(P3-S04).

## 5.2 Modified APIs

### 5.2.1 Controllers Losing `[Authorize]` (Instance-Wide)

The following controllers have **controller-level** `[Authorize]` removed
instance-wide — there is no BGud-only exception:

| Controller | File | Routes Affected |
|------------|------|-----------------|
| `BarcodeController` | `btrade.webapi/Controllers/BarcodeController.cs` | `GET api/barcodes/sync` (now header-resolved), `POST api/Barcode/sync` (keeps claim-based tenant for token-bearing consumers) |
| `BarcodeRegistrationController` | `btrade.webapi/Controllers/BarcodeRegistrationController.cs` | `POST api/barcode-registration` (now header-resolved `RequestedBy`), `GET api/BarcodeRegistration/status` (now header-resolved), `pending`/`ack` (keep claim-based behavior for token-bearing consumers) |
| `ReturnOrderController` | `btrade.webapi/Controllers/ReturnOrderController.cs` | `POST api/return-order` (now header-resolved), `GET api/ReturnOrder/incremental` (unchanged auth, now returns `SubmittedBy`) |
| `DriverController` | `btrade.webapi/Controllers/DriverController.cs` | `GET api/Driver/{serverId}` (now anonymous, keeps route contract), `POST api/Driver` (keeps body-bound behavior for token-bearing consumers) |

### 5.2.2 Header-Resolved Context on BGud Routes

The three BGud-consumed barcode routes and the return-order submit route now
read session context from headers:

| Route | Reads `X-Session-Location` | Reads `X-Session-Actor` | Action |
|-------|---------------------------|------------------------|--------|
| `GET api/barcodes/sync` | Yes | No | Resolves `ServerId` for the Gudang |
| `POST api/barcode-registration` | Yes | Yes | Resolves `ServerId` + `RequestedBy` |
| `GET api/BarcodeRegistration/status` | Yes | Yes | Resolves `ServerId` + `RequestedBy` (own-requests filter preserved) |
| `POST api/return-order` | Yes | Yes | Resolves `ServerId` + `SubmittedBy` |

### 5.2.3 `GET api/ReturnOrder/incremental` — Now Returns `SubmittedBy`

The incremental download response now includes a `SubmittedBy` field (the
resolved operator `UserId`), serialized in **PascalCase**. The only consumer,
`j07-btrade-sync`, deserializes case-insensitively and requires no change.

### 5.2.4 `POST api/User` — Accepts `Email` in Payload

The user projection ingest endpoint now accepts an `Email` field in the request
payload. When the field is absent (legacy `j07-btrade-sync` still deploying), it
defaults to `''`.

## 5.3 Unchanged APIs

| API | Status |
|-----|--------|
| `POST api/User` (authenticated) | **Keeps `[Authorize]`** — not consumed by BGud. |
| `GET api/Brg/{serverId}`, `GET api/Customer/{serverId}`, `GET api/SalesPerson/{serverId}` | Anonymous, unchanged contract. BGud supplies the `ServerId` from its local session. |
| `POST api/Order`, `POST api/CheckIn`, etc. (BTrade3 routes) | Unchanged. |

## 5.4 Dependency and Integration Changes

### 5.4.1 `j07-btrade-sync` — User Projection Extended

The `BTR_User → BTRDE_User` projection now includes the `Email` column:
- `Repository/UserDal.cs` `ListData()` selects `Email`.
- `Model/UserType.cs` carries `Email`.
- The payload posted to `POST api/User` now includes `Email` automatically.

The sync service **keeps its JWT** — it authenticates against `POST api/User`
which remains `[Authorize]`d. The bearer token header is intentionally not
removed from this caller.

### 5.4.2 `j07-btrade-sync` — Return-Order Attribution Relay

The return-order incremental download now includes `SubmittedBy`, which
`j07-btrade-sync` relays to the Main Office:
- `Model/ReturnOrderModel.cs` gains `SubmittedBy` (deserialized case-insensitively).
- `Repository/ReturnOrderDal.cs` `Insert` stamps `CreatedBy` from `SubmittedBy`
  when non-empty; otherwise falls back to the existing service-account identity.
- Legacy rows without `SubmittedBy` are unaffected (fall back to service-account).

---

# 6. Application Configuration

## 6.1 New Settings

### 6.1.1 BGud — Google Services Configuration

A new file is deployed with the BGud app:

**`src/BGud/app/google-services.json`**

This file registers BGud as an Android client within the **existing shared
Google OAuth project `btrade3-663be`** (the same project BTrade3 uses). It is
**not** a new OAuth registration.

| Field | Value |
|-------|-------|
| `project_id` | `btrade3-663be` |
| `package_name` | `com.elsasa.bgud` |
| `web_client_id` (type 3) | `405920502340-odieer196drj8fd5jinppg7hnj8s8bpa.apps.googleusercontent.com` (shared with BTrade3) |

**External prerequisite (EXT-01):** The Google Console `btrade3-663be` project
must have an Android client entry registered for:
- Package name: `com.elsasa.bgud`
- Signing certificate fingerprint: BGud's release/debug `SHA1`

Until this external registration is complete, runtime Google Sign-In will fail
on actual devices. The app compiles and deploys with placeholder values, but
cannot complete sign-in at runtime.

### 6.1.2 BGud — Gradle Plugin and Dependency

| File | Change |
|------|--------|
| `build.gradle.kts` (root) | `buildscript { dependencies { classpath("com.google.gms:google-services:4.3.15") } }` |
| `app/build.gradle.kts` | `apply plugin: id("com.google.gms.google-services")` ; `implementation(libs.play.services.auth)` |
| `gradle/libs.versions.toml` | Declares `playServicesAuth = "20.7.0"` and the `play-services-auth` library |

### 6.1.3 Cloud — DI Registration

The `SessionContextResolver` is registered in the Cloud application's DI
container:

| File | Change |
|------|--------|
| `btrade.webapi/Configurations/ApplicationService.cs` | `services.AddScoped<SessionContextResolver>()` (explicit registration, not auto-discovered by Scrutor scans) |

### 6.1.4 Cloud — Shared Header Constants

| File | Purpose |
|------|---------|
| `btrade.webapi/Infrastructure/SessionContextHeaders.cs` (new) | Defines the fixed header names `X-Session-Location` and `X-Session-Actor` and exposes `HttpRequest.GetSessionLocation()` / `HttpRequest.GetSessionActor()` extension methods. |

## 6.2 Modified Settings

### 6.2.1 BGud — Network Client (Breaking Change)

| File | Before | After |
|------|--------|-------|
| `network/ApiClient.kt` | `create(baseUrl, tokenProvider, enableLogging)` — accepts a bearer token provider | `create(baseUrl, sessionContextProvider, onSessionInvalidated, enableLogging)` — no token provider, attaches session headers |
| `network/AuthInterceptor.kt` | Adds `Authorization: Bearer <token>` to every request | **Deleted** — replaced by `SessionContextInterceptor.kt` |
| `network/SessionContextInterceptor.kt` (new) | — | Attaches `X-Session-Location` and `X-Session-Actor` when a session exists; never sends `Authorization` |

### 6.2.2 BGud — Session Storage (Breaking Change)

| File | Before | After |
|------|--------|-------|
| `datastore/SessionPreferencesDataSource.kt` | Stores `token`, `user_id`, `warehouse_code`, `office_code` | Stores `google_email`, `user_id`, `user_name`, `role_id`, `location_id`, `server_id` |
| `datastore/SessionPreferences.kt` (new) | — | Central definition of the six session keys and validity rule |
| `datastore/SessionBinding.kt` | Binds to `warehouseCode` | Binds to `locationId` (IR-09 — queued records never re-homed) |

### 6.2.3 BGud — Navigation Gate (Breaking Change)

| File | Before | After |
|------|--------|-------|
| `ui/Navigation.kt` | Start destination = `home` if `token` exists | Start destination = `home` if `google_email` (valid session) exists |

### 6.2.4 BGud — Login Flow (Breaking Change)

| File | Before | After |
|------|--------|-------|
| `ui/screen/LoginScreen.kt` | Username, Password, Gudang selector, Login button | Google Sign-In button + retained three-Gudang selector |
| `viewmodel/LoginViewModel.kt` | Posts to `POST api/Auth/login` | Google Sign-In → resolve email via `POST api/session/resolve` → persist session → login-time sync |
| `network/BtradeApiService.kt` | Declares `api/Auth/login`, `LoginRequest`, `LoginResult` | Declares `POST api/session/resolve`, `SessionResolveRequest`, `SessionResolveResult`, `WarehouseDto` |

## 6.3 Existing Settings (Unchanged)

| Setting | Component | Status |
|---------|-----------|--------|
| Cloud base URL | BGud `ApiClient` | Unchanged — single composition-root value |
| `BTR_WarehouseMapping` data | Cloud database | Unchanged — existing seed data reused |
| `UserController` `[Authorize]` | Cloud API | Unchanged — keeps authentication |
| BTrade3 OAuth web client id | Google project `btrade3-663be` | Unchanged — shared project reused |
| JWT token service / 480-minute lifetime | Cloud API | **Unchanged** — still active for `UserController` and `j07-btrade-sync`; simply not used on BGud's path |

## 6.4 Required Secrets, Credentials, and External Service Configurations

| Resource | Owner | Action |
|----------|-------|--------|
| Google OAuth project `btrade3-663be` | External (Google Cloud Console) | Register BGud Android client (package + SHA1 fingerprint). See §6.1.1. |
| BGud signing certificate | BGud build team | Provide the SHA1 fingerprint for the release/debug keystore to the Google project owner. |
| Cloud API secrets | Cloud operations | None new — the feature introduces no new secrets. The Google ID token is **never** sent to the Cloud; BGud operates anonymously. |
| `j07-btrade-sync` service-account JWT | Sync operations | Unchanged — the sync service keeps its existing JWT for `POST api/User` and return-order downloads. |

---

# 7. Infrastructure Requirements

## 7.1 New Services

**None.**

## 7.2 Additional Storage

**None.** All columns are added to existing tables with defaults. No new
database, file store, or blob container is required.

## 7.3 Queues and Caches

**None.**

## 7.4 External Dependencies

| Dependency | Details | Action Required |
|------------|---------|-----------------|
| Google OAuth project `btrade3-663be` | Shared with BTrade3. BGud registered as an Android client. | External — register BGud package + SHA1 in the Google Console. |
| Cloud API host | `dev.smart-ics.com:8089` (cleartext HTTP) | No change — existing host. BGud sends no credentials, so the cleartext transport constraint (RISK-004) does not gate this feature. |
| Main Office database | SQL Server (`btr.sql`) | Run the upgrade script (§3.3). |
| Cloud database | SQL Server (`btrade.sqldb`) | Run the upgrade script (§3.3). |

## 7.5 Network, Firewall, and DNS Changes

**None.** The Cloud API host, ports, and DNS are unchanged.

---

# 8. Deployment Order

> **Critical:** Follow this exact order. The Cloud must not require session
> headers before BGud ships, and BGud must not omit the bearer token before the
> Cloud stops requiring `[Authorize]`.

| Step | Component | Action | Gate for next step |
|------|-----------|--------|---------------------|
| 1 | Main Office database | Run `Upgrade_*.sql` to add `BTR_User.Email` + index. | Column exists. |
| 2 | Main Office application | Deploy User-menu UI changes (email field + duplicate check). | UI available. |
| 3 | `j07-btrade-sync` | Deploy projection extension (reads `Email`, posts to `POST api/User`). | Sync service includes `Email`. |
| 4 | Cloud database | Run `Upgrade_Google_SignIn.sql` to add `BTRADE_User.Email` + index and `BTRADE_ReturnOrder.SubmittedBy`. | Columns exist. |
| 5 | Cloud API | Deploy `SessionContextResolver`, `SessionController` (`POST api/session/resolve`), `[Authorize]` removal on four controllers, header-based context resolution, `SubmittedBy` in incremental download, error-mapping middleware (400/409). | API serves anonymous contract + resolution endpoint. |
| 6 | `j07-btrade-sync` | Deploy return-order attribution relay (`SubmittedBy` → `CreatedBy` mapping). | Attribution relay functional. |
| 7 | Google Console (external) | Register BGud Android client in `btrade3-663be` (package + SHA1). | **Prerequisite** for runtime sign-in. |
| 8 | BGud app | Deploy new APK to Play Store / test devices. | App ships. |

**Parallel-safe notes:**
- Steps 5 and 6 may deploy concurrently (different code paths; middleware change
  in step 5 is reused by step 6).
- Step 7 (external Google registration) must complete before runtime verification
  of BGud sign-in, but does not block any code deployment.
- Steps 2 and 3 may be performed by different teams in parallel after step 1.

---

# 9. Deployment Validation

## 9.1 Pre-Deployment Checklist

- [ ] Main Office database upgrade script ran successfully (`Email` column +
      `UX_BTR_User_Email` index on `BTR_User`).
- [ ] Cloud database upgrade script ran successfully (`Email` + `IX_BTRADE_User_Email`
      on `BTRADE_User`; `SubmittedBy` on `BTRADE_ReturnOrder`).
- [ ] Operator Google-email mapping is populated in the Main Office User menu
      for at least one test operator.
- [ ] `j07-btrade-sync` has run at least once after step 3 so the test
      operator's email is projected to `BTRADE_User`.
- [ ] Google Console `btrade3-663be` has an Android client entry for
      `com.elsasa.bgud` with the correct SHA1 fingerprint.
- [ ] BGud app compiles (`assembleDebug` succeeds) with `google-services.json`
      in place.

## 9.2 Post-Deployment Verification Steps

### V-01. Cloud API — Session Resolution Endpoint

1. `POST api/session/resolve` with `{ "Email": "<mapped-operator-email>" }`.
2. **Expected:** HTTP 200, JSendOk with `UserId`, `UserName`, `RoleId`, and
   `Warehouses` array (three entries: `GAMPING`/`JOGJA`, `CONCAT`/`JOGJA`,
   `MAGELANG`/`MGL`).
3. `POST api/session/resolve` with `{ "Email": "unmapped@example.com" }`.
4. **Expected:** HTTP 400, JSend failure.

### V-02. Cloud API — Anonymous Access to BGud Routes

1. `GET api/barcodes/sync` with `X-Session-Location: GAMPING` (no bearer token).
2. **Expected:** HTTP 200, returns barcode data for the resolved `ServerId`.
3. `GET api/barcodes/sync` with no `X-Session-Location` header.
4. **Expected:** HTTP 400 (malformed context).
5. `GET api/barcode-registration/status` with `X-Session-Location: GAMPING` and
   `X-Session-Actor: <mapped-email>` (no bearer token).
6. **Expected:** HTTP 200, returns only the actor's own requests.
7. `GET api/barcodes/sync` with a bearer token still works for BTrade3-style
   consumers (token is accepted but not required).

### V-03. Cloud API — Actor Attribution

1. `POST api/barcode-registration` with `X-Session-Location: GAMPING` and
   `X-Session-Actor: <mapped-email>`.
2. Submit a registration.
3. **Expected:** The registration record's `RequestedBy` equals the mapped BTR
   `UserId`.
4. `POST api/return-order` with the same headers.
5. **Expected:** The return-order record's `SubmittedBy` equals the mapped BTR
   `UserId`.

### V-04. Cloud API — 409 Session-Ended

1. `POST api/barcode-registration` with `X-Session-Actor: <email whose mapping
   was deleted or user deactivated>`.
2. **Expected:** HTTP 409 Conflict (JSend failure). BGud should clear its local
   session on this response.

### V-05. `j07-btrade-sync` — Projection and Relay

1. Run the sync service once.
2. Verify the `BTRADE_User` table now contains the operator's `Email`.
3. Submit a return order via BGud.
4. Run the sync download.
5. **Expected:** `BTR_ReturnOrder.CreatedBy` equals the operator's `UserId`
   (relayed from `SubmittedBy`).

### V-06. BGud App — Sign-In Flow

1. Launch the BGud app on a test device.
2. **Expected:** The login screen shows the Google Sign-In button and the
   three-Gudang selector (no username/password fields).
3. Tap the Google Sign-In button.
4. Sign in with a mapped Google account.
5. Select a Gudang (e.g., Gamping).
6. **Expected:** Navigation proceeds to Home; barcode sync and return-order
   reference sync run silently.
7. Sign in with an **unmapped** Google account.
8. **Expected:** Sign-in is refused with an administrator-visible message.

### V-07. BGud App — Legacy Consumer Verification

1. Verify `j07-btrade-sync` can still call `POST api/User` (still authenticated).
2. **Expected:** HTTP 200 (token accepted, `Email` projected).
3. Verify BTrade3 can still call its `POST api/Barcode/sync` and
   `POST api/Order` routes.
4. **Expected:** All unchanged and functional.

## 9.3 Smoke Test Scenarios

| # | Scenario | Expected Outcome |
|---|----------|-------------------|
| S-01 | Operator signs in with Google, selects Gamping Gudang | Reaches Home screen; local session stored with all six keys; no `token` key present. |
| S-02 | Operator signs in with Google, selects Concat Gudang | Reaches Home; `ServerId` resolves to `JOGJA` (Concat distinct from Gamping). |
| S-03 | Operator signs in with unmapped Google account | Sign-in refused; administrator message shown; no session created. |
| S-04 | Operator signs in, then logs out | Local session cleared; returns to login screen. Queued offline records are untouched. |
| S-05 | Operator signs in, then changes Gudang | Local session updated with new Gudang; returns to Home. Queued offline records retain their original `locationId` binding. |
| S-06 | Barcode registration submitted by BGud | `RequestedBy` in `BTRADE_BarcodeRegistrationRequest` equals the resolved BTR `UserId`. |
| S-07 | Return order submitted by BGud | `SubmittedBy` in `BTRDE_ReturnOrder` equals the resolved BTR `UserId`; after sync, `BTR_ReturnOrder.CreatedBy` on Main Office equals the same. |
| S-08 | Actor mapping removed mid-session; operator performs a write | Cloud returns HTTP 409; BGud clears local session; operator returned to sign-in. |
| S-09 | `GET api/BarcodeRegistration/status` called | Returns only the calling operator's own requests. |
| S-10 | Legacy `GET api/Driver/{serverId}` called | Works anonymously; uses the session's resolved `ServerId`. |

## 9.4 Rollback Procedure

If a critical issue is discovered after deployment:

1. **BGud app:** Roll back to the previous APK (reverts to username/password login).
   Legacy `token`/`office_code` session keys will be re-created on next login.
2. **Cloud API:** Redeploy the previous Cloud API build (restores `[Authorize]` on the
   four controllers and the old JWT-tenant-resolved routes). This is safe because
   the old BGud app still sends bearer tokens.
3. **Databases:** The schema changes are **additive and backward-compatible** —
   no rollback of columns or indexes is strictly required. If desired, the
   `Scripts/Upgrade_Google_SignIn.sql` script's changes can be reverted by
   dropping the added columns/indexes, but this is **not necessary** for a
   code-only rollback.
4. **Sync service:** Redeploy the previous `j07-btrade-sync` build.

**Data note:** Any `Email` values entered in the Main Office User menu or
`SubmittedBy`/`RequestedBy` audit values written during the rollout window
remain in the database after rollback. They are simply not consumed until the
feature is redeployed.

---

# 10. Acceptance Criteria Summary

The deployment is successful when all of the following are true:

1. A BGud operator can sign in with a Google account registered to an active BTR
   user, select one Gudang (Gamping/Concat/Magelang), and reach Home; an
   unregistered Google account is refused.
2. No `api/Auth/login` call, JWT, `Authorization` header, or server-side session
   token is used by BGud.
3. The local session stores `google_email`, `user_id`, `user_name`, `role_id`,
   `location_id`, and `server_id`; navigation is gated on local session state;
   logout and Gudang change clear the session.
4. The four BGud-consumed routes carry `X-Session-Location` and (where applicable)
   `X-Session-Actor`, resolve tenant/actor server-side, and require no
   authentication. `api/User` remains authenticated.
5. Barcode registrations record the resolved BTR `UserId` as `RequestedBy`; return
   orders record it as `SubmittedBy` and it reaches the Main Office return-order
   audit. `GET api/BarcodeRegistration/status` returns only the caller's own
   requests.
6. Login-time barcode-registry and return-order synchronization run under the
   session context without a bearer header; legacy `{serverId}` read routes
   continue to work with the session's resolved `ServerId`.
7. The Main Office User menu can create/modify/remove a Google email for a BTR
   user; the mapping is projected to the Cloud; uniqueness is enforced.
8. A session whose actor mapping disappears is rejected (409) and BGud returns
   the operator to sign-in; queued records keep their original `locationId`.
9. Existing non-BGud consumers (BTrade3, `j07-btrade-sync`) continue to function
   against the revised contract.
