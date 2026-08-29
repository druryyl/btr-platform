# Desktop Stock Opname Codebase Excavation

## 1. Executive Summary

### Scope and evidence basis

This report is a static codebase excavation of the current BTR repository. No runtime desktop session, production database, printed paper sample, or operator interview was available. Conclusions are therefore marked as **confirmed by code**, **inferred**, or **unknown**.

The currently reachable transaction entry point is the Inventory ribbon menu `IT1-Opname`, which opens `StokOpForm` (`src/j05-btr-distrib/btr.distrib/SharedForm/MainForm.Designer.cs:585-592`; `MainForm.cs:461-469`). Access to that menu is controlled by the role's permitted menu names, except for user `yudis` and role `SYSAD`, which bypass menu disabling (`MainForm.cs:101-129`). The reporting entry point `IF7-Stok Opname` opens the read-only `StokOpInfoForm` (`MainForm.Designer.cs:678-685`; `MainForm.cs:543-551`).

The active feature is not a formal Stock Opname session. It is a category-and-warehouse item worklist whose individual lines become persistent, already-posted adjustments when the operator edits `QtyOpnameInputStr`. There is no session header, status, counter assignment, print state, verification, approval, submission, or final posting command. `BTR_StokOp` is one row per entered item adjustment with a generated `StokOpId`; warehouse, category, and date are only screen/list filters, not a persisted session scope (`BTR_StokOp.sql:1-24`).

The operator selects a business date (`PeriodeOp`), Warehouse, and Kategori, then clicks **List Stok**. The application loads every item in the selected category, reads live warehouse balance from `BTR_StokBalanceWarehouse`, initializes the physical quantity equal to that live balance, and overlays previously entered rows for the same date and warehouse (`StokOpForm.cs:383-520`). The only editable grid field is `QtyOpnameInputStr`, a semicolon-delimited large-unit/small-unit value (`StokOpForm.cs:524-588`).

Changing a line immediately posts it. `StokOpForm.BrgGrid_CellValueChanged()` calls `SaveStokOpWorker.Execute()` (`StokOpForm.cs:297-335`). For a prior line, the worker first rolls back and deletes the old adjustment, then creates a new `BTR_StokOp` row and adjusts stock so current stock equals the entered physical quantity (`SaveStokOpWorker.cs:41-69`; `RollbackStokOpWorker.cs:41-79`; `GenStokStokOpWorker.cs:60-117`). Positive differences create inventory through `AddStokWorker`; negative differences consume FIFO stock through `RemoveFifoStokWorker`. Both paths update `BTR_Stok`, write `BTR_StokMutasi`, and refresh `BTR_StokBalanceWarehouse`.

The expected quantity used for posting is **live stock at the instant each line is processed**, not a stable count snapshot. The screen initially displays live stock, but the posting worker independently re-reads live stock and calculates `physical - current live stock` (`GenStokStokOpWorker.cs:62-64,105-117`). There is no warehouse freeze, cutoff timestamp, movement reconciliation, or concurrency token.

The Excel action in the active screen exports the currently loaded worklist/report and exposes expected, adjustment, ending, and HPP values; it does not save a session or mark counting as started (`StokOpForm.cs:122-270`). Thus it is not blind-count printing. A user can print the resulting workbook outside the application, but the code has no print template, printer API, signatures, page grouping, or linkage between a printed copy and later entry.

Readiness for Android-assisted counting is **low as a session authority**. BTR Desktop already has reusable item, warehouse, unit conversion, live stock adjustment, mutation, and reporting concepts. It lacks the session/snapshot/state/idempotency/concurrency structures required to safely own an offline or synchronized multi-user count. A future integration should call an application/service boundary owned by BTR; Android must not directly access the desktop database.

## 2. Feature Map

