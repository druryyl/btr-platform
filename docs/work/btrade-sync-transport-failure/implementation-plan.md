---
Title: BTR.Sync Transport Failure Diagnosis & Hardening
Code: BTR-SYNC-NET-001
Artifact: IMPLEMENTATION-PLAN
Version: 1.0
LastUpdated: 2026-10-22
Status: NOT-STARTED
Execution Approval: PENDING
---

# 1. Objective

Make `j07-btrade-sync` (BTR.Sync) report **why** a Cloud API call failed, harden the
HTTP transport so the common field failures cannot recur silently, and correct the
cycle/upload defects found alongside the incident.

Planning Mode: FEATURE-PLANNING

Referenced artifacts:

- INVESTIGATION: `docs/investigations/btrade-sync-transport-failure/investigation.md`
- Related (sibling defect class, j09): `docs/investigations/return-order-upload-url-investigation.md`
- Protocol reference: `docs/investigations/faktur-distribution-sync/investigation.md`

Architecture Applicability: ARCHITECTURE-NOT-REQUIRED

No architectural target-state artifact is required. The change is diagnostic and
hardening work inside the existing technical structure; no business rule, workflow or
data model changes. The current codebase is the source of current technical truth.

**Out of scope:** fixing the branch network itself (DNS / firewall / proxy). That is an
operations action, driven by the field check in Section 6 of the investigation and by
the new self-test delivered in P1-4.

---

# 2. Planning Scope

The scope is bounded to `src/j07-btrade-sync/j07-btrade-sync`:

1. **Shared HTTP layer** — one place that builds the `RestClient`, normalizes the base
   URL, applies the transport hardening, and formats a rich failure description.
2. **Failure reporting** — every service returns status code + response body + inner
   exception chain instead of a bare `ErrorMessage`.
3. **Durable logging** — the log survives app restart (file sink next to the existing
   on-screen `TextBox`).
4. **Connectivity self-test** — a button that walks DNS → TCP → real API call and
   reports the failing stage.
5. **Cycle correctness** — the timer cycle actually waits for each step; runs no longer
   overlap.
6. **Upload contract** — chunked PackingOrder batches, HTTP status surfaced, JWT
   attached to the previously unauthenticated paths.

---

# 3. Dependencies

- External: Cloud API relay `http://dev.smart-ics.com:8089/belajar-api` (j06-pkl-btrade-api).
- Runtime: .NET Framework 4.8, RestSharp 112.1.0 (already referenced).
- Sibling precedent: `j09-btr-retur-gudang` already solved the base-URL and
  rich-error classes (`BtradeApiClient.cs`, `RegistryHelper.NormalizeApiUrl`) — reuse
  the approach, do not introduce a new pattern.
- No dependency on any other slice; the phases below are independently shippable.

---

# 4. Progress Summary

| Phase | Implementation Status | Review Status | Progress |
| :--- | :--- | :--- | :--- |
| P1 - Shared HTTP layer & diagnostics | NOT-STARTED | NOT-REVIEWED | 0/5 |
| P2 - Transport hardening | NOT-STARTED | NOT-REVIEWED | 0/4 |
| P3 - Cycle & upload correctness | NOT-STARTED | NOT-REVIEWED | 0/3 |
| P4 - Authentication parity | NOT-STARTED | NOT-REVIEWED | 0/1 |
| P5 - Verification & knowledge | NOT-STARTED | NOT-REVIEWED | 0/2 |

---

# 5. Phases

## P1 - Shared HTTP layer & diagnostics

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

### P1-1 — New shared component `Shared/CloudApiClient.cs`

Create `j07_btrade_sync.Shared.CloudApiClient` (static or injectable helper) that
centralises:

- `ResolveBaseUrl()` — read `btrade-cloud-base-url` from `App.config`; **trim trailing
  `/`**; reject an empty value or a value without a scheme with an explicit message
  (`"Cloud base URL is not configured (AppSettings 'btrade-cloud-base-url')"`).
- `BuildEndpoint(string relativePath)` — compose `baseUrl.TrimEnd('/') + "/" + path`.
  All services stop composing URLs inline.
