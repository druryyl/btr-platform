---

Code: BTR-RETUR-SESSION-ACTOR-001
Artifact: REVIEW
Slice: P1-S03
ReviewIteration: 1
Decision: GO
---

# Slice P1-S03 — Send Operator Google Email as X-Session-Actor and Implement ResolveSessionAsync

Repository: `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure`

Reviewed against: IMPLEMENTATION-PLAN `BTR-RETUR-SESSION-ACTOR-001` (v1.1);
ARCHITECTURE `BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md` (v1.2 — TD-007, TD-012,
§4, §5.1, §6, §10); DEPLOYMENT-GUIDELINE `docs/work/bgud-google-signin/DEPLOYMENT-GUIDELINE.md`
§3–§5; Cloud contract `src/j06-pkl-btrade-api/btrade.webapi/Controllers/SessionController.cs`
and `btrade.application/UseCase/SessionContextResolver.cs`.

Reviewed slice: P1-S03 (Objective: attach the operator's registered Google email as
`X-Session-Actor` on all outbound Cloud requests and implement anonymous email
resolution against `POST api/session/resolve`).

This REVIEW artifact is written at the explicit instruction of the review run. Under
the standard review policy a GO decision does not require a REVIEW artifact; it is
produced here to preserve verifiable evidence, consistent with the P1-S01/P1-S02
reviews.

# Testing Gate

A GO decision applies only to this slice. It does not authorize testing. Testing may
begin only when the IMPLEMENTATION-PLAN is COMPLETED: every slice has implementation
status IMPLEMENTED and review status GO. That condition is **not met** (P2-S05 and
P3-S06 remain NOT-STARTED; P2-S04 is IMPLEMENTED/NOT-REVIEWED), so the plan-level
`Status` is left unchanged.

# Criteria-by-Criteria Verification

## C1 — Actor sourced from `LastOperatorEmail` (not `LastOperatorName`)

Verdict: PASS

Evidence (`src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure/Http/BtradeApiClient.cs`):

- Field renamed: `private readonly Func<string> _operatorEmailProvider;` (line 28). No
  `_operatorNameProvider` remains in the file.
- Explicit constructor parameter renamed: `Func<string> operatorEmailProvider = null`
  (line 85).
- Default provider reads the email key: `_operatorEmailProvider = operatorEmailProvider
  ?? (() => RegistryHelper.ReadString(RegistryHelper.KeyLastOperatorEmail,
  string.Empty));` (lines 92–93).
- `IRegistryHelper` constructor reads the email member:
  `() => registryHelper?.LastOperatorEmail ?? string.Empty` (line 108).

Cross-checks:
- `rg "LastOperatorName|KeyLastOperatorName|operatorNameProvider|_operatorNameProvider"`
  over `src/j09-btr-retur-gudang` shows **no** matches in `BtradeApiClient.cs`. The
  remaining `LastOperatorName` matches are the interface member, the RegistryHelper
  member (display/attribution persistence, P1-S02), Winform display/attribution call
  sites (out of scope; plan §2 keeps them name-based), and test doubles (P3-S06).
- `git diff` of the file confirms the rename: `- Func<string> operatorNameProvider`
  → `+ Func<string> operatorEmailProvider`, and
  `- RegistryHelper.ReadString(RegistryHelper.KeyLastOperatorName, ...)` →
  `+ RegistryHelper.ReadString(RegistryHelper.KeyLastOperatorEmail, ...)`, and
  `- () => registryHelper?.LastOperatorName ?? string.Empty` →
  `+ () => registryHelper?.LastOperatorEmail ?? string.Empty`.

## C2 — `CreateRequest` attaches `X-Session-Location` (WarehouseCode) and `X-Session-Actor` (email) only when non-blank

Verdict: PASS

Evidence (lines 117–134):

```csharp
var location = _warehouseCodeProvider?.Invoke();
if (!string.IsNullOrWhiteSpace(location))
{
    request.AddHeader(HeaderLocation, location);
}

var actor = _operatorEmailProvider?.Invoke();
if (!string.IsNullOrWhiteSpace(actor))
{
    request.AddHeader(HeaderActor, actor);
}
```