| Layer / Area | Component | Responsibility | Status | Evidence |
| --- | --- | --- | --- | --- |
| Menu/security | `MainForm.SetupUserMenu()` | Enables ribbon buttons contained in a user's role; `SYSAD` and `yudis` bypass | Active | `btr.distrib/SharedForm/MainForm.cs:101-129` |
| Transaction menu | `IT1OpnameMenu` | Opens `StokOpForm` | Active/reachable | `MainForm.Designer.cs:585-592`; `MainForm.cs:461-469` |
| Reporting menu | `IF7StokOpnameMenu` | Opens `StokOpInfoForm` | Active/reachable | `MainForm.Designer.cs:678-685`; `MainForm.cs:543-551` |
| Active entry UI | `StokOpForm` | Select date/warehouse/category, load items, edit counts, export Excel | Active | `StokOpForm.cs:28-86,122-270,273-397` |
| Active save orchestration | `SaveStokOpWorker` | Roll back prior line; create/save/post replacement line atomically for the new adjustment | Active | `SaveStokOpWorker.cs:23-76` |
| Adjustment calculation/posting | `GenStokStokOpWorker` | Reads current balance and posts delta to physical count | Active | `GenStokStokOpWorker.cs:60-117` |
| Edit rollback | `RollbackStokOpWorker` | Reverses stock mutation and deletes old `BTR_StokOp` row | Active | `RollbackStokOpWorker.cs:41-79` |
| Line construction | `StokOpBuilder` | Loads item/warehouse, converts units, calculates variance | Active | `StokOpBuilder.cs:57-170` |
| Persistence | `StokOpWriter`, `StokOpDal` | Generates `SOPN...` ID; inserts/updates/deletes line | Active | `StokOpWriter.cs:22-36`; `StokOpDal.cs:24-199` |
| Stock-in path | `AddStokWorker` | Adds a FIFO stock lot and regenerates balance | Active/indirect | `AddStokWorker.cs:68-97` |
| Stock-out path | `RemoveFifoStokWorker` | Removes FIFO stock and regenerates balance | Active/indirect | `RemoveFifoStokWorker.cs:62-108` |
| Stock ledger writer | `StokWriter` | Saves stock lot and replaces its mutation rows in a transaction | Active/indirect | `StokWriter.cs:31-55` |
| Balance source | `IStokBalanceWarehouseDal` / `BTR_StokBalanceWarehouse` | Current item/warehouse balance | Active | `StokOpForm.cs:456-477`; `GenStokStokOpWorker.cs:112-117`; table SQL |
| Opname line table | `BTR_StokOp` | Stores expected, physical, delta, item, warehouse, date, user | Active | `btr.sql/Tables/InventoryContext/BTR_StokOp.sql:1-24` |
| Stock lots | `BTR_Stok` | Stores remaining quantity/value by item/warehouse/reference | Active/indirect | `BTR_Stok.sql:1-24`; `StokDal.cs:24-80` |
| Movement ledger | `BTR_StokMutasi` | Stores `STOKOP` in/out mutations and reference | Active/indirect | `BTR_StokMutasi.sql:1-30`; `StokMutasiDal.cs:23-47` |
| Information UI | `StokOpInfoForm` / `StokOpInfoDal` | Read-only period report and Excel export | Active | `StokOpInfoForm.cs:123-248`; `StokOpInfoDal.cs:27-62` |
| Older direct entry | `OpnameForm`, `BTR_Opname` | Earlier one-item implementation using `OPNAME` movement code | Legacy/duplicate; no menu call found | `OpnameForm.cs:24-246`; `BTR_Opaname.sql:1-20`; repository-wide call search |
| Spreadsheet importer | `ImportOpnameForm` | Imports rows and posts legacy `BTR_Opname` adjustments | Implemented but unreachable from current menu | `ImportOpnameForm.cs:171-301`; `MainForm.cs:769-776` (opening code commented) |
| Import staging | `BTR_ImportOpname`, `ImportOpnameDal` | Bulk staging by item code/warehouse | Indirect/uncertain; not used by the visible importer flow | `BTR_ImportOpname.sql:1-8`; `ImportOpnameDal.cs:22-61` |
| Playground import | commented `TestPlayground` / SupportContext types | Old experimental import/post code | Obsolete/commented | `SharedForm/TestPlayground.cs:10-140`; SupportContext import files |
| Generic adjustment | `AdjustmentForm`, `BTR_Adjustment` | Separate inventory adjustment feature | Related but not part of reachable Stok Opname call path | `MainForm.cs:488-496`; `IT6-Adjustment` menu |
| External systems | BTR Gudang, BTR.Sync, API projects | Adjacent inventory/sync systems | No Stock Opname integration found | repository search; `docs/foundation/LANDSCAPE.md` |

## 3. Reconstructed Business Workflow

### Proven system workflow