- `CreateClient()` — `RestClient` with `RestClientOptions` carrying an explicit
  `Timeout` (30 s) and `MaxTimeout`; the proxy policy from P2-3.
- `DescribeFailure(RestResponse response)` — returns a single string containing:
  - `ResponseStatus` (e.g. `Error`, `TimedOut`, `Aborted`),
  - `StatusCode` (e.g. `0` for transport failures, `401`, `404`, `500`),
  - `ErrorMessage`,
  - the **inner exception chain** (`ex.InnerException…`, up to 5 levels, with types),
  - the final request URI,
  - a truncated `response.Content` (max 500 chars) when present.

Format example (target output for the operator):

```text
NETWORK/DNS failure | status=0 | responseStatus=Error | uri=http://dev.smart-ics.com:8089/belajar-api/api/PackingOrder/bulk | error=An error occurred while sending the request. | inner=System.Net.WebException: The remote name could not be resolved: 'dev.smart-ics.com' -> System.Net.Sockets.SocketException: No such host is known.
```

### P1-2 — Replace the `ErrorMessage`-only pattern in every service

Files: `Service/OrderIncrementalDownloadService.cs`, `Service/CheckInIncrementalDownloadService.cs`,
`Service/ReturnOrderIncrementalDownloadService.cs`, `Service/CustomerDownloadUpdatedService.cs`,
`Service/CustomerClearUpdateFlagService.cs`, `Service/PackingOrderUploadSvc.cs`,
`Service/BrgSyncService.cs`, `Service/KategoriSyncService.cs`, `Service/SalesPersonSyncService.cs`,
`Service/WilayahSyncService.cs`, `Service/DriverSyncService.cs`, `Service/UserSyncService.cs`,
`Service/CustomerUploadService.cs`, `Service/BarcodeSyncService.cs`,
`Service/BarcodeRegistrationRelayService.cs`.

Change every `return (false, response.ErrorMessage ?? response.StatusDescription, …)` to
`return (false, CloudApiClient.DescribeFailure(response), …)`.

`PackingOrderUploadSvc.UploadPackingOrder` additionally changes its guard from
`response.StatusCode != HttpStatusCode.OK` to `!response.IsSuccessful`, and returns
`DescribeFailure(response)` — so an HTTP 400/401/500 stops rendering as an empty
"Sync failed:" line.

**Behaviour preserved:** no endpoint, payload, header or business rule changes. Only
the failure text becomes more specific.

### P1-3 — Durable log sink

- Add `Shared/FileLogWriter.cs`: appends to
  `%LOCALAPPDATA%\BTRApp\j07-btrade-sync\sync-yyyyMMdd.log`, one line per event
  (`HH:mm:ss.fff | LEVEL | message`), with a 2 MB size guard (roll to `.1`).
- `SyncForm.LogMessage` writes to the `LogTextBox` **and** the file sink. Keep the
  existing on-screen behaviour and colouring unchanged.
- Log the exception chain of caught exceptions (`ex.ToString()`) instead of only
  `ex.Message` in the `catch` blocks of `SyncForm.cs`.

### P1-4 — Connectivity self-test (UI)

- Add a **"Test Connection"** button on `KonfigurasiForm` (next to the existing
  server-ID settings) — the form where the operator already configures the sync.
- The self-test runs three stages and reports the first failing stage:
  1. **DNS** — `Dns.GetHostAddresses(host)`; report the resolved addresses.
  2. **TCP** — `TcpClient.ConnectAsync(host, port)` with a 5 s timeout; report the
     resolved port and the outcome.
  3. **API** — one real call through the same `CloudApiClient` used by the services
     (e.g. `GET {baseUrl}/api/PackingOrder/pending/{serverId}/1`); report HTTP status
     and body.
- The result is shown in a message box **and** written to the file log.
- No new configuration is required to run the test.

### P1-5 — Surface the failure in the sync log

`SyncForm.LogMessage` currently renders every failure identically. Keep the red colour
but append the `ResponseStatus`/status code when present, so the operator can tell
"never reached the server" from "server rejected the request" at a glance.

---