`HeaderLocation = "X-Session-Location"` (line 21), `HeaderActor = "X-Session-Actor"`
(line 22). The location provider is unchanged (`_warehouseCodeProvider`, sourced from
`WarehouseCode`, line 90 / 107). The actor is now sourced from the email provider and
attached only when non-blank. Both headers are attached through this single
`CreateRequest`, so all outbound Cloud requests (return-order, barcode-registration,
master-data GETs) carry the email actor.

## C3 — Class-level XML documentation updated from "OperatorName" to "Google email"

Verdict: PASS

Evidence (lines 14–18):

```csharp
/// RestSharp HTTP API client targeting j06-pkl-btrade-api relay service.
/// Injects X-Session-Location (WarehouseCode) and X-Session-Actor (registered Google email) headers on all requests.
/// Traceable to: ARCHITECTURE §4 TD-007, TD-012, §5.1, §6, P2-S05.
```

`git diff` confirms the change from `X-Session-Actor (OperatorName)` to
`X-Session-Actor (registered Google email)`, and the traceability now cites TD-012.

## C4 — `ResolveSessionAsync(string email)` implemented against the Cloud contract

Verdict: PASS

Evidence (lines 251–333):

- **Request shape.** `var request = new RestRequest("api/session/resolve", Method.Post);`
  with `request.AddJsonBody(new { Email = email });` (lines 253–254) → JSON body
  `{ "Email": "<email>" }`. This matches the Cloud `SessionResolveRequest(string Email)`
  record (`SessionController.cs` line 62) and the DEPLOYMENT-GUIDELINE §5.1.1 contract.
- **No `X-Session-*` headers.** The method does **not** call `CreateRequest`; it builds
  a bare `RestRequest`, so `X-Session-Location`/`X-Session-Actor` are never attached.
  Confirmed by inspection (the only header-producing path is `CreateRequest`, lines
  117–134) and by the Cloud endpoint being anonymous.
- **2xx → `Ok(statusCode, rawResponse)` populated.** Lines 260–267 call
  `SessionResolveResultDto.Ok((int)response.StatusCode, response.Content)` and then
  `PopulateResolvedIdentity(result, response.Content)`.
  `PopulateResolvedIdentity` (lines 286–333) unwraps the JSendOk envelope
  (`data`/`Data`, matching `{ "status":"success", "code":"200", "data": {...} }`
  asserted by the j06 `SessionResolveTests`) and tolerates a bare payload; it reads
  `UserId`/`UserName`/`RoleId` (PascalCase, with camelCase fallback) and the
  `Warehouses` array into `LocationId`/`ServerId` — matching the Cloud
  `SessionResolveResponse` / `SessionWarehouseDto` records verbatim.
- **Non-2xx → `Fail(statusCode, errorDetail, rawResponse)`.** Lines 269–272 call
  `SessionResolveResultDto.Fail((int)response.StatusCode, ExtractErrorDetail(response),
  response?.Content)`. `ExtractErrorDetail` (lines 349–372) uses exactly the existing
  preference order: response `Content` → `ErrorMessage` → `StatusDescription` →
  `HTTP <code>`. The Cloud 400 refusal body
  (`{ "status":"Bad Request","code":"400","data":"Google account is not registered or is
  not active. Contact your administrator." }`) is therefore carried in `ErrorMessage`
  (raw content) and `RawResponse`.
- **Transport exception → `Fail(0, ex.Message, null)`.** Lines 274–277 catch and return
  `SessionResolveResultDto.Fail(0, ex.Message, null)`, mirroring the existing upload
  methods.

## C5 — `BtrReturGudang.Infrastructure` compiles with zero errors

Verdict: PASS

Command (run independently by the Reviewer):

```
dotnet build "D:\Project.Private\btr-platform\src\j09-btr-retur-gudang\BtrReturGudang.Infrastructure\BtrReturGudang.Infrastructure.csproj" -v minimal
```

Observed result:

```
BtrReturGudang.Domain -> ...\BtrReturGudang.Domain.dll
BtrReturGudang.AppTier -> ...\BtrReturGudang.AppTier.dll
BtrReturGudang.Infrastructure -> ...\BtrReturGudang.Infrastructure.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)
```