1. An authorized user opens Inventory → `IT1-Opname`. Menu authorization only controls whether the ribbon button is enabled (`MainForm.cs:101-129,461-469`).
2. The user chooses `PeriodeOp`, Warehouse, and Kategori, then clicks **List Stok** (`StokOpForm.Designer.cs:63-154`).
3. The screen lists all items returned by `IBrgDal.ListData(kategori)`, sorted by item code (`StokOpForm.cs:422-453`).
4. It reads current `BTR_StokBalanceWarehouse` quantities for the selected Warehouse and unit conversions for the selected Kategori. Both displayed expected and physical quantities are initialized to current stock (`StokOpForm.cs:456-491`).
5. It reads saved `BTR_StokOp` lines for the selected date and Warehouse, then overlays the first matching row per item (`StokOpForm.cs:494-520`; `StokOpDal.cs:168-197`).
6. The user edits only `QtyOpnameInputStr`; all other columns are read-only (`StokOpForm.cs:524-552`). The intended syntax is `large;small`.
7. On `CellValueChanged`, the input is parsed and the worker is called immediately (`StokOpForm.cs:297-380`). There is no separate Save/Post button.
8. If the displayed item already has a `StokOpId`, the old adjustment is reversed and its line deleted before replacement (`SaveStokOpWorker.cs:41-46,72-76`; `RollbackStokOpWorker.cs:41-49`).
9. A new `BTR_StokOp` line is saved with a generated `SOPN...` ID (`StokOpWriter.cs:22-30`).
10. Current live stock is re-read. Variance is calculated as `physical - current live stock` (`GenStokStokOpWorker.cs:62-64,105-117`).
11. A positive variance adds a stock lot; a negative variance removes FIFO stock. Mutation type is `STOKOP`, reference is `StokOpId`, description is `Stok-Op: <PeriodeOp>` (`GenStokStokOpWorker.cs:73-100`).
12. The resulting expected quantity is written back to the `BTR_StokOp` line (`SaveStokOpWorker.cs:58-65`) and grid totals are refreshed.
13. The user may export the loaded grid to Excel. Export does not change database state (`StokOpForm.cs:122-270`).
14. The separate `IF7-Stok Opname` screen reports non-zero physical rows for a date range and can export them (`StokOpInfoDal.cs:27-62`; `StokOpInfoForm.cs:39-111,228-248`).

### Inferred manual workflow

Code supports, but does not enforce, the likely practice: load an item/category/warehouse list, export it to Excel, print it externally, count physically, then return to the same screen and enter counts. Because the export shows expected quantities and values, the generated workbook is not a blind-count sheet. Because export creates no header/session/audit record, the system cannot know that counting started or relate a printed sheet to later entries.

No code proves that counting occurs after hours, that warehouse movements stop, that every line is counted, that supervisors sign paper, or that one user owns the count. Those remain operational assumptions requiring observation.

## 4. UI and User Actions

### Parameters and scope

- Date: `PeriodeOpText`; stored as date-only `PeriodeOp` on each entered line (`StokOpBuilder.cs:114-117`).
- Warehouse: required by ComboBox selection; loaded from all warehouses (`StokOpForm.cs:288-294`).
- Kategori: required by ComboBox selection; controls the generated item list (`StokOpForm.cs:278-286,422-453`).
- No branch, Depo, supplier, rack, item range, or selected-item mode exists in the active entry screen.

### Commands

- **List Stok**: loads/reloads worklist.
- Editable `Qty Opname` cell: saves and posts the line immediately.
- **Excel**: exports the current grid and starts Excel/default associated application.
- No Create, Save Draft, Verify, Approve, Post, Cancel, Close, Reopen, Delete, barcode, or batch-import command exists in the reachable active screen.

### Entry and display behavior

Expected stock is visible. Physical stock is initially copied from expected stock, not blank. This makes untouched and “counted equal” indistinguishable. `QtyOpnameInputStr` starts blank for unsaved lines, while computed physical columns already equal expected; however a line is only persisted when the editable cell changes.

The parser accepts two semicolon-separated integers and silently converts missing/non-numeric parts to zero (`StokOpForm.cs:361-380`). Negative integers can be parsed. The UI uses a text grid column, and no client or worker guard rejects negative physical values. Negative counts would generally result in a stock-out request exceeding available stock and fail, but that is an indirect failure, not a count validation.

Quantities are integers throughout `BTR_StokOp`, `BTR_Stok`, request types, and UI DTOs. Decimal quantities are not supported. The unit model uses the maximum item conversion as “large” and conversion 1 as “small” (`StokOpBuilder.cs:139-164`). No unit identity is stored on `BTR_StokOp`; only normalized quantities and the input string are stored.

Potential normalization defects are visible:

- `NormalizeInput()` tests `qtys[1] > conversion`, not `>=`, so exactly one conversion remains in the small-unit slot (`StokOpForm.cs:345-358`).
- Its conversion branch calculates `addedQty` from `qtys[0] / conversion` and then modifies the opposite elements, which does not match the documented intent; the builder later applies a different normalization algorithm (`StokOpBuilder.cs:151-164`).
- The builder stores `QtyOpnameInputStr = $"{qtyBesar};{sisaQtyKecil}"`, which can disagree with normalized `QtyBesarOpname` when small quantities carry into large units (`StokOpBuilder.cs:161-164`).

These are implementation defects confirmed by the formulas, though their operational frequency requires runtime data.

### Authorization

