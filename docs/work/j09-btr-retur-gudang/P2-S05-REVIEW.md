---

Code: BTR-RETUR-SESSION-ACTOR-001
Artifact: REVIEW
Slice: P2-S05
ReviewIteration: 2
Decision: GO
---

# Slice P2-S05 - Validate Operator Email via Session Resolve and Surface Refusal Clearly

Repository: `src/j09-btr-retur-gudang/BtrReturGudang.Winform`
Reviewed against: IMPLEMENTATION-PLAN `BTR-RETUR-SESSION-ACTOR-001` (v1.1);
ARCHITECTURE `BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md` (v1.2 - TD-007, TD-012,
§5.1/§5.2 SCR-07, §6, §10.4, §11.11/§11.13); j06 Cloud contract
`POST api/session/resolve` (`SessionController.cs`, `SessionResolveTests.cs`); and
`docs/work/bgud-google-signin/DEPLOYMENT-GUIDELINE.md` §3-§5.

Reviewed slice: P2-S05 (Objective: validate the configured operator email through
`POST api/session/resolve` and present the Cloud refusal message instead of an
opaque HTTP 409 at upload time).

# Testing Gate

A GO decision applies only to this slice and does not authorize testing. Testing
may begin only when the IMPLEMENTATION-PLAN is COMPLETED: every slice has
implementation status IMPLEMENTED and review status GO. That condition is **not
met** (P3-S06 is NOT-STARTED and P2-S05 is NO-GO), so the plan-level `Status` is
left unchanged at `NOT-STARTED`.

# Criteria Verification

## C1 - SettingsView "Verify Operator" action calls ResolveSessionAsync and shows outcome

Verdict: PARTIAL - the action/wiring is correct, but the failure text does not
surface the Cloud refusal message described by the criterion.

Evidence - action and success path (satisfied):

- Button `_btnVerifyOperator` ("🔐 Verifikasi Operator") is added to the identity
  card and wired: `_btnVerifyOperator.Click += async (s, e) => await
  OnVerifyOperatorClickAsync();` (`Views/SettingsView.cs` lines 256-269).
- `OnVerifyOperatorClickAsync` (lines 555-618) resolves the configured email. With
  the shared validator it calls `_sessionValidator.ValidateAsync()`, which invokes
  `IBtradeApiClient.ResolveSessionAsync(_registryHelper.LastOperatorEmail)`
  (`Services/OperatorSessionValidator.cs` lines 110-142, `OperatorEmail` line 39;
  `BtradeApiClient.ResolveSessionAsync` lines 251-278). The direct fallback path
  (lines 581-586) also calls `_apiClient.ResolveSessionAsync(email)`.
- Success shows the resolved identity verbatim: line 592
  `"✅ Operator terverifikasi: {result.UserName} (Role: {result.RoleId})"`.
  SATISFIED.

Evidence - failure path (NOT satisfied):

- The failure branch (lines 599-606) displays the DTO's raw `ErrorMessage`:
  `_lblSessionStatus.Text = !string.IsNullOrWhiteSpace(result.ErrorMessage) ?
  result.ErrorMessage : OperatorSessionValidator.DefaultRefusalMessage;`.
- For a real Cloud refusal, `BtradeApiClient.ResolveSessionAsync` builds the failed
  result with `ExtractErrorDetail(response)` (line 271). `ExtractErrorDetail`
  returns `response.Content` first (lines 349-372). The Cloud emits the 400 refusal
  as a JSend JSON envelope: `return BadRequest(new JSend(400, "Bad Request",
  ex.Message));` (`src/j06-pkl-btrade-api/btrade.webapi/Controllers/SessionController.cs`
  line 57), and `SessionResolveTests.Resolve_WithUnmappedEmail_Returns400JSendFailure`
  asserts the body is an object with `status`/`code`/`data` where `data` contains
  "administrator" (`src/j06-pkl-btrade-api/btrade.webapi.Test/SessionResolveTests.cs`
  lines 96-99).