The aggregate Infrastructure build gate deferred to P1-S03 by P1-S02 (the prior
`CS0535 'BtradeApiClient' does not implement interface member
'IBtradeApiClient.ResolveSessionAsync(string)'`) is closed: the project now builds with
0 errors, 0 warnings.

# Cloud Contract Conformance (j06 `POST api/session/resolve`)

Verified against `SessionController.cs` and `SessionResolveTests.cs`:

| Aspect | Cloud | j09 client | Match |
| :--- | :--- | :--- | :--- |
| Route / method | `POST /api/session/resolve` | `POST api/session/resolve` | Yes |
| Request body field | `Email` (`SessionResolveRequest(string Email)`) | `{ Email = email }` → `{"Email":...}` | Yes |
| Success envelope | `JSendOk` → `{ status, code, data: {...} }` | unwraps `data`/`Data` | Yes |
| Success payload | PascalCase `UserId`, `UserName`, `RoleId`, `Warehouses[{LocationId, ServerId}]` | reads those names (camelCase fallback) | Yes |
| Refusal | `400` JSend failure (`data` = message) | `Fail(status, ExtractErrorDetail, raw)` | Yes (per criterion) |
| Auth | anonymous, no `X-Session-*` | bare request, no `X-Session-*` | Yes |

# Architecture Compliance

- TD-007 — `X-Session-Actor` is the operator's registered Google email
  (`RegistryHelper.LastOperatorEmail`), never the display name; the free-text name no
  longer participates in Cloud identity resolution. The client sends the email only.
  PASS.
- TD-012 — resolution calls the anonymous `POST api/session/resolve` with
  `{ "Email": ... }` and no session headers; a `2xx` yields `UserId`/`UserName`/`RoleId`
  and the warehouse mapping; a `400` yields a failed result carrying the refusal detail.
  PASS.
- §5.1 / §6 — `BtradeApiClient` owns RestSharp calls, `X-Session-*` headers, and
  `ResolveSessionAsync`; the DI graph is unchanged (`Program.cs` constructs
  `new BtradeApiClient(sp.GetRequiredService<IRegistryHelper>())`, the
  `IRegistryHelper`-based constructor), so no `Program.cs` change is required. PASS.
- §10.4 — all outbound session-context requests send `X-Session-Location` and
  `X-Session-Actor`; the actor is never a free-text name, username, or password. PASS.
- Scope boundary — P1-S03 is bounded to `BtradeApiClient.cs` (Infrastructure). No UI
  (P2-S04/P2-S05), no test-double work (P3-S06), and no local record-attribution
  semantics change. PASS.

# Root-Cause Correction (409)

The 409 root cause (INVESTIGATION Findings 1–2: j09 sent a free-text operator name as
`X-Session-Actor`, which the Cloud could not resolve through `BTRADE_User.Email`) is
addressed: the actor is now the registered Google email. For a correctly configured and
registered operator, the Cloud `ResolveAccount` lookup succeeds and the 409 no longer
occurs. The remaining operational prerequisite (email registered in Main Office and
projected to Cloud) is explicitly out of the client's scope (plan §2, Investigation
Assumption 2/Constraint).

# Dependency Verification

- P1-S03 `Depends On: P1-S01, P1-S02`. Both are IMPLEMENTED with review status GO.
  `IBtradeApiClient.ResolveSessionAsync`/`SessionResolveResultDto`/`SessionWarehouseDto`
  (P1-S01) and `RegistryHelper.KeyLastOperatorEmail`/`LastOperatorEmail` (P1-S02) are
  present and consumed by this slice. Dependencies satisfied. PASS.

# Findings

## RV-001

Severity: NOTE

Description: The working-tree `git diff` of `BtradeApiClient.cs` against `HEAD` contains
changes beyond P1-S03's stated scope (return-type refactor to `ApiUploadResultDto`,
`CustomerCode`/`Wilayah` payload/parse additions, base-URL/client-resolution refactor,
`DownloadMasterDataAsync` server-target defaulting). These trace to concurrent,
separately-planned desktop-platform work (the return-order-upload and customer-sync
plans present in the working tree) and were not introduced by P1-S03; they do not alter
any P1-S03 criterion. This is the same repository-hygiene condition the P1-S01 (RV-001)
and P1-S02 (RV-001) reviews recorded.