## P2 - Transport hardening

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

### P2-1 — TLS protocol floor

In `Program.cs` (before `Application.Run`), set:

```csharp
ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
```

Rationale: .NET Framework 4.8 defaults to the OS `SchUseStrongCrypto` setting, which
on many branch workstations is TLS 1.0 only. Any HTTP→HTTPS redirect (the Cloud API
calls `UseHttpsRedirection()`, `btrade.webapi/Program.cs:18`) would then fail the
handshake with the exact transport error reported in the incident.

### P2-2 — Disable `Expect: 100-continue`

Set `ServicePointManager.Expect100Continue = false;` next to P2-1. The probe shows
RestSharp 112 sends `Expect: 100-continue` on POST bodies; several middleboxes answer
neither `100 Continue` nor a final status, which surfaces as a transport failure on
POST only (the PackingOrder upload).

### P2-3 — Proxy policy for the Cloud host

`HttpClient` honours the Windows system proxy by default, which breaks internal hosts
on branch workstations that are configured with a corporate proxy.

- Default: **bypass the proxy** for the Cloud API host (it is an internal host).
- Escape hatch: registry key `CloudApiUseProxy` (`HKCU\DrurySoftware\BTRApp`) —
  `"1"` forces the system proxy, `"0"` (default) bypasses it.
- Implement through `RestClientOptions.ConfigureMessageHandler` with an
  `HttpClientHandler { UseProxy = false }` when bypassing.

### P2-4 — Base URL normalization everywhere

Replace every inline `$"{baseUrl}/api/..."` with `CloudApiClient.BuildEndpoint("api/...")`
(P1-1). This closes the trailing-slash trap that produced HTTP 404 in the sibling
`j09-btr-retur-gudang` client, and makes a misconfigured URL fail with an explicit
message instead of a 404.

---

## P3 - Cycle & upload correctness

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

### P3-1 — Await each cycle step

`SyncForm.ProcessingTimer_Elapsed` (`SyncForm.cs:427-434`) awaits `Task.Run(() => …)`
around `async void` methods, so the cycle does not actually wait.

- Convert `ProcessOrder`, `ProcessCheckIn`, `ProcessReturnOrder`, `ProcessCustomer` and
  `ProcessPackingOrder` from `async void` to `async Task`.
- Call them with `await` directly (no `Task.Run` wrapper) — the continuations already
  resume on the UI context.
- The timer restarts only after the whole cycle completes (the existing `finally`
  block already does this — it becomes correct once the steps are awaited).

### P3-2 — No overlapping cycles

Add a guard (`_cycleRunning` flag) so a cycle that is still running when the timer
fires is skipped (and logged) instead of overlapping with itself. This prevents two
PackingOrder uploads of the same batch from racing on `UploadTimestamp`.

### P3-3 — Chunk the PackingOrder upload

`PackingOrderUploadSvc.UploadPackingOrder` sends the entire H-3 batch in one
unbounded POST (`docs/investigations/faktur-distribution-sync/investigation.md` §4.1
"Batch size: Unlimited").

- Send in batches of **max 200 orders per request** (constant, documented in code).
- Mark `UploadTimestamp` per batch **only after that batch returns HTTP 200**, so a
  partial failure re-uploads only the failed batch, not the whole window.
- Log per batch: batch size, HTTP status, duration.

Behaviour preserved: the "upload timestamp is set only on HTTP 200" rule and the
H-3 window are unchanged.

---

## P4 - Authentication parity

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

### P4-1 — Attach the JWT to the previously unauthenticated paths

`BtradeAuthService` (S3.6) is already used by the Barcode / User / Driver / ReturnOrder
services. Attach it — with the existing "401 → refresh once → retry" pattern — to:

- `PackingOrderUploadSvc` (upload)
- `OrderIncrementalDownloadService` (download)
- `CheckInIncrementalDownloadService` (download)
- `CustomerDownloadUpdatedService` / `CustomerClearUpdateFlagService`

The Cloud API has no `[Authorize]` on `PackingOrderController` today, so this change is
behaviour-neutral now and prevents a 401 storm if the Cloud enforces auth later.

