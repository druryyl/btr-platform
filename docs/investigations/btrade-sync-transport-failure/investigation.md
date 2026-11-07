# Investigation: BTR.Sync (j07-btrade-sync) cannot send or receive data

**Date:** 2026-10-22
**Artifact:** INVESTIGATION
**Target System:** `src/j07-btrade-sync/j07-btrade-sync` (BTR.Sync, .NET Framework 4.8 WinForms)
**Counterpart:** `src/j06-pkl-btrade-api` (Cloud API relay, ASP.NET Core)
**Trigger:** Operator report — the sync client logs, on every cycle:

```text
10.17.40 - Download failed: An error occurred while sending the request.
10.17.40 - Download failed: An error occurred while sending the request.
10.17.40 - Check-in download failed: An error occurred while sending the request.
10.17.40 - Sync failed: An error occurred while sending the request.
10.22.29 - Starting processing cycle...
```

The operator clarified that the failure surfaces on **PackingOrder upload** (the `Sync failed:` line).

---

## 1. Executive Summary

1. **The error is a transport failure, not a server rejection.** The string
   `An error occurred while sending the request.` is the default message of
   `System.Net.Http.HttpRequestException` and it is produced by RestSharp **only**
   when the request never obtained an HTTP response (`StatusCode = 0`). The Cloud API
   never saw the request. It is therefore **not** caused by the payload, the JSON
   shape, the JWT, the business data, or server-side validation.
2. **The client throws away the evidence.** `response.ErrorException` (the inner
   exception that names the actual root cause) and `response.StatusCode` are
   discarded; only `response.ErrorMessage` is logged, and only into an on-screen
   `TextBox` that is lost when the app closes. This is why the outage is
   undiagnosable in the field today. (Same defect class as RCA-2/RCA-3 in
   `docs/investigations/return-order-upload-url-investigation.md` for the sibling
   `j09-btr-retur-gudang` client.)
3. **The remaining candidate root causes are all workstation/network-level**, and
   each is distinguishable by the information the client currently throws away:
   DNS resolution failure, TCP connection refused, port 8089 filtered, system
   proxy interception, or TLS failure after an HTTP→HTTPS redirect.
4. **Four secondary defects were found** in the same code path and are included in
   the plan: unnormalized base URL composition, an unawaited `async void` timer
   cycle that lets runs overlap, no `Authorization` header on the
   Order/CheckIn/Customer/PackingOrder paths, and an inconsistent
   `ServerTargetID` default.

---

## 2. Affected Workflow

| Element | Detail |
|---|---|
| Business area | Inventory Area (Faktur / Packing Order distribution to Gudang) + Sales Area (Order & Check-In download) |
| Systems | BTR.Sync (`j07-btrade-sync`) → Cloud API (`j06-pkl-btrade-api`) |
| Upload direction | BTR.Sync → Cloud: `POST {baseUrl}/api/PackingOrder/bulk` (`PackingOrderUploadSvc.cs:30`) |
| Download direction | Cloud → BTR.Sync: `GET {baseUrl}/api/Order/incremental/{tgl1}/{tgl2}/{serverId}` (`OrderIncrementalDownloadService.cs:27`), `GET {baseUrl}/api/CheckIn/incremental/...` (`CheckInIncrementalDownloadService.cs:27`), `GET {baseUrl}/api/Customer/Updated/{serverId}` (`CustomerDownloadUpdatedService.cs:27`) |
| Configured base URL | `http://dev.smart-ics.com:8089/belajar-api` (`App.config:7`, identical in the deployed `publish-v19` artifact) |
| Impact | No data leaves BTR.Sync to the Cloud, and no Order / Check-In / Customer data is received. BTR.Sync is effectively offline. |

---

## 3. Evidence

### Evidence 1 — Which code line produced each log line

| Log line | Source | Service |
|---|---|---|
| `Download failed: …` | `SyncForm.cs:222` | `OrderIncrementalDownloadService` |
| `Download failed: …` | `SyncForm.cs:501` | `CustomerDownloadUpdatedService` |
| `Check-in download failed: …` | `SyncForm.cs:274` | `CheckInIncrementalDownloadService` |
| `Sync failed: …` | `SyncForm.cs:645` | `PackingOrderUploadSvc` (**the reported PackingOrder upload**) |

All four are produced by the same anti-pattern — only `ErrorMessage` survives:

```csharp
// PackingOrderUploadSvc.cs:45-49
var response = await client.ExecutePostAsync(req);
if (response.StatusCode != HttpStatusCode.OK)
    return (false, response.ErrorMessage);      // StatusCode + ErrorException dropped
```

```csharp
// OrderIncrementalDownloadService.cs:34-38 (identical in CheckIn / Customer)
var response = await client.ExecuteGetAsync(request);
if (!response.IsSuccessful)
    return (false, response.ErrorMessage ?? response.StatusDescription, null);
```