Role membership enables the whole menu. Once opened, no method-level permission, reauthentication, verifier/approver role, or database permission check specific to Stock Opname appears in the call path. Disabled UI buttons therefore are not a backend security boundary. The same logged-in `UserId` is written as the line user (`StokOpForm.cs:302-318`).

### Resume and multi-user behavior

Reloading the same date/Warehouse overlays previously entered lines, so work can be resumed after application restart (`StokOpForm.cs:494-520`). There is no owner lock. Multiple users can load and edit overlapping scopes. Only the last generated line IDs retained by each client are known locally.

## 5. Lifecycle and State Transitions

There is no status field and no formal lifecycle. The only persistent stages are inferred from row/mutation existence.

| Current State | Action | Preconditions | Next State | Side Effects | Evidence |
| --- | --- | --- | --- | --- | --- |
| Not represented | Load list | Date, Warehouse, Kategori selected | UI-only loaded line | Reads live balance; physical initialized equal to expected | `StokOpForm.cs:383-491` |
| UI-only loaded line | Export Excel | List loaded | Still UI-only | Creates workbook; no DB write | `StokOpForm.cs:122-270` |
| UI-only loaded line | Edit `QtyOpnameInputStr` | Parsable input; valid item/warehouse/unit | Persisted-and-posted item | Inserts `BTR_StokOp`; writes stock/mutations/balance | `StokOpForm.cs:297-335`; `SaveStokOpWorker.cs:41-69` |
| Persisted-and-posted item | Edit again from same loaded row | Local row has old `StokOpId` | Replacement persisted-and-posted item | Reverses old mutation, deletes old line, generates a new ID, reposts against live stock | `SaveStokOpWorker.cs:43-59`; `RollbackStokOpWorker.cs:41-79` |
| Persisted-and-posted item | Reload list | Same date and Warehouse | Displayed persisted line | First row matching item overlays live display | `StokOpForm.cs:494-520` |

No Printed, Counting, Entered, Verified, Approved, Posted, Cancelled, Closed, or Reopened state is represented. “Entered” and “Posted” are the same event.

## 6. Data Model

### `BTR_StokOp`

- Primary key: `StokOpId VARCHAR(13)`.
- Identity/scope: `BrgId`, `WarehouseId`, `PeriodeOp`.
- Audit: `StokOpDate`, `UserId`.
- Expected: `QtyBesarAwal`, `QtyKecilAwal`, `QtyPcsAwal`.
- Physical: `QtyBesarOpname`, `QtyKecilOpname`, `QtyPcsOpname`.
- Variance: `QtyBesarAdjust`, `QtyKecilAdjust`, `QtyPcsAdjust`.
- Raw-ish entry: `QtyOpnameInputStr`.
- No foreign keys, unique `(PeriodeOp, WarehouseId, BrgId)` constraint, status, version, session/header ID, approval fields, or print fields (`BTR_StokOp.sql:1-24`).

Each row is an item adjustment, not a document detail linked to a header. Multiple rows for the same date/Warehouse/item are database-valid. The UI selects only `FirstOrDefault` when reloading, with no `ORDER BY` in `StokOpDal.ListData()`, so duplicate selection is nondeterministic.

### Inventory data

`BTR_StokBalanceWarehouse` is a materialized/current balance keyed by `(BrgId, WarehouseId)` with integer `Qty` (`BTR_StokBalanceWarehouse.sql:1-12`). `BTR_Stok` stores stock lots with `StokId`, business date, reference, item, Warehouse, received/remaining quantity, and inventory value (`BTR_Stok.sql:1-24`). `BTR_StokMutasi` stores movement rows with reference, type, business and recording dates, in/out quantity, selling price, and description (`BTR_StokMutasi.sql:1-30`).

No batch, serial, expiry, damaged, quarantine, consignment, rack, or Depo attribute participates in the Stock Opname rows or posting calls. Warehouse is the only inventory-location dimension.

### Relationship summary

```text
BTR_StokOp (independent item adjustment)
  BrgId ----------> BTR_Brg
  WarehouseId ----> BTR_Warehouse
  StokOpId --------> used as ReffId (logical, no FK)
                       |
                       +--> BTR_Stok
                       +--> BTR_StokMutasi

BTR_StokBalanceWarehouse
  (BrgId, WarehouseId) = current aggregate balance
```

The SQL project defines no foreign-key relationship for these links.

### Direct answers

