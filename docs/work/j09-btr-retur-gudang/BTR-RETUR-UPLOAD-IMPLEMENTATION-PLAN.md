---
Title: Return Order Upload Route & Diagnostic Fix
Code: BTR-RETUR-UPLOAD-001
Artifact: IMPLEMENTATION-PLAN
Version: 1.0
LastUpdated: 2026-10-17
Status: COMPLETED
Execution Approval: APPROVED
---

# 1. Objective

Resolve Return Order upload failure caused by URL route truncation in RestSharp and enhance diagnostic error visibility across the client, sync engine, and user interface.

Planning Mode: FEATURE-PLANNING

Referenced artifacts:

- INVESTIGATION: `docs/investigations/return-order-upload-url-investigation.md`
- ARCHITECTURE: `docs/work/j09-btr-retur-gudang/BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md`

Architecture Applicability: ARCHITECTURE-NOT-REQUIRED

No architectural target-state artifact was required. Implementation relies on existing technical structure. Approved feasibility decisions and investigation findings are authoritative for the change. The current codebase is the source of current technical truth.

---

# 2. Planning Scope

The scope is bounded to `src/j09-btr-retur-gudang/BtrReturGudang.sln`:

1. Contract & DTO Enhancement: Introduce `ApiUploadResultDto` (`Success`, `StatusCode`, `ErrorMessage`, `RawResponse`) and update `IBtradeApiClient` methods to return `Task<ApiUploadResultDto>`.
2. URL Sanitization: Enforce trailing-slash normalization across `RegistryHelper`, `BtradeApiClient`, and `SettingsView` so that Base URLs (e.g. `http://dev.smart-ics.com:8089/belajar-api/`) prevent RestSharp from stripping `/belajar-api` when resolving `api/return-order`.
3. Error Diagnostic Capture: Capture HTTP status code (e.g. 404, 400, 500) and response content in `BtradeApiClient` on failure.
4. Sync Engine Error Logging: Propagate detailed HTTP error messages into `SyncService.TriggerDocumentUploadAsync`, `SyncService.ExecutePushQueueAsync`, `BTRG_SyncLog`, and `SyncResultDto`.
5. UI Warning Dialog: Display the specific failure `ErrorMessage` in `ReturnOrderListView` dialog on upload failure.
6. Test Verification: Update test doubles, add regression unit tests, and verify all 115+ automated tests pass cleanly.

---

# 3. Dependencies

- External: Cloud API relay endpoint `http://dev.smart-ics.com:8089/belajar-api/api/return-order`.
- For slice dependencies:
  - `Depends On` declares implementation prerequisites.
  - Dependencies reference Slice IDs only.
  - Dependency satisfaction requires the referenced slice to have implementation status `IMPLEMENTED`.
  - Dependency satisfaction does not require review status `GO`.

---

# 4. Progress Summary

| Phase | Implementation Status | Review Status | Progress |
| :--- | :--- | :--- | :--- |
| P1 - Contracts & DTOs | IMPLEMENTED | GO | 1/1 |
| P2 - Infrastructure Core | IMPLEMENTED | GO | 2/2 |
| P3 - Sync Engine & Audit | IMPLEMENTED | GO | 1/1 |
| P4 - Presentation Layer | IMPLEMENTED | GO | 1/1 |
| P5 - Verification & Testing | IMPLEMENTED | GO | 1/1 |

---

# 5. Phases

## P1 - Contracts & DTOs

Implementation Status: IMPLEMENTED  
Review Status: GO  

### P1-S01

Title: Introduce `ApiUploadResultDto` and Update `IBtradeApiClient` Contract

Implementation Status: IMPLEMENTED  
Review Status: GO  

Objective:  
Define a rich API upload response model carrying status code, success flag, error message, and raw response payload. Update the `IBtradeApiClient` contract to return `Task<ApiUploadResultDto>`.

Depends On: None

Repository: `src/j09-btr-retur-gudang/BtrReturGudang.AppTier`