### Evidence 2 — The deployed HTTP stack (identical to the failing deployment)

`publish-v19.7z` → `publish\Application Files\j07-btrade-sync_1_0_0_19\`:

| File | Version |
|---|---|
| `RestSharp.dll` | **112.1.0** (`+bf4008f972257908d666b2884ec7fe330b759424`) |
| `j07-btrade-sync.exe.config` | identical to `App.config` → `btrade-cloud-base-url = http://dev.smart-ics.com:8089/belajar-api` |

### Evidence 3 — Reproduction against the deployed RestSharp 112.1.0

A probe was compiled with `csc.exe` against the **exact `RestSharp.dll` shipped in the
deployed artifact**, driving a local `HttpListener` and a closed port.

**Case C/D — connection refused and DNS failure:**

```text
CASE C status=0 error=An error occurred while sending the request.
  EX[0] System.Net.Http.HttpRequestException :: An error occurred while sending the request.
  EX[1] System.Net.WebException :: Unable to connect to the remote server
  EX[2] System.Net.Sockets.SocketException :: No connection could be made because the target machine actively refused it 127.0.0.1:18099

CASE D status=0 error=An error occurred while sending the request.
  EX[0] System.Net.Http.HttpRequestException :: An error occurred while sending the request.
  EX[1] System.Net.WebException :: The remote name could not be resolved: 'no-such-host-xyz.invalid'
```

**Case E — server answers with an HTTP error status:**

```text
HTTP-400 -> StatusCode=BadRequest         ResponseStatus=Error ErrorMessage=[<null>] Content={"status":"error",...}
HTTP-401 -> StatusCode=Unauthorized        ResponseStatus=Error ErrorMessage=[<null>] Content={"status":"error",...}
HTTP-500 -> StatusCode=InternalServerError ResponseStatus=Error ErrorMessage=[<null>] Content={"status":"error",...}
```

### Evidence 4 — Conclusions drawn from the probe

| Observation | Conclusion |
|---|---|
| `ErrorMessage == "An error occurred while sending the request."` **and** `StatusCode == 0` | The request never reached an HTTP server → transport layer failure |
| Same string is produced by DNS failure **and** by connection refused | The message is generic; the **inner exception** is the only discriminator — and it is discarded |
| For HTTP 400/401/500, `ErrorMessage` is **null** and the status is only in `StatusCode` | A server-side rejection would currently render as `Sync failed: ` with an **empty** reason — which is **not** what the operator sees |
| The reported log line contains the full transport string | **The Cloud API is not the cause.** The failure happens before the request leaves the workstation (DNS/TCP/proxy/TLS) |
| All four failures share the same timestamp (10.17.40) | The cycle steps ran **concurrently** and each failed in under a second — consistent with an immediate connection failure, not a timeout |

### Evidence 5 — Suspects that are ruled out

| Suspect | Verdict | Reason |
|---|---|---|
| Wrong/rotated JWT | Ruled out | A bad token yields HTTP 401 → `ErrorMessage` would be **null** (Evidence 3, Case E). Also, `PackingOrderController` carries no `[Authorize]` (`btrade.webapi/Controllers/PackingOrderController.cs:43-49`). |
| Double-serialized JSON body | Ruled out | The probe shows RestSharp 112 sends an `AddJsonBody(string)` payload **verbatim** (`BODY : {"ListPackingOrder":[…]}`), not re-quoted. The claim in `docs/investigations/faktur-distribution-sync/investigation.md:521` ("JSON double-serialization") does not hold for RestSharp 112. |
| URL segment placeholders unresolved | Ruled out | The probe shows placeholders in the **client base URL** are substituted correctly: `RAW URL : /api/Order/incremental/2026-10-19/2026-10-22/JOG`. |
| `OfficeCode` / `ServerTargetID` empty | Ruled out as the cause of *this* error | These produce HTTP 4xx (route/validation), i.e. `ErrorMessage = null`, not the transport string. They remain latent defects (Findings 6.3 / 6.4). |
| Timeout | Ruled out | The failures are logged within the same second; RestSharp's default timeout is 100 s. |

---

## 4. Root Cause Analysis

### RCA-1 — Primary: the request fails at the transport layer, before reaching the Cloud API

The client cannot establish a usable connection to `http://dev.smart-ics.com:8089`.
Because the inner exception is discarded (RCA-2), the exact sub-cause cannot be
separated from the log alone. The candidates, in likelihood order:

| # | Sub-cause | Distinguishing evidence (currently hidden) |
|---|---|---|
| 1 | **DNS failure** — the branch workstation/network cannot resolve `dev.smart-ics.com` | `WebException: The remote name could not be resolved` |
| 2 | **TCP refused / port 8089 filtered** by the branch firewall | `SocketException: No connection could be made … actively refused` / `timed out` |
| 3 | **System proxy interception** — Windows WinINET proxy is honoured by `HttpClient` by default; an internal host reached through a corporate proxy fails | proxy host in the inner exception / `WebProxy` used |
| 4 | **HTTP→HTTPS redirect + TLS failure** — the Cloud API calls `UseHttpsRedirection()` (`btrade.webapi/Program.cs:18`) and .NET Framework 4.8 defaults `ServicePointManager.SecurityProtocol` to the OS default (often TLS 1.0), so a redirect to `https://…:8089` fails the handshake | `WebException: Could not create SSL/TLS secure channel` / `trust relationship` |

Note: the same host is reachable from other BTR clients (Android BGud / BTrade3 and
`j07-btr-gudang` all target `http://dev.smart-ics.com:8089/belajar-api`), so the
server side is expected to be healthy — the defect is specific to **this
workstation's path to it**.

### RCA-2 — The failure is invisible because the diagnostics are discarded (enabling defect)

| Aspect | Current behaviour | Consequence |
|---|---|---|
| Exception detail | `response.ErrorException` (inner exception chain) never read | The only actionable datum — the actual root cause — is thrown away |
| HTTP status | `response.StatusCode` never logged | Transport failures and HTTP rejections are indistinguishable |
| Response body | `response.Content` never logged | Server-side error messages (`{"status":"error","message":…}`) are invisible |
| Log sink | `SyncForm.LogMessage` writes only to `LogTextBox` (`SyncForm.cs:164-180`); there is **no file logging anywhere in j07** | All evidence is lost when the app is closed or the machine reboots |
| Log level | Transport failure, HTTP 4xx and HTTP 5xx all render as one red line | Operators cannot triage; support cannot triage |

### RCA-3 — Cycle scheduling defect: the "sequential" cycle is actually concurrent

`SyncForm.ProcessingTimer_Elapsed` (`SyncForm.cs:427-434`) awaits
`Task.Run(() => ProcessOrder(...))`, but `ProcessOrder`, `ProcessCheckIn`,
`ProcessReturnOrder` and `ProcessCustomer` are **`async void`**. `Task.Run` therefore
completes at the first `await` inside them, so the timer does **not** wait for the
step to finish. Consequences:

* Steps overlap (consistent with all four errors sharing one timestamp).
* `finally { processingTimer.Start(); }` (`SyncForm.cs:442-446`) restarts the 5-minute
  timer **regardless** of whether the previous cycle finished, so a slow or large
  PackingOrder batch can overlap the next cycle.
* `ProcessPackingOrder` additionally fires its result through
  `result.ContinueWith(...)` (`SyncForm.cs:634-647`) without awaiting, so the upload
  outcome is never correlated with the cycle that started it.

### RCA-4 — Base URL composition is not normalized

Every service composes the endpoint as `$"{baseUrl}/api/..."` with no trimming
(`PackingOrderUploadSvc.cs:30`, `OrderIncrementalDownloadService.cs:27`,
`CheckInIncrementalDownloadService.cs:27`, `CustomerDownloadUpdatedService.cs:27`, …).
Only `BtradeAuthService.cs:90` trims. A trailing slash in
`btrade-cloud-base-url` therefore produces `…/belajar-api//api/PackingOrder/bulk` —
the identical defect that produced HTTP 404 in `j09-btr-retur-gudang`
(`docs/investigations/return-order-upload-url-investigation.md`, RCA-1). Not the cause
of the current incident (it would yield 404, not a transport error) but it is the same
latent trap and the fix belongs in the same place.

### RCA-5 — `ServerTargetID` default is inconsistent

`KonfigurasiForm.cs:35` reads `ServerTargetID` with default `"JOG"`, while every
service reads it with the default empty string (`OrderIncrementalDownloadService.cs:25`,
`CheckInIncrementalDownloadService.cs:25`, `PackingOrderUploadSvc.cs:28`, …). An empty
value produces a malformed route (`…/incremental/{tgl1}/{tgl2}/`) → HTTP 404. Again a
latent 404-class defect, not the current transport error.

### RCA-6 — Authentication asymmetry

`BtradeAuthService` (JWT, S3.6) is wired only into
`BarcodeRegistrationRelayService`, `UserSyncService`, `BarcodeSyncService`,
`DriverSyncService` and `ReturnOrderIncrementalDownloadService`.
The **Order / Check-In / Customer / PackingOrder** paths send **no** `Authorization`
header. The Cloud API currently has no `[Authorize]` on `PackingOrderController`
(`[Authorize]` was removed instance-wide except `UserController`), so this is not the
current cause — but it will break the moment the Cloud enforces auth on those routes,
and it is an inconsistency worth closing in the same pass.