- Therefore `result.ErrorMessage` is the full raw envelope string, e.g.
  `{"status":"Bad Request","code":"400","data":"Google account is not registered or
  is not active. Contact your administrator."}`, and the status label shows that raw
  envelope verbatim - not the criterion's expected text
  `"Google account is not registered or is not active. Contact your administrator."`.
- No JSend `data`/`message` parsing exists anywhere in the presentation layer:
  `rg "JsonDocument|JsonElement|JObject|GetProperty|RawResponse|\"data\""` over
  `BtrReturGudang.Winform` returns no matches. `DefaultRefusalMessage` is only used
  when `ErrorMessage` is blank (only the unconfigured-email path), so it does not
  cover a real Cloud 400.

## C2 - Startup and operator-change validation; unresolved operator warned and uploads pre-empted

Verdict: SATISFIED (validation, warning, and pre-emption logic), with a residual
NOTE on the pre-emption guarantee window (RV-004).

Evidence:

- Startup validation: `MainForm` wires `Shown += async (s, e) => await
  ValidateOperatorSessionAsync();` (line 95); `ValidateOperatorSessionAsync`
  (lines 562-594) calls `_sessionValidator.ValidateAsync()`, updates the status
  strip, and warns on a definite refusal via `WarnUnresolvedOperator`
  (lines 599-609).
- Operator change: `SettingsView.OnSwitchOperatorClickAsync` re-verifies after the
  dialog saves (line 545); `MainForm.OnSwitchOperatorClick` calls
  `ValidateOperatorSessionAsync()` after a dialog save (line 550).
- Pre-emption of every Cloud upload path:
  - Background/manual sync: `MainForm.ExecuteBackgroundSyncPassAsync` returns early
    with `RefusalMessage` and the warning when `_sessionValidator.IsUnresolved`
    (lines 698-704). This covers the 5-minute timer, the manual "Sync Sekarang"
    button, and the initial post-startup pass (lines 502, 513-518, 689). Barcode
    registration upload is performed by the sync pass, so it is covered.
  - Return-order "Simpan & Upload": `ReturnOrderEntryView.SaveAndUploadAsync` saves
    the draft locally, then pre-empts `CommitAndUploadAsync` and returns a failed
    `SyncResultDto` carrying the message when unresolved (lines 1295-1308).
  - Return-order "Upload Ulang": `ReturnOrderListView.OnUploadOrderClickAsync`
    pre-empts `CommitAndUploadAsync`/`TriggerDocumentUploadAsync` with the message
    (lines 836-845).
- Definite-refusal gating (P1-S03 offline tolerance preserved): `IsUnresolved`
  returns false when no validation has run, false on success, and only true on a
  blank configured email or a server verdict with `StatusCode >= 400`
  (`OperatorSessionValidator.cs` lines 71-87). A transport failure returns
  `Fail(0, ...)` (`ValidateAsync` lines 130-138), so `StatusCode 0` does not block -
  the offline-tolerant upload path is intact and P1-S03 is not regressed.

## C3 - BtrReturGudang.Winform compiles with zero errors

Verdict: SATISFIED

Evidence - independently reproduced by the Reviewer:

```
dotnet build src/j09-btr-retur-gudang/BtrReturGudang.Winform/BtrReturGudang.Winform.csproj -v minimal
```

Observed:

```
BtrReturGudang.Domain -> ...\BtrReturGudang.Domain.dll
BtrReturGudang.AppTier -> ...\BtrReturGudang.AppTier.dll
BtrReturGudang.Infrastructure -> ...\BtrReturGudang.Infrastructure.dll
BtrReturGudang.Winform -> ...\BtrReturGudang.Winform.exe

Build succeeded.
    0 Warning(s)
    0 Error(s)
```

# Architecture Compliance