1. Persistent session/document: **No**; only per-item adjustment rows.
2. Multiple sessions: not meaningful; multiple line groups can share date/Warehouse.
3. Same Warehouse/date: yes, lines and even duplicates are allowed.
4. Item counted once or multiple times: intended once per loaded grid, but database allows duplicates and edits replace history.
5. Count history: only current surviving line plus mutation consequences; edit removes prior line.
6. Recount preservation: no.
7. Original quantities after edit: prior line deleted; rollback mutations may remain as ledger events depending on rollback worker behavior, but no explicit version link.
8. Expected snapshot: stored per posted line, captured at that line's posting instant.
9. Physical separate from variance: yes.
10. Adjustment separate: yes.
11. Zero versus not counted: a persisted zero can be distinguished by row existence, but the report filters zero physical totals; UI defaults create ambiguity.
12. UOM stored: no unit names, only decomposed integer quantities and input string.
13. Batch/serial/expiry/damaged/quarantine: absent.
14. Location-level: Warehouse only; no rack/Depo.
15. Actor identities: one `UserId`; no separate counter/verifier/approver/poster.

## 7. Expected Stock and Snapshot Semantics

The active UI first reads `BTR_StokBalanceWarehouse` by Warehouse and copies `Qty` into both expected and physical columns (`StokOpForm.cs:456-481`). This is only a display initialization.

At posting, `GenStokStokOpWorker.GetCurrentStok()` independently reads the same current balance by item and selects the target Warehouse (`GenStokStokOpWorker.cs:112-117`). It computes:

```text
QtyPcsAdjust = QtyPcsOpname - current BTR_StokBalanceWarehouse.Qty
```

(`GenStokStokOpWorker.cs:105-109`).

The line's stored `QtyPcsAwal` is then set to the current quantity returned by the posting worker (`SaveStokOpWorker.cs:58-65`). Therefore:

> The current feature compares each physical count against live stock at the time that individual line is processed. It does not use a stable Stock Opname snapshot.

Movement after list/export but before entry changes the effective expected quantity. Movement after one line is posted but before another is posted means lines in the same apparent date/Warehouse count can have different cutoff instants. `PeriodeOp` is a user-selected business date used on mutations, not a captured cutoff timestamp. Historical expected stock is not reconstructed.

Stock balance is the sum/current aggregate of stock lots; adding recomputes from lot quantities (`AddStokWorker.cs:88-96`) and removal calls `IGenStokBalanceWorker` (`RemoveFifoStokWorker.cs:97-107`). Reserved, ordered, in-transit, damaged, and consignment stock are not included by the Stock Opname code path. Negative system balance behavior is unclear from static code; physical negative is unsupported operationally but not explicitly validated.

## 8. Physical Count and Variance Rules

- Formula: `Variance/adjustment = Physical - Current live stock`.
- Positive variance: creates stock at current item `Hpp` (`GenStokStokOpWorker.cs:77-85`).
- Negative variance: removes FIFO stock; insufficient positive FIFO quantity throws `ArgumentException` (`RemoveFifoStokWorker.cs:74-94`).
- Valuation displayed: current `BTR_Brg.Hpp * quantity`, not a stored historical count valuation (`StokOpForm.cs:644-647`; `StokOpInfoDal.cs:42`).
- Integer quantities only.
- Large/small conversion uses maximum configured conversion; small unit is conversion 1.
- No tolerance or variance threshold.
- No manual variance field.
- Zero variance: `BTR_StokOp` row is saved, but no stock mutation is generated (`SaveStokOpWorker.cs:58-65`; `GenStokStokOpWorker.cs:65-71`).
- Zero physical: can be saved; `StokOpInfoDal` excludes rows where `QtyBesarOpname + QtyKecilOpname == 0` (`StokOpInfoDal.cs:49-53`).
- Blank entry parses as zero if an edit event is raised (`StokOpForm.cs:361-380`).
- Inactive/zero-stock items: inclusion depends on `IBrgDal.ListData(kategori)`; no active or nonzero filter is present in the form itself.
- Items absent from category list cannot be added in the active screen.
- Duplicate items are not guarded by database constraints.
- Barcode/alternate item identifiers are not used; item selection is generated from `BrgId`, displayed with `BrgCode`/`BrgName`.

## 9. Verification and Approval

No verification, approval, rejection, threshold approval, recount, or credential confirmation is present. The menu role controls create/edit/post collectively. The same user who edits the cell causes posting, and only that `UserId` is stored.

There is no backend authorization in `SaveStokOpWorker`, `GenStokStokOpWorker`, `StokOpWriter`, or DALs. Any in-process caller with these services can execute them. Database enforcement depends solely on the application's database credentials, not business roles.

## 10. Posting and Inventory Adjustment

### Technical sequence

