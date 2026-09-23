# ISSUE

## Metadata

ID: `BGUD-GOOGLE-SIGNIN-001`  
Type: `CHANGE-REQUEST`  
Status: `New`  
Title: **BGud — Replace username/password login with Google Sign-In and stop authenticating Cloud endpoints**

## Source

Reported By: Repository owner (direct request)  
Reported Date: 2026-09-23

## Description

The BGud Android app (`src/BGud`) currently authenticates operators with a
username, password, and warehouse selection. The requested change has two
related parts:

1. **Replace the login method** with Google Sign-In, following the approach
   currently implemented in BTrade3 (`src/BTrade3`).
2. **Remove endpoint authentication** from the Cloud endpoints consumed by
   BGud, matching the unauthenticated endpoint consumption used by BTrade3.

The reporter also requested confirmation of whether BGud login currently uses
the user credentials defined in the `BTR_User` table.

## Desired Outcome

- BGud operators sign in with Google instead of entering a BTR username and
  password.
- BGud's Cloud API communication no longer carries or requires authentication,
  consistent with BTrade3's API communication.
- The existing warehouse-selection behavior is either preserved or explicitly
  clarified as part of the change.

## Current Situation

### BGud login

| Area | Current behavior |
|---|---|
| Login UI | `LoginScreen.kt` collects username, password, and warehouse. |
| Login request | `LoginViewModel.kt` posts `{ userId, password, locationId }` to `POST api/Auth/login` (I-07). |
| Session | The returned JWT, user, warehouse code, and office code are stored in `SessionPreferencesDataSource`. |
| Subsequent requests | `AuthInterceptor.kt` attaches `Authorization: Bearer <token>` to every request. |
| Navigation | `Navigation.kt` requires a stored token before allowing access to operational screens (IR-M8). |

### Server side

- `AuthController.cs` exposes `POST api/Auth/login` as `[AllowAnonymous]`.
- `BarcodeController`, `BarcodeRegistrationController`,
  `ReturnOrderController`, `DriverController`, and `UserController` are
  `[Authorize]`.
- Credentials are validated against a replicated projection. The credential
  store of record is the Main Office `BTR_User` table
  (`src/j05-btr-distrib/btr.sql/Tables/Helper/BTR_User.sql`), replicated to
  the Cloud as `BTRADE_User` by `j07-btrade-sync`; the Cloud verifies the
  SHA-256 password hash.

**Confirmation:** Yes, BGud login is ultimately based on `BTR_User` accounts.
BGud does not read `BTR_User` directly; it sends credentials to the Cloud API,
which verifies them against the `BTRADE_User` projection replicated from
`BTR_User`.

### BTrade3 reference behavior

- `LoginScreen.kt` uses Google Sign-In, reads the signed-in email, and passes
  it to `onUserSignedIn`; no server login call or application token is used.
- `GoogleSignInHelper.kt` configures Google Sign-In with an ID-token request.
- `Navigation.kt` stores the email in SharedPreferences and uses its presence
  as the logged-in state.
- `NetworkModule.kt` has no authentication interceptor.
- `ApiService.kt` consumes `Brg/{serverId}`, `Customer/{serverId}`,
  `SalesPerson/{serverId}`, `Order`, `Customer` (PATCH), and `CheckIn`
  without authentication. The tenant is supplied explicitly, such as through
  `serverId`.

## Evidence

### BGud

- `src/BGud/app/src/main/java/com/elsasa/bgud/viewmodel/LoginViewModel.kt` —
  login request, JWT storage, and login-time master-data synchronization.
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/LoginScreen.kt` —
  username, password, and warehouse fields.
- `src/BGud/app/src/main/java/com/elsasa/bgud/network/AuthInterceptor.kt` —
  bearer header attachment.
- `src/BGud/app/src/main/java/com/elsasa/bgud/network/BtradeApiService.kt` —
  BGud endpoint declarations.
- `src/BGud/app/src/main/java/com/elsasa/bgud/datastore/SessionPreferencesDataSource.kt` —
  JWT session persistence.
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/Navigation.kt` —
  token-gated navigation and Cloud base URL.

### Server and credential source

- `src/j06-pkl-btrade-api/btrade.webapi/Controllers/*.cs` —
  `[Authorize]` and `[AllowAnonymous]` attributes.
- `src/j05-btr-distrib/btr.sql/Tables/Helper/BTR_User.sql` — `BTR_User` schema.
- `docs/work/barcode-registry/BARCODE-REGISTRY-ARCHITECTURE.md` — IR-05 and
  the `BTRADE_User` credential projection.

### BTrade3 reference implementation

- `src/BTrade3/.../util/GoogleSignInHelper.kt`
- `src/BTrade3/.../ui/screen/LoginScreen.kt`
- `src/BTrade3/.../network/NetworkModule.kt`
- `src/BTrade3/.../network/ApiService.kt`
- `src/BTrade3/.../ui/Navigation.kt`

## Notes

The following points require clarification during the next workflow stage.
They do not define analysis, architecture, implementation, planning, or
testing strategy.

1. **Endpoint scope:** Should authentication be removed only from the login
   call, or from every BGud endpoint: `barcodes/sync`,
   `barcode-registration`, `BarcodeRegistration/status`, `Brg/{serverId}`,
   `return-order`, `Customer`, `SalesPerson`, and `Driver`?
2. **Client and server scope:** Does “not using authentication” apply only to
   the BGud client bearer header, or also to the `[Authorize]` requirements on
   the corresponding server controllers?
3. **Tenant resolution:** The current design resolves the tenant from the JWT
   and does not send `ServerId`. If authentication is removed, how should the
   Cloud identify the requested tenant or warehouse?
4. **Warehouse selector:** Should the existing Gudang Gamping, Concat, and
   Magelang selector remain, or should it follow the BTrade3-style server
   selector (JOG / MGL)?
5. **Google account authority:** May any Google account sign in, or must the
   account be mapped to an existing BTR user or allow-list?
6. **Google client configuration:** Should BGud reuse BTrade3's Google OAuth
   web client and project, or use a separate BGud registration?
7. **Login-time synchronization:** Should the master-data synchronization
   currently triggered by login continue to run at login time?
8. **Issue tracking:** Should these related changes remain in one combined
   change request, or be split into separate ISSUE artifacts?