- TD-007 - the actor remains the registered Google email
  (`RegistryHelper.LastOperatorEmail`); P2-S05 adds no name-based actor. PASS.
- TD-012 item 1 (configuration) / item 3 (gate: warn + pre-empt uploads) - the
  shared `OperatorSessionValidator` caches the resolution outcome; `IsUnresolved`
  gates uploads; the shell and all views share one singleton
  (`Program.cs` line 61), and constructors take an optional trailing validator
  parameter preserving existing call sites. PASS.
- TD-012 item 2 and §11.13 ("a `400` response ... is surfaced to the operator
  verbatim as a sign-in refusal"; "the Cloud message surfaced to the operator") -
  PARTIAL: for a real 400 the operator sees the raw JSend envelope rather than the
  Cloud refusal message (see C1 failure path / RV-001).
- §10.4 / §5.2 SCR-07 - session verification via `POST api/session/resolve` is
  realized in `SettingsView`. PASS.
- Dependency `P1-S03` (IMPLEMENTED, GO) and `P2-S04` (IMPLEMENTED, GO) - satisfied.
- Repository ownership - all changes are inside `BtrReturGudang.Winform`. PASS.

# Scope / Repository Hygiene

`git status` shows a broad working tree containing changes from other
concurrent plans (return-order-upload, customer-sync) plus the prior P1/P2 slices;
`git diff` is therefore not slice-isolable. No file outside P2-S05's declared set
(`Services/OperatorSessionValidator.cs` (new), `Program.cs`, `SettingsView.cs`,
`MainForm.cs`, `ReturnOrderEntryView.cs`, `ReturnOrderListView.cs`) was changed by
this slice. No test file references the new APIs
(`rg "LastOperatorEmail|OperatorSessionValidator|ResolveSessionAsync"` over
`*Tests*.cs` returns no matches), confirming P2-S05 did not touch P3-S06's scope.
No scope creep attributable to P2-S05.

# Findings

## RV-001

Severity: MAJOR

Description: The failure path surfaces the raw JSend failure envelope instead of
the Cloud refusal message, so the criterion's stated failure text
("Google account is not registered or is not active. Contact your administrator.")
is not produced, and architecture §11.13 / TD-012 item 2 ("the Cloud message
surfaced to the operator", "surfaced ... as a sign-in refusal") is only partially
satisfied. `SessionResolveResultDto.ErrorMessage` is populated with the raw
response body (`ExtractErrorDetail` content-first, P1-S03), which for a 400 is the
full JSend envelope. The SettingsView status label and every pre-emption message
(`MainForm`, `ReturnOrderEntryView`, `ReturnOrderListView`) therefore display that
envelope, i.e. still a raw server payload rather than the operator-readable
refusal. The P1-S03 review (RV-003) explicitly handed this JSON-parsing step to
P2-S05.

Evidence:
- `Views/SettingsView.cs` lines 599-606 (failure shows `result.ErrorMessage`
  verbatim, else `DefaultRefusalMessage`).
- `Services/OperatorSessionValidator.cs` lines 93-103 (`RefusalMessage` returns
  `_lastResult.ErrorMessage` verbatim).
- `Http/BtradeApiClient.cs` lines 251-278 and 349-372 (`Fail(..., ExtractErrorDetail,
  ...)`; content-first ordering).
- `btrade.webapi/Controllers/SessionController.cs` line 57 and
  `btrade.webapi.Test/SessionResolveTests.cs` lines 96-99 (Cloud 400 body is a JSend
  envelope whose `data` holds the message).
- `rg "JsonDocument|JsonElement|JObject|GetProperty|RawResponse|\"data\""` over
  `BtrReturGudang.Winform` -> no matches (no parsing in the presentation layer).

Required Correction: In the `BtrReturGudang.Winform` presentation layer, populate
the operator-facing refusal text from the Cloud refusal *message* rather than the
raw response body: parse the JSend failure `data` (falling back to `message`, then
`DefaultRefusalMessage`) from the carried `ErrorMessage`/`RawResponse`, and use that
single clear text consistently in (a) the `SettingsView` failure status label and
(b) `OperatorSessionValidator.RefusalMessage` so all pre-emption sites
(`MainForm`, `ReturnOrderEntryView.SaveAndUploadAsync`,
`ReturnOrderListView.OnUploadOrderClickAsync`) show the same clear message. Do not
display the raw JSON envelope. This correction is confined to P2-S05's repository
and does not change the approved slice structure.

Status: RESOLVED (Iteration 2)

Resolution (Iteration 2): `OperatorSessionValidator.ExtractRefusalMessage(...)` now
unwraps the Cloud JSend failure `data`/`message` from the carried
`RawResponse`/`ErrorMessage`, returns non-JSON details as-is, and falls back to
`DefaultRefusalMessage` for a JSON body with no usable message. `RefusalMessage`
returns `ExtractRefusalMessage(_lastResult)` and `SettingsView`'s failure label uses
`OperatorSessionValidator.ExtractRefusalMessage(result)`. Independently reproduced
against the exact j06 envelope (`{"status":"Bad Request","code":"400","data":"Google
account is not registered or is not active. Contact your administrator."}`): the
Cloud message is returned, not the raw envelope. See `## Iteration 2`.

## RV-002

Severity: NOTE (escalation to Architect)

Description: There is a cross-slice contract inconsistency between P1-S03 and
P2-S05. P1-S03's approved criterion mandates `SessionResolveResultDto.ErrorMessage
= ExtractErrorDetail(...)` with response `Content` first (raw JSON), while P2-S05's
criterion states "failure shows `ErrorMessage` verbatim (e.g. 'Google account is not
registered ...')" - i.e. it presumes `ErrorMessage` already carries the friendly
refusal. The two criteria cannot both hold for the same 400. The plan needs the
Architect to decide whether the parsed message belongs in the Infrastructure
`ErrorMessage` (P1-S03) or in the presentation layer (P2-S05). RV-001's correction
implements the latter within P2-S05 scope; the plan-level inconsistency is recorded
here for the Architect.

Evidence: P1-S03 criterion (plan §P1-S03 bullet 3) and
`BtradeApiClient.ExtractErrorDetail` (lines 349-372) vs. P2-S05 criterion (plan
§P2-S05 bullet 1); P1-S03-REVIEW.md RV-003.

Required Correction: None from the Reviewer for P2-S05 acceptance beyond RV-001.
Escalate to ica-architect to reconcile the ErrorMessage contract.

Status: OPEN (informational; escalation)

## RV-003

Severity: NOTE

Description: `OperatorSessionValidator.IsUnresolved` blocks uploads on any
`StatusCode >= 400`, which includes transient 5xx server errors that are not a
definite operator refusal (a definite refusal is the Cloud 400/403/409 verdict).
Transport/offline (`StatusCode 0`) is correctly non-blocking, so the explicit
requirement is met; a 5xx would pre-empt uploads until network/server recovery.

Evidence: `Services/OperatorSessionValidator.cs` lines 71-87.

Required Correction: None required for acceptance. Optional: restrict the blocking
condition to definite refusal codes (e.g. 400/403/409) and treat 5xx like a
transport failure.

Status: ADDRESSED (Iteration 2, informational). `IsUnresolved` now blocks only on
`IsDefiniteRefusal` (`400`/`403`/`409`) or a blank email; transport (`0`) and 5xx are
non-blocking. Independently reproduced: status 0/500/502 do not block; 400/403/409 and
blank email do.

## RV-004

Severity: NOTE

Description: The startup validation runs asynchronously on `Shown`, and the initial
background sync pass is scheduled after a fixed 1500 ms delay; if validation has not
completed when an upload action fires, `IsUnresolved` is false (`_lastResult ==
null`) and the upload is attempted. P1-S03 already surfaces the server error for
that upload, so this is a small pre-emption-guarantee window, not a silent failure.

Evidence: `Views/MainForm.cs` lines 95, 513-518, 698-704; `OperatorSessionValidator`
lines 71-87.

Required Correction: None required for acceptance.

Status: OPEN (informational)

## RV-005

Severity: NOTE

Description: `OperatorSessionValidator.Invalidate()`, `HasBeenValidated`, and
`LastResult` are public API members that are currently unused in production
(likely consumed by the P3-S06 test doubles). No acceptance impact.

Evidence: `rg "\.Invalidate\(\)|HasBeenValidated|LastResult\b"` over
`src/j09-btr-retur-gudang` returns only the declarations.

Required Correction: None.

Status: OPEN (informational)

## RV-006

Severity: NOTE

Description: The `BtrReturGudang.Winform.Tests` project does not compile (4
`CS0535` errors: `MockRegistryHelper`, `ModalDialogTests.FakeRegistryHelper`,
`EndToEndWorkflowIntegrationTests.TestRegistryHelper` missing
`IRegistryHelper.LastOperatorEmail`, and `MockBtradeApiClient` missing
`IBtradeApiClient.ResolveSessionAsync`). This is P3-S06's explicit deliverable and
is outside P2-S05's build gate (which requires only `BtrReturGudang.Winform` to
compile, verified in C3). Recorded so it is not mistaken for a P2-S05 defect.

Evidence: `dotnet build src/j09-btr-retur-gudang/BtrReturGudang.Winform.Tests/BtrReturGudang.Winform.Tests.csproj`
-> `Build FAILED`, `4 Error(s)`.

Required Correction: None for P2-S05. Owned by P3-S06.

Status: RESOLVED by P3-S06 (Iteration 2, informational). P3-S06 is now IMPLEMENTED and
`BtrReturGudang.Winform.Tests` compiles and passes (67/0). Not a P2-S05 defect.

# Current Decision

> Superseded by the Iteration 2 re-review (`## Iteration 2` below, Decision: GO).
> The text below is the Iteration 1 (ReviewIteration 1) decision, preserved as history.

NO-GO

C2 and C3 are satisfied, and the C1 action/wiring and success display are correct.
C1's failure behavior is not: for an actual Cloud refusal the operator is shown the
raw JSend envelope, not the Cloud refusal message the criterion and architecture
require. RV-001 is a MAJOR finding (mandatory acceptance/architecture behavior only
partially implemented), so the slice is not accepted. RV-002 is escalated to the
Architect as a cross-slice contract inconsistency. No BLOCKER findings.

# Re-Review History

- ReviewIteration 1 (2026-10-17): Initial review. Decision NO-GO. Finding RV-001
  (MAJOR) open; RV-002..RV-006 informational.
- ReviewIteration 2 (2026-10-17): Re-review after implementer remediation. Decision
  GO. RV-001 (MAJOR) RESOLVED; RV-003 ADDRESSED; RV-006 resolved by P3-S06;
  RV-002/RV-004/RV-005 remain informational. No BLOCKER or MAJOR findings remain.

## Iteration 2

Re-review of slice P2-S05 after the implementer remediation recorded in the plan's
P2-S05 Implementation Notes (2026-10-17). Reviewed against the same approved slice
definition (IMPLEMENTATION-PLAN v1.1, P2-S05), ARCHITECTURE v1.2 (TD-007, TD-012,
§11.8/§11.13), and the j06 Cloud contract.

Decision: GO

### Per-criterion verdict

- C1 - SettingsView "Verify Operator" action resolves via `POST api/session/resolve`
  and shows the outcome, including the Cloud refusal message: SATISFIED.
  - Success path unchanged and correct:
    `"✅ Operator terverifikasi: {result.UserName} (Role: {result.RoleId})"`
    (`Views/SettingsView.cs` line 593).
  - Failure path (RV-001): now
    `_lblSessionStatus.Text = OperatorSessionValidator.ExtractRefusalMessage(result);`
    (line 606), so a Cloud 400 JSend envelope yields the human-readable Cloud message,
    not the raw envelope.
- C2 - Startup and operator-change validation; unresolved operator warned and uploads
  pre-empted with the same clear message: SATISFIED. All pre-emption sites use
  `_sessionValidator.RefusalMessage`, which now returns
  `ExtractRefusalMessage(_lastResult)`:
  - `Views/MainForm.cs` lines 576-579 (startup / operator change) and 698-701
    (background/manual sync pass).
  - `Views/ReturnOrderEntryView.cs` lines 1295-1307 ("Simpan & Upload").
  - `Views/ReturnOrderListView.cs` lines 836-840 ("Upload Ulang").
- C3 - `BtrReturGudang.Winform` compiles with zero errors: SATISFIED (see evidence).

### RV-001 re-verification (independent)

RV-001 required the operator-facing refusal text to come from the Cloud refusal
*message* rather than the raw response body, in both the `SettingsView` failure label
and `OperatorSessionValidator.RefusalMessage` (hence all pre-emption messages), with
parse order `RawResponse` → `ErrorMessage`, then `data` → `message` →
`DefaultRefusalMessage`, and non-JSON details surfaced as-is.

Source inspection:
- `OperatorSessionValidator.ExtractRefusalMessage` (lines 132-151) iterates
  `[RawResponse, ErrorMessage]`; for each candidate it calls `ExtractJsEndMessage`,
  returns a non-JSON candidate as-is, and skips (continues) a JSON body with no usable
  message; finally returns `DefaultRefusalMessage`.
- `ExtractJsEndMessage` (lines 159-193) parses a JSON object and reads `data`
  (string, or object with `message`/`data`) then top-level `message`, case-insensitively.
- `RefusalMessage` (lines 111-120) returns `ExtractRefusalMessage(_lastResult)`;
  `SettingsView` (line 606) uses `ExtractRefusalMessage(result)` for both the
  shared-validator and direct-API-client failure paths.
- The real P1-S03 shape is `Fail(status, ExtractErrorDetail(response), response?.Content)`
  (`Http/BtradeApiClient.cs` lines 269-272) with `Content` first, so for a Cloud 400 both
  `ErrorMessage` and `RawResponse` carry the JSend envelope; the parse order still yields
  the message. The j06 envelope is `{"status":"Bad Request","code":"400","data":"<ex.Message>"}`
  (`SessionController.cs` line 57), where `<ex.Message>` is exactly
  `"Google account is not registered or is not active. Contact your administrator."`
  (`SessionContextResolver.cs` line 24; asserted by `SessionResolveTests.cs` line 99).

Observed behavior - independent harness (compiled against the built
`BtrReturGudang.Winform`/`BtrReturGudang.AppTier` assemblies, run in the pre-approved
temp dir outside the repository):
`dotnet run --project C:\Users\drury\AppData\Local\Temp\opencode\rv001-harness\rv001-harness.csproj -v quiet`

```
=== C1: real Cloud JSend 400 envelope ===
PASS RawResponse+ErrorMessage = envelope -> Cloud message
PASS ErrorMessage only = envelope -> Cloud message
=== C1: fallbacks / non-JSON / parse order ===
PASS non-JSON RawResponse surfaced as-is
PASS non-JSON ErrorMessage surfaced as-is
PASS JSON envelope without data/message -> DefaultRefusalMessage
PASS JSend top-level message used
PASS null result -> DefaultRefusalMessage
=== RV-003: IsUnresolved offline tolerance / definite refusal ===
PASS StatusCode 0 (transport) does not block
PASS StatusCode 500 (transient) does not block
PASS StatusCode 502 (transient) does not block
PASS StatusCode 400 (refusal) blocks
PASS StatusCode 403 (refusal) blocks
PASS StatusCode 409 (refusal) blocks
PASS blank configured email blocks
ALL CHECKS PASSED
```

Conclusion: RV-001 is RESOLVED. The raw JSON envelope is never surfaced; the Cloud
refusal message is shown consistently in the `SettingsView` failure label and in every
upload pre-emption message.

### RV-003 re-verification and P1-S03 offline tolerance

`IsUnresolved` (lines 72-91) returns false when no validation ran, false on success,
true for a blank configured email, and otherwise true only for `IsDefiniteRefusal`
(`400`/`403`/`409`). A transport failure returns `Fail(0, ...)`, so status 0 is
non-blocking and the P1-S03 offline-tolerant upload path is preserved. 5xx is now
treated like a transport failure (RV-003 addressed). Reproduced in the harness above.

### Build / test evidence (independently reproduced by the Reviewer)

```
dotnet build src/j09-btr-retur-gudang/BtrReturGudang.Winform/BtrReturGudang.Winform.csproj -v minimal
```
Observed: `Build succeeded. 0 Warning(s) 0 Error(s)` (Domain, AppTier, Infrastructure,
Winform all built).

```
dotnet test src/j09-btr-retur-gudang/BtrReturGudang.Winform.Tests/BtrReturGudang.Winform.Tests.csproj -v minimal
```
Observed: `Passed! - Failed: 0, Passed: 67, Skipped: 0, Total: 67`.

### Scope / repository hygiene

- The remediation is confined to `BtrReturGudang.Winform`:
  `Services/OperatorSessionValidator.cs` and `Views/SettingsView.cs`.
- `BtrReturGudang.Infrastructure/Http/BtradeApiClient.cs` is unchanged in the
  remediated area: `ResolveSessionAsync` still returns
  `Fail(status, ExtractErrorDetail(response), response?.Content)` with `Content` first,
  and `ExtractErrorDetail` remains content-first. P1-S03 is not altered.
- No test file references the new parsing API (`rg "ExtractRefusalMessage"` over
  `BtrReturGudang.Winform.Tests` returns no matches); the remediation added no test
  changes. The tests present are P3-S06's.
- No file outside P2-S05's declared Winform set was changed.

### Architecture compliance

- TD-007 - actor remains the registered Google email. PASS.
- TD-012 item 2 - "a `400` response ... is surfaced to the operator verbatim as a
  sign-in refusal": the Cloud message is now surfaced. PASS.
- TD-012 item 3 / §11.13 - "an unregistered or inactive email is refused with the Cloud
  message surfaced to the operator, and no Cloud upload is attempted with an unresolved
  or name-based actor": PASS (gate + pre-emption + message).
- §11.8 / TD-012 item 1 configuration and §5.2 SCR-07 resolution: PASS.
- Dependencies P1-S03 (IMPLEMENTED, GO) and P2-S04 (IMPLEMENTED, GO): satisfied.

### Findings (Iteration 2)

- RV-001: RESOLVED. No residual BLOCKER or MAJOR finding.
- RV-002 (NOTE, escalation to Architect): cross-slice `ErrorMessage` contract
  inconsistency (P1-S03 raw-content vs. P2-S05 friendly-message expectation). Resolved in
  behavior within P2-S05 scope by the presentation-layer extraction; the plan-level
  contract inconsistency remains for the Architect. No P2-S05 correction required.
- RV-003 (NOTE): ADDRESSED.
- RV-004, RV-005 (NOTE): remain informational, no acceptance impact.
- RV-006 (NOTE): resolved by P3-S06 (test project now compiles; suite passes).

### Testing Gate

A GO decision applies only to this slice and does not authorize testing. Testing may
begin only when the IMPLEMENTATION-PLAN is COMPLETED: every slice IMPLEMENTED and GO.
That condition is not met (P3-S06 is IMPLEMENTED but NOT-REVIEWED), so the plan-level
`Status` is left unchanged (`NOT-STARTED`).