**Dependency:** `SyncUserId` / `SyncPassword` / `SyncLocationId` must exist in
`HKCU\DrurySoftware\BTRApp` (or the `btrade-sync-*` app settings). If they are absent,
the login call fails — the service must then fall back to the current unauthenticated
request instead of failing the whole sync (log a warning once).

---

## P5 - Verification & knowledge

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

### P5-1 — Verification procedure

| # | Check | Expected result |
| :--- | :--- | :--- |
| 1 | Point the app at a closed port (`http://127.0.0.1:1`) and run one cycle | The log shows the inner exception chain (`SocketException: … actively refused`) and `status=0` — not a bare "An error occurred while sending the request." |
| 2 | Point the app at a non-resolvable host | The log shows `The remote name could not be resolved` |
| 3 | Run against the live Cloud API | HTTP 200 and the normal "Download done" / "Done" lines |
| 4 | Make the server answer 401/500 | The log shows the status code and the response body instead of an empty reason |
| 5 | "Test Connection" button with a wrong DNS name / blocked port / healthy API | Each stage reports its own outcome |
| 6 | Set `btrade-cloud-base-url` with a trailing slash | Requests still resolve to `…/belajar-api/api/…` (no `//`) |
| 7 | Observe one full cycle | Steps run in order; the next cycle starts only after the previous one finished |
| 8 | Upload > 200 packing orders in one window | The log shows multiple batches, each marked uploaded only on HTTP 200 |

### P5-2 — Knowledge updates

- Add `docs/features/btr-sync/feature.md` (new permanent artifact) describing the
  sync client's responsibilities, endpoints, configuration keys, failure semantics and
  the operator runbook (including the Section 6 field check).
- Update `docs/investigations/faktur-distribution-sync/investigation.md` §4.1/§4.2,
  where the "JSON double-serialization" finding (§"Ranked failure points" #15) is
  disproved for RestSharp 112, and where the batch size is now bounded (P3-3).
- Update `docs/foundation/LANDSCAPE.md` only if the ownership of the transport layer
  changes — it does not in this plan.

---

# 6. Risks & Mitigation

| Risk | Likelihood | Mitigation |
| :--- | :--- | :--- |
| Logging the full exception chain exposes sensitive data (URLs, tokens) | Low | The chain contains URIs and OS errors, not credentials; the JWT is never logged. Review before enabling file logging in a shared environment |
| File logging fills the disk | Low | 2 MB per day-file guard with rollover; path under `%LOCALAPPDATA%` |
| TLS floor change breaks an internal endpoint that only supports TLS 1.0 | Very low | The Cloud API is reached over plain HTTP on 8089 today; TLS 1.2/1.3 only widens support |
| Bypassing the proxy breaks a site that genuinely reaches the Cloud through a proxy | Low | Registry override `CloudApiUseProxy=1` restores the system proxy without a rebuild |
| Chunking the upload changes server load | Low | 200 orders/request is far below the server's single-transaction limit |
| Auth parity introduces a login failure that blocks the sync | Medium | P4-1 specifies a graceful fallback to the current unauthenticated behaviour with a warning |
| Awaiting the cycle lengthens the interval between runs | Low | The cycle is bounded by the 30 s request timeout; the 5-minute timer restarts after completion, preserving the existing schedule |

---

# 7. Out of Scope

- Branch network / DNS / firewall configuration (operations action, driven by the
  field check).
- Cloud API changes (`src/j06-pkl-btrade-api`) — no server-side change is required.
- The H-3 date window and the `UploadPackingOrder` default-off behaviour — pre-existing
  business decisions, preserved.
- `j07-btr-gudang`, `j09-btr-retur-gudang` and the Android clients — different clients,
  separate scope.

---

# 8. Execution Approval

Implementation must not start before explicit approval of this plan
(`Execution Approval: PENDING`). On approval, execute P1 → P2 → P3 → P4 → P5 in order:
P1 makes the next outage diagnosable, P2 prevents the common transport failures, P3
corrects the cycle and the upload contract, P4 aligns authentication, P5 verifies and
updates the permanent knowledge artifacts.