```text
StokOpForm.BrgGrid_CellValueChanged
  -> SaveStokOpWorker.Execute
     -> if editing: RollbackStokOpWorker.Execute
        -> reverse stock effect
        -> delete old BTR_StokOp row
     -> StokOpBuilder.Create / QtyOpname / User / Periode
     -> StokOpWriter.Save
        -> insert BTR_StokOp with new SOPN id
     -> GenStokStokOpWorker.Execute
        -> read current BTR_StokBalanceWarehouse
        -> variance = physical - current
        -> positive: AddStokWorker
           -> StokWriter -> BTR_Stok + BTR_StokMutasi
           -> update BTR_StokBalanceWarehouse
        -> negative: RemoveFifoStokWorker
           -> consume BTR_Stok FIFO
           -> StokWriter -> BTR_Stok + BTR_StokMutasi
           -> regenerate BTR_StokBalanceWarehouse
     -> update BTR_StokOp expected/variance fields
  -> refresh grid
```

The new line save, stock generation, and line update are enclosed in `TransHelper.NewScope()` (`SaveStokOpWorker.cs:55-68`), and nested stock writers use transaction scopes. Assuming `TransHelper` creates an ambient `TransactionScope` and all connections enlist, the new-post path is atomic. This must be runtime/database verified.

The rollback happens **before** that new outer scope (`SaveStokOpWorker.cs:43-56`). Rollback itself is transactional, but if creating the replacement fails, the old count and its stock effect have already been removed. This is a confirmed cross-transaction recovery risk.

Posting is not idempotent by business key. A blank/new `StokOpId` always generates a new ID. Duplicate submission from another client or a stale row can create another adjustment. There is no posted flag to prevent reposting. Editing is implemented as destructive reversal plus replacement, not an immutable correction/reversal document.

There is no accounting journal call. Financial effect is inventory valuation inside stock lots/movement reporting using item HPP; general ledger is outside BTR's stated scope.

## 11. Printing Workflow

The active transaction screen's **Excel** button is the only preparation output found. It creates `StokOp_<timestamp>.xlsx` in the OS temporary directory and launches it (`StokOpForm.cs:127-270`). It is not a report definition or direct print operation.

Workbook content:

- company name/address;
- title and selected date;
- Kategori and Warehouse;
- item code/name;
- expected quantities (`Qty Awal`);
- variance (`Qty Adjust`);
- ending/physical quantities (`Qty Akhir`);
- HPP unit, before, adjustment, and ending values;
- subtotal values.

Rows follow the active grid order, which originates from `BrgCode` sorting (`StokOpForm.cs:428-452`). There is no grouping, explicit page break, signature area, counter/verifier name, session/document number, or blind-count hiding. Expected stock and cost are printed visibly.

Excel generation does not call a writer/DAL and does not mutate state. Re-export is possible, but it has no reprint identity. The application does not know counting began and cannot link the workbook to later entries.

The `IF7` information screen separately exports already-entered results (`StokOpInfoForm.cs:39-111`); it is retrospective reporting, not count-sheet creation.

## 12. Concurrency and Failure Handling

No freeze, lock, isolation hint, row version, optimistic token, unique scope constraint, or application-level mutex was found.

| Scenario | Current handling |
| --- | --- |
| Export/print succeeds but no count is saved | Expected behavior; export has no persistence or session |
| Crash before editing a line | Nothing saved; displayed defaults lost |
| Crash after line event succeeds | That individual line is already posted and resumable |
| Crash during new posting | Ambient transaction appears intended to roll back line/stock/balance together; runtime enlistment unverified |
| Failure after old edit rollback but before replacement | Old row/effect remain removed because rollback is a separate completed transaction |
| Stock changes after list/export | Posting uses new live balance, silently changing variance basis |
| User posts twice with no prior ID/stale client | New IDs and duplicate adjustments are possible |
| Two users edit same date/Warehouse/item | No coordination; each posts against whatever balance exists at its instant |
| Two users create overlapping Warehouse counts | Allowed |
| Same saved line edited by two stale clients | Both may attempt rollback/delete and repost; behavior can fail or produce misleading results |
| Negative variance exceeds available FIFO stock | Throws `Stok tidak mencukupi`; UI has no local recovery/validation shown |
| Database timeout | No explicit retry or idempotency key |
| Zero count | Row may persist but is excluded from `IF7` report |

Race conditions remain possible between reading live balance (`GenStokStokOpWorker.cs:112-117`) and writing stock. The code shows no SQL locking around that read. Concurrent normal stock transactions are not blocked.

## 13. Audit and Downstream Effects

Posted differences use mutation type `STOKOP`, `ReffId = StokOpId`, business date `PeriodeOp`, and description `Stok-Op: <date>` (`GenStokStokOpWorker.cs:80-91`). `BTR_StokMutasi` records item, Warehouse, input/output quantity, `MutasiDate`, `PencatatanDate`, type, reference, and description (`BTR_StokMutasi.sql:1-18`). Kartu Stok reports consume the general stock-mutation ledger, so Stock Opname adjustments are expected to appear there.

