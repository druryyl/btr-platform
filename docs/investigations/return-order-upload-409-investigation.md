# BUG-INVESTIGATION

Code: BTR-RETUR-SESSION-ACTOR-001
Artifact: BUG-INVESTIGATION
Type: BUG
Date: 2026-10-17
Status: COMPLETED

## Context

Issue: Failed Return Order upload from the j09 warehouse desktop client
(`src/j09-btr-retur-gudang/BtrReturGudang.sln`) to the Cloud staging relay
(`src/j06-pkl-btrade-api`). No formal ISSUE artifact was raised for this defect;
the investigation is based on the reported operational error and repository
evidence.

Problem Summary: When uploading a Return Order, the Cloud relay rejected the
request with:

> Failed to upload Return Order '01M53D0977E2SZQBY6ERPDQAHM': HTTP 409:
> {"status":"Conflict","code":"409","data":"Google account is not registered or
> is not active. Contact your administrator."}.

The rejected document remained locally in `Draft` state.

## Current State

1. The operator works in the j09 Windows desktop client and saves a Return Order
   with "Simpan & Upload" (`F10`) or "Upload Ulang".
2. The client posts the document to `POST api/return-order` on
   `j06-pkl-btrade-api`, attaching workstation session headers
   `X-Session-Location` and `X-Session-Actor`.
3. The Cloud relay resolves the session context from those headers rather than
   from a JWT, and rejects the request with **HTTP 409 Conflict** and the message
   "Google account is not registered or is not active. Contact your administrator.".
4. The desktop client surfaces the rejection (diagnostics added in a prior fix)
   and leaves the document `Draft` for retry.
5. Live registry inspection of a workstation showed `LastOperatorName` is a
   free-text operator display name (and can be empty); no Google-email value is
   stored or transmitted.

## Problem Analysis

### Finding 1 — The Cloud actor contract now requires a Google email

Commit `af966a47` ("Ubah login BGud menggunakan Google sign-in") changed the
shared Cloud controllers to resolve session context from headers under the BGud
Google sign-in contract:

- `btrade.webapi/Controllers/ReturnOrderController.cs` — `Submit` now calls
  `_sessionContextResolver.ResolveTenant(Request.GetSessionLocation())` (line 35)
  and `_sessionContextResolver.ResolveAccount(Request.GetSessionActor())` (line 37).
  `[Authorize]` is removed and no JWT claim is read on this path.
- `btrade.application/UseCase/SessionContextResolver.cs` — `ResolveAccount` treats
  the `X-Session-Actor` value as **the Google email** and looks it up against
  `BTRADE_User.Email` (`IUserDal.GetByEmail`). If the email is blank, absent, or
  not matched to an active user, it throws `SessionAccountUnresolvableException`
  with the exact observed message (lines 23–24, 44–58).
- `btrade.infrastructure/BarcodeFeature/UserDal.cs` — `GetByEmail` queries
  `WHERE Email = @Email` on `BTRADE_User`; a blank email never matches (lines 78–101).
- `btrade.webapi/Middlewares/ErrorHandlerMiddleware.cs` — maps
  `SessionAccountUnresolvableException` to **HTTP 409 Conflict** (lines 36–41).
- These header-resolved routes are also documented in the approved BGud feature
  (`docs/features/bgud-operator-signin/feature.md`) and deployment guideline
  (`docs/work/bgud-google-signin/DEPLOYMENT-GUIDELINE.md` §5.2.2, which lists
  `POST api/return-order` as reading `X-Session-Actor`).

Conclusion (supported by evidence): the 409 is the Cloud's intended response when
the carried actor cannot be resolved to a BTR user through `BTRADE_User.Email`.

### Finding 2 — The j09 client still sends an operator name, not a Google email

The desktop client was built against the earlier contract and was not updated:

- `BtrReturGudang.Infrastructure/Http/BtradeApiClient.cs` — the class doc states
  `X-Session-Actor` is the "OperatorName" (line 16); the default actor provider
  reads `RegistryHelper.KeyLastOperatorName` (lines 92–93); `CreateRequest`
  attaches `X-Session-Actor` from that provider (lines 127–131).
- `BtrReturGudang.Infrastructure/Helpers/RegistryHelper.cs` — `LastOperatorName`
  is a free-text string read from `HKCU\Software\BTRApp\ReturGudang` (lines 78–82).
- `BtrReturGudang.Winform/Views/OperatorSwitcherDialog.cs` — the operator is typed
  as a display name ("Nama Operator / Petugas Gudang"), with no email concept
  (lines 143–159).
- No email or Google-identity concept exists anywhere in `j09` (repository search
  for `email`/`google` returns no production matches).

Conclusion (supported by evidence): a free-text operator name (or an empty value)
is transmitted as `X-Session-Actor`, so the Cloud email lookup cannot succeed, and
`SessionAccountUnresolvableException` → 409 is returned by design.

### Finding 3 — Contract drift affects more than return-order upload

The same header-resolved actor resolution is applied on other j09-used routes:

- `POST api/barcode-registration` and `GET api/BarcodeRegistration/status` also
  call `ResolveAccount` on `X-Session-Actor`
  (`btrade.webapi/Controllers/BarcodeRegistrationController.cs` lines 58, 80).
- `GET api/barcodes/sync` reads only `X-Session-Location` (tenant), so it is not
  affected by the actor defect.

Conclusion (supported by evidence): barcode-registration upload from the desktop
client is subject to the same refusal once its actor is involved.

### Finding 4 — Tenant resolution is not the failing step

In the observed transaction the actor check failed (HTTP 409) after the tenant
check (`ResolveTenant`, executed first at line 35) had already succeeded.
Conclusion (supported by evidence): the workstation `WarehouseCode` maps to a
valid `BTR_WarehouseMapping` location; only the actor contract is broken.

### Root Cause

The j09 desktop client and the j06 Cloud relay drifted apart on the semantics of
`X-Session-Actor`. `j06` was changed to require a registered Google email resolved
via `BTRADE_User.Email`; `j09` continues to send a free-text operator name. The
Cloud therefore cannot resolve the actor and correctly returns the HTTP 409
"Google account is not registered or is not active" refusal.

## Affected Components

- **Desktop client (j09):**
  - `BtrReturGudang.Infrastructure/Http/BtradeApiClient.cs` — session-header
    construction and request assembly.
  - `BtrReturGudang.Infrastructure/Helpers/RegistryHelper.cs` — workstation
    operator configuration.
  - `BtrReturGudang.AppTier/Contracts/IRegistryHelper.cs` — operator configuration
    contract.
  - `BtrReturGudang.Winform/Views/OperatorSwitcherDialog.cs`,
    `SettingsView.cs`, `MainForm.cs` — operator identity capture/display.
  - `BtrReturGudang.Infrastructure/Services/SyncService.cs` — upload result
    propagation (already surfaces the 409 detail).
- **Cloud relay (j06):** `ReturnOrderController`, `BarcodeRegistrationController`,
  `SessionContextResolver`, `UserDal.GetByEmail`, `ErrorHandlerMiddleware`,
  `SessionController` (`POST api/session/resolve`), `BTRADE_User.Email`.
- **Integration / data:** `X-Session-Actor` header contract; Main Office
  `BTR_User.Email` mapping and its projection to Cloud `BTRADE_User.Email` via
  `j07-btrade-sync`.

## Impact Assessment

- **Business Impact:** Warehouse operators cannot submit captured Return Orders
  to the Cloud from the desktop client; intake documents stay `Draft` and are not
  relayed upstream. This blocks the return-order synchronization capability the
  desktop client exists to provide.
- **Operational Impact:** The failure message ("Google account is not registered
  or is not active") is accurate for the Cloud but misleading for the operator,
  because the actual cause is a missing/invalid actor contract on the device, not
  necessarily an unregistered account. Operators may be blocked until identity
  configuration is corrected.
- **Technical Impact:** Client/server contract drift on a shared API consumed by
  both BGud and the desktop client. No Cloud defect was found; the Cloud behavior
  is correct and intended. The desktop client risks silent or repeated 409s on
  every actor-bearing upload (return-order and barcode-registration).

## Assumptions

1. The Cloud Google-email actor contract (BGud sign-in) is authoritative and
   intended to remain in force; it will not be reverted to name-based identity.
2. The operator's Google email is (or will be) registered in Main Office
   `BTR_User.Email` and projected to Cloud `BTRADE_User.Email` by
   `j07-btrade-sync`; the investigation does not treat the mapping as owned by the
   desktop client.
3. The workstation `WarehouseCode` correctly maps to a `BTR_WarehouseMapping`
   location (supported by Finding 4).
4. The Cloud `POST api/session/resolve` endpoint is available and authoritative for
   pre-validating an operator email.

## Open Questions

- **OQ-001 (non-blocking):** Should the desktop client eventually adopt a real
  Google OAuth sign-in instead of a workstation-configured registered email? No
  evidence requires it now; deferred as a future enhancement and not a blocker for
  the correction.

## Recommended Decision

The desktop client's session actor input must conform to the Cloud's anonymous
session-context contract: the operator identity carried on outbound requests must
be the registered Google email (not a display name), and the configured identity
should be validated against the Cloud session-resolution capability before
warehouse work is uploaded. The Cloud contract remains authoritative and
unchanged.

## Decision

Align the j09 desktop actor contract to the Cloud contract:

1. The operator identity carried as `X-Session-Actor` is the operator's registered
   Google email, configurable on the workstation; the operator display name is
   retained only for display and local record attribution.
2. The configured operator email is validated through the Cloud anonymous
   `POST api/session/resolve` endpoint, and its refusal message is surfaced to the
   operator instead of an opaque HTTP 409.
3. No change is made to `j06-pkl-btrade-api`; the approved Cloud contract and the
   Main Office `BTR_User.Email` mapping remain authoritative.

## Decision Rationale

- **Contract alignment with the approved identity model:** The selected direction
  realizes the already-approved BGud identity decision (Google email as actor,
  resolved server-side to the authoritative BTR `UserId`), preserving
  per-operator attribution without reintroducing authentication.
- **No server change / non-invasive:** The Cloud contract is correct and shared
  with BGud; correcting the desktop client avoids regressing BGud or other
  consumers.
- **Rejected alternative — server fallback to BTR username:** Extending
  `ResolveAccount` to also match `BTRADE_User.UserName` would contradict the
  approved Google-identity decision and weaken the anonymous-posture guard by
  allowing unauthenticated name strings to resolve as actors.
- **Rejected alternative — configuration-only workaround:** Entering the email
  into the existing operator-name field would unblock uploads without code
  changes, but would store an email as `OperatorName` in local records and leave
  the misleading failure mode in place; it is at best a stopgap.
- **Constraint / trade-off:** The correction depends on an operational
  prerequisite — the operator's email must be registered in Main Office and
  projected to the Cloud — which is a deployment/registration concern, not a
  defect of the client or the Cloud.
- Supporting evidence: Findings 1–4 (Cloud contract and 409 mapping; j09
  name-based actor; shared routes; tenant resolution succeeding).

## Architecture Applicability

### Decision

**ARCHITECTURE-REQUIRED**

### Rationale

The correction changes a client-side integration boundary and session-context
contract, and alters component responsibilities (operator identity configuration
and Cloud email-resolution behavior). The capability ARCHITECTURE
(`docs/work/j09-btr-retur-gudang/BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md`) was
updated to v1.2 to define the Google-email actor contract and the session
resolution flow (TD-007, TD-012) before implementation planning. The existing
architecture remains authoritative.

---

## Related Artifacts

| Artifact | Relevance |
| :--- | :--- |
| `docs/work/j09-btr-retur-gudang/BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md` | Capability architecture; v1.2 TD-007 / TD-012 define the corrected actor contract. |
| `docs/work/j09-btr-retur-gudang/BTR-RETUR-SESSION-ACTOR-IMPLEMENTATION-PLAN.md` | Approved implementation plan (`BTR-RETUR-SESSION-ACTOR-001`) realizing this decision. |
| `docs/features/bgud-operator-signin/feature.md` | Approved feature defining the Google-email operator identity and refusal behavior. |
| `docs/work/bgud-google-signin/DEPLOYMENT-GUIDELINE.md` | Cloud contract: session headers, `POST api/session/resolve`, and 400/409 mapping. |
| `docs/investigations/return-order-upload-url-investigation.md` | Prior related defect (RestSharp trailing-slash / 404 and error masking) on the same upload path. |
| `src/j06-pkl-btrade-api` — `SessionContextResolver`, `ReturnOrderController`, `UserDal`, `ErrorHandlerMiddleware` | Cloud side of the actor contract (authoritative). |
| `src/j09-btr-retur-gudang` — `BtradeApiClient`, `RegistryHelper`, `OperatorSwitcherDialog` | Desktop side carrying the stale name-based actor. |
