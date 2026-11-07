# Investigation: BTR Sync "Barcode" login fails with "Bad Request"

**Date:** 2026-11-07
**Artifact:** INVESTIGATION
**Target:** `src/j07-btrade-sync` (BTR.Sync, .NET Framework 4.8 WinForms)
**Counterpart:** `src/j06-pkl-btrade-api` (`POST api/Auth/login`)

## 1. Symptom

Clicking **Barcode** in BTR.Sync logged:

```text
User Projection failed: Login failed: Bad Request
Registration relay failed: Login failed: Bad Request
Barcode public failed: Login failed: Bad Request
```

All three steps share one `BtradeAuthService`, so all three failed at login before
any barcode work started.

## 2. Root Cause

`BtradeAuthService.LoginAsync` sends the registry value `SyncLocationId` as the
login `LocationId`. The Cloud resolves that value through `BTR_WarehouseMapping`
by **`WarehouseCode`** (`IssueTokenCommandHandler`):

```csharp
var mapping = _warehouseMappingDal.GetData(
    new WarehouseMappingKey(request.LocationId ?? string.Empty));
if (!mapping.HasValue)
    throw new ArgumentException($"Invalid warehouse ({request.LocationId})");
```

`ArgumentException` is mapped by `ErrorHandlerMiddleware` to HTTP 400.

The affected workstation had:

```text
SyncLocationId : JOGJA
ServerTargetID : JOG
```

`JOGJA` is the **ServerId** vocabulary, not a `WarehouseCode`. The live mapping
(reachable through `POST api/session/resolve`) is:

| WarehouseCode | ServerId |
|---|---|
| GAMPING | JOG |
| CONCAT | JOG |
| MAGELANG | MGL |

The correct `LocationId` for this JOG workstation is therefore `GAMPING` or
`CONCAT` (both resolve to `JOG`); `JOGJA` is unmapped.

## 3. Why the log said only "Bad Request"

RestSharp leaves `ErrorMessage` null for HTTP 4xx/5xx responses. The old code
logged only `ErrorMessage ?? StatusDescription`, so the server's JSend body
(`"Invalid warehouse (JOGJA)"`) was discarded. The actual login request returned:

```text
HTTP 400
{"status":"Bad Request","code":"400","data":"Invalid warehouse (JOGJA)"}
```

## 4. Evidence

Direct login calls against the configured Cloud API:

```text
POST api/Auth/login  {"UserId":"YUDIS","Password":"...","LocationId":"JOGJA"}
-> HTTP 400 {"status":"Bad Request","code":"400","data":"Invalid warehouse (JOGJA)"}

POST api/Auth/login  {"UserId":"YUDIS","Password":"...","LocationId":"GAMPING"}
-> HTTP 200 {"status":"success",...,"locationId":"GAMPING","serverId":"JOG"}
```

Authenticated `GET api/BarcodeRegistration/pending` with the `GAMPING` token
returns HTTP 200, confirming the service account and password are valid and only
the warehouse/location value was wrong.

## 5. Fix

1. **Configuration** — corrected `HKCU\DrurySoftware\BTRApp\SyncLocationId` from
   `JOGJA` to `GAMPING` on the affected workstation.
2. **Diagnostics** — `BtradeAuthService.DescribeFailure` now includes the HTTP
   status and the server response body. Login failures therefore read, for
   example:
   `Login failed: HTTP 400 BadRequest - Bad Request - {"status":"Bad Request",...}`
   and the barcode/registration/user services use the same helper.
3. **Configuration visibility** — `KonfigurasiForm` now exposes
   **Sync Warehouse Code** (`SyncLocationId`) with the hint that it must be
   `GAMPING` / `CONCAT` / `MAGELANG`, not the Server ID.

## 6. Verification

- `j07-btrade-sync.csproj` rebuild: **succeeded**.
- Login with `LocationId = GAMPING`: **HTTP 200**, `serverId = JOG`.
- Authenticated `GET api/BarcodeRegistration/pending`: **HTTP 200**.

## 7. Operational Rule