---

## 5. Root Cause Validation (Architect)

| Check | Result |
|---|---|
| Does the reported string appear anywhere in the Cloud API code? | No — it is a .NET Framework `HttpRequestException` message, not a server message |
| Would a server-side rejection produce this string? | No — probe Case E: `ErrorMessage` is `null` for 400/401/500 |
| Would an auth failure produce this string? | No — same as above (HTTP 401 → `ErrorMessage = null`) |
| Would a payload/serialization defect produce this string? | No — the body is transmitted verbatim (probe Case B) |
| Is the failure deterministic across all four endpoints and all cycles? | Yes (every log line, every cycle) → environmental, not data-dependent |
| Is the deployed artifact the same as the repository source? | Yes — `RestSharp 112.1.0` and the same `btrade-cloud-base-url` in `publish-v19` |

**Validation outcome:** the technical cause is confirmed to be at the transport layer.
The sub-cause (DNS / port / proxy / TLS) cannot be separated from the current
artifacts alone and requires one field check (Section 6) or the diagnostic change
planned in `docs/work/btrade-sync-transport-failure/implementation-plan.md`.

---

## 6. Immediate Field Check (run on the affected workstation)

Run before changing any code — this identifies the sub-cause in under a minute:

```powershell
# 1. DNS
Resolve-DnsName dev.smart-ics.com

# 2. TCP reachability to the API port
Test-NetConnection dev.smart-ics.com -Port 8089

# 3. Effective proxy (HttpClient honours WinINET by default)
netsh winhttp show proxy
Get-ItemProperty 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Internet Settings' |
    Select-Object ProxyEnable, ProxyServer, ProxyOverride

# 4. Real API call from the same workstation (bypass any proxy)
curl.exe -v --noproxy '*' http://dev.smart-ics.com:8089/belajar-api/api/PackingOrder/pending/JOG/1
```

| Result | Sub-cause | Action |
|---|---|---|
| Step 1 fails | DNS | Fix DNS (VPN / DNS server / hosts entry) for the branch network |
| Step 1 OK, step 2 fails | Firewall / port / server down | Open TCP 8089 branch→server, or confirm the API is running |
| Steps 1–2 OK, step 4 fails with a proxy error | System proxy | Add the Cloud host to the proxy bypass list, or make the client bypass the proxy for the Cloud host (Plan item P2-3) |
| Step 4 returns a redirect to `https://` and then fails | TLS / redirect | Fix the redirect target and enable TLS 1.2/1.3 on the client (Plan item P2-1) |

---

## 7. Recommended Solution (summary — detail in the implementation plan)

**P1 — Make the failure diagnosable** (fixes the reason this incident is still open):
rich failure reporting (status code + response body + full inner-exception chain +
final request URI), file-based logging that survives app restart, and a
"Test Connection" self-test that walks DNS → TCP → API call.

**P2 — Harden the transport**: explicit TLS 1.2/1.3, `Expect: 100-continue` off,
proxy bypass for the Cloud host, normalized base URL, bounded request timeout.

**P3 — Fix the cycle and the upload contract**: await each step (replace `async void`
with `Task`), stop overlapping cycles, chunk the PackingOrder batch, log the HTTP
status, keep the "mark uploaded only on HTTP 200" rule, and attach the JWT to the
Order/Check-In/Customer/PackingOrder paths.

**P4 — Knowledge**: update the permanent artifacts
(`docs/features/transport` or a new `docs/features/btr-sync/feature.md`,
`docs/foundation/LANDSCAPE.md` if the ownership of the transport layer changes) and
close this investigation.

---

## 8. Severity & Risk

| Item | Assessment |
|---|---|
| Business severity | **High** — no Faktur/Packing Order leaves the branch; Order and Check-In data stops arriving; the branch operates blind against the Cloud |
| Data loss | None from the failure itself. Packing Orders stay `UploadTimestamp = 3000-01-01` and are re-uploaded by the next cycle (H-3 window) — but rows older than H-3 are permanently excluded (pre-existing window defect) |
| Blast radius | The affected workstation/network only; other BTR clients are unaffected |
| Change risk of the plan | Low — the plan is diagnostic and hardening work; no business rule and no data model changes |

---

## 9. Artifacts

| Artifact | Path |
|---|---|
| This investigation | `docs/investigations/btrade-sync-transport-failure/investigation.md` |
| Implementation plan | `docs/work/btrade-sync-transport-failure/implementation-plan.md` |
| Related (sibling defect class) | `docs/investigations/return-order-upload-url-investigation.md` |
| Related (protocol reference) | `docs/investigations/faktur-distribution-sync/investigation.md` |
