# Investigation Report: BTRG_Customer Remains Empty After 'Sync Sekarang'

**Date:** 2026-10-16  
**Artifact:** INVESTIGATION  
**Target System:** `src/j09-btr-retur-gudang/BtrReturGudang.sln`  
**Database:** Local SQL Server `btrgd`  
**Trigger:** User clicked "Sync Sekarang" button in the WinForms application sidebar, but `BTRG_Customer` table remained at 0 rows.

---

## 1. Executive Summary

When the user triggered "Sync Sekarang" from the application sidebar, the sync engine completed and marked the operation as `SUCCESS` (displaying green "Terakhir Sync: ... (Sukses)"), yet `BTRG_Customer` contained 0 rows.

Direct inspection of the user's running application process (PID 9968), Windows Registry, local SQL database (`btrgd`), and cloud relay endpoints revealed the root cause:
1. The registry key `HKCU\Software\BTRApp\ReturGudang\ServerTargetId` held an empty string (`""`), written by previous application defaults.
2. `RegistryHelper.ReadString` treats empty strings as valid non-null values, bypassing the fallback `"JOG"` and returning `""`.
3. `SyncService` passed `""` to `BtradeApiClient.DownloadMasterDataAsync`, which defaulted empty server IDs to `"DEFAULT"`.
4. The cloud API (`http://dev.smart-ics.com:8089/belajar-api/api/Customer/DEFAULT`) responded with HTTP 200 and an empty dataset `{"status":"success","code":"200","data":[]}`.
5. In contrast, querying the correct server target `JOG` (`/api/Customer/JOG`) returns **2,280** customers, **3,271** products, and **43** salespersons.
6. Because the HTTP response was successful with 0 records, the sync logged `MasterData | SUCCESS | Pulled: 0` without updating `BTRG_Customer`.

---

## 2. Concrete Evidence & Findings

### Evidence 1: Live Registry State
Querying `HKCU\Software\BTRApp\ReturGudang` on the workstation:
```powershell
Get-ItemProperty -Path 'HKCU:\Software\BTRApp\ReturGudang'
```
Output:
```text
WarehouseCode       : 
ServerTargetId      : 
LastOperatorName    : 
EnableSoundFeedback : 1
```
`ServerTargetId` exists in the registry as `""` (empty string).

### Evidence 2: BTRG_SyncLog Entries
Querying `BTRG_SyncLog` in local database `btrgd`:
```sql
SELECT TOP 6 SyncTimestamp, SyncType, Status, RecordsPulled, ErrorMessage 
FROM BTRG_SyncLog ORDER BY SyncTimestamp DESC;
```
Output:
```text
[10/16/2026 21:03:35] FullSync   | SUCCESS | Pulled: 0 | Full sync pass completed: 0 barcodes pushed, 0 orders pushed, 0 records pulled.
[10/16/2026 21:03:35] MasterData | SUCCESS | Pulled: 0 | Reference sync complete: 0 items, 0 barcodes, 0 customers, 0 drivers, 0 sales.
[10/16/2026 21:03:06] FullSync   | SUCCESS | Pulled: 0 | Full sync pass completed: 0 barcodes pushed, 0 orders pushed, 0 records pulled.
[10/16/2026 21:03:06] MasterData | SUCCESS | Pulled: 0 | Reference sync complete: 0 items, 0 barcodes, 0 customers, 0 drivers, 0 sales.
```
Both manual sync triggers succeeded with `Pulled: 0`.

### Evidence 3: Cloud Endpoint Response Comparison
Querying the cloud endpoints with `"DEFAULT"` vs `"JOG"`:
| Endpoint | HTTP Status | Records Returned |
| -------- | ----------- | ---------------- |
| `GET /api/Customer/DEFAULT` | 200 OK | `0` (`data: []`) |
| `GET /api/Brg/DEFAULT` | 200 OK | `0` (`data: []`) |
| `GET /api/SalesPerson/DEFAULT` | 200 OK | `0` (`data: []`) |
| **`GET /api/Customer/JOG`** | **200 OK** | **`2,280`** |
| **`GET /api/Brg/JOG`** | **200 OK** | **`3,271`** |
| **`GET /api/SalesPerson/JOG`** | **200 OK** | **`43`** |

Because `"DEFAULT"` is not a valid depot/branch code in the upstream BTR system, the cloud API returns an empty array rather than a 404/500 error.

---

## 3. Root Cause Analysis

### RCA-1: Flawed Default Fallback in `RegistryHelper`
In `BtrReturGudang.Infrastructure\Helpers\RegistryHelper.cs`:
```csharp
public static string ReadString(string keyName, string defaultValue = "")
{
    using (var key = Registry.CurrentUser.OpenSubKey(SubKeyPath, writable: false))
    {
        if (key == null) return defaultValue;
        var val = key.GetValue(keyName);
        return val != null ? val.ToString() : defaultValue; // BUG: if val is "", it returns "" instead of defaultValue!
    }
}
```
If the registry value exists as `""` (empty string), `val != null` is true. `ReadString` returns `""` instead of `DefaultServerTargetId` (`"JOG"`).

