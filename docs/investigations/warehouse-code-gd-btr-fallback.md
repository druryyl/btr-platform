# BUG-INVESTIGATION

Code: BTR-RETUR-WAREHOUSE-FALLBACK-001
Artifact: BUG-INVESTIGATION
Type: BUG
Date: 2026-10-17
Status: COMPLETED

## Context

Issue: A Return Order uploaded from the j09 warehouse desktop client
(`src/j09-btr-retur-gudang/BtrReturGudang.sln`) was persisted with
`WarehouseCode = 'GD-BTR'` even though the workstation is configured with
`GAMPING` in `[F5] Pengaturan` → **Identitas Workstation & Shift Operator** →
*Kode Depo / Warehouse*. The same document also shows `OperatorName = 'OPERATOR'`
instead of the configured shift operator.

No formal ISSUE artifact was raised for this defect; the investigation is based
on the reported operational symptom and repository/database evidence.

Problem Summary: The uploaded Return Order carries a placeholder warehouse code
and operator name that were never configured by the workstation, instead of the
values entered in `[F5] Pengaturan`.

## Current State

1. The operator configures the workstation in `[F5] Pengaturan` → **Identitas
   Workstation & Shift Operator**: *Kode Depo / Warehouse*, *Server Target ID*,
   *Operator Shift Aktif* (name + Google email).
2. `OnSaveAllClick` persists the warehouse code to
   `HKCU\Software\BTRApp\ReturGudang\WarehouseCode` through `IRegistryHelper`
   (`RegistryHelper.WarehouseCode` setter).
3. The operator records a Return Order in the Intake view and triggers
   "Simpan & Upload" (`F10`).
4. The uploaded/persisted document shows `WarehouseCode = 'GD-BTR'` and
   `OperatorName = 'OPERATOR'`.

## Evidence

Workstation registry (`HKCU\Software\BTRApp\ReturGudang`), read on the affected
PC:

```text
WarehouseCode       : GAMPING
ServerTargetId      : JOG
LastOperatorName    : YUDIS
LastOperatorEmail   : drury.yl@gmail.com
```

Local warehouse database (`btrgd.dbo.BTRG_ReturnOrder`), the single affected row:

```text
ReturnOrderId      : 01M53D0977E2SZQBY6ERPDQAHM
ReturnOrderDate    : 2026-10-17
CustName           : ADAM GROUP GROSIR
WarehouseCode      : GD-BTR
OperatorName       : OPERATOR
StatusSync         : Synced
```

The configured registry values (`GAMPING`, `YUDIS`) do not appear in the
document; instead the document carries `GD-BTR` and `OPERATOR`, which are the
fallback literals declared in the entry view.

## Problem Analysis

### Finding 1 — `GD-BTR` is a hardcoded fallback literal in the entry view

`BtrReturGudang.Winform/Views/ReturnOrderEntryView.cs`, `SaveDraftAsync()`:

```csharp
var warehouseCode = _registryHelper?.WarehouseCode ?? "GD-BTR";
if (string.IsNullOrWhiteSpace(warehouseCode)) warehouseCode = "GD-BTR";

var operatorName = _registryHelper?.LastOperatorName ?? "OPERATOR";
if (string.IsNullOrWhiteSpace(operatorName)) operatorName = "OPERATOR";
```

`GD-BTR` exists nowhere else in the repository; every other view uses `GDG-PST`
as its placeholder (`MainForm.cs`, `BarcodeRegistrationDialog.cs`,
`QuickBarcodeRegistrationDialog.cs`). It is a stray value, not a configured one.

Note that `RegistryHelper.WarehouseCode` returns `string.Empty` (never `null`)
when unset, so the `?? "GD-BTR"` is effectively dead; the
`IsNullOrWhiteSpace(...)` guard is what substitutes `GD-BTR`. The same holds for
`OPERATOR`.

### Finding 2 — The fallback only triggers when the workstation identity is blank

`SaveDraftAsync()` reads the warehouse code and operator name live from
`IRegistryHelper` at save time. `RegistryHelper` reads
`HKCU\Software\BTRApp\ReturGudang` on every access, and both the Settings view
and the entry view receive the same DI singleton (`Program.cs`:
`AddSingleton<IRegistryHelper, RegistryHelper>()`). Therefore `GD-BTR`/`OPERATOR`
can only be written when both registry values were blank (or `_registryHelper`
was null) at the moment the draft header was first saved — i.e. the draft was
created before the workstation identity was configured.

### Finding 3 — The identity is frozen at creation and never refreshed on update

Once a draft exists, `SaveAndUploadAsync()` → `SaveDraftAsync()` takes the
update branch (`CurrentReturnOrderId` is not blank) and calls
`IReturnOrderService.UpdateAsync`. `ReturnOrderService.UpdateAsync` updates the
customer, driver, salesman, and notes only; it never sets `WarehouseCode` or
`OperatorName`. A draft created while the registry was empty therefore keeps
`GD-BTR`/`OPERATOR` forever, even after `[F5]` is configured and the document is
re-uploaded.

### Finding 4 — The placeholder value is uploaded verbatim