Completion Criteria:  
- `BtrReturGudang.AppTier/DTOs/ApiUploadResultDto.cs` created with properties `bool Success`, `int StatusCode`, `string ErrorMessage`, `string RawResponse`, and static factory helpers `Ok(int statusCode = 200, string rawResponse = null)` and `Fail(int statusCode, string errorMessage, string rawResponse = null)`.
- `BtrReturGudang.AppTier/Contracts/IBtradeApiClient.cs` updated:
  - `Task<ApiUploadResultDto> UploadReturnOrderAsync(ReturnOrderDto dto);`
  - `Task<ApiUploadResultDto> UploadBarcodeRegistrationAsync(BarcodeRegistrationDto dto);`
- `BtrReturGudang.AppTier` project compiles with zero errors.

Notes:  
Downstream slices in Infrastructure and Presentation depend on this contract.

---

## P2 - Infrastructure Core

Implementation Status: IMPLEMENTED  
Review Status: GO  

### P2-S02

Title: Trailing-Slash URL Normalization in `RegistryHelper` and `BtradeApiClient`

Implementation Status: IMPLEMENTED  
Review Status: GO  

Objective:  
Prevent RestSharp from stripping `/belajar-api` by ensuring that all configured and resolved Base URLs strictly terminate with a trailing slash (`/`).

Depends On: None

Repository: `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure`

Completion Criteria:  
- `RegistryHelper.DefaultApiBaseUrl` constant updated to `"http://dev.smart-ics.com:8089/belajar-api/"`.
- Static helper `RegistryHelper.NormalizeApiUrl(string url)` implemented to trim whitespace and append `'/'` if non-empty and missing trailing slash.
- `RegistryHelper.ApiBaseUrl` getter and setter apply `NormalizeApiUrl`.
- `BtradeApiClient.GetCurrentBaseUrl()` applies `RegistryHelper.NormalizeApiUrl` to all resolved base URLs.
- Existing workstation registry values (such as `http://dev.smart-ics.com:8089/belajar-api`) are transparently normalized to `http://dev.smart-ics.com:8089/belajar-api/` when read.

Notes:  
Can be implemented concurrently with P1-S01.

---

### P2-S03

Title: API Client Error Extraction and `ApiUploadResultDto` Implementation

Implementation Status: IMPLEMENTED  
Review Status: GO  

Objective:  
Implement error capture and `ApiUploadResultDto` construction in `BtradeApiClient.UploadReturnOrderAsync` and `BtradeApiClient.UploadBarcodeRegistrationAsync`.

Depends On: P1-S01, P2-S02

Repository: `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure`

Completion Criteria:  
- `BtradeApiClient.UploadReturnOrderAsync` returns `ApiUploadResultDto.Ok((int)response.StatusCode, response.Content)` when HTTP response indicates success (`2xx`).
- On non-success HTTP response or exception, extracts `(int)response.StatusCode` and error detail from `response.Content` / `response.ErrorMessage` / `response.StatusDescription` and returns `ApiUploadResultDto.Fail(statusCode, errorDetail, response.Content)`.
- `BtradeApiClient.UploadBarcodeRegistrationAsync` implemented with matching error extraction logic.
- Target POST URL created via `CreateRequest("api/return-order", Method.Post)` resolves against normalized `BaseUrl` to `http://dev.smart-ics.com:8089/belajar-api/api/return-order`.
- `BtrReturGudang.Infrastructure` compiles cleanly against `BtrReturGudang.AppTier`.

Notes:  
Eliminates error swallowing inside the HTTP client.

