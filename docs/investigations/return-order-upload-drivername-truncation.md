# BUG-INVESTIGATION

Code: BTR-RETUR-DRIVERNAME-001
Artifact: BUG-INVESTIGATION
Type: BUG
Date: 2026-10-17
Status: COMPLETED

## Context

Issue: Failed Return Order upload from the j09 warehouse desktop client
(`src/j09-btr-retur-gudang/BtrReturGudang.sln`) to the Cloud relay
(`src/j06-pkl-btrade-api`). No formal ISSUE artifact was raised for this defect;
the investigation is based on the reported operational error and repository
evidence.

Problem Summary: When uploading a Return Order saved without a Driver, the Cloud
relay rejected the request with:

> Failed to upload Return Order '01M53D0977E2SZQBY6ERPDQAHM': HTTP 500:
> {"status":"Internal Server Error","code":"500","data":"String or binary data
> would be truncated in table 'DbKerjaPraktek.dbo.BTRADE_ReturnOrder', column
> 'DriverName'. Truncated value: '-- Pilih Driver (Ops'.\r\nThe statement has
> been terminated."}

The rejected document remained locally in `Draft` state.

## Current State

1. The operator works in the j09 Windows desktop client and saves a Return Order
   without selecting a Driver (an optional field, ADR-RO-005).
2. The client stores the document locally and, on "Simpan & Upload" (`F10`) or
   "Upload Ulang", posts it to `POST api/return-order` on `j06-pkl-btrade-api`.
3. The Cloud relay inserts the header into `BTRADE_ReturnOrder`. The `DriverName`
   column is `VARCHAR(20)`; the incoming value is the driver dropdown's prompt
   text, which is longer than 20 characters.
4. SQL Server raises error 8152 (string or binary data would be truncated); the
   relay maps it to **HTTP 500 Internal Server Error** and the document stays
   `Draft`.

## Problem Analysis

### Finding 1 — The desktop dropdown prompt is captured as data

`BtrReturGudang.Winform/Views/ReturnOrderEntryView.cs`:

- `LoadReferenceData()` seeds the optional Driver dropdown with a placeholder
  item whose constructor arguments are `(id = null, name = "-- Pilih Driver
  (Opsional) --")` and selects it by default (`SelectedIndex = 0`).
- `SaveDraftAsync()` reads the selected item as a `KeyValueItem` and passes both
  `driver?.Id` and `driver?.Name` into `IReturnOrderService.CreateAsync` /
  `UpdateAsync`.
- Because the placeholder item's `Name` *is* the prompt string, an unselected
  Driver contributes the 29-character literal `-- Pilih Driver (Opsional) --` as
  `DriverName`.

The same defect exists for the optional Salesman dropdown (`-- Pilih Salesman
(Opsional) --`).

### Finding 2 — The value is persisted locally, then uploaded verbatim

- `ReturnOrder` stores `DriverName` with no optional-field normalization
  (`BtrReturGudang.Domain/Entities/ReturnOrder.cs`); the local column is
  `BTRG_ReturnOrder.DriverName VARCHAR(100)` (`BtrReturGudang.SqlDb/Scripts/
  001_CreateWarehouseTables.sql`), wide enough not to fail locally.
- `SyncService.TriggerDocumentUploadAsync` / `RunFullSyncPassAsync` map the domain
  entity through `ReturnOrderDto.FromDomain` and call
  `BtradeApiClient.UploadReturnOrderAsync`, which places `DriverName =
  dto.DriverName` into the `POST api/return-order` payload
  (`BtrReturGudang.Infrastructure/Http/BtradeApiClient.cs`).
- The Cloud column is `BTRADE_ReturnOrder.DriverName VARCHAR(20)`
  (`btrade.sqldb/ReturnOrderContext/BTRADE_ReturnOrder.sql`,
  `btrade.sqldb/Scripts/Create_BTRADE_ReturnOrder.sql`), so the 29-character
  prompt overflows on insert.

### Finding 3 — The Cloud side is behaving correctly

`ReturnOrderController.Submit` → `ReturnOrderUploadCommandHandler` maps
`DriverName ?? string.Empty` into `ReturnOrderType` and persists it verbatim
(transport-only relay, P-02). The relay adds no validation and has no way to
distinguish a legitimate driver name from a UI prompt; the truncation is a data
defect originating on the client. `SalesPersonName` is also `VARCHAR(100)` on the
Cloud, which is why the analogous Salesman prompt fails silently (it is stored
rather than rejected).

### Finding 4 — The failure is not name-length in the Cloud master

The Cloud `BTRADE_Driver.DriverName` is itself `VARCHAR(20)`
(`btrade.sqldb/DriverContext/BTRADE_Driver.sql`), so any driver name legitimately
synced from the Cloud fits the `BTRADE_ReturnOrder.DriverName` column. The defect
is solely the client transmitting a UI prompt, not a mismatch between legitimate
driver name lengths and the column.

### Root Cause

The j09 desktop client uses the dropdown placeholder's display text as its value,
so leaving the optional Driver (or Salesman) unselected transmits the prompt
string `-- Pilih Driver (Opsional) --` as `DriverName`. This 29-character value
overflows the Cloud `BTRADE_ReturnOrder.DriverName VARCHAR(20)` column and the
insert fails with a truncation error surfaced as HTTP 500.

## Affected Components

- **Desktop client (j09):**
  - `BtrReturGudang.Winform/Views/ReturnOrderEntryView.cs` — placeholder items and
    the driver/salesman capture in `SaveDraftAsync`.
  - `BtrReturGudang.AppTier/DTOs/ReturnOrderDto.cs` — `FromDomain` mapping used by
    the upload path.
  - `BtrReturGudang.Infrastructure/Http/BtradeApiClient.cs` — outbound payload
    construction.
- **Cloud relay (j06):** `ReturnOrderController`, `ReturnOrderUploadCommandHandler`,
  `ReturnOrderDal`, `BTRADE_ReturnOrder` (`DriverName VARCHAR(20)`) — unchanged;
  correct as-is.

## Impact Assessment

- **Business Impact:** Return Orders captured without a Driver cannot be uploaded
  to the Cloud; intake documents remain `Draft` and are not relayed upstream. Any
  draft previously saved with the placeholder also fails on retry.
- **Operational Impact:** The 500 message is low-level and does not indicate the
  real cause to the operator.
- **Technical Impact:** Client defect polluting persisted and transmitted data;
  the Salesman field has the same defect but is masked by the wider Cloud column,
  resulting in incorrect denormalized names on the relay.

## Recommended Decision

Correct the desktop client so optional Driver/Salesman fields never carry the
dropdown prompt:

1. Separate the dropdown placeholder's display text from its value, so an
   unselected optional field contributes no id and no name.
2. Normalize at the mapping/boundary: when the optional id is blank, emit a blank
   name, so drafts already persisted with the prompt upload successfully without a
   data migration.
3. Do not change `j06-pkl-btrade-api`; the Cloud relay is correct and the
   legitimate driver-name length fits the existing column.

## Decision Rationale

- **Root-cause alignment:** fixing the placeholder value stops the prompt from
  ever being stored; the boundary normalization additionally rescues existing
  drafts (the observed `01M53D09…` case) with no schema change.
- **Minimal blast radius:** changes are confined to the j09 client; no Cloud
  contract, schema, or shared consumer (BGud) is touched.
- **Consistency with domain intent:** ADR-RO-005 defines Driver/Salesman as
  optional and blank when absent; a name without an id is not a meaningful value.
- **Rejected alternative — widen the Cloud column:** would allow the prompt
  string to be stored as if it were a driver name, masking the data defect and
  polluting the relay.
- **Rejected alternative — server-side truncation/validation:** treats the
  symptom on the relay while leaving the client emitting invalid data.

## Architecture Applicability

### Decision

**ARCHITECTURE-NOT-REQUIRED**

### Rationale

The correction is a client-side defect fix within the existing component
responsibilities defined by
`docs/work/j09-btr-retur-gudang/BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md`; it
introduces no new boundary, contract, or architectural decision. ADR-RO-005
(optional Salesman/Driver) is reaffirmed, not changed.

---

## Related Artifacts

| Artifact | Relevance |
| :--- | :--- |
| `docs/work/return-order/RETURN-ORDER-FEASIBILITY-ASSESSMENT.md` | ADR-RO-005 — Salesman/Driver optional, blank when absent. |
| `docs/investigations/return-order-upload-409-investigation.md` | Prior related defect on the same upload path (session actor contract). |
| `docs/investigations/return-order-upload-url-investigation.md` | Prior related defect on the same upload path (RestSharp URL / error masking). |
| `src/j06-pkl-btrade-api` — `ReturnOrderController`, `ReturnOrderUploadCommandHandler`, `BTRADE_ReturnOrder.sql` | Cloud relay (authoritative, unchanged). |
| `src/j09-btr-retur-gudang` — `ReturnOrderEntryView`, `ReturnOrderDto`, `BtradeApiClient` | Desktop side carrying the placeholder value. |