Evidence: `git diff -- .../Http/BtradeApiClient.cs` shows the mixed changes; the file's
own traceability for the upload methods cites `ARCHITECTURE-NOT-REQUIRED, P2-S03` (the
prior plan), not this slice.

Required Correction: None for P1-S03 acceptance. No acceptance impact.

Status: OPEN (informational; no acceptance impact)

## RV-002

Severity: NOTE

Description: The class-level XML documentation line 17 still cites `P2-S05` in its
traceability token (`... §6, P2-S05`), a stale reference carried over from the prior
plan's numbering for the API-client slice. The required documentation change
("OperatorName" → registered Google email) was made correctly, so this does not affect
the criterion.

Evidence: `BtradeApiClient.cs` line 17; contrast with the method-level `ResolveSessionAsync`
doc which cites `ARCHITECTURE §4 TD-012`.

Required Correction: None required for acceptance. Optional traceability cleanup.

Status: OPEN (informational; no acceptance impact)

## RV-003

Severity: NOTE

Description: On a non-2xx resolution response, `ErrorMessage` is populated with the raw
response content (per the criterion's mandated `ExtractErrorDetail` order — content
first), i.e. the full JSend failure JSON rather than only the inner refusal message.
Downstream P2-S05 must parse the JSend `data`/`message` before displaying it so the
operator sees the refusal verbatim ("Google account is not registered or is not active.
Contact your administrator.") rather than raw JSON. This behavior is exactly what the
P1-S03 criterion prescribes and is therefore not a P1-S03 defect.

Evidence: `BtradeApiClient.cs` lines 269–272 and `ExtractErrorDetail` lines 349–372;
Cloud 400 body shape in `SessionController.cs` line 57 and `SessionResolveTests.cs`
lines 97–99.

Required Correction: None for P1-S03. Recorded as an input to P2-S05.

Status: OPEN (informational; downstream input)

## RV-004

Severity: NOTE

Description: The `BtrReturGudang.Infrastructure.Tests` project is currently stale —
`BtradeApiClientTests.FakeRegistryHelper` (and `SyncServiceTests`) do not implement
`IRegistryHelper.LastOperatorEmail`, and the actor assertions still expect the operator
name. The test project therefore does not compile until P3-S06 updates the test doubles
and regression assertions. This is P3-S06's explicit deliverable ("Every `IRegistryHelper`
test double ... implements `LastOperatorEmail`"; "`BtradeApiClientTests` asserts outbound
requests carry `X-Session-Actor == LastOperatorEmail`"), not a P1-S03 criterion.

Evidence: `rg "LastOperatorEmail"` over the two test files returns no matches;
`BtradeApiClientTests.cs` lines 21, 61, 75, 84–99, 152–156.

Required Correction: None for P1-S03. Owned by P3-S06.

Status: OPEN (informational; no acceptance impact)

## RV-005

Severity: NOTE

Description: The plan front-matter `Status` remains `NOT-STARTED` while several slices
are IMPLEMENTED/reviewed. This is a plan bookkeeping inconsistency recorded for
traceability; the COMPLETED condition is not met (P2-S05, P3-S06 NOT-STARTED), so the
plan-level `Status` is left unchanged by the Reviewer (consistent with P1-S01 RV-003 and
P1-S02 RV-002).

Evidence: Plan v1.1 front matter `Status: NOT-STARTED`; §4 Progress Summary.

Required Correction: None from the Reviewer. Plan owner/Architect may align the
plan-level status when all slices complete.

Status: OPEN (informational; no acceptance impact)

# Current Decision

GO

No BLOCKER or MAJOR findings. All five completion criteria are satisfied by the actual
files and independently confirmed by the Reviewer's own build (0 errors, 0 warnings).
The actor is sourced from `LastOperatorEmail` with no lingering name-based actor, the
`ResolveSessionAsync` request/response shape matches the Cloud `POST api/session/resolve`
contract (body field, status mapping, JSend `data` unwrapping, error-detail order), the
409 root cause is corrected, and both declared dependencies (P1-S01, P1-S02) are
IMPLEMENTED/GO.

# Re-Review History

None — this is the initial review record for P1-S03.
