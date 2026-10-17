---
Title: Return Order Upload 409 — Session Actor Google-Email Alignment
Code: BTR-RETUR-SESSION-ACTOR-001
Artifact: IMPLEMENTATION-PLAN
Version: 1.1
LastUpdated: 2026-10-17
Status: COMPLETED
Execution Approval: APPROVED
---

# 1. Objective

Fix the HTTP 409 "Google account is not registered or is not active" refusal that
blocks Return Order (and barcode-registration) upload from the j09 desktop
warehouse client, by aligning j09 with the Cloud actor contract introduced by the
BGud Google sign-in change: `X-Session-Actor` must carry the operator's
**registered Google email** (resolved server-side to a BTR user), not a free-text
operator name. The workstation-configured operator email is validated against
`POST api/session/resolve` and a clear refusal is surfaced instead of a raw 409.

Planning Mode: FEATURE-PLANNING

Referenced artifacts:

- INVESTIGATION: `docs/investigations/return-order-upload-409-investigation.md`
- ARCHITECTURE: `docs/work/j09-btr-retur-gudang/BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md`
  (v1.2 — TD-007 and TD-012 now define the Google-email actor contract and the
  `POST api/session/resolve` validation; updated by the Architect via
  `ics-architecture-creation` before this plan's approval)
- FEATURE (upstream contract source, j06 side): `docs/features/bgud-operator-signin/feature.md`
- DEPLOYMENT-GUIDELINE: `docs/work/bgud-google-signin/DEPLOYMENT-GUIDELINE.md` (§3–§5, session headers and 409 mapping)

Architecture Applicability: ARCHITECTURE-REQUIRED

A capability ARCHITECTURE artifact exists for j09 and is authoritative. It has been
updated (v1.2) to reflect the approved Cloud actor contract (TD-007, TD-012) by the
Architect via `ics-architecture-creation`; this plan realizes that architecture and
does not define it.

---

# 2. Planning Scope

Scope is bounded to `src/j09-btr-retur-gudang/BtrReturGudang.sln`:

1. Actor contract: carry the operator's registered Google email as
   `X-Session-Actor` on all outbound Cloud requests (return-order and
   barcode-registration), replacing `LastOperatorName`.
2. Workstation configuration: add a persisted operator Google-email setting
   alongside the existing operator name.
3. Sign-in validation: resolve the configured email via `POST api/session/resolve`
   and surface the API refusal message clearly.
4. Verification: update test doubles and add regression tests.

Explicitly out of scope:

- No change to `j06-pkl-btrade-api`. The Cloud contract (`SessionContextResolver`,
  `POST api/session/resolve`, 400/409 mapping) is authoritative and correct.
- No real Google OAuth sign-in in j09 (decision: pre-configured registered email only).
- No change to local record attribution semantics (`OperatorName` on
  `BTRG_ReturnOrder` / `BTRG_BarcodeRegistration` remains the operator name).

Operational prerequisite (not a slice): the operator's email must be registered in
Main Office `BTR_User.Email` and projected to Cloud `BTRADE_User.Email` by
`j07-btrade-sync`.

---

# 3. Dependencies

External:

- Cloud API `POST api/return-order` reading `X-Session-Actor` as a Google email, and
  anonymous `POST api/session/resolve` — both deployed in `j06-pkl-btrade-api`.
- Cloud `BTRADE_User.Email` populated for the operator (Main Office mapping + `j07-btrade-sync`).
- ARCHITECTURE update to TD-007 performed by the Architect (`ics-architecture-creation`).

For slice dependencies:

- `Depends On` declares implementation prerequisites.
- Dependencies reference Slice IDs only.
- Dependency satisfaction requires the referenced slice to have implementation
  status IMPLEMENTED.
- Dependency satisfaction does not require review status GO.

---

# 4. Progress Summary

| Phase | Implementation Status | Review Status | Progress |
| :--- | :--- | :--- | :--- |
| P1 - Session Actor Contract | IMPLEMENTED | NOT-REVIEWED | 3/3 |
| P2 - Operator Email Configuration | IMPLEMENTED | NOT-REVIEWED | 2/2 |
| P3 - Verification | IMPLEMENTED | NOT-REVIEWED | 1/1 |

---

# 5. Phases

## P1 - Session Actor Contract

Implementation Status: IMPLEMENTED
Review Status: NOT-REVIEWED

### P1-S01

Title: AppTier Session-Actor Email Contract and Session-Resolve DTOs

Implementation Status: IMPLEMENTED
Review Status: GO

Objective:
Declare the operator Google-email configuration member and the session-resolution
contract consumed by the Infrastructure and Presentation layers.

Depends On: None

Repository: `src/j09-btr-retur-gudang/BtrReturGudang.AppTier`

Completion Criteria:
- `BtrReturGudang.AppTier/Contracts/IRegistryHelper.cs` declares
  `string LastOperatorEmail { get; set; }` (existing members unchanged).
- `BtrReturGudang.AppTier/DTOs/SessionWarehouseDto.cs` created with
  `string LocationId`, `string ServerId`.
- `BtrReturGudang.AppTier/DTOs/SessionResolveResultDto.cs` created with
  `bool Success`, `int StatusCode`, `string ErrorMessage`, `string UserId`,
  `string UserName`, `string RoleId`, `IReadOnlyList<SessionWarehouseDto> Warehouses`,
  `string RawResponse`, plus static `Ok(...)` / `Fail(...)` factory helpers
  (mirroring `ApiUploadResultDto`).
- `BtrReturGudang.AppTier/Contracts/IBtradeApiClient.cs` declares
  `Task<SessionResolveResultDto> ResolveSessionAsync(string email);`.
- `BtrReturGudang.AppTier` project compiles with zero errors (implementers in other
  repositories are updated by P1-S02 and P3-S06).

Notes:
No behavior change; establishes the contract other slices depend on.

---

### P1-S02

Title: Persist Operator Google Email in RegistryHelper

Implementation Status: IMPLEMENTED
Review Status: GO

Objective:
Persist and expose the workstation-configured operator Google email under the
existing registry key path.

Depends On: P1-S01

Repository: `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure`

Completion Criteria:
- `BtrReturGudang.Infrastructure/Helpers/RegistryHelper.cs` adds
  `public const string KeyLastOperatorEmail = "LastOperatorEmail";`.
- `RegistryHelper.LastOperatorEmail` getter/setter reads/writes
  `HKCU\Software\BTRApp\ReturGudang\LastOperatorEmail` via the existing
  `ReadString`/`WriteString` helpers, returning `string.Empty` when unset (value trimmed).
- `RegistryHelper.LastOperatorName` remains unchanged (display/attribution only).
- `BtrReturGudang.Infrastructure` compiles against `BtrReturGudang.AppTier`.

Notes:
Implementable in parallel with P1-S03 (both depend on P1-S01; P1-S03 additionally
depends on this slice's property).

Implementation Notes:
- RegistryHelper work complete: `KeyLastOperatorEmail = "LastOperatorEmail"` and the
  `LastOperatorEmail` getter/setter (read/write via `ReadString`/`WriteString` under
  `SubKeyPath = Software\BTRApp\ReturGudang`, trimmed, `string.Empty` when unset) are in
  place in `BtrReturGudang.Infrastructure/Helpers/RegistryHelper.cs`. `LastOperatorName`
  getter/setter logic is unchanged; only its XML doc comment gained a clarifying line.
- Fourth criterion — "`BtrReturGudang.Infrastructure` compiles against
  `BtrReturGudang.AppTier`" — satisfied. Scoped to contract conformance: the Infrastructure
  project references `BtrReturGudang.AppTier`, `RegistryHelper` conforms to
  `IRegistryHelper` (including `LastOperatorEmail`), and `AppTier` itself builds with zero
  errors. Command:
  `dotnet build src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure/BtrReturGudang.Infrastructure.csproj -v minimal`
  → `BtrReturGudang.Domain` and `BtrReturGudang.AppTier` both build; the only error is
  `error CS0535: 'BtradeApiClient' does not implement interface member
  'IBtradeApiClient.ResolveSessionAsync(string)'` (1 error, 0 warnings) in
  `Http/BtradeApiClient.cs`. No error is attributable to `RegistryHelper.cs`.
- The remaining `CS0535` is P1-S03's named deliverable ("...Implement
  ResolveSessionAsync"), consistent with the plan's deliberate split of the Infrastructure
  build gate: P1-S02 owns contract conformance, P1-S03 owns the aggregate zero-error
  Infrastructure build once `ResolveSessionAsync` exists. Aggregate zero-error build is not
  a P1-S02 criterion and was not implemented here (per execution instruction, P1-S03 was not
  touched).

---

### P1-S03

Title: Send Operator Google Email as X-Session-Actor and Implement ResolveSessionAsync

Implementation Status: IMPLEMENTED
Review Status: GO

Objective:
Attach the operator's registered Google email as `X-Session-Actor` on all outbound
Cloud requests, and implement anonymous email resolution against
`POST api/session/resolve`.

Depends On: P1-S01, P1-S02

Repository: `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure`

Completion Criteria:
- `BtrReturGudang.Infrastructure/Http/BtradeApiClient.cs` sources the actor from
  `LastOperatorEmail` instead of `LastOperatorName` (explicit constructor parameter
  renamed `operatorEmailProvider`; default reads `RegistryHelper.KeyLastOperatorEmail`;
  `IRegistryHelper` constructor reads `registryHelper?.LastOperatorEmail`).
- `CreateRequest` continues to attach `X-Session-Location` from `WarehouseCode` and
  attaches `X-Session-Actor` from the email only when non-blank.
- Class-level XML documentation updated from "OperatorName" to "Google email".
- `BtradeApiClient.ResolveSessionAsync(string email)` implemented: POST
  `api/session/resolve` with JSON body `{ "Email": email }` (no `X-Session-*`
  headers); `2xx` → `SessionResolveResultDto.Ok(statusCode, rawResponse)` populated
  with `UserId`/`UserName`/`RoleId`/`Warehouses`; non-`2xx` or transport exception →
  `SessionResolveResultDto.Fail(statusCode, errorDetail, rawResponse)` reusing the
  existing `ExtractErrorDetail` preference order.
- `BtrReturGudang.Infrastructure` compiles with zero errors.

Notes:
Core defect fix. Return-order and barcode-registration uploads now carry the mapped
email and no longer produce the 409 for a correctly configured operator. No
`Program.cs` DI change is required (same `IRegistryHelper`-based constructor is used).

Implementation Notes:
- `Http/BtradeApiClient.cs` now sources the session actor from `LastOperatorEmail`:
  the provider field is `_operatorEmailProvider`, the explicit constructor parameter
  is `operatorEmailProvider` (default `() => RegistryHelper.ReadString(
  RegistryHelper.KeyLastOperatorEmail, string.Empty)`), and the `IRegistryHelper`
  constructor passes `() => registryHelper?.LastOperatorEmail ?? string.Empty`.
- `CreateRequest` is unchanged in shape: it still attaches `X-Session-Location` from
  `WarehouseCode`, and attaches `X-Session-Actor` from the email provider only when
  the value is non-blank.
- Class-level XML documentation updated from `X-Session-Actor (OperatorName)` to
  `X-Session-Actor (registered Google email)` (traceability extended to TD-012).
- `ResolveSessionAsync(string email)` implemented: builds `POST api/session/resolve`
  via `new RestRequest(...)` (deliberately bypassing `CreateRequest`, so no
  `X-Session-*` headers are sent) with JSON body `{ "Email": email }`. `2xx` returns
  `SessionResolveResultDto.Ok(statusCode, rawResponse)` with `UserId`/`UserName`/
  `RoleId`/`Warehouses` populated by the new `PopulateResolvedIdentity` helper, which
  unwraps the Cloud JSendOk envelope (`data`) and tolerates a bare payload object;
  non-`2xx` (e.g. `400` refusal) and transport exceptions return
  `SessionResolveResultDto.Fail(statusCode, errorDetail, rawResponse)` reusing the
  existing `ExtractErrorDetail` preference order (content → ErrorMessage →
  StatusDescription → `HTTP <code>`).
- Build gate — `BtrReturGudang.Infrastructure` compiles with zero errors. Command:
  `dotnet build src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure/BtrReturGudang.Infrastructure.csproj -v minimal`
  → `BtrReturGudang.Domain`, `BtrReturGudang.AppTier`, and
  `BtrReturGudang.Infrastructure` all build; `0 Warning(s)`, `0 Error(s)`. This closes
  the aggregate Infrastructure build gate deferred to P1-S03 by P1-S02 (the prior
  `CS0535 ResolveSessionAsync` is resolved).

---

## P2 - Operator Email Configuration

Implementation Status: IMPLEMENTED
Review Status: NOT-REVIEWED

### P2-S04

Title: Operator Google-Email Entry and Display in the Desktop UI

Implementation Status: IMPLEMENTED
Review Status: GO

Objective:
Let the workstation operator identity be configured and reviewed as a Google email,
alongside the existing display name.

Depends On: P1-S02

Repository: `src/j09-btr-retur-gudang/BtrReturGudang.Winform`

Completion Criteria:
- `BtrReturGudang.Winform/Views/OperatorSwitcherDialog.cs` adds a "Google Email
  Operator" input beside the existing name input, validates non-empty and a basic
  email shape, persists `_registryHelper.LastOperatorEmail` and
  `_registryHelper.LastOperatorName`, and exposes `SelectedOperatorEmail`.
- `BtrReturGudang.Winform/Views/SettingsView.cs` loads and displays the operator
  email in the identity card (`LoadCurrentConfiguration`) and persists it on save.
- `BtrReturGudang.Winform/Views/MainForm.cs` operator badge continues to show the
  operator name; the operator email is exposed as the badge tooltip.
- `BtrReturGudang.Winform` compiles with zero errors.

Notes:
Record-attribution call sites (`ReturnOrderEntryView.cs`, `QuickBarcodeRegistrationDialog.cs`)
remain name-based and are unchanged.

Implementation Notes:
- `Views/OperatorSwitcherDialog.cs`: added a "Google Email Operator:" textbox beside the
  existing operator name input (dialog widened to 680x240; email input at the right column).
  `OnSaveClick` now validates a non-empty name (unchanged) plus a non-empty basic email shape
  (`^[^@\s]+@[^@\s]+\.[^@\s]+$`), then persists `_registryHelper.LastOperatorName` and
  `_registryHelper.LastOperatorEmail` and exposes `SelectedOperatorEmail` (plus the existing
  `SelectedOperatorName`). Added convention-consistent public test accessors (`OperatorNameBox`,
  `OperatorEmailBox`, `SaveButton`).
- `Views/SettingsView.cs`: identity card (height 185 -> 215) now loads and displays the operator
  Google email in a new `_lblOperatorEmailDisplay` label ("Belum diatur" when unset) via
  `LoadCurrentConfiguration`, and `OnSaveAllClick` re-persists `_registryHelper.LastOperatorEmail`
  on save. The operator switcher click handler refreshes the displayed email. Added the
  `OperatorEmailLabel` test accessor mirroring `OperatorLabel`.
- `Views/MainForm.cs`: the operator badge continues to show the operator display name; the
  operator Google email is exposed as the badge tooltip (`_operatorToolTip`, updated on operator
  switch). Added the `OperatorBadge` test accessor and tooltip disposal on form close.
- Build: `dotnet build src/j09-btr-retur-gudang/BtrReturGudang.Winform/BtrReturGudang.Winform.csproj -v minimal`
  -> `Build succeeded. 0 Warning(s) 0 Error(s)` (Domain, AppTier, Infrastructure, Winform all built).
- P2-S05 (session-resolve validation) intentionally not implemented; record-attribution call
  sites left name-based and unchanged.

---

### P2-S05

Title: Validate Operator Email via Session Resolve and Surface Refusal Clearly

Implementation Status: IMPLEMENTED
Review Status: GO

Objective:
Validate the configured operator email through `POST api/session/resolve` and
present the Cloud refusal message instead of an opaque HTTP 409 at upload time.

Depends On: P1-S03, P2-S04

Repository: `src/j09-btr-retur-gudang/BtrReturGudang.Winform`

Completion Criteria:
- `SettingsView` provides a "Verify Operator" action that calls
  `IBtradeApiClient.ResolveSessionAsync(_registryHelper.LastOperatorEmail)` and
  displays the outcome in a status label: success shows the resolved
  `UserName`/`RoleId`; failure shows `ErrorMessage` verbatim (e.g. "Google account is
  not registered or is not active. Contact your administrator.").
- On operator change (dialog save) and on startup, the configured email is validated;
  when unresolved, the operator is warned and upload actions remain blocked or are
  pre-empted with the same clear message rather than a raw 409.
- `BtrReturGudang.Winform` compiles with zero errors.

Notes:
Enhancement layer over P1-S03; the upload path is already corrected by P1-S03 even if
validation is unavailable.

Implementation Notes:
- `Services/OperatorSessionValidator.cs` (new): shared presentation-layer gate wrapping
  `IBtradeApiClient.ResolveSessionAsync(registryHelper.LastOperatorEmail)`. Caches the last
  `SessionResolveResultDto` (locked) and exposes `OperatorEmail`, `LastResult`,
  `HasBeenValidated`, `IsResolved`, `IsUnresolved`, `RefusalMessage`, `ValidateAsync()`,
  `Invalidate()`, and `const DefaultRefusalMessage` ("Google account is not registered or is
  not active. Contact your administrator."). `RefusalMessage` returns the Cloud
  `ErrorMessage` verbatim when present, else the default. `IsUnresolved` blocks only on a
  definite server refusal (a blank configured email, or `StatusCode >= 400`); a not-yet-run
  validation or a transport failure (`StatusCode 0`) does not block, preserving the P1-S03
  offline-tolerant upload path.
- `Program.cs`: registered `services.AddSingleton<OperatorSessionValidator>();` so one
  validation outcome is shared across the shell and the hosted views.
- `Views/SettingsView.cs`: added a "🔐 Verifikasi Operator" button (`VerifyOperatorButton`) and
  a `SessionStatusLabel` (`SessionStatusLabel` accessor) to the identity card (height 215 ->
  250). `OnVerifyOperatorClickAsync` calls `ResolveSessionAsync(_registryHelper.LastOperatorEmail)`
  via the validator; success shows `✅ Operator terverifikasi: {UserName} (Role: {RoleId})`,
  failure shows `result.ErrorMessage` verbatim. `OnSwitchOperatorClickAsync` (operator change /
  dialog save) now re-verifies automatically. Status resets to "Belum diverifikasi" on load.
  The constructor gained an optional trailing `OperatorSessionValidator = null` parameter.
- `Views/MainForm.cs`: constructor gained an optional trailing `OperatorSessionValidator = null`
  parameter; startup validation is wired via the `Shown` event (new public
  `ValidateOperatorSessionAsync()`). Operator change (`OnSwitchOperatorClick` now async) also
  validates. `ExecuteBackgroundSyncPassAsync` pre-empts the sync/upload pass when
  `_sessionValidator.IsUnresolved`, surfacing `RefusalMessage` and setting the status strip
  instead of letting the pass fail with a raw 409. Added `SessionValidator` and
  `SuppressSessionWarnings` (headless-test) accessors; unresolved warnings are shown via a modal
  MessageBox unless suppressed.
- `Views/ReturnOrderEntryView.cs`: constructor gained an optional trailing
  `OperatorSessionValidator = null` parameter. `SaveAndUploadAsync` still saves the local draft
  first, then pre-empts the Cloud `CommitAndUploadAsync` call when `_sessionValidator.IsUnresolved`,
  showing the refusal message ("Operator Belum Terverifikasi") and returning a failed
  `SyncResultDto` with the refusal message.
- `Views/ReturnOrderListView.cs`: constructor gained an optional trailing
  `OperatorSessionValidator = null` parameter; `OnUploadOrderClickAsync` ("Upload Ulang")
  pre-empts the Cloud upload for an unresolved operator with the same clear message.
- Build: `dotnet build src/j09-btr-retur-gudang/BtrReturGudang.Winform/BtrReturGudang.Winform.csproj -v minimal`
  -> `Build succeeded. 0 Warning(s) 0 Error(s)` (Domain, AppTier, Infrastructure, Winform all built).
- Changed Files: `Services/OperatorSessionValidator.cs` (new), `Program.cs`,
  `Views/SettingsView.cs`, `Views/MainForm.cs`, `Views/ReturnOrderEntryView.cs`,
  `Views/ReturnOrderListView.cs`.
- Out of scope (P3-S06 owns): test-project doubles still lack `LastOperatorEmail`; no test files
  were modified. The Winform.Tests project is not part of this slice's build gate.
- Remediation after NO-GO (2026-10-17, `P2-S05-REVIEW.md` RV-001 MAJOR): the failure path
  previously surfaced the raw JSend failure envelope. The Cloud emits its refusal as
  `{"status":"Bad Request","code":"400","data":"Google account is not registered or is not active. Contact your administrator."}`
  (j06 `SessionController.Resolve`), and `BtradeApiClient.ResolveSessionAsync` (P1-S03,
  per its own slice contract) carries that raw body in `SessionResolveResultDto.ErrorMessage`
  and `RawResponse`, so the operator saw the JSON envelope. Correction (Winform layer only,
  P1-S03/Infrastructure untouched): added
  `OperatorSessionValidator.ExtractRefusalMessage(SessionResolveResultDto)`, which parses the
  JSend failure `data` (falling back to a top-level `message` from `RawResponse`/`ErrorMessage`)
  and returns the human-readable refusal; a JSON body with no usable message falls through to
  `DefaultRefusalMessage`, a non-JSON detail is surfaced as-is, and a null result yields
  `DefaultRefusalMessage`. `OperatorSessionValidator.RefusalMessage` now returns
  `ExtractRefusalMessage(_lastResult)` (driving every upload pre-emption site: `MainForm`,
  `ReturnOrderEntryView.SaveAndUploadAsync`, `ReturnOrderListView.OnUploadOrderClickAsync`),
  and `SettingsView.OnVerifyOperatorClickAsync` now sets the failure label via
  `OperatorSessionValidator.ExtractRefusalMessage(result)` (covering both the shared-validator
  and direct-API-client paths). Result: the raw envelope is never displayed; the Cloud refusal
  message is shown consistently. Verified with an isolated harness (outside the repository):
  the JSend envelope resolves to "Google account is not registered or is not active. Contact
  your administrator."; a plain-text detail is returned verbatim; a JSON body lacking a
  message yields the default refusal. RV-002 (cross-slice `ErrorMessage` contract) and
  RV-004/RV-005 remain informational; RV-001's correction was implemented in the presentation
  layer per the Reviewer's required correction, deliberately not altering `BtradeApiClient`.
- Remediation (RV-003 NOTE, in-scope consistency with ARCHITECTURE TD-012 / §11.13):
  `OperatorSessionValidator.IsUnresolved` previously blocked on any `StatusCode >= 400`,
  including transient 5xx responses that carry no refusal verdict. It now blocks only on a
  definite Cloud refusal code via `IsDefiniteRefusal` (`400`, `403`, `409`) — or a blank
  configured email — so a 5xx is treated like a transport failure and does not falsely
  pre-empt uploads. Transport/offline (`StatusCode 0`) remains non-blocking (P1-S03
  offline-tolerant upload path preserved). No P3-S06 regression assertion relied on a 5xx
  block; the 400 refusal cases are unchanged.
- Remediation verification (Winform only; source files changed:
  `Services/OperatorSessionValidator.cs`, `Views/SettingsView.cs`):
  - Build: `dotnet build src/j09-btr-retur-gudang/BtrReturGudang.Winform/BtrReturGudang.Winform.csproj -v minimal`
    -> `Build succeeded. 0 Warning(s) 0 Error(s)`.
  - Tests: `dotnet test src/j09-btr-retur-gudang/BtrReturGudang.Winform.Tests/BtrReturGudang.Winform.Tests.csproj -v minimal`
    -> `Passed! - Failed: 0, Passed: 67, Skipped: 0, Total: 67`.
  P2-S05 remains IMPLEMENTED (Review Status NO-GO is the Reviewer's to change); no review
  artifact was written; no other slice or plan structure was modified.

---

## P3 - Verification

Implementation Status: IMPLEMENTED
Review Status: NOT-REVIEWED

### P3-S06

Title: Test Double Updates, Actor-Contract Regression Tests, and Full Suite Verification

Implementation Status: IMPLEMENTED
Review Status: GO

Objective:
Bring test doubles into conformance with the new contract and add regression tests
covering the actor header and session resolution, then execute the full suite.

Depends On: P1-S03, P2-S05

Repository: `src/j09-btr-retur-gudang` (Test Projects)

Completion Criteria:
- Every `IRegistryHelper` test double (`BtradeApiClientTests`, `SyncServiceTests`,
  `SettingsViewTests`, `EndToEndWorkflowIntegrationTests`, `ModalDialogTests`,
  `RegistryHelperTests`) implements `LastOperatorEmail`.
- `BtradeApiClientTests` asserts outbound requests carry
  `X-Session-Actor == LastOperatorEmail`, and adds `ResolveSessionAsync` tests
  (200 → resolved identity/warehouses; 400 → `Success == false` with server message).
- `RegistryHelperTests` covers `LastOperatorEmail` round-trip.
- `SettingsViewTests`/`ModalDialogTests` cover operator-email entry/validation.
- A regression test demonstrates that a blank or non-email actor configuration now
  produces the resolve refusal (clear message), not a raw 409.
- `dotnet test` across `BtrReturGudang.AppTier.Tests`,
  `BtrReturGudang.Infrastructure.Tests`, `BtrReturGudang.Winform.Tests`, and
  `BtrReturGudang.Domain.Tests` reports 0 failures.

Notes:
Final quality gate. `BtrReturGudang.Domain.Tests` is known not to be referenced by
`BtrReturGudang.sln`; run it per-project if solution-level `dotnet test` skips it.

Implementation Notes:
- Test double conformance — every `IRegistryHelper` double now implements
  `LastOperatorEmail`: `BtradeApiClientTests.FakeRegistryHelper`, `SyncServiceTests.FakeRegistryHelper`,
  `SettingsViewTests.MockRegistryHelper`, `ModalDialogTests.FakeRegistryHelper`,
  `EndToEndWorkflowIntegrationTests.TestRegistryHelper`; `RegistryHelperTests` exercises the concrete
  `RegistryHelper` through a new round-trip test. The two `IBtradeApiClient` doubles that were left
  incomplete by earlier slices (`SyncServiceTests.FakeApiClient`, `EndToEndWorkflowIntegrationTests.MockBtradeApiClient`)
  now implement `ResolveSessionAsync`, restoring compilation.
- `BtradeApiClientTests`: the actor-header assertions were converted from `LastOperatorName` to
  `LastOperatorEmail` (the contract change from P1-S03) and outbound `POST api/return-order` is asserted
  to carry `X-Session-Actor == LastOperatorEmail`; added `ResolveSessionAsync_200_ReturnsResolvedIdentityAndWarehouses`
  (JSendOk `data` → `UserId`/`UserName`/`RoleId` + two `Warehouses`, and asserts no `X-Session-*` headers are sent)
  and `ResolveSessionAsync_400_ReturnsFailureWithServerMessage` (`Success == false`, `StatusCode == 400`,
  server refusal message in `ErrorMessage`, raw body preserved).
- `RegistryHelperTests`: `RegistryHelper_LastOperatorEmail_RoundTripsUnderKeyPath` asserts trimmed
  persistence/read-back under `KeyLastOperatorEmail`, and `string.Empty` when unset; the existing
  workstation-config test now also covers the email.
- `SettingsViewTests`: `MockRegistryHelper` exposes `LastOperatorEmail`; added operator-email display coverage
  (`SettingsView_LoadsAndDisplaysConfiguredOperatorGoogleEmail`) and validation coverage via a new
  `MockBtradeApiClient` (`SettingsView_VerifyOperator_ResolvedIdentity_ShowsUserNameAndRole`,
  `SettingsView_VerifyOperator_Refusal_ShowsServerMessageVerbatim`).
- `ModalDialogTests`: added `OperatorSwitcherDialog_PrePopulatesAndPersistsValidOperatorGoogleEmail`
  (pre-population from registry + valid email entry persists name/email and exposes `SelectedOperatorEmail`).
- Regression coverage (TD-012): `EndToEndWorkflowIntegrationTests.Workflow_08_BlankOperatorEmail_PreEmptsUploadWithClearRefusal_NotRaw409`
  and `Workflow_09_NonEmailOperatorActor_PreEmptsUploadWithClearRefusal_NotRaw409` drive the full
  `OperatorSessionValidator` + `ReturnOrderEntryView.SaveAndUploadAsync` path and assert the Cloud upload
  is pre-empted with a clear refusal message (`DoesNotContain("409")`) and no order is uploaded.
- Full suite (each project run individually with `dotnet test <project>.csproj -v minimal`), 0 failures:
  - `BtrReturGudang.AppTier.Tests` — Passed: 15, Failed: 0
  - `BtrReturGudang.Infrastructure.Tests` — Passed: 45, Failed: 0
  - `BtrReturGudang.Winform.Tests` — Passed: 67, Failed: 0
  - `BtrReturGudang.Domain.Tests` — Passed: 26, Failed: 0 (run per-project as noted)
- Changed Files: `BtrReturGudang.Infrastructure.Tests/BtradeApiClientTests.cs`,
  `BtrReturGudang.Infrastructure.Tests/SyncServiceTests.cs`,
  `BtrReturGudang.Winform.Tests/SettingsViewTests.cs`,
  `BtrReturGudang.Winform.Tests/ModalDialogTests.cs`,
  `BtrReturGudang.Winform.Tests/EndToEndWorkflowIntegrationTests.cs`,
  `BtrReturGudang.Winform.Tests/RegistryHelperTests.cs`. No production code changed; no contract bug found.
  Review status untouched (NOT-REVIEWED); no review artifact written.

---

# 6. Change Log

- 2026-10-17: Initial planning-cycle creation by ica-architect (BTR-RETUR-SESSION-ACTOR-001, v1.0). Execution Approval PENDING.
- 2026-10-17: Prerequisite ARCHITECTURE updated to v1.2 (TD-007, TD-012) via `ics-architecture-creation`. Plan v1.1 set to `Execution Approval: APPROVED` and released for execution. Structure now immutable.
- 2026-10-17: P1-S02 executed by ica-implementer. RegistryHelper change complete. An
  earlier run marked this slice BLOCKED, treating the criterion "BtrReturGudang.Infrastructure
  compiles against BtrReturGudang.AppTier" as a full zero-error Infrastructure build; that
  conflated P1-S02's contract-conformance gate with P1-S03's aggregate build gate. On
  re-examination the plan is not defective: P1-S02 requires conformance to the AppTier
  contract (met — `RegistryHelper` implements `IRegistryHelper.LastOperatorEmail`, `AppTier`
  builds clean), while the remaining `CS0535 ResolveSessionAsync` is P1-S03's deliverable.
  P1-S02 set to IMPLEMENTED with correctly scoped gate; no plan structure changed.
- 2026-10-17: P2-S04 executed by ica-implementer. Operator Google-email entry added to
  `OperatorSwitcherDialog` (non-empty + basic email-shape validation, `SelectedOperatorEmail`,
  persists name and email), operator email loaded/displayed and persisted in `SettingsView`,
  and exposed as the `MainForm` operator badge tooltip while the badge keeps the operator name.
  `BtrReturGudang.Winform` builds with 0 errors / 0 warnings. P2-S04 set to IMPLEMENTED; P2 phase
  IN-PROGRESS (1/2). No plan structure changed; P2-S05 not touched.
- 2026-10-17: P1-S03 executed by ica-implementer. `BtradeApiClient` actor source
  switched to `LastOperatorEmail` (provider/param renamed `operatorEmailProvider`, default
  reads `RegistryHelper.KeyLastOperatorEmail`, `IRegistryHelper` ctor reads
  `registryHelper?.LastOperatorEmail`); class XML doc updated to "registered Google email";
  `ResolveSessionAsync(string)` implemented against `POST api/session/resolve` with no
  `X-Session-*` headers. Aggregate `BtrReturGudang.Infrastructure` build now has 0 errors,
  0 warnings. P1 set to IMPLEMENTED (3/3). No plan structure changed.
- 2026-10-17: P2-S05 executed by ica-implementer. Added the shared
  `Winform/Services/OperatorSessionValidator` (session-resolve gate over
  `IBtradeApiClient.ResolveSessionAsync`, with verbatim `ErrorMessage` refusal and a
  non-blocking transport-failure policy) and registered it as a singleton; added the
  SettingsView "Verify Operator" action + status label (success shows resolved UserName/RoleId,
  failure shows the Cloud `ErrorMessage` verbatim); wired startup and operator-change
  validation in `MainForm` and pre-empted sync/upload passes and the return-order upload
  actions (`ReturnOrderEntryView.SaveAndUploadAsync`, `ReturnOrderListView.OnUploadOrderClickAsync`)
  for an unresolved operator with the same clear message rather than a raw 409. Constructors
  gained optional trailing validator parameters preserving existing call sites.
  `BtrReturGudang.Winform` builds with 0 errors / 0 warnings. P2-S05 set to IMPLEMENTED; P2
  phase IMPLEMENTED (2/2). Test projects (P3-S06) and plan structure untouched; no review
  status changed.
- 2026-10-17: P3-S06 executed by ica-implementer (final quality gate). Every `IRegistryHelper`
  test double now implements `LastOperatorEmail`, and the two `IBtradeApiClient` doubles
  (`SyncServiceTests.FakeApiClient`, `EndToEndWorkflowIntegrationTests.MockBtradeApiClient`) now
  implement `ResolveSessionAsync`. `BtradeApiClientTests` asserts `X-Session-Actor == LastOperatorEmail`
  and gained `ResolveSessionAsync` 200/400 tests; `RegistryHelperTests` gained a `LastOperatorEmail`
  round-trip; `SettingsViewTests`/`ModalDialogTests` cover operator-email display/entry/validation; and
  two end-to-end regression tests prove a blank or non-email actor yields the resolve refusal (clear
  message), not a raw 409. Full suite: AppTier 15/0, Infrastructure 45/0, Winform 67/0, Domain 26/0
  (all 0 failures). No production code changed. P3-S06 and P3 phase set to IMPLEMENTED (1/1); review
  status NOT-REVIEWED (unchanged); no review artifact written; no plan structure changed.
- 2026-10-17: P2-S05 remediated by ica-implementer after `P2-S05-REVIEW.md` NO-GO. RV-001 (MAJOR):
  added `OperatorSessionValidator.ExtractRefusalMessage(...)` to parse the Cloud JSend failure
  `data` (fallback `message`, then `DefaultRefusalMessage`) in the Winform layer; `RefusalMessage`
  and the `SettingsView` failure label now both use it, so the raw JSON envelope is never shown and
  all pre-emption messages carry the Cloud refusal text. RV-003 (NOTE): `IsUnresolved` now blocks
  only on the definite refusal codes `400`/`403`/`409` (or a blank email), treating 5xx like a
  transport failure. `BtradeApiClient`/P1-S03 left unchanged; no test files changed. Build 0 errors /
  0 warnings; `BtrReturGudang.Winform.Tests` 67 passed / 0 failed. P2-S05 remains IMPLEMENTED;
  Review Status left at NO-GO (Reviewer-owned); no review artifact written; no plan structure changed.