Audit limitations:

- `BTR_StokMutasi` has no user field.
- User is available only on the surviving `BTR_StokOp` line.
- Editing deletes the old line, so prior expected/physical/user values are not retained as count history.
- No previous/resulting balance columns are stored directly on the mutation row; reconstruction requires ordering and other stock data.
- No session reference groups multiple item rows.
- No cancellation/reversal link or reason is stored.
- `PencatatanDate` distinguishes recording time from business date in the ledger, but the count line itself only has creation time and business date.
- No accounting journal integration was found.

The `IF7` report joins item, Warehouse, category, supplier, and current item HPP, orders by date/supplier/category/item code, and excludes zero physical rows (`StokOpInfoDal.cs:29-53`). Because it uses current `BTR_Brg.Hpp`, historical report valuation can change when master HPP changes.

## 14. Tests and Evidence Coverage

No Stock Opname unit, integration, database, or UI tests were found under `btr.test` by searches for `StokOp`, `STOKOP`, or `OPNAME`. No manual test document specific to Stock Opname was found.

Critical uncovered behavior includes:

- item/unit input normalization;
- zero versus blank semantics;
- live-balance variance timing;
- positive and negative posting;
- insufficient FIFO stock;
- edit rollback/replacement failure;
- transaction enlistment/atomicity;
- duplicate and concurrent submissions;
- role enforcement;
- Excel count-sheet content;
- `IF7` zero-count omission;
- mutation/audit reconstruction.

## 15. Android Extension Readiness Matrix

| Required Capability | Existing Support | Evidence | Gap / Concern |
| --- | --- | --- | --- |
| Stock Opname session | Absent | No header/session table or ID | Per-item IDs cannot represent one count |
| Session identifier | Absent | `StokOpId` belongs to one item row | No batch correlation |
| Item scope | Partial | Date/Warehouse/Kategori UI filters | Kategori not stored; scope not frozen |
| Expected-stock snapshot | Partial | Expected stored per posted line | Captured independently at posting, not session start |
| Assignment to counter | Absent | One posting `UserId` | No assignment/ownership |
| Count-line status | Absent | Row presence only | Cannot distinguish assigned/downloaded/counting/submitted |
| Draft count | Absent | Edit posts immediately | No safe mobile draft ingestion |
| Physical quantity | Supported | `Qty*Opname` columns | Integer, two-unit model only |
| Multiple count attempts | Absent | Edit deletes prior row | No history |
| Recount | Absent | Destructive replacement | No recount reason/attempt |
| Submission | Absent | Cell edit equals posting | No batch submit |
| Verification | Absent | No fields/actions | — |
| Approval | Absent | No fields/actions | — |
| Posting | Supported per item | `SaveStokOpWorker` → stock workers | Coupled to entry; not idempotent by business key |
| Audit trail | Partial | `BTR_StokOp` + `BTR_StokMutasi` | No immutable history, session, approval, or mutation user |
| Synchronization identity | Absent | Server-generated per-line ID | No client-generated stable ID |
| Idempotency | Absent | New ID generated on blank request | Duplicate mobile delivery unsafe |
| Offline data package | Absent | No API/package | — |
| Conflict detection | Absent | No version/snapshot hash | Live movements and concurrent edits undetected |
| Item scan identity | Partial | `BrgCode` exists | No barcode path proven; alternate codes unknown |
| Warehouse scope | Supported | `WarehouseId` throughout | No Depo/rack granularity |
| Unit conversion | Partial | max conversion + base unit | No explicit selected unit; only two-level representation |
| Reporting | Supported per line | `IF7` and Excel | Zero physical omitted; no session report |

### Safest future integration boundaries

Without designing the final solution, the safest boundary is above the stock mutation workers: a BTR-owned application/API service should validate synchronized counts and invoke inventory adjustment logic transactionally. Reusable lower-level concepts include `IBrgDal`/item master, `IWarehouseDal`, unit conversion data, live balance reads, `IAddStokWorker`, `IRemoveFifoStokWorker`, stock balance regeneration, and mutation reporting. The Android application must not write `BTR_StokOp`, `BTR_Stok`, or `BTR_StokMutasi` directly.

## 16. Key Findings

### Confirmed behavior

1. `IT1-Opname` opens `StokOpForm`; `IF7` is reporting.
2. Physical quantity starts equal to visible live stock.
3. Each cell edit is an immediate inventory posting.
4. Variance is `physical - live stock at processing time`.
5. Positive differences add stock; negative differences consume FIFO stock.
6. Results write `BTR_StokOp`, `BTR_Stok`, `BTR_StokMutasi`, and balance.
7. Excel output is non-blind and non-persistent.