Implementation Notes:  
- `BtradeApiClient.UploadReturnOrderAsync` and `UploadBarcodeRegistrationAsync` now return `Task<ApiUploadResultDto>`.
- Success path (`2xx`, via new `IsSuccessResponse`) returns `ApiUploadResultDto.Ok((int)response.StatusCode, response.Content)`.
- Non-success path returns `ApiUploadResultDto.Fail((int)response.StatusCode, errorDetail, response.Content)` where `errorDetail` is extracted by new `ExtractErrorDetail` helper preferring `Content`, then `ErrorMessage`, then `StatusDescription`.
- Exceptions during the HTTP call return `ApiUploadResultDto.Fail(0, ex.Message, null)`; errors are no longer swallowed.
- Request construction (`CreateRequest("api/return-order", Method.Post)`, headers, JSON payload) is unchanged; URL resolution relies on the P2-S02 normalized `BaseUrl` (`http://dev.smart-ics.com:8089/belajar-api/`).
- Minimal mechanical call-site adaptation applied in `SyncService` (`TriggerDocumentUploadAsync`, `RunFullSyncPassAsync`) to consume `ApiUploadResultDto.Success` so `BtrReturGudang.Infrastructure` compiles; detailed HTTP status/error propagation and `BTRG_SyncLog` diagnostics remain the scope of P3-S04.
- Verified: `dotnet build src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure/BtrReturGudang.Infrastructure.csproj` → Build succeeded, 0 Warning(s), 0 Error(s).

---

## P3 - Sync Engine & Audit

Implementation Status: IMPLEMENTED  
Review Status: GO  

### P3-S04

Title: Sync Service Diagnostic Propagation & `BTRG_SyncLog` Persistence

Implementation Status: IMPLEMENTED  
Review Status: GO  

Objective:  
Consume `ApiUploadResultDto` in `SyncService`, propagate HTTP status code and server error message to `SyncResultDto.Fail(...)`, and record failure details in `BTRG_SyncLog`.

Depends On: P2-S03

Repository: `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure`

Completion Criteria:  
- `SyncService.TriggerDocumentUploadAsync(string returnOrderId)` checks `uploadResult.Success`:
  - When true: marks order as `Synced` via `_returnOrderRepo.MarkSyncedAsync`, logs `"SUCCESS"` in `_syncLogRepo.LogSyncAsync`, and returns `SyncResultDto.Ok("ReturnOrder", pushed: 1)`.
  - When false: constructs descriptive error `$"HTTP {uploadResult.StatusCode}: {uploadResult.ErrorMessage}"`, logs `"FAILED"` with details in `_syncLogRepo.LogSyncAsync`, and returns `SyncResultDto.Fail("ReturnOrder", $"Failed to upload Return Order '{returnOrderId}': {errorSummary}")`.
- `SyncService.ExecutePushQueueAsync()` updated to handle `ApiUploadResultDto` and log detailed error message on any failed upload in the queue.
- SQLite `BTRG_SyncLog` records contain full HTTP status and response context for failed attempts.

Notes:  
Ensures audit traceability of upload rejections.

Implementation Notes:  
- `SyncService.TriggerDocumentUploadAsync(string returnOrderId)` consumes `ApiUploadResultDto`:
  - Success path marks the order `Synced` via `_returnOrderRepo.MarkSyncedAsync`, logs `"SUCCESS"` (`SyncType = "ReturnOrder"`, `RecordsPushed = 1`) via `_syncLogRepo.LogSyncAsync`, and returns `SyncResultDto.Ok("ReturnOrder", pushed: 1)`.
  - Failure path builds `errorSummary = $"HTTP {uploadResult.StatusCode}: {uploadResult.ErrorMessage}"`, logs `"FAILED"` to `BTRG_SyncLog` including `errorSummary` and the raw response context (`uploadResult.RawResponse`), and returns `SyncResultDto.Fail("ReturnOrder", $"Failed to upload Return Order '{returnOrderId}': {errorSummary}")`.