`SyncService.TriggerDocumentUploadAsync` maps the local entity through
`ReturnOrderDto.FromDomain` (which copies `order.WarehouseCode`) and posts it via
`BtradeApiClient.UploadReturnOrderAsync` (`WarehouseCode = dto.WarehouseCode`).
The Cloud `ReturnOrderController.Submit` stores `request.WarehouseCode` verbatim
into `BTRADE_ReturnOrder.WarehouseCode`; the `X-Session-Location` header is used
only to resolve the tenant `ServerId`, not to populate the stored warehouse code.
The placeholder therefore propagates to the Cloud relay unchanged.

### Root Cause

`ReturnOrderEntryView.SaveDraftAsync()` silently substitutes the hardcoded
placeholder literals `GD-BTR` and `OPERATOR` when the workstation identity is
not configured, and `ReturnOrderService.UpdateAsync()` never refreshes that
identity. A draft created before `[F5]` was configured is persisted with the
placeholder values, frozen on subsequent updates, and uploaded to the Cloud
verbatim — producing the observed `WarehouseCode = 'GD-BTR'`,
`OperatorName = 'OPERATOR'` row.

The domain already treats the warehouse as mandatory
(`ReturnOrder.cs` — "Warehouse is mandatory (BR-005, BR-006)"), so substituting a
guessable default is contrary to intent: the correct behavior is to refuse the
save and direct the operator to `[F5] Pengaturan`.

## Affected Components

- **Desktop client (j09):**
  - `BtrReturGudang.Winform/Views/ReturnOrderEntryView.cs` — `SaveDraftAsync`
    placeholder substitution.
  - `BtrReturGudang.AppTier/Services/ReturnOrderService.cs` — `UpdateAsync` does
    not refresh the workstation identity.
  - `BtrReturGudang.Winform.Tests/ReturnOrderEntryViewTests.cs` — tests relied on
    the fallback (registry helper passed as `null`).
- **Cloud relay (j06):** `ReturnOrderController`,
  `ReturnOrderUploadCommandHandler`, `BTRADE_ReturnOrder` — unchanged; relay
  behavior (store `request.WarehouseCode`) is correct.

## Impact Assessment

- **Business Impact:** Return Orders can be recorded and uploaded under an
  incorrect warehouse/tenant context whenever the workstation is not configured
  before capture; downstream routing (tenant `ServerId`) and attribution are
  driven by the session header, but the stored warehouse code is wrong and
  misleading.
- **Operational Impact:** The operator receives no signal that the workstation
  identity is missing; the wrong value is silently persisted.
- **Technical Impact:** Stale identity is frozen into drafts by the update path,
  so correcting `[F5]` does not repair an in-flight draft.

## Recommended Decision

Fail fast instead of substituting placeholders:

1. In `SaveDraftAsync`, when `WarehouseCode` or `LastOperatorName` is blank (or
   `_registryHelper` is unavailable), play the warning cue, surface a clear
   message directing the operator to `[F5] Pengaturan`, and return `null` without
   calling `CreateAsync`/`UpdateAsync`. Remove the `GD-BTR` and `OPERATOR`
   literals.
2. Do not change `j06-pkl-btrade-api`; the Cloud relay is correct.
3. Correction of the pre-existing wrong rows (`BTRG_ReturnOrder` locally and
   `BTRADE_ReturnOrder` in the Cloud) is **out of scope** of this correction and
   is handled separately.

## Decision Rationale

- **Root-cause alignment:** removing the placeholder stops invalid identity data
  from ever being written; blocking at the boundary matches the mandatory
  warehouse invariant (BR-005, BR-006).
- **Minimal blast radius:** changes are confined to the j09 client; no Cloud
  contract, schema, or shared consumer is touched.
- **Operator clarity:** an explicit message replaces a silent, incorrect default.
- **Rejected alternative — keep a fallback default (`GDG-PST`):** would continue
  to persist a warehouse the workstation is not assigned to, masking the
  configuration gap.
- **Rejected alternative — refresh identity on every update:** leaves the
  silent-fallback defect in place for newly created drafts; blocking is simpler
  and safer.

## Architecture Applicability

### Decision

**ARCHITECTURE-NOT-REQUIRED**

### Rationale

The correction is a client-side defect fix within the existing component
responsibilities defined by
`docs/work/j09-btr-retur-gudang/BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md`; it
introduces no new boundary, contract, or architectural decision and reaffirms the
existing mandatory-warehouse invariant.

---

## Related Artifacts

| Artifact | Relevance |
| :--- | :--- |
| `docs/features/return-order-desktop/feature.md` | Workstation precondition: "configured with a valid Warehouse / Depo assignment"; `WarehouseCode` ownership. |
| `docs/work/return-order/RETURN-ORDER-DOMAIN.md` | BR-005/BR-006 — warehouse is mandatory. |
| `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/ReturnOrderEntryView.cs` | Carries the `GD-BTR`/`OPERATOR` fallback literals. |
| `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/Services/ReturnOrderService.cs` | `UpdateAsync` does not refresh the workstation identity. |
| `src/j06-pkl-btrade-api` — `ReturnOrderController` | Cloud relay (authoritative, unchanged). |