### Implementation defect

1. The two input-normalization algorithms disagree and contain suspicious operand/carry logic (`StokOpForm.cs:345-358`; `StokOpBuilder.cs:151-164`).
2. Duplicate scope rows are allowed, while reload picks the first unordered item match.
3. Zero physical counts are omitted from the `IF7` report.
4. Old adjustment rollback commits before replacement, allowing loss of the prior result if replacement fails.

### Architectural limitation

1. No formal session, status model, snapshot, immutable history, idempotency, or concurrency control.
2. Entry and posting are inseparable.
3. Only Warehouse/item and integer two-level units are modeled.
4. UI menu permissions are not reinforced in application services.

### Operational dependency

1. Accuracy depends on controlling stock movements outside the software.
2. Paper/Excel identity and supervisory sign-off exist, if at all, outside the system.
3. Operators must understand the `large;small` entry convention.

### Missing business rule

1. Cutoff/freeze and movement reconciliation.
2. Counter/verifier/approver separation.
3. Scope completion and explicit zero handling.
4. Recount, tolerance, rejection, cancellation, and reopening.
5. Multi-user ownership and conflict resolution.

### Unknown requiring stakeholder confirmation

1. Whether `StokOpForm` is the production-operational path in every deployed branch/version.
2. Actual paper procedure, count timing, signatures, and movement freeze practice.
3. Database transaction enlistment and production constraints/triggers beyond the SQL project.

## 17. Open Questions

| Question | Why it matters | Code evidence | Likely stakeholder |
| --- | --- | --- | --- |
| Is `IT1-Opname` the path used in every deployed desktop version, or are `OpnameForm`/import builds still deployed? | Determines authoritative behavior and migration scope | Current menu opens `StokOpForm`; import opening is commented; legacy code still compiles | Desktop release owner / branch IT |
| What exact paper sheet is used operationally? | Code export is not blind and has no signatures; a separate template may exist | Only `PrintToExcel()` found | Warehouse supervisor / inventory admin |
| Are transactions stopped during counting, and at what time? | No system cutoff/freeze exists; live posting can invalidate paper counts | Live balance read at each line | Operations manager |
| Does `Warehouse` correspond to the physical count area, or must counts be separated by Depo/rack? | Android line cardinality and assignment depend on location semantics | Stock Opname stores only Warehouse | Warehouse/data master owner |
| Does `BrgCode` represent a scannable barcode, and are alternates used? | Mobile scan identity cannot be assumed | Active flow uses `BrgId`/`BrgCode`; no barcode lookup | Product master owner |
| Is a physical zero expected to appear in `IF7` reports? | Current query excludes it, risking missing evidence of stockouts | `QtyBesarOpname + QtyKecilOpname <> 0` | Inventory controller |
| Who is allowed to count, verify, approve, and post today? | Code only models one menu permission and one user | No workflow roles/fields | Business owner / security administrator |
| Are database triggers, jobs, or production-only stored procedures present? | They could alter atomicity/audit conclusions | SQL project shows application SQL only | DBA |
| Does `TransHelper.NewScope()` enlist every independently opened `SqlConnection` in production? | Atomicity conclusion depends on ambient transaction configuration/MSDTC behavior | Nested workers open separate connections | DBA / platform maintainer |
| Are negative or fractional physical quantities ever legitimate? | Current integer model and weak validation may reject valid cases or accept invalid input | `INT` quantities; text parser accepts negative integers | Inventory controller |

## 18. Recommended Next Investigation

1. Observe a complete production walkthrough: list preparation, paper printing, physical count, entry, correction, and reporting.
2. Obtain anonymized samples of the actual pre-count sheet, completed sheet, and `IF7` result export.
3. Profile SQL during one positive, negative, zero, and edited count to confirm transaction boundaries and exact mutation records.
4. Inspect the deployed database schema for triggers, additional indexes/constraints, jobs, and drift from the SQL project.
5. Run a controlled concurrent test with two desktop users editing the same item and Warehouse.
6. Test failure injection between rollback and replacement save.
7. Interview Inventory Administration and Warehouse supervisors about cutoff, blind counting, roles, recounts, location granularity, and explicit-zero requirements.
8. Inventory deployed application versions/configurations to determine whether legacy `OpnameForm` or `ImportOpnameForm` remains operational anywhere.
9. Examine item-master barcode/alternate-code and unit data with representative products.
10. Define and approve permanent feature knowledge under `docs/features/stock-opname/feature.md` after stakeholder validation; this excavation remains temporary evidence, not authoritative future-state design.

