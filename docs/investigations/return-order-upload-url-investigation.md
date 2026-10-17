# Investigation Report: Return Order Upload Failure & API Route Resolution

**Date:** 2026-10-17  
**Artifact:** INVESTIGATION  
**Target System:** `src/j09-btr-retur-gudang/BtrReturGudang.sln`  
**Components:** `BtradeApiClient`, `RegistryHelper`, `SyncService`, `ReturnOrderListView`  
**Trigger:** Operator clicked "☁️ Upload Ulang" in `ReturnOrderListView` (or "Simpan & Upload [F10]" in `ReturnOrderEntryView`), but the upload failed with a warning dialog.

---

## 1. Executive Summary

When attempting to upload Return Order documents to the Cloud API relay (`j06-pkl-btrade-api`), the upload failed.
Code analysis, runtime testing, and registry examination revealed two interrelated defects:

1. **RestSharp / URI Resolution Defect (RCA-1)**:
   The configured Base URL in the Windows Registry and application constants was set without a trailing slash: `"http://dev.smart-ics.com:8089/belajar-api"`.
   Under RFC 3986 and .NET `System.Uri` combining rules, when a Base URL has a path without a trailing slash, combining it with a relative resource path (such as `"api/return-order"`) strips the final path segment (`/belajar-api`).
   Consequently, requests were dispatched to **`http://dev.smart-ics.com:8089/api/return-order`** instead of the intended target **`http://dev.smart-ics.com:8089/belajar-api/api/return-order`**, triggering HTTP 404 (Not Found) errors on the server.

2. **Error Diagnostic Masking Defect (RCA-2)**:
   `IBtradeApiClient.UploadReturnOrderAsync` was defined to return only a `bool` (`true`/`false`).
   When the HTTP request failed (HTTP 404), `BtradeApiClient` returned `false`, and `SyncService` generated a generic error string (`"Failed to upload Return Order '{returnOrderId}' to Cloud API."`).
   Furthermore, `ReturnOrderListView.OnUploadOrderClickAsync` discarded even that generic error string, displaying only `"Gagal mengupload dokumen retur [...] ke Cloud API relay."`.
   Neither the user interface nor the `BTRG_SyncLog` table captured the HTTP status code (404) or server response payload, masking the root cause.

3. **Workstation Registry State**:
   Live inspection of the current workstation's registry confirmed that `HKCU\Software\BTRApp\ReturGudang\ApiBaseUrl` was already populated with `http://dev.smart-ics.com:8089/belajar-api` (lacking a trailing slash).

---

## 2. Concrete Evidence & Findings

### Evidence 1: Live Registry Value on Current Machine
Running PowerShell command:
```powershell
Get-ItemProperty -Path "HKCU:\Software\BTRApp\ReturGudang"
```
Output:
```text
WarehouseCode       : 
ServerTargetId      : JOG
LastOperatorName    : 
EnableSoundFeedback : 1
ApiBaseUrl          : http://dev.smart-ics.com:8089/belajar-api
```
The stored `ApiBaseUrl` explicitly lacks a trailing slash.

### Evidence 2: RFC 3986 / System.Uri Resolution Demonstration
Testing URI resolution in .NET:
```powershell
[System.Uri]::new([System.Uri]::new('http://dev.smart-ics.com:8089/belajar-api'), 'api/return-order').AbsoluteUri
```
**Output:**
```text
http://dev.smart-ics.com:8089/api/return-order
```
*Note:* The `/belajar-api` segment was completely stripped.

When tested with a trailing slash:
```powershell
[System.Uri]::new([System.Uri]::new('http://dev.smart-ics.com:8089/belajar-api/'), 'api/return-order').AbsoluteUri
```
**Output:**
```text
http://dev.smart-ics.com:8089/belajar-api/api/return-order
```
*Note:* The `/belajar-api` segment is preserved.

