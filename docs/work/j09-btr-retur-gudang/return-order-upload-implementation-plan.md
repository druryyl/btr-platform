---
Title: Return Order Upload Route & Diagnostic Fix
Code: BTR-RETUR-UPLOAD-001
Artifact: IMPLEMENTATION-PLAN
Version: 1.0
LastUpdated: 2026-10-17
Status: NOT-STARTED
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
| P1 - Contracts & DTOs | NOT-STARTED | NOT-REVIEWED | 0/1 |
| P2 - Infrastructure Core | NOT-STARTED | NOT-REVIEWED | 0/2 |
| P3 - Sync Engine & Audit | NOT-STARTED | NOT-REVIEWED | 0/1 |
| P4 - Presentation Layer | NOT-STARTED | NOT-REVIEWED | 0/1 |
| P5 - Verification & Testing | NOT-STARTED | NOT-REVIEWED | 0/1 |

---

# 5. Phases

## P1 - Contracts & DTOs

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED  

### P1-S01

Title: Introduce `ApiUploadResultDto` and Update `IBtradeApiClient` Contract

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED  

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

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED  

### P2-S02

Title: Trailing-Slash URL Normalization in `RegistryHelper` and `BtradeApiClient`

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED  

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

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED  

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

---

## P3 - Sync Engine & Audit

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED  

### P3-S04

Title: Sync Service Diagnostic Propagation & `BTRG_SyncLog` Persistence

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED  

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

---

## P4 - Presentation Layer

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED  

### P4-S05

Title: UI Warning Dialog Error Visibility & Settings URL Sanitization

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED  

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

---

## P5 - Verification & Testing

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED  

### P5-S06

Title: Test Double Updates, Unit Tests, and Full Solution Verification

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED  

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

---

# 6. Change Log

- 2026-10-17: Initial plan creation by ica-architect. Granted `Execution Approval: APPROVED` for autonomous execution via `ica-developer`.