- Queue processing lives in `SyncService.RunFullSyncPassAsync()` (the plan's `ExecutePushQueueAsync()`); the enumerated push-queue loops now handle `ApiUploadResultDto` explicitly. On any failed barcode registration or return-order upload in the queue, a `"FAILED"` `BTRG_SyncLog` entry is written carrying the HTTP status code, extracted error message, and raw response context.
- Diagnostics persist through the existing `SyncLogEntry.ErrorMessage` column of `BTRG_SyncLog` (existing log repository supports no dedicated status-code column); the HTTP status and response context are embedded in that field, matching the established logging pattern.
- Verified: `dotnet build src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure/BtrReturGudang.Infrastructure.csproj` → Build succeeded, 0 Warning(s), 0 Error(s).

---

## P4 - Presentation Layer

Implementation Status: IMPLEMENTED  
Review Status: GO  

### P4-S05

Title: UI Warning Dialog Error Visibility & Settings URL Sanitization

Implementation Status: IMPLEMENTED  
Review Status: GO  

Objective:  
Display specific failure details in `ReturnOrderListView.cs` dialog when upload fails, and ensure `SettingsView.cs` normalizes URLs upon saving and testing.

Depends On: P3-S04

Repository: `src/j09-btr-retur-gudang/BtrReturGudang.Winform`

Completion Criteria:  
- `ReturnOrderListView.OnUploadOrderClickAsync` extracts `result?.ErrorMessage` (from `_orderService.CommitAndUploadAsync`) or `syncResult?.ErrorMessage` (from `_syncService.TriggerDocumentUploadAsync`).
- When upload fails, the `MessageBox` includes the extracted failure message:
  ```csharp
  var msg = !string.IsNullOrWhiteSpace(failureReason)
      ? $"Gagal mengupload dokumen retur [{_selectedOrder.ReturnOrderId}] ke Cloud API relay:\r\n{failureReason}"
      : $"Gagal mengupload dokumen retur [{_selectedOrder.ReturnOrderId}] ke Cloud API relay.";
  MessageBox.Show(msg, "Upload Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
  ```
- `SettingsView.cs` loads, saves, and tests API URLs using `RegistryHelper.NormalizeApiUrl(...)`.
- `BtrReturGudang.Winform` compiles with zero errors.

Notes:  
Aligns `ReturnOrderListView` error UX with `ReturnOrderEntryView`.

Implementation Notes:  
- `ReturnOrderListView.OnUploadOrderClickAsync` now captures `failureReason` from `result?.ErrorMessage` (`IReturnOrderService.CommitAndUploadAsync`) or `syncResult?.ErrorMessage` (`ISyncService.TriggerDocumentUploadAsync`). On failure the warning `MessageBox` includes the extracted message, falling back to the original message when empty (mirrors the `err = syncResult?.ErrorMessage ?? ...` pattern in `ReturnOrderEntryView.SaveAndUploadAsync`).
- `SettingsView.cs` now routes API URLs through `RegistryHelper.NormalizeApiUrl(...)` at load (`BuildCloudApiCard` text initialization and `LoadCurrentConfiguration`), on save (`OnSaveAllClick` normalizes before persisting and reflects the persisted normalized value back into the textbox), and on test (`OnTestApiClickAsync` normalizes the URL before the HTTP GET and reflects it back).
- No Infrastructure changes were made; `RegistryHelper.NormalizeApiUrl` was consumed as an existing member.
- Verified: `dotnet build src/j09-btr-retur-gudang/BtrReturGudang.Winform/BtrReturGudang.Winform.csproj -c Debug` (net471) → Build succeeded, 0 Warning(s), 0 Error(s).
- Changed files: `BtrReturGudang.Winform/Views/ReturnOrderListView.cs`, `BtrReturGudang.Winform/Views/SettingsView.cs`.  

---

## P5 - Verification & Testing

Implementation Status: IMPLEMENTED  
Review Status: GO  

### P5-S06

Title: Test Double Updates, Unit Tests, and Full Solution Verification

Implementation Status: IMPLEMENTED  
Review Status: GO  

Objective:  
Update test mocks to conform to `Task<ApiUploadResultDto>`, author regression tests for URL normalization and error propagation, and execute the complete test suite.

Depends On: P4-S05

Repository: `src/j09-btr-retur-gudang` (Test Projects)

Completion Criteria:  
- `FakeApiClient` (in `SyncServiceTests.cs`) and `MockBtradeApiClient` (in `EndToEndWorkflowIntegrationTests.cs`) updated to return `Task<ApiUploadResultDto>`.
- In `BtradeApiClientTests.cs`:
  - Test added verifying that passing `"http://dev.smart-ics.com:8089/belajar-api"` (without trailing slash) normalizes `BaseUrl` to `"http://dev.smart-ics.com:8089/belajar-api/"` and builds request URI targeting `"http://dev.smart-ics.com:8089/belajar-api/api/return-order"`.
  - Test added verifying that HTTP non-2xx response returns `ApiUploadResultDto` with `Success == false`, correct status code, and extracted error message.
- In `SyncServiceTests.cs`:
  - Test added verifying that upload failure propagates status code and error message into `SyncResultDto` and `BTRG_SyncLog`.
- `dotnet test` executes across all test projects (`BtrReturGudang.Domain.Tests`, `BtrReturGudang.AppTier.Tests`, `BtrReturGudang.Infrastructure.Tests`, `BtrReturGudang.Winform.Tests`) with 100% pass rate (115+ passed, 0 failed).

Notes:  
Final quality gate confirming zero regression.

Implementation Notes:  
- `FakeApiClient` (`SyncServiceTests.cs`) and `MockBtradeApiClient` (`EndToEndWorkflowIntegrationTests.cs`) now implement `IBtradeApiClient` against `Task<ApiUploadResultDto>`: success outcomes use `ApiUploadResultDto.Ok(...)`, offline/failure outcomes use `ApiUploadResultDto.Fail(...)`. Existing boolean result flags were replaced with `ApiUploadResultDto` result properties.
- `BtradeApiClientTests.cs` updated existing upload assertions to `result.Success` / `result.StatusCode`, and added two regression tests:
  - `UploadReturnOrderAsync_NormalizesConfiguredBaseUrl_AndBuildsBelajarApiRequestUri`: constructs the client with `http://dev.smart-ics.com:8089/belajar-api` (no trailing slash), asserts `client.BaseUrl == "http://dev.smart-ics.com:8089/belajar-api/"`, captures the POST request, and resolves it via `RestSharp.BuildUriExtensions.BuildUri` against the normalized base URL, asserting the resolved URI is `http://dev.smart-ics.com:8089/belajar-api/api/return-order`.
  - `UploadReturnOrderAsync_Non2xxResponse_ReturnsFailureWithStatusCodeAndErrorMessage`: a captured 404 response returns `ApiUploadResultDto` with `Success == false`, `StatusCode == 404`, and the extracted `Content` as `ErrorMessage`/`RawResponse`.
- `SyncServiceTests.cs` added `TriggerDocumentUploadAsync_FailedUpload_PropagatesHttpStatusAndErrorIntoResultAndLog`: verifies a `Fail(404, "Return order endpoint not found", ...)` result propagates `HTTP 404` plus the server message into both `SyncResultDto.ErrorMessage` and the persisted `BTRG_SyncLog` `ErrorMessage`, and that the order stays `Draft`.
- Test execution (Debug, net471):
  - `dotnet test BtrReturGudang.sln` → AppTier.Tests 15/15 passed, Infrastructure.Tests 43/43 passed, Winform.Tests 60/60 passed; 0 failed.
  - `dotnet test BtrReturGudang.Domain.Tests\BtrReturGudang.Domain.Tests.csproj` → Domain.Tests 26/26 passed; 0 failed.
  - Aggregate: 144 passed, 0 failed, 0 skipped across all four test projects. The three newly added regression tests pass.
- Finding (informational, pre-existing): `BtrReturGudang.Domain.Tests` is not referenced by `BtrReturGudang.sln`, so a solution-level `dotnet test` does not execute it. It was run explicitly per-project (the plan notes permit per-project execution); no production code or plan structure was changed to accommodate this.
- Changed files: `BtrReturGudang.Infrastructure.Tests/BtradeApiClientTests.cs`, `BtrReturGudang.Infrastructure.Tests/SyncServiceTests.cs`, `BtrReturGudang.Winform.Tests/EndToEndWorkflowIntegrationTests.cs`. No production source was modified.

---

# 6. Change Log

- 2026-10-17: Initial plan creation by ica-architect. Granted `Execution Approval: APPROVED` for autonomous execution via `ica-developer`.