### Evidence 3: Client Request Assembly in `BtradeApiClient.cs`
In `BtrReturGudang.Infrastructure\Http\BtradeApiClient.cs` (lines 68-73 & 143):
```csharp
var options = new RestClientOptions(_activeBaseUrl) // _activeBaseUrl = "http://dev.smart-ics.com:8089/belajar-api"
{
    Timeout = TimeSpan.FromSeconds(30)
};
_restClient = new RestClient(options);
...
var request = CreateRequest("api/return-order", Method.Post);
```
RestSharp builds the request URI from `options.BaseUrl` and `request.Resource`. Because `BaseUrl` has no trailing slash, RestSharp produces `http://dev.smart-ics.com:8089/api/return-order`.

### Evidence 4: Masked Failure Dialog in `ReturnOrderListView.cs`
In `BtrReturGudang.Winform\Views\ReturnOrderListView.cs` (lines 835-864):
```csharp
if (_orderService != null)
{
    var result = await _orderService.CommitAndUploadAsync(_selectedOrder.ReturnOrderId);
    success = result != null && result.Success;
}
...
else
{
    _audioHelper?.PlayWarning();
    MessageBox.Show(
        $"Gagal mengupload dokumen retur [{_selectedOrder.ReturnOrderId}] ke Cloud API relay.",
        "Upload Gagal",
        MessageBoxButtons.OK,
        MessageBoxIcon.Warning);
}
```
Notice that `result?.ErrorMessage` is never displayed in the message box, preventing the operator from seeing why the upload was rejected.

---

## 3. Root Cause Analysis (RCA)

### RCA-1: Missing Trailing Slash in Base URL Configuration
- In `RegistryHelper.cs`: `DefaultApiBaseUrl = "http://dev.smart-ics.com:8089/belajar-api"`.
- When users configure `ApiBaseUrl` in `SettingsView`, or when default values are persisted, no trailing slash was enforced.
- When `RestClient` resolves relative endpoint paths (`api/return-order`, `api/barcode-registration`, `api/barcodes/sync`, `api/Customer/{serverId}`, `api/Brg/{serverId}`), the missing trailing slash causes .NET URI parser to replace `/belajar-api` with the resource path.

### RCA-2: Coarse-Grained `Task<bool>` in `IBtradeApiClient`
- `IBtradeApiClient.UploadReturnOrderAsync` returned `bool`, discarding `response.StatusCode`, `response.Content`, and `response.ErrorMessage`.
- `SyncService` was unable to report the real HTTP failure reason into `SyncResultDto.ErrorMessage` or `SyncLogEntry`.

### RCA-3: Incomplete Error Propagation in UI
- `ReturnOrderListView.cs` did not consume or display `result.ErrorMessage` upon failure.

---

## 4. Agreed Decisions (from `/grill-me`)

1. **Automatic Base URL Sanitization**:
   - Update `RegistryHelper.DefaultApiBaseUrl` to `"http://dev.smart-ics.com:8089/belajar-api/"`.
   - In `RegistryHelper.ApiBaseUrl` and `BtradeApiClient.GetCurrentBaseUrl()`, ensure any resolved URL automatically ends with `/`.
   - Existing workstations with un-slashed registry values will be transparently normalized at runtime.

2. **Rich Error Visibility in Client & Sync Engine**:
   - Introduce an `ApiUploadResultDto` (or enhance return model) carrying `bool Success`, `int StatusCode`, `string ErrorMessage`, and `string ResponseContent`.
   - `SyncService` captures and logs HTTP status codes and response messages to `BTRG_SyncLog` and `SyncResultDto`.

3. **Consistent UI Failure Display**:
   - Update `ReturnOrderListView.cs` to show `result?.ErrorMessage` in the failure warning dialog, matching `ReturnOrderEntryView.cs`.

---

## 5. Next Steps

Proceed with architectural review and execution of `docs/work/j09-btr-retur-gudang/return-order-upload-implementation-plan.md`.