### RCA-2: Hardcoded `"DEFAULT"` Fallback in `BtradeApiClient`
In `BtrReturGudang.Infrastructure\Http\BtradeApiClient.cs`:
```csharp
public async Task<MasterSyncDto> DownloadMasterDataAsync(string serverId)
{
    var effectiveServerId = !string.IsNullOrWhiteSpace(serverId)
        ? serverId
        : "DEFAULT"; // BUG: "DEFAULT" hits empty cloud dataset. Must default to JOG / registry value.
```
When `serverId` was passed as `""`, `BtradeApiClient` substituted `"DEFAULT"` rather than consulting `_registryHelper.ServerTargetId` or `DefaultServerTargetId`.

### RCA-3: Passthrough of Empty Target in `SyncService`
In `BtrReturGudang.Infrastructure\Services\SyncService.cs`:
```csharp
public async Task<SyncResultDto> PullReferenceDeltasAsync(string serverId = null)
{
    var effectiveServerId = !string.IsNullOrWhiteSpace(serverId)
        ? serverId
        : _registryHelper?.ServerTargetId; // If ServerTargetId is "", passes "" down to DownloadMasterDataAsync
```
`SyncService` did not guard against `_registryHelper?.ServerTargetId` returning empty or whitespace.

### RCA-4: Static BaseUrl in Singleton `BtradeApiClient`
`BtradeApiClient` is registered as a Singleton in `Program.cs`. Its internal `RestClient` is initialized once at startup with `_baseUrl`. If a user updates the API base URL in `SettingsView`, the running `BtradeApiClient` instance continues using the initial URL until the application is restarted.

---

## 4. Recommended Solution

To ensure reliable, fail-safe synchronization:

1. **Fix `RegistryHelper` Default Handling:**
   - In `ReadString`: If the stored string is empty or whitespace, return `defaultValue`.
   - In `ServerTargetId` and `ApiBaseUrl` getters: Ensure `string.IsNullOrWhiteSpace(val)` returns `DefaultServerTargetId` (`"JOG"`) and `DefaultApiBaseUrl`.
   - In `ServerTargetId` setter: If set to empty or whitespace, write `DefaultServerTargetId`.

2. **Fix `BtradeApiClient.DownloadMasterDataAsync`:**
   - Replace fallback `"DEFAULT"` with dynamic resolution:
     `!string.IsNullOrWhiteSpace(serverId) ? serverId : (!string.IsNullOrWhiteSpace(_registryHelper?.ServerTargetId) ? _registryHelper.ServerTargetId : RegistryHelper.DefaultServerTargetId)`.
   - Never query `"DEFAULT"`.

3. **Fix `SyncService.PullReferenceDeltasAsync`:**
   - Fall back to `RegistryHelper.DefaultServerTargetId` (`"JOG"`) whenever `serverId` and `_registryHelper.ServerTargetId` are empty or whitespace.

4. **Dynamic Client Resolution in `BtradeApiClient`:**
   - Recreate or reconfigure `RestClient` if `_registryHelper.ApiBaseUrl` changes, allowing runtime setting adjustments without restarting the application.

5. **Workstation Registry Auto-Repair:**
   - When the application starts up or accesses `RegistryHelper`, if `ServerTargetId` or `ApiBaseUrl` is empty, automatically initialize them to `"JOG"` and `"http://dev.smart-ics.com:8089/belajar-api"`.

---

## 5. Implementation Plan

Following user approval, the following changes will be applied:

1. **`BtrReturGudang.Infrastructure\Helpers\RegistryHelper.cs`:**
   - Update `ReadString` to return `defaultValue` when `string.IsNullOrWhiteSpace(val.ToString())`.
   - Update `ServerTargetId` property getter/setter to strictly enforce `"JOG"`.
   - Update `ApiBaseUrl` property getter/setter to strictly enforce `DefaultApiBaseUrl`.

2. **`BtrReturGudang.Infrastructure\Http\BtradeApiClient.cs`:**
   - Change `effectiveServerId` resolution to use `RegistryHelper.DefaultServerTargetId` ("JOG").
   - Support dynamic base URL updates from `_registryHelper`.

3. **`BtrReturGudang.Infrastructure\Services\SyncService.cs`:**
   - Guard against empty string in `effectiveServerId`, defaulting to `RegistryHelper.DefaultServerTargetId`.

4. **Unit & Integration Tests:**
   - Add unit test verifying `RegistryHelper.ServerTargetId` returns `"JOG"` when registry contains empty string `""`.
   - Add unit test verifying `SyncService` and `BtradeApiClient` resolve `"JOG"` when given `""` or null.
   - Run live sync test verifying `BTRG_Customer` is populated with 2,280 rows.