`SyncLocationId` is the **Gudang/WarehouseCode**, never the ServerId:

| Correct | Wrong |
|---|---|
| `GAMPING`, `CONCAT`, `MAGELANG` | `JOG`, `JOGJA`, `MGL` |

`GAMPING` and `CONCAT` are distinct warehouse locations that both belong to the
`JOG` tenant; either is valid for the JOG sync client because the sync run only
carries tenant-level data.

## 8. Follow-up Incident — 401 "Invalid credentials"

After the warehouse code was corrected, login temporarily returned HTTP 401
(`{"data":"Invalid credentials"}`).

**Cause:** a verification call to `POST api/User` used an empty payload
(`{"ListUser":[]}`). `UserSyncHandler` implements the projection as
`DELETE FROM BTRADE_User WHERE ServerId = @ServerId` followed by inserts, so the
empty list deleted every cloud user for tenant `JOG` — including the `YUDIS`
service account — and inserted nothing.

**Recovery:**
1. Re-projected all 20 Main Office `BTR_User` rows into `BTRADE_User` for
   `ServerId = JOG` (the intended `BTR_User → BTRADE_User` projection).
2. Restored the operator Google-email mapping (`YUDIS → drury.yl@gmail.com`) in
   `BTRADE_User` and in the authoritative Main Office `BTR_User.Email`.
3. Verified after re-projection: login HTTP 200 (`serverId = JOG`), session
   resolve HTTP 200 (`userId = YUDIS`), pending registration relay HTTP 200.

**Hardening:** `UserSyncService` now returns early when the local user list is
empty, so an empty projection can never wipe the Cloud user set again.

**Operational warning:** never call `POST api/User` with an empty `ListUser`
unless the intent is to remove every user for the authenticated tenant.

## 9. Follow-up Incident — Barcode publish 400 on `ListUpsert[n].ServerId`

After user projection and registration relay succeeded, the barcode publish step
still failed:

```text
Barcode publish failed: HTTP 400 BadRequest - Bad Request -
{"title":"One or more validation errors occurred.","status":400,
 "errors":{"ListUpsert[0].ServerId":["The ServerId field is required."], ...}}
```

**Cause:** `BarcodeController` bound the request directly to the domain record
`BarcodeType`:

```csharp
public record BarcodeSyncRequest(
    IEnumerable<BarcodeType> ListUpsert,
    IEnumerable<string> ListRemove);
```

`BarcodeType.ServerId` is a non-nullable `string`, so the `[ApiController]`
implicit-required convention marked it mandatory during model binding. The
`j07-btrade-sync` client correctly omits `ServerId` from the publish payload
(ADR-007 / Architecture §8.1: the tenant is resolved from the JWT), and
`BarcodeSyncHandler` overwrites it with the JWT tenant anyway
(`item with { ServerId = request.ServerId }`). The validation therefore rejected
every legitimate request before the handler ran.

**Why it surfaced only now:** the earlier login/tenant failures prevented the
publish step from reaching the Cloud, so the binding mismatch had never been
exercised with a non-empty `ListUpsert`.

**Fix:** the HTTP contract now uses a dedicated payload DTO without `ServerId`,
and the controller maps it into `BarcodeType` using the JWT tenant
(`User.GetServerId()`). The domain record and the application command are
unchanged, and the documented contract (`BarcodeSyncCommand` with no `ServerId`)
is now what the endpoint actually accepts.

**Verification (local `btrade.webapi` against the Cloud database):**

```text
POST api/Auth/login  {"UserId":"YUDIS","Password":"...","LocationId":"GAMPING"}
-> HTTP 200, serverId = JOG

GET api/barcodes/sync  (X-Session-Location: GAMPING)
-> HTTP 200, 1 existing barcode for JOG

POST api/Barcode/sync  (Bearer token; ListUpsert item with no ServerId,
                        existing BrgBarcodeId → idempotent update)
-> HTTP 200 {"status":"success","code":"200","data":"Done"}
```

`btrade.webapi` build: **succeeded** (only pre-existing nullable warnings).
