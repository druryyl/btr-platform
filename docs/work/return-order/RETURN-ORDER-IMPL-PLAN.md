# IMPLEMENTATION PLAN

# Return Order

| Field | Value |
| ----- | ----- |
| Deliverable | `RETURN-ORDER-IMPL-PLAN.md` |
| Location | `docs/work/return-order/RETURN-ORDER-IMPL-PLAN.md` |
| Method | `docs/skills/planning-skill.md` (Mode A — Architecture-Driven Planning) |
| Planning Authority | `docs/work/return-order/RETURN-ORDER-ARCHITECTURE.md` |
| Inputs | `RETURN-ORDER-FEASIBILITY-ASSESSMENT.md` (GAP/ADR source), `RETURN-ORDER-DOMAIN.md` (business reference) |
| Implementation target | `src/BGud` (primary; Phase P4). Mandatory supporting changes in `src/j05-btr-distrib` (P1), `src/j06-pkl-btrade-api` (P2), `src/j07-btrade-sync` (P3) per the approved architecture |
| Author role | Planner (Planning only) |
| Status | **DRAFT** |

> This document is a planning artifact. It translates the approved architecture
> into implementation phases and slices. It does not implement, review,
> redesign, or create business or architecture decisions. The architecture
> (`RETURN-ORDER-ARCHITECTURE.md`) is authoritative: all gaps, ADRs
> (ADR-RO-001 … ADR-RO-008), principles (P-01 … P-13), invariants
> (INV-01 … INV-14), and interpretation-register selections (IR-RO-01 …
> IR-RO-11) are consumed as given and are not re-decided here.

---

## 1. Planning Authority

```text
ARCHITECTURE
```

Reference: `docs/work/return-order/RETURN-ORDER-ARCHITECTURE.md`

The architecture is authoritative. This plan does not reinterpret any
architecture decision, invariant, or interpretation-register selection. The
architecture settled the feasibility's non-blocking realization items
(INFO-001/INFO-002, C-1…C-6) as technical selections IR-RO-01 … IR-RO-11;
this plan consumes those selections directly. Where the architecture records
confirmations (§24), they are carried as pre-implementation confirmations
(§4), not resolved here.

---

## 2. Scope Summary

Realize the Return Order business capability as architected: a warehouse
operational document recorded offline in BGud, relayed through Cloud staging,
downloaded by `j07-btrade-sync`, and imported into the Main Office, where it
becomes the authoritative source document for Sales Return generation.

1. **Main Office (`src/j05-btr-distrib`)** — the authoritative `ReturnOrder`
   aggregate and tables `BTR_ReturnOrder` / `BTR_ReturnOrderItem` (distinct
   from `ReturJual`), lifecycle `Synced → Imported` (IR-RO-09), ULID identity
   `ReturnOrderId` vs business number `ReturnOrderNo` (IR-RO-01, IR-RO-07),
   `ImportReturnOrderCommand`, `CompleteReturnOrderCommand`,
   `GenerateSalesReturnFromReturnOrderCommand`, and the single Desktop
   "Generate Return Order" surface (`Retur Penjualan → Generate Return Order`).
2. **Cloud (`src/j06-pkl-btrade-api`)** — the `ReturnOrder` staging relay
   (idempotent submit, incremental download + acknowledgement, IR-RO-08),
   the `BTRADE_Driver` projection, and the centralized `BTR_WarehouseMapping`
   (`WarehouseCode` → `ServerId`, resolved at login, IR-RO-04). Never an
   authority (P-02).
3. **`src/j07-btrade-sync`** — the incremental download service staging Return
   Orders into the Main Office, the in-process `ImportReturnOrderCommand`
   dispatch via `MainOfficeCommandExecutor`, and the `DriverSyncService`
   projection uploader (IR-RO-06).
4. **BGud (`src/BGud`, primary target)** — offline create/update/delete/search/
   synchronize on the existing Room + Retrofit + WorkManager stack, reusing
   Barang/Barcode item identification and Customer/SalesPerson/Driver reference
   caches; device lifecycle `Draft → Synced` only (ADR-RO-006).

Out of scope (by architecture decision, not repeated as slices): inventory
update/valuation, customer balance, credit notes, accounting, approval/
rejection workflow, principal claim, disposal, settlement, invoice-level
tracking (DOMAIN §15); pricing/inventory/finance (stay in `ReturJual`,
ADR-RO-007); direct Mobile → Main Office (ADR-RO-001); snapshot replacement
(P-03); sync-back of `Imported`/`ReturnOrderNo` to BGud (ADR-RO-006);
background/scheduled/realtime sync (OQ-1); hardcoded warehouse mapping in
BGud (ADR-RO-008); a migration framework (P-12); a Return Order review/
approval workflow (P-13).

---

## 3. Impact Inventory

### Backend

| Component | System | Source |
| --------- | ------ | ------ |
| `BTR_ReturnOrder` / `BTR_ReturnOrderItem` DDL + `.sqlproj` + upgrade script | `btr.sql` | Arch §6.1, §10.1 |
| `ReturnOrderModel` / `ReturnOrderItemModel` + `IReturnOrderKey` | `btr.domain/InventoryContext/ReturnOrderAgg` | Arch §5.1 |
| `IReturnOrderDal` contract + `ReturnOrderDal` / `ReturnOrderItemDal` (Dapper) | `btr.application` / `btr.infrastructure` | Arch §5.1, §7.1 |
| `ReturnOrderBuilder` + `ReturnOrderValidator` + `ReturnOrderWriter` | `btr.application` | Arch §7.1 |
| `ImportReturnOrderCommand` | `btr.application` | Arch §7.1, IR-RO-07 |
| `CompleteReturnOrderCommand` | `btr.application` | Arch §7.1, IR-RO-05 |
| `GenerateSalesReturnFromReturnOrderCommand` | `btr.application` | Arch §7.1, §8.5 |
| `GetReturnOrderQuery` / `ListReturnOrderQuery` / `ListReturnOrderByCustomerQuery` | `btr.application` | Arch §7.1 |
| `ReturnOrderType` / `ReturnOrderItemType` + `IReturnOrderKey` | `btrade.domain/ReturnOrderFeature` | Arch §5.2 |
| `IReturnOrderDal` / `IReturnOrderItemDal` contracts + Dapper DALs | `btrade.application` / `btrade.infrastructure` | Arch §7.2 |
| `ReturnOrderUploadCommand` / `ReturnOrderIncrementalDownloadQuery` | `btrade.application` | Arch §7.2 |
| `BTR_WarehouseMapping` + resolution at login (`IssueTokenCommand`) | `btrade.sqldb` / `btrade.application` | Arch §6.3, IR-RO-04, §9.2 |
| `DriverType` + `IDriverDal` / `DriverDal` + `DriverSyncCommand` + `DriverListDataQuery` | `btrade.domain` / `btrade.application` / `btrade.infrastructure` | Arch §5.3, §7.2 |
| `ReturnOrderModel` / `ReturnOrderItemType` (sync transport) | `j07-btrade-sync/Model` | Arch §4.3 |
| `ReturnOrderDal` / `ReturnOrderItemDal` (Main Office staging) | `j07-btrade-sync/Repository` | Arch §4.3, §8.2 |
| `ReturnOrderIncrementalDownloadService` | `j07-btrade-sync/Service` | Arch §4.3, §8.2 |
| `MainOfficeCommandExecutor` extension (import dispatch) | `j07-btrade-sync/Shared` | Arch §4.3, §8.2 |
| `DriverSyncService` | `j07-btrade-sync/Service` | Arch §4.3, IR-RO-06 |
| `j07` client authentication (JWT on I-RO-02 / I-RO-06) | `j07-btrade-sync` | Arch §9.1, §8.6 |
| Room entities: `return_order_entity`, `return_order_item_entity`, `customer_entity`, `salesperson_entity`, `driver_entity` + DAOs + `AppDatabase` v2 migration | BGud `model/`, `dao/`, `database/` | Arch §6.4 |
| `BtradeApiService` endpoints + DTOs (submit Return Order, Customer/SalesPerson/Driver reference) + ULID generation | BGud `network/`, `model/api/` | Arch §8.1, IR-RO-01 |
| `ReturnOrderSyncRepository`, `ReturnOrderSyncWorker` | BGud `repository/`, `sync/` | Arch §4.4, §20 |
| ViewModels + screens (List / Create / Detail / Edit / Synchronization) | BGud `viewmodel/`, `ui/` | Arch §11.1, §19.1 |

### Database

| Object | System | Source |
| ------ | ------ | ---------- |
| `BTR_ReturnOrder` (`ReturnOrderId` ULID `VARCHAR(26)` PK, `ReturnOrderNo`, `ReturnOrderDate`, `WarehouseCode`, `CustomerId`, optional `SalesPersonId`/`DriverId`, `Note`, `Status` `Synced`/`Imported`, audit, `RowVer`; no FK) | `btr.sql` | Arch §6.1, IR-RO-01/09/11 |
| `BTR_ReturnOrderItem` (`(ReturnOrderId, NoUrut)` PK, `BrgId`, `BrgCode`, `Qty DECIMAL(18,2)`, `SatId`, `JenisRetur`) | `btr.sql` | Arch §6.1, IR-RO-02/03/10 |
| Idempotent upgrade script(s) | `btr.sql/Scripts` | Arch §10.1 |
| `BTRADE_ReturnOrder` (`(ReturnOrderId, ServerId)` PK, `StatusSync`) + `BTRADE_ReturnOrderItem` | `btrade.sqldb` | Arch §6.2, IR-RO-08 |
| `BTR_WarehouseMapping` (`WarehouseCode` PK → `ServerId`) + seed (`GAMPING`/`CONCAT`→`JOGJA`, `MAGELANG`→`MGL`) | `btrade.sqldb` | Arch §6.3, IR-RO-04 |
| `BTRADE_Driver` (`(DriverId, ServerId)` PK) | `btrade.sqldb` | Arch §6.3 |
| Idempotent upgrade script(s) | `btrade.sqldb/Scripts` | Arch §10.1 |
| Room: 5 new entities, version bump 1 → 2, non-destructive migration | BGud | Arch §6.4 |
| DataStore keys: `serverId`, `last_customer_sync`, `last_salesperson_sync`, `last_driver_sync` | BGud `datastore/` | Arch §6.4, §8.1 |

### Frontend

| Component | System | Source |
| --------- | ------ | ------ |
| Return Order List / Create / Detail (incl. Delete) / Edit / Synchronization screens (SCR-MOB-RO-001…005) | BGud | Arch §11.1, §12 |
| Home + Navigation wiring | BGud | Arch §13.1 |
| `GenerateReturnOrderForm` (SCR-DESK-RO-001) — single Desktop surface | `btr.distrib` | Arch §11.2, §12.5 |
| `BTR_Menu` / `BTR_RoleMenu` seed + ribbon wiring (`Retur Penjualan → Generate Return Order`) | `btr.sql` / `btr.distrib` | Arch §13.2, §9.3 |

### Integration

| # | Integration | Systems | Auth | Source |
| - | ----------- | ------- | ---- | ------ |
| I-RO-01 | `POST /api/return-order` (idempotent by ULID `ReturnOrderId`) | BGud → Cloud | JWT | Arch §8.1 |
| I-RO-02 | `GET /api/ReturnOrder/incremental/{tgl1}/{tgl2}/{serverId}` + download-coupled ack | `j07-btrade-sync` → Cloud | JWT | Arch §8.2, §8.6 |
| I-RO-03 | `GET /api/Customer/{serverId}` (existing, reused) | Cloud → BGud | Existing | Arch §8.3 |
| I-RO-04 | `GET /api/SalesPerson/{serverId}` (existing, reused) | Cloud → BGud | Existing | Arch §8.3 |
| I-RO-05 | `GET /api/Driver/{serverId}` (new) | Cloud → BGud | JWT | Arch §8.3 |
| I-RO-06 | `POST /api/Driver` projection upload | `j07-btrade-sync` → Cloud | JWT | Arch §8.4 |
| I-RO-07 | `ReturnOrderDal` staging (upsert by id, `Status = Synced`) | `j07-btrade-sync` → Main Office | in-process | Arch §8.2 |
| I-RO-08 | `MainOfficeCommandExecutor` → `ImportReturnOrderCommand` | `j07-btrade-sync` → Main Office | in-process | Arch §8.2 |
| I-RO-09 | `GenerateSalesReturnFromReturnOrderCommand` → `ReturJual` | Main Office (Desktop) | — | Arch §8.5 |
| — | Login warehouse selection → `BTR_WarehouseMapping` → `ServerId` (session stores both) | BGud → Cloud | — | Arch §9.2, IR-RO-04 |
| — | Sync triggers: Login Sync + Manual Sync Now only | BGud | — | Arch §20, OQ-1 |

### Security

- Reuse existing JWT + session-bound location model; no new mechanism (Arch §9.1).
- `ServerId` never client-supplied on write payloads; resolved server-side from
  the JWT (P-06, §9.2). `BTR_WarehouseMapping` resolves `WarehouseCode` →
  `ServerId` at login; BGud never hardcodes it (ADR-RO-008).
- `j07-btrade-sync` presents a JWT on I-RO-02 / I-RO-06 (Arch §9.1).
- Role gate: Warehouse Officer (create/update/delete/sync) vs Office Admin
  (complete Salesman/Driver + generate Sales Return) (Arch §9.3).

---

## 4. Pre-Implementation Confirmations

These are architecture-recorded confirmations (§24), not slices. Each must be
answered before the slice(s) that depend on it begin. They are listed here so
the planner does not invent decisions.

| # | Item | Source | Blocks |
| - | ---- | ------ | ------ |
| C-1 | Confirm the `ReturnOrderNo` prefix/format and sequencing configuration (`INunaCounterBL` mechanism is fixed by IR-RO-07; the concrete prefix is business data). | Arch §24, IR-RO-07, R-09 | S1.5 |
| C-2 | Confirm the Desktop menu identifiers and role grants for the single `Generate Return Order` surface under the `Retur Penjualan` parent (`BTR_Menu.MenuId` / `BTR_RoleMenu` values). | Arch §24, §13.2, §9.3 | S1.9 |
| C-3 | Knowledge Curator pass: update `RETURN-ORDER-DOMAIN.md` vocabulary from `Good`/`Broken` to `BAGUS`/`RUSAK` (§3, §5, §9, BR-013/BR-014). | Arch §24, R-06 | Review (parallel) |
| C-4 | Produce `RETURN-ORDER-WORKFLOW.md` if the artifact chain requires a standalone workflow artifact. | Arch §24, §1 Inputs | Review (parallel) |

---

## 5. Phases

| Phase | Name | System(s) | Business Value |
| ----- | ---- | --------- | -------------- |
| P1 | Main Office Return Order (authority, import, generate) | `j05-btr-distrib` | Return Orders can be held, numbered, completed, and consumed into Sales Return generation |
| P2 | Cloud staging relay + Warehouse Mapping + Driver projection | `j06-pkl-btrade-api` | Return Orders can be submitted idempotently and downloaded incrementally; warehouse/tenant resolution and Driver reference data are available |
| P3 | Synchronization client | `j07-btrade-sync` | Return Orders flow Cloud → Main Office (download → stage → import); Driver projection is populated |
| P4 | BGud Return Order capture (primary target) | `src/BGud` | Warehouse Officers record, amend, delete (pre-sync), and synchronize Return Orders offline |

Phase order respects the architecture's recorded sequencing (§24 Planner
Guidance): Customer/SalesPerson/Driver reference caches precede capture
(P4 internal); the centralized Warehouse Mapping precedes office import
(P2 → P3); the Cloud relay precedes the `j07-btrade-sync` download (P2 → P3);
the Main Office Return Order table and import command precede Generate Sales
Return (P1 internal); import runs after download within a sync run (P3
internal). BGud (P4) is the primary implementation target; P1–P3 are mandatory
supporting deliverables required by ADR-RO-001/007/008.

---

## 6. Slices

Complexity scale: **1 = Easy, 5 = Most Complex**. Complexity drives the AI
model and reasoning level selected for implementation.

### Phase P1 — Main Office Return Order

#### S1.1 — `BTR_ReturnOrder` + `BTR_ReturnOrderItem` schema, sqlproj registration, upgrade script

- **Objective:** Create the authoritative Return Order tables exactly per
  architecture §6.1, register them in `btr.sql.sqlproj`, and ship an idempotent
  upgrade script.
- **Dependencies:** None.
- **Complexity:** 2
- **Acceptance Criteria:**
  - `BTR_ReturnOrder` matches §6.1: `ReturnOrderId VARCHAR(26)` ULID PK (IR-RO-01), `ReturnOrderNo VARCHAR(20)`, `ReturnOrderDate DATETIME DEFAULT '3000-01-01'`, `WarehouseCode VARCHAR(20)`, `CustomerId VARCHAR(6)`, `SalesPersonId VARCHAR(5)`, `DriverId VARCHAR(5)`, `Note VARCHAR(100)`, `Status VARCHAR(10) DEFAULT 'Synced'`, audit columns, `RowVer ROWVERSION`; indexes `IX_BTR_ReturnOrder_Status`, `IX_BTR_ReturnOrder_CustomerId`.
  - `BTR_ReturnOrderItem` matches §6.1: PK `(ReturnOrderId, NoUrut)`, `BrgId VARCHAR(6)`, `BrgCode VARCHAR(20)`, `Qty DECIMAL(18,2)`, `SatId VARCHAR(7)`, `JenisRetur VARCHAR(5)` (IR-RO-02/03/10). No `BrgName` column (resolved at read, §5.1).
  - No foreign keys on either table (IR-RO-11); `ReturJual` persistence untouched (INV-14).
  - Both tables registered in `btr.sql.sqlproj`; an idempotent upgrade script (`IF OBJECT_ID(...) IS NULL ...`) exists in `btr.sql/Scripts/` and is re-runnable.
- **Review Focus:** Persistence Compliance (§6.1); IR-RO-01/02/03/10/11; GAP-002 separation.

#### S1.2 — `ReturnOrderModel` / `ReturnOrderItemModel` + `IReturnOrderKey`

- **Objective:** Create the domain model and key interface per §5.1, mirroring
  `ReturJualModel`/`IReturJualKey` layering (P-05).
- **Dependencies:** S1.1.
- **Complexity:** 2
- **Acceptance Criteria:**
  - `ReturnOrderModel : IReturnOrderKey` with `ReturnOrderId` (ULID), `ReturnOrderNo`, `ReturnOrderDate`, `WarehouseCode`, `CustomerId`, optional `SalesPersonId`/`DriverId`, `Note`, `Status` (`Synced`/`Imported`, IR-RO-09), audit fields, `ListItem`.
  - `ReturnOrderItemModel : IReturnOrderKey, IBrgKey` with `ReturnOrderId`, `NoUrut`, `BrgId`, `BrgCode`, `Qty` (decimal), `SatId`, `JenisRetur` (VO-01: `BAGUS`/`RUSAK`); `BrgName` resolved at read, not persisted.
  - `IReturnOrderKey { string ReturnOrderId }`.
  - The model carries no tenancy concept (Arch §23.1); identity (`ReturnOrderId`) and business number (`ReturnOrderNo`) are separate fields, never merged (INV-01).
- **Review Focus:** Architecture Compliance (§5.1); P-05 layering; IR-RO-09 vocabulary; GAP-001 distinctness from `ReturJual`.

#### S1.3 — `IReturnOrderDal` contract + `ReturnOrderDal` / `ReturnOrderItemDal` (Dapper)

- **Objective:** Implement the DAL contract and Dapper implementations per the
  §5.1 repository composition, mirroring `IReturJualDal` / `ReturJualDal`.
- **Dependencies:** S1.1, S1.2.
- **Complexity:** 3
- **Acceptance Criteria:**
  - `IReturnOrderDal` composes `IInsert<ReturnOrderModel>`, `IUpdate<ReturnOrderModel>`, `IGetData<ReturnOrderModel, IReturnOrderKey>`, `IListData<ReturnOrderModel>` plus specialized members `GetByReturnOrderId`, `ListByStatus(string)`, `Exists(IReturnOrderKey)` (§5.1).
  - `ReturnOrderItemDal` uses delete-by-parent then bulk insert, mirroring `ReturJualItemDal`.
  - Reads never touch `BTR_ReturJual` (INV-14); `ListByStatus` backs the Generate worklist via `IX_BTR_ReturnOrder_Status`.
- **Review Focus:** Persistence Compliance (§5.1); Dapper conventions; INV-14 separation.

#### S1.4 — `ReturnOrderBuilder` + `ReturnOrderValidator` + `ReturnOrderWriter`

- **Objective:** Implement the supporting components mirroring `ReturJual`:
  builder, FluentValidation validator, and single-transaction writer.
- **Dependencies:** S1.2, S1.3.
- **Complexity:** 3
- **Acceptance Criteria:**
  - `ReturnOrderBuilder` provides `Create()`, `Load(IReturnOrderKey)`, `Attach()`, field setters; resolves `BrgCode`/`BrgName` and `WarehouseName`/`CustomerName`/`SalesPersonName`/`DriverName` on read (§7.1).
  - `ReturnOrderValidator : AbstractValidator<ReturnOrderModel>` enforces INV-02 … INV-09 (Customer exists; WarehouseCode mandatory; ≥1 item; item exists; `Qty > 0`; `(BrgId, SatId)` valid in `BTR_BrgSatuan`; `JenisRetur` `BAGUS`/`RUSAK`; Salesman/Driver optional).
  - `ReturnOrderWriter` commits header + items in one `TransHelper.NewScope()` transaction (P-04), mirroring `ReturJualWriter.Save`.
- **Review Focus:** Validation Ownership (§18.1); INV-02…09 coverage; P-04 single transaction.

#### S1.5 — `ImportReturnOrderCommand`

- **Objective:** Implement the idempotent import command: assign `ReturnOrderNo`
  (`INunaCounterBL`, IR-RO-07), persist header + items including the captured
  `WarehouseCode` with `Status = Synced`, in one transaction (INV-11).
- **Dependencies:** S1.3, S1.4. C-1 (numbering prefix/format).
- **Complexity:** 4
- **Acceptance Criteria:**
  - Assigns `ReturnOrderNo` via `INunaCounterBL` following the `ReturJualWriter` convention (IR-RO-07, C-1); the ULID `ReturnOrderId` is preserved as identity and idempotency key (INV-01).
  - Validates INV-02 … INV-09 through `ReturnOrderValidator`; persists `WarehouseCode` (no office-side tenant resolution — the Cloud resolved tenancy at login, IR-RO-04) with `Status = Synced`.
  - Re-import of an already-imported `ReturnOrderId` is a no-op that never re-numbers or duplicates (INV-11, `Exists` check).
  - Full item list (`BrgId`, `Qty`, `SatId`, `JenisRetur`) is persisted unchanged — no small-unit normalization (INV-07, ADR-RO-003).
  - One transaction per Return Order (P-04).
- **Review Focus:** ADR-RO-002 (numbering authority); INV-11 idempotency; ADR-RO-003 unit fidelity; IR-RO-04 boundary.

#### S1.6 — `CompleteReturnOrderCommand`

- **Objective:** Implement optional Salesman/Driver completion on a `Synced`
  order (IR-RO-05), without re-numbering or altering items.
- **Dependencies:** S1.3, S1.4.
- **Complexity:** 2
- **Acceptance Criteria:**
  - Completes `SalesPersonId`/`DriverId` on a `Synced` order only (INV-09, ADR-RO-005).
  - Never re-numbers (`ReturnOrderNo` unchanged), never alters items or `JenisRetur`.
  - Rejects completion of an `Imported` order (INV-10, ADR-RO-006).
- **Review Focus:** IR-RO-05 semantics; INV-09/10; no side effects beyond the two optional fields.

#### S1.7 — `GenerateSalesReturnFromReturnOrderCommand`

- **Objective:** Implement Sales Return generation: group selected `Synced`
  orders by Customer + Return Type + Salesman + Driver, delegate to the existing
  `ReturJual` capability, and mark consumed orders `Imported` (§8.5).
- **Dependencies:** S1.5, S1.6; existing `ReturJualBuilder`/`ReturJualWriter`. 
- **Complexity:** 5
- **Acceptance Criteria:**
  - Grouping key is exactly `(CustomerId, JenisRetur, SalesPersonId, DriverId)`; one Return Order may fan out into multiple `ReturJual` documents (INV-12).
  - Generates valid `ReturJual` header + item records via the existing builder/writer; `JenisRetur` maps directly to `ReturJual.JenisRetur` (`BAGUS`/`RUSAK`, IR-RO-02); recorded `Qty`/`SatId` flow into the existing `ReturJual` item-entry machinery (unit conversion stays there, ADR-RO-003).
  - Resolves the authoritative `BTR_Warehouse.WarehouseId` for each generated `ReturJual` from the Return Order's `WarehouseCode` using the existing warehouse master (§8.5).
  - Performs **no** pricing/inventory/finance (INV-13); generated documents remain completable in the existing `RT1-Retur Jual` flow.
  - Consumed orders transition `Synced → Imported` (INV-10); regeneration of an `Imported` order is prevented (Arch §10.6).
  - `ReturJual` persistence is never modified by this feature (INV-14).
- **Review Focus:** ADR-RO-007 (grouping, delegation, no pricing); INV-12/13/14; IR-RO-02 mapping; §8.5 flow.

#### S1.8 — Queries: `GetReturnOrderQuery`, `ListReturnOrderQuery`, `ListReturnOrderByCustomerQuery`

- **Objective:** Implement the read-side queries backing the Generate surface
  (§7.1).
- **Dependencies:** S1.3.
- **Complexity:** 2
- **Acceptance Criteria:**
  - `GetReturnOrderQuery` returns header + items by id.
  - `ListReturnOrderQuery` returns `Synced` (not yet `Imported`) orders by date/Customer, ordered for the worklist.
  - `ListReturnOrderByCustomerQuery` provides Customer-scoped listing.
  - All reads are office-side and never touch `BTR_ReturJual` (INV-14).
- **Review Focus:** Read-model correctness; INV-10 (`Imported` excluded from the Generate worklist).

#### S1.9 — Desktop `GenerateReturnOrderForm` + menu/roles

- **Objective:** Implement the single Return Order surface (SCR-DESK-RO-001):
  view `Synced` orders, complete Salesman/Driver inline, select, generate;
  wire the menu `Retur Penjualan → Generate Return Order` and role grants. No
  review/approval screens (P-13).
- **Dependencies:** S1.6, S1.7, S1.8. C-2 (menu identifiers/role grants).
- **Complexity:** 4
- **Acceptance Criteria:**
  - `GenerateReturnOrderForm` opens as an MDI child via `BringMdiChildToFrontIfLoaded<T>()`, with the §12.5 layout (toolbar, filter panel, worklist, item detail with inline Salesman/Driver completion, result region).
  - Worklist uses `ListReturnOrderQuery`; inline completion uses `CompleteReturnOrderCommand`; Generate uses `GenerateSalesReturnFromReturnOrderCommand`; generated `ReturJual` opens in the existing `ReturJualForm`.
  - Interaction rules IR-D1 … IR-D3 hold: Generate requires selection; empty Salesman/Driver is allowed (optional); `Imported` orders are view-only and cannot regenerate.
  - `BTR_Menu` row (parent `Retur Penjualan`) + `BTR_RoleMenu` grant seeded per C-2; reachable only for granted roles via `MainForm.SetupUserMenu`.
- **Review Focus:** Workflow Compliance (view → complete → generate); GAP-006/ADR-RO-007 single-surface; IR-D1…D3; Security Compliance (role gating).

### Phase P2 — Cloud Staging Relay + Warehouse Mapping + Driver Projection

#### S2.1 — `BTRADE_ReturnOrder` + `BTRADE_ReturnOrderItem` schema, sqlproj registration, upgrade script

- **Objective:** Create the Cloud relay tables per §6.2, register them in
  `btrade.sqldb.sqlproj`, and ship an idempotent upgrade script.
- **Dependencies:** None.
- **Complexity:** 2
- **Acceptance Criteria:**
  - `BTRADE_ReturnOrder` matches §6.2: PK `(ReturnOrderId VARCHAR(26), ServerId VARCHAR(5))`, `ReturnOrderDate DATETIME`, `WarehouseCode`, `CustomerId` + `CustomerName`, `SalesPersonId` + `SalesPersonName`, `DriverId` + `DriverName`, `Note`, `StatusSync VARCHAR(10) DEFAULT 'TERKIRIM'` (IR-RO-08); index `IX_BTRADE_ReturnOrder_ServerId_Status`.
  - `BTRADE_ReturnOrderItem` matches §6.2: PK `(ReturnOrderId, NoUrut)`, `BrgId`, `BrgCode`, `BrgName`, `Qty DECIMAL(18,2)`, `SatId`, `JenisRetur`.
  - Both tables registered in `btrade.sqldb.sqlproj`; idempotent upgrade script exists in `btrade.sqldb/Scripts/` and is re-runnable.
- **Review Focus:** Persistence Compliance (§6.2); IR-RO-01/08; relay-not-authority shaping (P-02).

#### S2.2 — `ReturnOrderType` / `ReturnOrderItemType` + `IReturnOrderKey`

- **Objective:** Implement the Cloud relay models per §5.2, mirroring
  `OrderModel`/`OrderItemType` shape.
- **Dependencies:** S2.1.
- **Complexity:** 2
- **Acceptance Criteria:**
  - `ReturnOrderType : IReturnOrderKey, IServerId` with `ReturnOrderId`, `ServerId`, `ReturnOrderDate` (string `yyyy-MM-dd`), `WarehouseCode`, `CustomerId`/`CustomerName`, `SalesPersonId`/`SalesPersonName`, `DriverId`/`DriverName`, `Note`, `StatusSync` (mutable for the download transition), `ListItems`.
  - `ReturnOrderItemType` record carries `ReturnOrderId`, `NoUrut`, `BrgId`, `BrgCode`, `BrgName`, `Qty`, `SatId`, `JenisRetur`.
  - These types carry no validation logic (transport-only, P-02).
- **Review Focus:** Architecture Compliance (§5.2); relay model fidelity; tenant scoping via `IServerId`.

#### S2.3 — `IReturnOrderDal` / `IReturnOrderItemDal` contracts + Dapper DALs

- **Objective:** Implement the relay DAL contracts and Dapper implementations,
  mirroring `OrderDal`/`OrderItemDal` (delete-then-insert upsert, `SqlBulkCopy`
  for items, `ServerId`-scoped listing).
- **Dependencies:** S2.1, S2.2.
- **Complexity:** 3
- **Acceptance Criteria:**
  - DALs implement the same generic composition as `IOrderDal`/`IOrderItemDal` and are auto-registered by the existing Scrutor scan.
  - All reads/writes are scoped by `ServerId` (tenant boundary); delete-then-insert by `ReturnOrderId` supports idempotent resubmission (Arch §10.6).
  - Known `OrderDal` schema-drift defects (`OrderNote` mismatch, `FakturId` typo) are not copied.
- **Review Focus:** Tenant scoping; idempotent upsert; Dapper conventions.

#### S2.4 — `BTR_WarehouseMapping` + login resolution (WarehouseCode → ServerId)

- **Objective:** Realize the centralized Warehouse Mapping (ADR-RO-008, IR-RO-04):
  add `BTR_WarehouseMapping` with its seed, and wire `IssueTokenCommand` to
  resolve `ServerId` from the selected `WarehouseCode` so the session carries
  both values (Arch §9.2).
- **Dependencies:** None.
- **Complexity:** 3
- **Acceptance Criteria:**
  - `BTR_WarehouseMapping` exists in `btrade.sqldb/WarehouseContext` (`WarehouseCode VARCHAR(20)` PK, `ServerId VARCHAR(5)`), registered in `btrade.sqldb.sqlproj`; idempotent seed (`GAMPING`→`JOGJA`, `CONCAT`→`JOGJA`, `MAGELANG`→`MGL`).
  - Login resolves `ServerId` from the selected `WarehouseCode` via the mapping; an unmapped code fails explicitly (INV-03, R-03) — no fallback.
  - The session carries both `WarehouseCode` and `ServerId` (§8.1, §9.2); the mapping is seed/migration data only — no maintenance UI (ADR-RO-008).
- **Review Focus:** ADR-RO-008 / IR-RO-04 (centralized, seed-only, not hardcoded); R-03 (explicit failure on unmapped code); login/token issuance integration.

#### S2.5 — `BTRADE_Driver` + `DriverType` + DAL + `DriverSyncCommand` + `DriverListDataQuery` + `DriverController`

- **Objective:** Add the Driver projection (GAP-013): table, domain type, DAL,
  sync/query use cases, and reference routes mirroring the SalesPerson pattern.
- **Dependencies:** None.
- **Complexity:** 3
- **Acceptance Criteria:**
  - `BTRADE_Driver` matches §6.3 (`(DriverId, ServerId)` PK, `DriverName`, `IsAktif`), registered in `btrade.sqldb.sqlproj` + idempotent upgrade script.
  - `DriverType : IDriverKey, IServerId` per §5.3; `IDriverDal`/`DriverDal` mirror `SalesPersonDal`.
  - `DriverListDataQuery` serves `GET /api/Driver/{serverId}` (I-RO-05); `DriverSyncCommand` upserts by `DriverId` + `ServerId` (delete-then-insert, mirroring `SalesPersonSyncCommand`), served by `POST /api/Driver` (I-RO-06).
- **Review Focus:** GAP-013 closure; reference-pattern fidelity (SalesPerson); tenant scoping.

#### S2.6 — `ReturnOrderUploadCommand` + `ReturnOrderIncrementalDownloadQuery`

- **Objective:** Implement the submit and incremental-download use cases,
  mirroring `OrderUploadCommandHandler` / `OrderIncrementalDownloadQueryHandler`
  (idempotent submit; download-coupled acknowledgement, IR-RO-08).
- **Dependencies:** S2.2, S2.3.
- **Complexity:** 4
- **Acceptance Criteria:**
  - Submit stages header + items in one transaction via delete-then-insert by `ReturnOrderId`; resubmission replaces the staged copy, never duplicates (Arch §10.6). `StatusSync = TERKIRIM`.
  - The Cloud performs no business validation and authors no authoritative state (P-02).
  - Incremental download returns only `TERKIRIM` rows scoped to `ServerId` + periode and flips them to `DOWNLOADED` in the same transaction (acknowledgement, IR-RO-08).
  - Items are returned with their parent; orders without items are not silently dropped.
- **Review Focus:** Idempotency (ADR-RO-002); acknowledgement semantics (IR-RO-08); P-02 relay-never-authority.

#### S2.7 — `ReturnOrderController` (submit + incremental download)

- **Objective:** Implement `POST /api/return-order` and
  `GET /api/ReturnOrder/incremental/{tgl1}/{tgl2}/{serverId}`, both JWT-
  authenticated, with `ServerId` resolved server-side on the write path (P-06).
- **Dependencies:** S2.6.
- **Complexity:** 3
- **Acceptance Criteria:**
  - `POST /api/return-order` (I-RO-01) is `[Authorize]`; `ServerId` comes from `User.GetServerId()` — never from the request body (§8.1); the payload carries no `ServerId` (§19.3).
  - `GET /api/ReturnOrder/incremental/{tgl1}/{tgl2}/{serverId}` (I-RO-02) is `[Authorize]`; the route shape mirrors the CheckIn/Order convention (path `serverId`).
  - Unauthenticated calls are rejected; responses use the existing `JSendOk` envelope.
  - Route casing: `POST /api/return-order` verbatim; the incremental route follows the existing controller convention.
- **Review Focus:** Security Compliance (JWT, server-resolved tenant); ADR-007/P-06; routing conventions (§8.6).

### Phase P3 — Synchronization Client

#### S3.1 — `ReturnOrderModel` / `ReturnOrderItemType` (sync transport)

- **Objective:** Add the sync-side transport models mirroring `OrderModel`/
  `OrderItemType` (id key interface, static key factory, `ListItems`), and
  register the new files in the non-SDK csproj (`<Compile Include>`).
- **Dependencies:** None.
- **Complexity:** 2
- **Acceptance Criteria:**
  - `ReturnOrderModel` implements `IReturnOrderKey { string ReturnOrderId }` with a `Key(string)` factory; fields mirror the cloud `ReturnOrderType` (Customer, WarehouseCode, optional Salesman/Driver, Note, `StatusSync`, items).
  - `ReturnOrderItemType` carries `BrgId`, `BrgCode`, `BrgName`, `Qty`, `SatId`, `JenisRetur` (ADR-RO-003/004).
  - Both files listed in `j07-btrade-sync.csproj`.
- **Review Focus:** Transport-model fidelity; csproj registration.

#### S3.2 — `ReturnOrderDal` / `ReturnOrderItemDal` (Main Office staging)

- **Objective:** Implement Dapper DALs staging downloaded Return Orders into
  `BTR_ReturnOrder`/`BTR_ReturnOrderItem` (upsert by `ReturnOrderId`,
  `SqlBulkCopy` for items), mirroring `CheckInDal`/`OrderItemDal`.
- **Dependencies:** S1.1 (Main Office tables), S3.1.
- **Complexity:** 3
- **Acceptance Criteria:**
  - Insert/Update/GetData by `ReturnOrderId`; existence-check then insert/update (upsert-by-id; idempotent re-download).
  - Item rows delete-then-`SqlBulkCopy` by `ReturnOrderId` (re-download never duplicates items).
  - Staged rows carry `Status = Synced` (office vocabulary, IR-RO-09).
  - Known `OrderDal` defects not copied.
- **Review Focus:** Idempotent staging; staging-not-authority (numbering/mapping owned by office commands, IR-RO-07).

#### S3.3 — `ReturnOrderIncrementalDownloadService`

- **Objective:** Implement the cloud download service mirroring
  `OrderIncrementalDownloadService` (RestSharp GET against I-RO-02,
  `ApiResponse<T>` envelope, `(bool, string, List<ReturnOrderModel>)` result,
  JWT bearer).
- **Dependencies:** S2.7 (cloud endpoint), S3.1.
- **Complexity:** 3
- **Acceptance Criteria:**
  - Fetches a periode-bounded incremental set for the configured `ServerTargetID`; returns success/error/payload exactly like the existing services.
  - Non-success envelope status is treated as failure; empty payload is a valid result.
  - No snapshot replacement of any local or remote store (P-03).
- **Review Focus:** Pattern fidelity; no snapshot semantics; JWT bearer present (I-RO-02).

#### S3.4 — `MainOfficeCommandExecutor` extension (import dispatch)

- **Objective:** Extend the in-process executor to dispatch the office
  `ImportReturnOrderCommand` (S1.5), so numbering and Warehouse persistence live
  in exactly one place (Arch §4.3, IR-RO-05).
- **Dependencies:** S1.5.
- **Complexity:** 4
- **Acceptance Criteria:**
  - The executor registers the Return Order application components (DALs, builder/writer/validator, counter) alongside the existing barcode registrations and exposes the import dispatch.
  - Import assigns `ReturnOrderNo` and persists the `WarehouseCode` through the office command only — the sync client performs no numbering or mapping (ADR-RO-002/008).
  - Dispatch remains in-process (no HTTP/message bus).
- **Review Focus:** Single-authority validation (IR-08 pattern); ADR-RO-002/008 authority boundaries.

#### S3.5 — `DriverSyncService`

- **Objective:** Populate `BTRADE_Driver` from Main Office `BTR_Driver`,
  mirroring `SalesPersonSyncService` (IR-RO-06).
- **Dependencies:** S2.5 (cloud Driver routes).
- **Complexity:** 3
- **Acceptance Criteria:**
  - Publishes `BTR_Driver` → `BTRADE_Driver` via `POST /api/Driver` (I-RO-06), upsert by `DriverId` + `ServerId`.
  - Mirrors the existing SalesPerson/Customer uploader service shape; idempotent re-upload.
- **Review Focus:** IR-RO-06 fidelity; uploader-pattern reuse; idempotency.

#### S3.6 — Sync client authentication (JWT on I-RO-02 / I-RO-06)

- **Objective:** Enable `j07-btrade-sync` to present a JWT on the Return Order
  write endpoints (I-RO-02, I-RO-06) per Arch §9.1, mirroring the barcode
  registry sync-client service-account model (AUTH-GAP-001 precedent).
- **Dependencies:** S2.4 (token issuance / mapping), S2.7 (I-RO-02), S2.5 (I-RO-06), S3.3, S3.5 (callers).
- **Complexity:** 4
- **Acceptance Criteria:**
  - A dedicated service account authenticates against `POST api/Auth/login` and receives a JWT.
  - `I-RO-02` and `I-RO-06` requests carry `Authorization: Bearer <token>`; the token is cached and refreshed on rejection (no re-login per request).
  - `200 OK` on authenticated calls; `401` without a token.
  - Tenant binding: the token is issued for the mapped `ServerId`; no `ServerId` is added to Return Order command payloads (P-06).
- **Review Focus:** Security Compliance (service-account model); token lifecycle; P-06 tenant binding.

#### S3.7 — `SyncForm` wiring (download → stage → import + Driver upload)

- **Objective:** Wire the Return Order flow into the existing sync run:
  `ProcessReturnOrder` executes download (S3.3) → stage (S3.2) → import (S3.4),
  gated by a `DownloadReturnOrder` registry flag; plus the Driver upload
  (S3.5); triggered from startup, the auto timer, and manual download buttons —
  mirroring `ProcessOrder`/`ProcessCheckIn`.
- **Dependencies:** S3.1, S3.2, S3.3, S3.4, S3.5, S3.6.
- **Complexity:** 4
- **Acceptance Criteria:**
  - Each downloaded Return Order is processed idempotently (skip/upsert by `ReturnOrderId`); failures are logged per row without corrupting other rows.
  - Download → stage → import run sequentially within a run; a failure never marks an order imported unless actually committed.
  - Feature flag and triggers match existing conventions (`RegistryHelper`, constructor, timer, manual buttons, optional `KonfigurasiForm` checkbox).
  - Existing CheckIn/Order/Barcode flows are unchanged.
- **Review Focus:** Workflow Compliance (§8.2 ordering); idempotency across reruns; existing-behavior preservation.

### Phase P4 — BGud Return Order Capture (primary implementation target)

#### S4.1 — Room entities, DAOs, AppDatabase registration, migration 1 → 2

- **Objective:** Add the five Room entities + DAOs per §6.4, register them in
  `AppDatabase`, bump version to 2, and provide a **non-destructive**
  `Migration(1, 2)` (existing barcode/barang caches and the pending queue must
  survive).
- **Dependencies:** None.
- **Complexity:** 4
- **Acceptance Criteria:**
  - `return_order_entity` matches §6.4: `returnOrderId` (ULID string PK, IR-RO-01), `customerId`/`customerCode`/`customerName`, `warehouseCode`, `salesPersonId`/`salesPersonName`, `driverId`/`driverName`, `note`, `status` (`DRAFT`/`SYNCED`), `createdAt`, `createdBy`; index on `status`.
  - `return_order_item_entity` matches §6.4: `returnOrderId` + `noUrut` (composite PK), `brgId`/`brgCode`/`brgName`, `qty` (REAL), `satId`, `jenisRetur` (`BAGUS`/`RUSAK`).
  - `customer_entity` (customerId PK, customerCode, customerName, address), `salesperson_entity` (salesPersonId PK, salesPersonName), `driver_entity` (driverId PK, driverName, isAktif).
  - DAOs expose upsert/list/delete-by-parent/search primitives; `AppDatabase` exposes all five DAOs.
  - `Migration(1, 2)` creates only the five new tables; existing data preserved; app builds (`assembleDebug`).
- **Review Focus:** Persistence Compliance (§6.4); migration safety (no destructive fallback); device vocabulary (ADR-RO-006).

#### S4.2 — API service + DTOs (submit Return Order, reference endpoints)

- **Objective:** Extend `BtradeApiService` with `POST api/return-order` and the
  Customer/SalesPerson/Driver reference GETs, with DTOs following the existing
  `JSendEnvelope` + PascalCase `@SerializedName` conventions (§19.3).
- **Dependencies:** S4.1.
- **Complexity:** 3
- **Acceptance Criteria:**
  - `POST api/return-order` (I-RO-01) submits the Return Order with the ULID `ReturnOrderId` as the idempotency key; **no `ServerId` appears anywhere in the request body** (§8.1, P-06).
  - `GET api/Customer/{serverId}` (I-RO-03), `GET api/SalesPerson/{serverId}` (I-RO-04), `GET api/Driver/{serverId}` (I-RO-05) reuse the existing `serverId` path-param convention (`GET api/Brg/{serverId}` precedent).
  - All calls attach the JWT through the existing `AuthInterceptor`; the submit DTO carries header (ReturnOrderId, ReturnOrderDate, WarehouseCode, CustomerId + names, optional Salesman/Driver, Note) + items (`BrgId`/`BrgCode`/`BrgName`, `Qty`, `SatId`, `JenisRetur`), per §8.1.
- **Review Focus:** Security Compliance (JWT, no client-supplied `ServerId`); envelope/DTO conventions (§19.3).

#### S4.3 — Reference caches download (Customer / SalesPerson / Driver) + session state

- **Objective:** Implement the replace-cache download for Customer / SalesPerson
  / Driver into Room, with DataStore last-sync timestamps and the session
  carrying `WarehouseCode` + `ServerId` (§8.1, §9.2), mirroring the existing
  Barang/Barcode download pattern.
- **Dependencies:** S4.1, S4.2. Runtime source: I-RO-03/04 (existing) + I-RO-05 (S2.5).
- **Complexity:** 4
- **Acceptance Criteria:**
  - Downloads replace the local cache (`deleteAll` + `upsertAll`) per reference type; timestamps advance only on committed download (`last_customer_sync`, `last_salesperson_sync`, `last_driver_sync`).
  - Customer cache supports offline selection of the mandatory Customer (GAP-004); Salesman/Driver caches are optional data (ADR-RO-005).
  - Session holds both `WarehouseCode` and `ServerId` (resolved at login, IR-RO-04/§9.2); reference GETs use the `ServerId` path param.
  - Runs as part of Login Sync (OQ-1); failures never block navigation (existing login-sync semantics).
- **Review Focus:** Cache-replace pattern fidelity; ADR-RO-005 (optional fields); §9.2 session shape; OQ-1.

#### S4.4 — Return Order capture repository + validation + ULID generation

- **Objective:** Implement the local capture repository: create/update/delete
  drafts with domain rules enforced locally, generating the ULID `ReturnOrderId`
  and stamping the session-bound `WarehouseCode`.
- **Dependencies:** S4.1, S4.2 (ULID dependency).
- **Complexity:** 4
- **Acceptance Criteria:**
  - `ReturnOrderId` is generated on the device as a **ULID** matching the platform's `Ulid.NewUlid().ToString()` format (IR-RO-01); new orders are `DRAFT`; `WarehouseCode` is stamped from the session binding (BR-005/006).
  - Customer mandatory from the local cache (BR-001/002, GAP-004); Salesman/Driver optional (BR-013–016, ADR-RO-005).
  - At least one item (BR-012); item exists in the cached Item Master (BR-008); `Qty > 0` (BR-010); unit mandatory, recorded as `SatId` from the item's cached unit data (BR-011, ADR-RO-003); no small-unit normalization anywhere (P-09).
  - Per-item `JenisRetur` is exactly `BAGUS`/`RUSAK` (ADR-RO-004); mixed types allowed (DOMAIN §9).
  - Update/delete rejected once `SYNCED` (BR-017–020); pre-sync delete is local-only and never propagates (GAP-014).
- **Review Focus:** Business-rule enforcement (BR-001…BR-020, §18.3); IR-RO-01 ULID; P-09 unit fidelity; GAP-014 delete semantics.

#### S4.5 — `ReturnOrderSyncRepository` + `ReturnOrderSyncWorker`

- **Objective:** Implement the sync run: submit `DRAFT` orders
  (`POST api/return-order`, mark `SYNCED` on success), then download the
  reference caches; trigger points are Login Sync and Manual Sync Now only
  (OQ-1), mirroring the `BarcodeSyncWorker` conventions (unique work, KEEP,
  connectivity constraint, no periodic scheduling).
- **Dependencies:** S4.3, S4.4; runtime endpoints I-RO-01 (S2.7), I-RO-05 (S2.5).
- **Complexity:** 5
- **Acceptance Criteria:**
  - Each `DRAFT` order is submitted once per run; `2xx` → local `SYNCED`; failure keeps `DRAFT` and counts as failed without aborting the run.
  - Resubmission after failure is safe (cloud idempotent on the ULID `ReturnOrderId`).
  - Device shows only `DRAFT`/`SYNCED`; no import outcome or `ReturnOrderNo` is ever downloaded (ADR-RO-006) — there is no status refresh step.
  - Reference downloads run after submission (mirror barcode ordering); one sync run at a time (KEEP); no background/scheduled/realtime triggers (OQ-1).
  - `SYNCED` orders are read-only (BR-018/020).
- **Review Focus:** Sync ordering (§20); idempotent resubmission; ADR-RO-006 (no sync-back); OQ-1 compliance.

#### S4.6 — Return Order List screen (SCR-MOB-RO-001)

- **Objective:** Implement the searchable local list (BC-004) with status filter
  `Draft`/`Synced` only (ADR-RO-006), using the existing paged/debounced
  local-search pattern.
- **Dependencies:** S4.4.
- **Complexity:** 2
- **Acceptance Criteria:**
  - Lists local Room rows with default 50 rows, load-more paging, min 3 chars, 300 ms debounce (§20); search by Customer name/date/status.
  - Status display vocabulary is exactly `Draft`/`Synced` (§14.4).
  - Rows open Detail; a Create action (FAB/toolbar) exists (§12.1).
- **Review Focus:** UI State Compliance; local-only source (no network); ADR-RO-006 vocabulary.

#### S4.7 — Create Return Order screen (SCR-MOB-RO-002)

- **Objective:** Implement capture: mandatory Customer picker (local cache),
  read-only session-bound Warehouse, optional Salesman/Driver pickers, Notes,
  and item lines identified by Barcode Scan (reusing `BarcodeScannerView`) or
  Manual Item Search (BR-009), with Qty / Unit / Return Type per line.
- **Dependencies:** S4.1, S4.3 (caches), S4.4.
- **Complexity:** 5
- **Acceptance Criteria:**
  - Customer mandatory from the local cache (BR-001/002, GAP-004); Salesman/Driver optional (ADR-RO-005); Warehouse read-only (session-bound, BR-005/006).
  - Item identification supports Barcode Scan and Manual Item Search against local caches (BR-009), reusing `BarcodeScannerView` and the existing search pattern; no network call (§17.1, P-07).
  - Per line: `Qty > 0` (IR-M4), unit from the item's cached unit data recorded as `SatId` (IR-M5), `JenisRetur` `BAGUS`/`RUSAK` (IR-M6); at least one valid line before Save (IR-M2, BR-012).
  - Save writes locally only (`DRAFT`), offline-first (BG-003); Save enabled only when Customer set and ≥1 valid line (§14.1).
- **Review Focus:** BR-001…BR-012 / IR-M1…M6 enforcement; component reuse; ADR-RO-003/004 fidelity; offline-first write.

#### S4.8 — Return Order Detail screen (SCR-MOB-RO-003, incl. Delete action)

- **Objective:** Implement the read-only detail view with status-gated Edit and
  Delete actions (Delete is a `Draft`-only action on Detail, not a separate
  screen — §11.1).
- **Dependencies:** S4.4.
- **Complexity:** 2
- **Acceptance Criteria:**
  - Shows header (Customer, Warehouse, optional Salesman/Driver, Notes, status) and item lines (Item, Qty, Unit, Return Type) (§12.3).
  - Status shows only `Draft`/`Synced` (ADR-RO-006); Edit and Delete enabled only while `Draft` (IR-M7/M8, BR-017–020).
  - Delete removes the order and its items in one local transaction, never propagates (GAP-014).
- **Review Focus:** UI State Compliance (§14.2); IR-M7/M8 gating; GAP-014 local-only delete.

#### S4.9 — Edit Return Order screen (SCR-MOB-RO-004)

- **Objective:** Implement draft-only modification (BC-002).
- **Dependencies:** S4.4.
- **Complexity:** 3
- **Acceptance Criteria:**
  - All editable fields (Customer, optional Salesman/Driver, Notes, item lines) can be changed while `DRAFT` (§12.4).
  - `SYNCED` orders reject entry into edit (BR-018); the UI never offers edit for `SYNCED`.
  - Changes persist locally only; the order remains `DRAFT`.
- **Review Focus:** BR-017/018; local-only update semantics (GAP-014).

#### S4.10 — Synchronization screen extension (SCR-MOB-RO-005)

- **Objective:** Extend the existing Synchronization surface with Return Order
  sync state and the Manual Sync Now trigger (Login Sync already covered by
  S4.3/S4.5), per §14.3 and OQ-1.
- **Dependencies:** S4.5.
- **Complexity:** 3
- **Acceptance Criteria:**
  - Shows Return Order sync state (last reference sync timestamps, pending/synced counts) alongside the existing barcode state (§19.1 `ReturnOrderSyncViewModel`).
  - Sync Now triggers the Return Order sync worker in addition to the barcode worker; offline disables Sync Now (IR-M9, existing guard).
  - State flow `Idle → Synchronizing → Synchronized | Failed` (§14.3); no background/scheduled/realtime sync (OQ-1).
- **Review Focus:** UI State Compliance (§14.3); OQ-1 trigger inventory; existing synchronization behavior preserved.

#### S4.11 — Home + Navigation wiring

- **Objective:** Wire the Return Order entry point into Home and register all
  new routes in `ui/Navigation.kt` following the existing string-route and
  `ViewModelFactory` conventions (§13.1).
- **Dependencies:** S4.6, S4.7, S4.8, S4.9, S4.10.
- **Complexity:** 3
- **Acceptance Criteria:**
  - Home shows a Return Order quick action → `return_order_list`.
  - Routes match §13.1: `return_order_list`, `return_order_create`, `return_order_detail?returnOrderId={id}`, `return_order_edit?returnOrderId={id}`; transitions and conditions per §13.1 (edit/delete gated on `Draft`).
  - Each destination constructs its ViewModel through a hand-written `ViewModelFactory` (§19.1, existing convention).
  - Existing routes and start-destination logic are unchanged.
- **Review Focus:** Navigation compliance (§13.1); ViewModel/Factory convention; existing-behavior preservation.

---

## 7. Dependency Graph (Summary)

```text
P1 (Main Office)
S1.1 ─ S1.2 ─ S1.3 ─ S1.4 ─ S1.5 ─ S1.6 ┐
                                    └─ S1.7 ─ S1.9 (C-2)
S1.3 ─ S1.8 ───────────────────────────┘
(C-1 on S1.5)

P2 (Cloud)
S2.1 ─ S2.2 ─ S2.3 ─ S2.6 ─ S2.7
S2.4 (independent; login resolution)
S2.5 (independent)

P3 (Sync)
S3.1 ─┬─ S3.2 ─┐
      └─ S3.3 ─┼─ S3.7
S1.5 ─ S3.4 ───┤
S2.5 ─ S3.5 ───┤
S2.4, S2.7, S2.5, S3.3, S3.5 ─ S3.6 ─┘

P4 (BGud, primary)
S4.1 ─┬─ S4.2 ─ S4.3 ─┐
      ├─ S4.4 ────────┼─ S4.5 ─ S4.10
      └─ S4.4 ─ S4.6 ─┴─ S4.7, S4.8, S4.9 ─ S4.11

Cross-phase:
S1.1 → S3.2          (Main Office tables precede j07 staging DALs)
S2.5 → S4.3          (Driver route precedes BGud Driver cache download)
S2.7 → S3.3, S4.5    (Cloud relay endpoints precede download/submit consumers)
S1.5 → S3.4          (office import command precedes executor extension)
S2.4 → S3.6          (login/token issuance precedes sync-client auth)
```

---

## 8. Progress Tracker

Lifecycle: `PLANNED → IN IMPLEMENTATION → IMPLEMENTED → IN REVIEW → GO`
(or `NO-GO → REMEDIATION → IN REVIEW → GO`).

| Slice | Status | Complexity | System |
| ----- | ------ | ---------- | ------ |
| S1.1 | GO | 2 | `btr.sql` |
| S1.2 | GO | 2 | `btr.domain` |
| S1.3 | GO | 3 | `btr.application` / `btr.infrastructure` |
| S1.4 | GO | 3 | `btr.application` |
| S1.5 | GO | 4 | `btr.application` |
| S1.6 | GO | 2 | `btr.application` |
| S1.7 | GO | 5 | `btr.application` |
| S1.8 | PLANNED | 2 | `btr.application` |
| S1.9 | PLANNED | 4 | `btr.distrib` / `btr.sql` |
| S2.1 | GO | 2 | `btrade.sqldb` |
| S2.2 | GO | 2 | `btrade.domain` |
| S2.3 | GO | 3 | `btrade.application` / `btrade.infrastructure` |
| S2.4 | GO | 3 | `btrade.sqldb` / `btrade.application` |
| S2.5 | GO | 3 | `btrade.sqldb` / `btrade.domain` / `btrade.webapi` |
| S2.6 | GO | 4 | `btrade.application` |
| S2.7 | PLANNED | 3 | `btrade.webapi` |
| S3.1 | GO | 2 | `j07-btrade-sync` |
| S3.2 | PLANNED | 3 | `j07-btrade-sync` |
| S3.3 | PLANNED | 3 | `j07-btrade-sync` |
| S3.4 | PLANNED | 4 | `j07-btrade-sync` |
| S3.5 | PLANNED | 3 | `j07-btrade-sync` |
| S3.6 | PLANNED | 4 | `j07-btrade-sync` |
| S3.7 | PLANNED | 4 | `j07-btrade-sync` |
| S4.1 | PLANNED | 4 | BGud |
| S4.2 | PLANNED | 3 | BGud |
| S4.3 | PLANNED | 4 | BGud |
| S4.4 | PLANNED | 4 | BGud |
| S4.5 | PLANNED | 5 | BGud |
| S4.6 | PLANNED | 2 | BGud |
| S4.7 | PLANNED | 5 | BGud |
| S4.8 | PLANNED | 2 | BGud |
| S4.9 | PLANNED | 3 | BGud |
| S4.10 | PLANNED | 3 | BGud |
| S4.11 | PLANNED | 3 | BGud |

### 8.1 Implementation History

| Slice | Start | End | Implementer Notes |
| ----- | ----- | --- | ----------------- |
| S1.1 | 2026-09-17 | 2026-09-17 | Created `BTR_ReturnOrder.sql` + `BTR_ReturnOrderItem.sql` verbatim per Arch §6.1 (no FK, IR-RO-11); registered both in `btr.sql.sqlproj`; added idempotent re-runnable upgrade script `btr.sql/Scripts/Create_BTR_ReturnOrder.sql` (`IF OBJECT_ID(...) IS NULL` guards for tables, `sys.indexes` guards for indexes). Verification: SSDT build of `btr.sql.sqlproj` succeeds (dacpac generated); upgrade script executed twice against a throwaway LocalDB database with no errors (idempotent); columns/PKs/indexes/defaults confirmed against §6.1 via `sys.columns` / `sys.indexes` / `sys.default_constraints`; zero foreign keys on both tables. No `ReturJual` object touched (INV-14). |
| S1.2 | 2026-09-17 | 2026-09-17 | Created `ReturnOrderModel` (+ `IReturnOrderKey`) and `ReturnOrderItemModel` in `btr.domain/InventoryContext/ReturnOrderAgg/` per Arch §5.1; registered both files in `btr.domain.csproj` (`<Compile Include>`). `ReturnOrderModel : IReturnOrderKey` carries `ReturnOrderId`, `ReturnOrderNo`, `ReturnOrderDate`, `WarehouseCode`, `CustomerId`, `SalesPersonId`, `DriverId`, `Note`, `Status`, audit fields (`CreatedBy`/`CreatedDate`/`ModifiedBy`/`ModifiedDate`), `ListItem`; identity (`ReturnOrderId`) and business number (`ReturnOrderNo`) kept as separate fields (INV-01). `ReturnOrderItemModel : IReturnOrderKey, IBrgKey` carries `ReturnOrderId`, `NoUrut`, `BrgId`, `BrgCode`, `BrgName`, `Qty` (decimal), `SatId`, `JenisRetur` (`BAGUS`/`RUSAK`, VO-01). `BrgName` and the header display names (`WarehouseName`/`CustomerName`/`SalesPersonName`/`DriverName`) are read-convenience only — not persisted (no such columns in §6.1) — mirroring `ReturJualModel` and satisfying the S1.4 builder read-resolution requirement (§7.1). No tenancy concept on either model (Arch §23.1); layering mirrors `ReturJualModel`/`ReturJualItemModel` (P-05). Verification: `MSBuild` Debug build of `btr.domain.csproj` (Rebuild, 0 errors) plus dependent `btr.application` and `btr.infrastructure` (0 errors). No `ReturJual` file touched (INV-14). |
| S1.3 | 2026-09-17 | 2026-09-17 | Created `IReturnOrderDal` + `IReturnOrderItemDal` contracts in `btr.application/InventoryContext/ReturnOrderAgg/Contracts/` and `ReturnOrderDal` + `ReturnOrderItemDal` Dapper implementations in `btr.infrastructure/InventoryContext/ReturnOrderAgg/` per Arch §5.1; registered all four files in `btr.application.csproj` / `btr.infrastructure.csproj` (`<Compile Include>`). `IReturnOrderDal` composes `IInsert<ReturnOrderModel>`, `IUpdate<ReturnOrderModel>`, `IGetData<ReturnOrderModel, IReturnOrderKey>`, `IListData<ReturnOrderModel>` plus specialized `GetByReturnOrderId`, `ListByStatus(string)`, `Exists(IReturnOrderKey)`. `ReturnOrderDal` writes exactly the §6.1 columns (no `RowVer`); `GetData` delegates to `GetByReturnOrderId`; reads join `BTR_Customer`/`BTR_SalesPerson`/`BTR_Driver` for the model's read-convenience display names, mirroring `ReturJualDal`; `ListByStatus` filters on `Status` (backs the Generate worklist via `IX_BTR_ReturnOrder_Status`); `Exists` is a `COUNT(1)` guard backing import idempotency (INV-11). `ReturnOrderItemDal` composes `IInsertBulk`/`IDelete<IReturnOrderKey>`/`IListData<ReturnOrderItemModel, IReturnOrderKey>`, deletes by parent then `SqlBulkCopy`-inserts (never maps the non-persisted `BrgName`), and resolves `BrgName` via `BTR_Brg` on read, mirroring `ReturJualItemDal`. No warehouse join: `WarehouseCode` has no office master in this slice; `WarehouseName` resolution remains with the S1.4 builder (§7.1). Reads never touch `BTR_ReturJual` (INV-14). Verification: `MSBuild` Rebuild of `btr.infrastructure.csproj` (with `btr.domain`/`btr.nuna`/`btr.application`) → Build succeeded, 0 warnings, 0 errors. No `ReturJual` file touched (INV-14). |
| S1.4 | 2026-09-17 | 2026-09-17 | Created `ReturnOrderBuilder` + `ReturnOrderValidator` + `ReturnOrderWriter` in `btr.application/InventoryContext/ReturnOrderAgg/Workers/` per Arch §7.1 (mirroring the `ReturJual` supporting components); registered all three files in `btr.application.csproj` (`<Compile Include>`). `ReturnOrderBuilder : IReturnOrderBuilder` provides `Create()` (`ReturnOrderDate = 3000-01-01`, empty `ListItem`), `Load(IReturnOrderKey)` (header via `IReturnOrderDal.GetData`, items via `IReturnOrderItemDal.ListData`), `Attach(ReturnOrderModel)`, field setters `Customer`/`Warehouse`/`SalesPerson`/`Driver`/`ReturnOrderDate`/`AddItem` (auto-assigns `NoUrut`)/`Note`, and `Build()` (`RemoveNull`). On `Load` it resolves the read-convenience display names only when missing: `WarehouseName` via `BTR_Warehouse`, `CustomerName`/`SalesPersonName`/`DriverName` via their masters, and item `BrgCode`/`BrgName` via `BTR_Brg` (§7.1); the S1.3 DAL already joins Customer/SalesPerson/Driver/BrgName, so resolution is a no-op when the DAL supplied them (`WarehouseName` is the gap the S1.3 review deferred here). `ReturnOrderValidator : AbstractValidator<ReturnOrderModel>` enforces INV-02 (Customer exists via `ICustomerDal`), INV-03 (`WarehouseCode` mandatory), INV-04 (at least one item), INV-05 (item exists via `IBrgDal`), INV-06 (`Qty > 0`), INV-07 (`(BrgId, SatId)` valid in `BTR_BrgSatuan` via `IBrgSatuanDal`, mirroring `BrgBarcodeValidator`), INV-08 (`JenisRetur` `BAGUS`/`RUSAK`); INV-09 (Salesman/Driver optional) is realized by the absence of a rule. `ReturnOrderWriter : INunaWriter2<ReturnOrderModel>` mirrors `ReturJualWriter.Save`: normalizes each item `ReturnOrderId`, existence-check then `Insert`/`Update`, delete-by-parent then `SqlBulkCopy` item insert, all inside one `TransHelper.NewScope()` transaction (P-04). The writer intentionally performs no numbering (S1.5 owns `ReturnOrderNo` via `INunaCounterBL`, IR-RO-07) and no validation (invoked by `ImportReturnOrderCommand`, S1.5) — exactly as `ReturJualWriter` does. Automated DI resolution: builder/writer are auto-registered by the existing Scrutor scan (`INunaBuilder<>`/`INunaWriter2<>`) and the validator by `AddValidatorsFromAssembly`. No `ReturJual`/`BTR_ReturJual` object touched (INV-14). Verification: `MSBuild` Rebuild of `btr.application.csproj` → Build succeeded, 0 warnings, 0 errors. |

| S1.5 | 2026-09-17 | 2026-09-17 | Created `ImportReturnOrderCommand` + `ImportReturnOrderHandler` in `btr.application/InventoryContext/ReturnOrderAgg/` (mirroring the in-process command precedent `BrgContext/BrgBarcodeAgg/ProcessBarcodeRegistrationRequestCommand` + `RegisterBrgBarcodeCommand`); registered the file in `btr.application.csproj` (`<Compile Include>`). Handler flow: (1) `Guard` on `ReturnOrderId`; (2) **INV-11 / Arch §10.6 idempotency** — `IReturnOrderDal.Exists(key)`; if the existing order already carries a `ReturnOrderNo` it is an already-imported order and is returned unchanged (no counter call, no write: never re-numbers or duplicates). (3) Builds the `ReturnOrderModel` from the relayed payload — the device ULID `ReturnOrderId` is preserved as identity/idempotency key (INV-01) and never merged with the number, `Status = Synced` (IR-RO-09), `WarehouseCode` persisted verbatim (no office-side tenant resolution, IR-RO-04), item lines copied unchanged with `NoUrut` re-sequenced and no small-unit normalization (INV-07, ADR-RO-003). (4) `IValidator<ReturnOrderModel>` = `ReturnOrderValidator` `ValidateAndThrow` enforces INV-02…INV-09. (5) `ReturnOrderNo = _counter.Generate("RETO", IDFormatEnum.PREFYYMnnnnnC)` per **C-1 (confirmed: prefix `RETO`, format `PREFYYMnnnnnC`)** and IR-RO-07 / ADR-RO-002 — numbering is office-owned. (6) Audit from `DateTimeProvider` (audit columns are `NOT NULL`). (7) `IReturnOrderWriter.Save` commits header + items in one `TransHelper.NewScope()` transaction (P-04). Note: the `Exists`-but-unnumbered branch is deliberate and required by the approved stage-then-import flow (Arch §8.2: `ReturnOrderDal` stages the row with `Status = Synced`, then `ImportReturnOrderCommand` assigns the number); it is not a scope expansion. No `ReturJual`/`BTR_ReturJual` object touched (INV-14); no `ServerId` concept on the office model (Arch §23.1). Verification: `MSBuild` Debug build of `btr.application.csproj` (Build succeeded, 0 warnings, 0 errors) plus the dependent `btr.infrastructure`/`btr.distrib` chain (0 errors). Handler/validator auto-registered by the existing MediatR assembly scan (`RegisterServicesFromAssembly`) and `AddValidatorsFromAssembly`; `INunaCounterBL`/`DateTimeProvider` already registered in `btr.distrib` and the j07 `MainOfficeCommandExecutor` (S3.4 will dispatch). |

| S1.6 | 2026-09-17 | 2026-09-17 | Created `CompleteReturnOrderCommand` + `CompleteReturnOrderHandler` + `CompleteReturnOrderResponse` in `btr.application/InventoryContext/ReturnOrderAgg/` (mirroring the office mutation precedent `BrgContext/BrgBarcodeAgg/DeactivateBrgBarcodeCommand`); registered the file in `btr.application.csproj` (`<Compile Include>`). Handler flow: (1) `Guard` on `ReturnOrderId`; (2) `IReturnOrderBuilder.Load` (header + items); (3) **INV-10 / ADR-RO-006** — completion is allowed only while `Status = Synced`; any other status (notably `Imported`) throws `InvalidOperationException` and the order is never modified; (4) only the two optional reference fields are completed through the approved builder setters `SalesPerson(ISalesPersonKey)` / `Driver(IDriverKey)` (INV-09, ADR-RO-005), which resolve the master name; a blank input clears that optional field; (5) `ReturnOrderNo`, `Status` and every item line (incl. `JenisRetur`) are left untouched — no re-numbering, no item change; (6) audit (`ModifiedBy`/`ModifiedDate`) stamped from `DateTimeProvider`/`UserId`; (7) `IReturnOrderWriter.Save` persists header + items unchanged in one `TransHelper.NewScope()` transaction (P-04). No `ReturJual`/`BTR_ReturJual` object touched (INV-14); no `ServerId` concept (Arch §23.1). Verification: `MSBuild` Debug build of `btr.application.csproj` succeeded (0 errors); dependent `btr.infrastructure` / `btr.distrib` chain succeeded (`btr.distrib.exe` produced). **INFO-001:** no automated tests were added; the slice's acceptance criteria do not require them and `btr.test` has no DB-bound harness for command slices (S1.1–S1.5 precedent). |
| S1.7 | 2026-09-17 | 2026-09-17 | Created `GenerateSalesReturnFromReturnOrderCommand` + `GenerateSalesReturnFromReturnOrderHandler` + `GeneratedSalesReturnModel`/response in `btr.application/InventoryContext/ReturnOrderAgg/` (mirroring the office command precedent `ImportReturnOrderCommand`/`CompleteReturnOrderCommand`); registered the file in `btr.application.csproj` (`<Compile Include>`). Handler flow: (1) `Guard`; the selected `ReturnOrderId`s are trimmed, de-duplicated and an empty selection is rejected; (2) `IReturnOrderBuilder.Load` each selected order (header + items) and reject any order whose `Status != Synced` with `InvalidOperationException` — an `Imported` order can never be regenerated (INV-10, Arch §10.6); (3) every selected item is flattened into lines and grouped via `GroupBy(CustomerId, JenisRetur, SalesPersonId, DriverId)` — exactly the ADR-RO-007 / INV-12 key, so one Return Order fans out into multiple `ReturJual` documents when its items' Return Type differs; (4) per group the authoritative warehouse is resolved from the Return Order's `WarehouseCode` through the existing warehouse master (`IWarehouseDal.GetData(new WarehouseModel(code))`, §8.5), the `ReturJual` header is built through the existing `IReturJualBuilder` (`Create`/`Customer`/`Warehouse`/`ReturJualDate`/`JenisRetur`/`User`, plus `SalesPerson`/`Driver` only when present on the source order — INV-09/ADR-RO-005), each item is added through the existing `ICreateReturJualItemWorker` (the ReturJual item-entry machinery) and header + items are persisted with the existing `IReturJualWriter.Save`; (5) each consumed order is stamped `Status = Imported` and saved through `IReturnOrderWriter` inside the same `TransHelper.NewScope()` transaction (nested writer scopes join the ambient transaction, P-04); (6) the response returns the generated `ReturJualId`/`ReturJualCode` list and the consumed `ReturnOrderId` list. **No pricing/inventory/finance (INV-13):** the command passes `HrgInputStr = "0"` so `CreateReturJualItemWorker` performs no invoice-history price lookup and the generated documents carry zero price; the Office Admin completes pricing in the existing RT1-Retur Jual flow (DOMAIN §4). **Unit fidelity (ADR-RO-003):** the recorded `Qty`/`SatId` are handed to the existing item-entry worker with the value placed in the slot of the recorded `BTR_BrgSatuan` unit (big-unit slot when `SatId` is the item's big unit, otherwise the small-unit slot); the Return Order command performs no small-unit conversion arithmetic — the worker owns unit handling — and `(int)Qty`/`SatId` are additionally carried into the generated item's `SubQty`/`SubSatuan`. No `ReturJual` persistence/DAL/domain file was modified (INV-14); no `ServerId` concept (§23.1). Verification: `MSBuild` Debug build of `btr.application.csproj` succeeded (0 warnings, 0 errors); dependent `btr.infrastructure`/`btr.distrib` chain succeeded (`btr.distrib.exe` produced). **INFO-001:** no automated tests were added; the slice's acceptance criteria do not require them and `btr.test` has no DB-bound harness for command slices (S1.1–S1.6 precedent). |

| S2.1 | 2026-09-17 | 2026-09-17 | Created `BTRADE_ReturnOrder.sql` + `BTRADE_ReturnOrderItem.sql` verbatim per Arch §6.2 under `btrade.sqldb/ReturnOrderContext/` (relay-only: no audit columns, no soft delete, no FK — §6.2 "transport-only"); registered both files and the new `ReturnOrderContext` folder in `btrade.sqldb.sqlproj`; added idempotent re-runnable upgrade script `btrade.sqldb/Scripts/Create_BTRADE_ReturnOrder.sql` (`IF OBJECT_ID(...) IS NULL` guards for tables, `sys.indexes` guard for `IX_BTRADE_ReturnOrder_ServerId_Status`) and registered it as `<None Include>`. Header table: PK `(ReturnOrderId, ServerId)`, `ReturnOrderDate DATETIME NOT NULL` (no default — verbatim §6.2), display names (`CustomerName`/`SalesPersonName`/`DriverName`) carried as relay payload, `StatusSync VARCHAR(10) DEFAULT 'TERKIRIM'` (IR-RO-08); item table: PK `(ReturnOrderId, NoUrut)`, `BrgName VARCHAR(60)` relay-only, `Qty DECIMAL(18,2)`. Verification: SSDT `MSBuild /t:Build` of `btrade.sqldb.sqlproj` succeeded (0 errors, `btrade.sqldb.dacpac` generated); upgrade script executed twice against a throwaway LocalDB database (both runs exit 0, idempotent); columns/types/nullability/defaults, PK column order and `IX_BTRADE_ReturnOrder_ServerId_Status` confirmed against §6.2 via `sys.columns` / `sys.index_columns` / `sys.indexes` / `sys.default_constraints`; zero foreign keys on both tables. No existing table, column or index modified (Arch §10.1, GAP-002); no `ReturJual`/`BTR_ReturnOrder` object touched (INV-14). |
| S2.2 | 2026-09-17 | 2026-09-17 | Created `ReturnOrderType` (+ `IReturnOrderKey`) and `ReturnOrderItemType` in `btrade.domain/ReturnOrderFeature/` per Arch §5.2, mirroring the `OrderModel`/`OrderItemType` shape (feasibility §4.1). `ReturnOrderType : IReturnOrderKey, IServerId` carries `ReturnOrderId`, `ServerId`, `ReturnOrderDate` (string `yyyy-MM-dd`, mirroring `OrderModel`'s string dates), `WarehouseCode`, `CustomerId`/`CustomerName`, `SalesPersonId`/`SalesPersonName`, `DriverId`/`DriverName`, `Note`, `StatusSync` (public setter — mutable for the download `TERKIRIM → DOWNLOADED` transition, IR-RO-08) and `ListItems`. Shape fidelity with `OrderModel`: parameterless constructor (DTO/Dapper read), full constructor, private setters for the immutable fields, and the static `Key(string)` factory — the latter is the same mechanical delete/get-by-id key helper `OrderModel.Key` provides and is used by the S2.6 delete-then-insert submit path (`OrderUploadCommandHandler` precedent); it is shape fidelity, not a new decision. `ReturnOrderItemType` is the §5.2 record carrying `ReturnOrderId`, `NoUrut`, `BrgId`, `BrgCode`, `BrgName`, `Qty` (decimal), `SatId`, `JenisRetur` and implementing `IReturnOrderKey`. `IReturnOrderKey { string ReturnOrderId { get; } }` is declared alongside `ReturnOrderType` (mirroring `OrderModel.cs`/`IOrderKey`). The fields align with the S2.1 relay columns (§6.2), including the denormalized `CustomerName`/`SalesPersonName`/`DriverName`/`BrgName` payload. Both types are transport-only with no validation logic (P-02); tenant scoping is via `IServerId`. `btrade.domain` is SDK-style with implicit compile globbing, so no `.csproj` registration was needed. Verification: `dotnet build btrade.domain.csproj` succeeded (0 errors); dependent `btrade.application` / `btrade.infrastructure` / `btrade.webapi` build succeeded (0 errors). The only new warnings are CS8618 nullable warnings from the parameterless constructor (13), identical in kind to the pre-existing 15 on `OrderModel` — the accepted convention for this project. No `ReturJual`/office (`btr.domain`) object touched. |

| S2.3 | 2026-09-17 | 2026-09-17 | Created `IReturnOrderDal` + `IReturnOrderItemDal` contracts in `btrade.application/Contract/` (Arch §7.2 location) and `ReturnOrderDal` + `ReturnOrderItemDal` Dapper implementations in `btrade.infrastructure/Repository/` per Arch §7.2/§10.6, mirroring `OrderDal`/`OrderItemDal`/`IOrderDal`/`IOrderItemDal`. `IReturnOrderDal` composes `IInsert<ReturnOrderType>`, `IUpdate<ReturnOrderType>`, `IDelete<IReturnOrderKey>`, `IGetDataMayBe<ReturnOrderType, IReturnOrderKey>`, `IListDataMayBe<ReturnOrderType, Periode, IServerId>` plus `void Delete(IServerId server)`; `IReturnOrderItemDal` composes `IInsertBulk<ReturnOrderItemType>`, `IDelete<IReturnOrderKey>`, `IListData<ReturnOrderItemType, IReturnOrderKey>`. `ReturnOrderDal` writes exactly the §6.2 columns (no `OrderNote`-style drift, no `FakturId` typo): `ReturnOrderId`/`ServerId`/`ReturnOrderDate`/`WarehouseCode`/`CustomerId`/`CustomerName`/`SalesPersonId`/`SalesPersonName`/`DriverId`/`DriverName`/`Note`/`StatusSync`; `Delete(IReturnOrderKey)` deletes by `ReturnOrderId` (backs the S2.6 delete-then-insert resubmission), `Delete(IServerId)` purges by tenant, and `ListData(Periode, IServerId)` is tenant-scoped (`WHERE ReturnOrderDate BETWEEN @Tgl1 AND @Tgl2 AND ServerId = @ServerId`, served by the `IX_BTRADE_ReturnOrder_ServerId_Status` leading column); the `(ReturnOrderId, ServerId)` PK carries the tenant boundary. `ReturnOrderItemDal` deletes by parent then `SqlBulkCopy`-inserts all 8 §6.2 item columns (incl. relay-only `BrgName`), mirroring `OrderItemDal`. Both DALs are auto-registered by the existing Scrutor scan (`IInsert<>`/`IUpdate<>`/`IDelete<>`/`IGetDataMayBe<,>`/`IListDataMayBe<,>`/`IListData<,>` with `AsSelfWithInterfaces`); the SDK-style projects use implicit compile globbing, so no `.csproj` edit was needed. Verification: `dotnet build btrade.webapi.csproj` succeeded (0 errors; only the pre-existing `NETSDK1138` net6.0-EOL warning and the S2.2 `CS8618` warnings remain). No existing DAL modified. |

| S2.4 | 2026-09-17 | 2026-09-17 | Created `BTR_WarehouseMapping` (`WarehouseCode VARCHAR(20)` PK → `ServerId VARCHAR(5)`, both `NOT NULL DEFAULT('')`) verbatim per Arch §6.3 at `btrade.sqldb/WarehouseContext/BTR_WarehouseMapping.sql`; registered in `btrade.sqldb.sqlproj` (`<Build Include>`). Added idempotent re-runnable seed script `btrade.sqldb/Scripts/Create_BTR_WarehouseMapping.sql` (`IF OBJECT_ID(...) IS NULL` create guard + `NOT EXISTS` seed guards): `GAMPING`→`JOGJA`, `CONCAT`→`JOGJA`, `MAGELANG`→`MGL`; registered as `<None Include>`. Wired `IssueTokenCommandHandler` (login, `POST api/Auth/login`) to resolve the tenant through the centralized mapping: the selected warehouse code (the existing login `LocationId` value — BGud's warehouse selector already carries `GAMPING`/`CONCAT`/`MAGELANG` there, and the barcode-registry login precedent named that field `LocationId`; no wire-contract change, IR-RO-04/§9.2) is looked up via the new `IWarehouseMappingDal`; an unmapped code throws explicitly (`ArgumentException`), **no fallback** (INV-03, R-03, P-11), and the resolved `WarehouseCode` + `ServerId` are returned in `IssueTokenResult` and embedded in the JWT (`locationId`/`serverId` claims) so the session carries both (§8.1, §9.2). Supporting prerequisites not named in the §3 impact inventory (barcode-registry S3.6 INFO-001 precedent): `WarehouseMappingType` + `IWarehouseMappingKey` (`btrade.domain/WarehouseFeature/`, mirroring `LocationType`), `IWarehouseMappingDal` (`btrade.application/Contract/`, mirroring `ILocationDal`) and `WarehouseMappingDal` (`btrade.infrastructure/WarehouseFeature/`, Dapper `IGetDataMayBe<,>`, auto-registered by the existing Scrutor scan, mirroring `LocationDal`). `IssueTokenCommand`/`IssueTokenResult` wire shape unchanged (no BGud contract break); `BTRADE_Location`/`ILocationDal` left untouched but no longer on the login path (the mapping is authoritative, ADR-RO-008). Seed/migration only — no maintenance UI/CRUD (ADR-RO-008). Verification: SSDT `MSBuild /t:Build` of `btrade.sqldb.sqlproj` succeeded (0 errors, `btrade.sqldb.dacpac` generated); seed script executed twice against a throwaway LocalDB database (both runs exit 0, idempotent; 3 rows); columns/types/nullability/defaults and `PK_BTR_WarehouseMapping` (clustered on `WarehouseCode`) confirmed via `sys.columns`/`sys.default_constraints`/`sys.index_columns`; zero foreign keys; mapped lookup (`GAMPING`) returns `JOGJA`, unmapped (`NOWHERE`) returns no row. `dotnet build btrade.webapi.csproj` succeeded (0 errors; only the pre-existing `NETSDK1138` net6.0-EOL + `CS8618` relay-model warnings); `dotnet test btrade.webapi.Test` → 4/4 passed. |

| S2.5 | 2026-09-17 | 2026-09-17 | Created `BTRADE_Driver` verbatim per Arch §6.3 at `btrade.sqldb/DriverContext/BTRADE_Driver.sql` (`DriverId VARCHAR(5)` + `DriverName VARCHAR(20)` + `IsAktif BIT DEFAULT(1)` + `ServerId VARCHAR(5)`, clustered PK `(DriverId, ServerId)`); registered in `btrade.sqldb.sqlproj` (`<Folder Include="DriverContext" />` + `<Build Include>`). Added idempotent re-runnable upgrade script `btrade.sqldb/Scripts/Create_BTRADE_Driver.sql` (`IF OBJECT_ID(...) IS NULL` create guard); registered as `<None Include>`. Created `DriverType` + `IDriverKey` in `btrade.domain/DriverFeature/DriverType.cs` per §5.3: `DriverType : IDriverKey, IServerId` (record, positional `DriverId`/`DriverName`/`IsAktif`/`ServerId`) — realized exactly as written, with `IDriverKey : IServerId` mirroring the `ISalesPersonKey : IServerId` / `ICustomerKey : IServerId` convention so the DAL's `Delete(IDriverKey)` stays tenant-scoped (the §5.3 sketch's bare `IDriverKey` would have forced a non-tenant-scoped delete, contradicting the "tenant scoping" review focus); transport-only, no validation (P-02). Created `IDriverDal` (`btrade.application/Contract/`) and `DriverDal` (`btrade.infrastructure/Repository/`) mirroring `ISalesPersonDal`/`SalesPersonDal` one-for-one: `IInsert<DriverType>`, `IUpdate<DriverType>`, `IDelete<IDriverKey>`, `IGetDataMayBe<DriverType, IDriverKey>`, `IListDataMayBe<DriverType, IServerId>` + `Delete(IServerId server)`; `Delete(key)` scopes `DriverId` + `ServerId`, `Delete(server)` purges by tenant, `ListData(server)` filters `ServerId`; auto-registered by the existing Scrutor scan (no `.csproj` edits — SDK-style implicit globbing, per S2.3 precedent). Created `DriverSyncCommand` + `DriverSyncHandler` (`btrade.application/UseCase/DriverSyncCommand.cs`) mirroring `SalesPersonSyncCommand`: `TransHelper.NewScope()` → `Delete(IServerId)` (full tenant replace) → `Insert` each `ListDriver` → `Complete()`, i.e. delete-then-insert upsert by `DriverId` + `ServerId`. Created `DriverListDataQuery` + `DriverListDataHandler` (`btrade.application/UseCase/DriverListQuery.cs`) mirroring `SalesPersonListDataQuery`, returning `Enumerable.Empty<DriverType>()` when the DAL has no rows. Created `DriverController` (`btrade.webapi/Controllers/`) with `[Route("api/[controller]")]` → `GET /api/Driver/{serverId}` (I-RO-05) and `POST /api/Driver` (I-RO-06), both `JSendOk`-enveloped, mirroring `SalesPersonController`; `[Authorize]` was added at class level per Arch §8.3 (I-RO-05/I-RO-06 auth = JWT), §9.1 ("`j07-btrade-sync` presents a JWT on write endpoints I-RO-02, I-RO-06") and §9.4 ("Endpoint enforcement: Cloud API middleware/attributes") — an authority realization, not a new decision; the existing `SalesPerson`/`Customer` reference routes are left untouched. Verification: `dotnet build btrade.webapi.csproj` succeeded (0 errors; only the pre-existing `NETSDK1138` net6.0-EOL and `CS8618` warnings from S2.2/existing types — no new warnings); SSDT `MSBuild /t:Build btrade.sqldb.sqlproj` succeeded (`btrade.sqldb.dacpac` generated); the upgrade script executed twice against a throwaway LocalDB database (both runs exit 0, idempotent); columns/types/nullability/defaults, PK column order `(DriverId, ServerId)` and zero foreign keys confirmed against §6.3 via `sys.columns` / `sys.index_columns` / `sys.default_constraints` / `sys.foreign_keys`; no existing table, column or index modified (GAP-002). **INFO-001:** no automated tests were added; the slice's acceptance criteria do not require them and the cloud side has no DB-bound test harness (S2.1–S2.4 precedent). |

| S2.6 | 2026-09-17 | 2026-09-17 | Created `ReturnOrderUploadCommand` + `ReturnOrderUploadCommandHandler` and `ReturnOrderIncrementalDownloadQuery` + `ReturnOrderIncrementalDownloadQueryHandler` in `btrade.application/UseCase/` per Arch §7.2, mirroring `OrderUploadCommandHandler` / `OrderIncrementalDownloadQueryHandler`; `btrade.application` is SDK-style with implicit compile globbing, so no `.csproj` registration was needed. **(Upload)** the handler builds a `ReturnOrderType` from the command with `StatusSync = "TERKIRIM"` (relay state only — no validation, no stamping, P-02), copies each `ReturnOrderItemType` onto `ListItems`, then in one `TransHelper.NewScope()` transaction performs delete-then-insert by `ReturnOrderId`: `_returnOrderDal.Delete(ReturnOrderType.Key(request.ReturnOrderId))` → `_returnOrderDal.Insert(model)` → `_returnOrderItemDal.Delete(ReturnOrderType.Key(request.ReturnOrderId))` → `_returnOrderItemDal.Insert(model.ListItems)` → `trans.Complete()`, so resubmission replaces the staged header + items and never duplicates (idempotency by ULID, Arch §10.6 / ADR-RO-002). The command carries `ServerId` on the record for the `(ReturnOrderId, ServerId)` PK (S2.7 resolves it server-side from the JWT and passes it in; no client-supplied body field). **(Incremental download)** parses `Periode` from `Tgl1`/`Tgl2` via `ToDate(DateFormatEnum.YMD)`, reads `_returnOrderDal.ListData(periode, request)` (tenant-scoped `ServerId`, S2.3), filters to `StatusSync == "TERKIRIM"` (empty → `Enumerable.Empty`), attaches each parent's items via `_returnOrderItemDal.ListData(order)`, then flips every returned order to `"DOWNLOADED"` and `Update`s it inside the same `TransHelper.NewScope()` transaction (download-coupled acknowledgement, IR-RO-08). **Deliberate difference from the Order precedent:** the `if (listItem.Count == 0) continue;` guard was intentionally omitted, so Return Orders with no item rows are still returned with an empty `ListItems` rather than being silently dropped (S2.6 acceptance criterion). No `BTR_ReturnOrder`/`ReturJual`/Main Office object touched (INV-14); no business validation or authoritative state authored (P-02). Verification: `dotnet build btrade.webapi.csproj -t:Rebuild` succeeded (0 errors); the only warnings are pre-existing (`NETSDK1138` net6.0-EOL and `CS8618` on the S2.2 `ReturnOrderType` / existing `OrderModel`), none originate from the two new files. |

| S3.1 | 2026-09-17 | 2026-09-17 | Created `ReturnOrderModel` (+ `IReturnOrderKey`) in `j07-btrade-sync/Model/ReturnOrderModel.cs` and `ReturnOrderItemType` in `j07-btrade-sync/Model/ReturnOrderItemType.cs`, mirroring `OrderModel`/`OrderItemType`. `ReturnOrderModel : IReturnOrderKey` carries `ReturnOrderId`, `ReturnOrderDate` (string `yyyy-MM-dd`, mirroring cloud `ReturnOrderType` and `OrderModel` string dates), `WarehouseCode`, `CustomerId`/`CustomerName`, `SalesPersonId`/`SalesPersonName`, `DriverId`/`DriverName`, `Note`, `StatusSync`, `ListItems`, plus the static `Key(string)` factory (same mechanical delete/get-by-id key helper the `OrderModel.Key` precedent provides for the S3.2/S3.3 delete-then-insert and staging paths). `ReturnOrderItemType : IReturnOrderKey` carries `ReturnOrderId`, `NoUrut`, `BrgId`, `BrgCode`, `BrgName`, `Qty` (decimal, no small-unit normalization, ADR-RO-003), `SatId`, `JenisRetur` (ADR-RO-004). No `ServerId` on either sync-side type (mirroring `OrderModel`/`OrderItemType`, which carry no tenant field; tenant binding stays server-side per P-06/S3.6). Both files registered in `j07-btrade-sync.csproj` (`<Compile Include>`). Transport-only, no validation logic (P-02). Verification: `MSBuild` Debug build of `j07-btrade-sync.csproj` (`/p:SignManifests=false`) succeeded — `j07-btrade-sync.exe` produced, 0 errors (only pre-existing warnings: `CS0436 PackingOrderItemModel` type conflict + `CS0108 KonfigurasiForm.CancelButton`; the `MSB3482` signing error on Rebuild is a pre-existing environment issue — no signing certificate installed — unrelated to this slice). |

### 8.2 Review History

| Slice | Date | Result | Findings | Remediation |
| ----- | ---- | ------ | -------- | ----------- |
| S1.1 | 2026-09-17 | GO | None blocking. **INFO-001:** no automated test project targets `btr.sql` (SSDT project); build integrity was verified by `MSBuild /t:Build` → `btr.sql.dacpac` generated with 0 errors, and the upgrade script was executed twice against a throwaway LocalDB database (both runs exit 0, re-runnable), with schema confirmed via `sys.columns` / `sys.indexes` / `sys.default_constraints`. **INFO-002:** the DDL is intentionally duplicated between the SSDT table files (`Tables/InventoryContext/`) and the idempotent upgrade script, mirroring the accepted Barcode Registry S1.1 convention (Arch §10.1); the table files are the source of truth and the script header cross-references them. | None required. |
| S1.2 | 2026-09-17 | GO | None blocking. **INFO-001:** `ReturnOrderModel` carries four read-convenience display fields (`WarehouseName`/`CustomerName`/`SalesPersonName`/`DriverName`) in addition to the §5.1 field list; they are not persisted (no columns in §6.1) and are required for the S1.4 builder's read-resolution (§7.1), mirroring `ReturJualModel` (P-05). Consistent with the same read-resolution statement already made for `BrgName` in §5.1/§VO-01; informational only. **INFO-002:** the slice is a POCO model + key interface with no behavior, so no unit tests were added and none are required by the slice's acceptance criteria; build integrity was verified by `MSBuild` Debug Rebuild of `btr.domain.csproj` (0 errors) plus dependent `btr.application` and `btr.infrastructure` (0 errors). | None required. |
| S1.3 | 2026-09-17 | GO | None blocking. Acceptance criteria individually verified: (1) `IReturnOrderDal` composes `IInsert<ReturnOrderModel>`, `IUpdate<ReturnOrderModel>`, `IGetData<ReturnOrderModel, IReturnOrderKey>`, `IListData<ReturnOrderModel>` plus `GetByReturnOrderId`, `ListByStatus(string)`, `Exists(IReturnOrderKey)` — exact §5.1 composition; `IDelete` deliberately omitted (office never deletes, §6.1 soft-delete strategy). (2) `ReturnOrderItemDal` delete-by-parent then `SqlBulkCopy` insert, mirroring `ReturJualItemDal` (mapping only the 7 §6.1 columns; `BrgName` resolved on read via `BTR_Brg`). (3) no `BTR_ReturJual` reference in any new file (INV-14); `ListByStatus` filters `Status = @Status` and backs the Generate worklist via `IX_BTR_ReturnOrder_Status`; `Exists` (`COUNT(1) > 0`) backs import idempotency (INV-11). Both DALs are auto-registered by the existing Scrutor scan (`IInsert<>`/`IUpdate<>`/`IGetData<,>`/`IListData<>`/`IDelete<>`/`IListData<,>` with `AsSelfWithInterfaces`). Build: `MSBuild` Rebuild of `btr.infrastructure.csproj` succeeded, 0 warnings, 0 errors. **INFO-001:** header reads join `BTR_Customer`/`BTR_SalesPerson`/`BTR_Driver` for the read-convenience display names (mirroring `ReturJualDal`); `WarehouseName` is not resolved in the DAL because `WarehouseCode` has no office master in this slice — resolution remains with the S1.4 builder (§7.1). **INFO-002:** DAL slices have no automated test harness in `btr.test` (DB-bound); build integrity and SQL-to-§6.1 column traceability were verified manually, consistent with the S1.1/S1.2 review precedent. | None required. |
| S1.4 | 2026-09-17 | GO | None blocking. Acceptance criteria individually verified. **(1) Builder:** `ReturnOrderBuilder : IReturnOrderBuilder` exposes `Create()`, `Load(IReturnOrderKey)`, `Attach(ReturnOrderModel)` and field setters (`Customer`, `Warehouse`, `SalesPerson`, `Driver`, `ReturnOrderDate`, `AddItem`, `Note`), and on `Load` resolves the §7.1 read fields — `WarehouseName` (`BTR_Warehouse`), `CustomerName`/`SalesPersonName`/`DriverName` (respective masters) and item `BrgCode`/`BrgName` (`BTR_Brg`); resolution is conditional, so it is a no-op where the S1.3 DAL already joined the name (Customer/SalesPerson/Driver/BrgName). **(2) Validator:** `ReturnOrderValidator : AbstractValidator<ReturnOrderModel>` enforces INV-02 (`ICustomerDal` existence), INV-03 (`WarehouseCode` not empty), INV-04 (≥1 item), INV-05 (`IBrgDal` existence), INV-06 (`Qty > 0`), INV-07 (`(BrgId, SatId)` in `BTR_BrgSatuan` via `IBrgSatuanDal`, mirroring `BrgBarcodeValidator`), INV-08 (`JenisRetur` `BAGUS`/`RUSAK`); INV-09 is satisfied by the absence of any Salesman/Driver rule. **(3) Writer:** `ReturnOrderWriter : INunaWriter2<ReturnOrderModel>` wraps existence-checked `Insert`/`Update` plus delete-by-parent + `SqlBulkCopy` item insert in a single `TransHelper.NewScope()` transaction (P-04), mirroring `ReturJualWriter.Save`; no numbering (S1.5/IR-RO-07 owns `ReturnOrderNo`) and no office-side tenant/warehouse resolution (IR-RO-04). **Authority:** complies with Arch §7.1, §5.1, §23.1 and P-04; no architecture/invariant contradiction. **Scope:** exactly three new files under `InventoryContext/ReturnOrderAgg/Workers/` plus their `btr.application.csproj` registration; no unauthorized scope; nothing missing from the slice. **Cross-check:** no `ReturJual`/`BTR_ReturJual` object touched (INV-14); builder/writer auto-registered by the existing `INunaBuilder<>`/`INunaWriter2<>` Scrutor scan and the validator by `AddValidatorsFromAssembly`. **Build:** `MSBuild` Rebuild of `btr.application.csproj` succeeded, 0 warnings, 0 errors. **INFO-001:** the validator is authored here but its invocation site is `ImportReturnOrderCommand` (S1.5) per §7.1/§18.1; it is intentionally not injected into `ReturnOrderWriter`, which mirrors `ReturJualWriter.Save` (that writer does not validate). **INFO-002:** this slice is application components over DB-bound DALs with no `btr.test` harness, so no automated tests were added and none are required by the acceptance criteria; build integrity was verified manually, consistent with the review precedent. | None required. |
| S1.5 | 2026-09-17 | GO | None blocking. Acceptance criteria individually verified against `ImportReturnOrderCommand.cs`. **(1) Numbering:** `ReturnOrderNo = _counter.Generate("RETO", IDFormatEnum.PREFYYMnnnnnC)`, office-owned per ADR-RO-002/IR-RO-07, with the C-1 confirmation (prefix `RETO`, format `PREFYYMnnnnnC`) recorded in the implementation history; the ULID `ReturnOrderId` is copied from the relay and never overwritten, and is never merged with the number (INV-01). **(2) Validation + persistence:** `IValidator<ReturnOrderModel>` (resolved to `ReturnOrderValidator` via `AddValidatorsFromAssembly`) `ValidateAndThrow` enforces INV-02…INV-09; `WarehouseCode` is persisted verbatim (no office-side tenant resolution, IR-RO-04) with `Status = Synced` (IR-RO-09). **(3) Idempotency:** `IReturnOrderDal.Exists(key)` guard; an order already carrying `ReturnOrderNo` is returned unchanged — the counter and writer are never reached, so a re-import never re-numbers or duplicates (INV-11, Arch §10.6). **(4) Item fidelity:** item lines are copied unchanged (only `ReturnOrderId`/`NoUrut` normalized), no small-unit normalization (INV-07, ADR-RO-003). **(5) Transaction:** `IReturnOrderWriter.Save` commits header + items in one `TransHelper.NewScope()` transaction (P-04). **Authority:** complies with Arch §7.1/§8.2 (stage→import), ADR-RO-002/003/005, INV-01/07/11, IR-RO-04/07/09; no contradiction. **Scope:** exactly one new file under `InventoryContext/ReturnOrderAgg/` plus its `btr.application.csproj` registration; no unauthorized scope; nothing missing from the slice. **Cross-check:** no `BTR_ReturJual` reference (INV-14); no `ServerId` on the office model (§23.1); MediatR handler auto-registered by the existing assembly scan. **Build:** `MSBuild` Debug build of `btr.application.csproj` succeeded (0 warnings, 0 errors); dependent `btr.infrastructure`/`btr.distrib` chain succeeded. **INFO-001:** no automated tests were added; the slice's acceptance criteria do not require them and the handler is DAL-bound (DB), consistent with the S1.1–S1.4 precedent. **INFO-002:** the `Exists`-but-unnumbered branch proceeds to numbering rather than no-op'ing; this is required by the approved §8.2 staging-then-import flow (staging writes the row with `Status = Synced` before the office import assigns the number) and the criterion's "already-imported" condition (number present) is honored; informational, not a scope expansion. | None required. |

| S1.6 | 2026-09-17 | GO | None blocking. Acceptance criteria individually verified against `CompleteReturnOrderCommand.cs`. **(1) Synced-only completion:** the handler `Load`s the aggregate via `IReturnOrderBuilder` and throws `InvalidOperationException` unless `Status == "Synced"` (INV-10, ADR-RO-006); only `SalesPersonId`/`DriverId` are set (INV-09, ADR-RO-005, through the approved builder setters which resolve the master name); a blank input clears the corresponding optional field. **(2) No re-number / no item change:** `ReturnOrderNo` is never assigned and `Status` never transitions; `ListItem` (incl. `JenisRetur`) is carried through `Load`→`Save` unchanged. **(3) Imported rejected:** an `Imported` (or any non-`Synced`) order fails the status guard before any write. **Authority:** complies with Arch §7.1 (`CompleteReturnOrderCommand`), §8.5 flow, IR-RO-05, INV-09/10, ADR-RO-005/006; no contradiction. **Scope:** exactly one new file under `InventoryContext/ReturnOrderAgg/` plus its `btr.application.csproj` registration; no unauthorized scope; nothing missing from the slice. **Cross-check:** no `ReturJual`/`BTR_ReturJual` reference (INV-14); no `ServerId` concept (§23.1); handler auto-registered by the existing MediatR assembly scan. **Build:** `MSBuild` Debug build of `btr.application.csproj` succeeded (0 warnings, 0 errors); dependent `btr.infrastructure`/`btr.distrib` chain succeeded (`btr.distrib.exe` produced). **INFO-001:** no automated tests were added and none are required by the slice's acceptance criteria (`btr.test` has no DB-bound command harness; S1.1–S1.5 precedent). **INFO-002:** `IReturnOrderWriter.Save` re-saves the unchanged item rows (delete-by-parent + bulk insert, approved S1.4 behavior mirroring `ReturJualWriter`); no item data is altered and no side effect occurs beyond the two optional fields. | None required. |
| S1.7 | 2026-09-17 | GO | None blocking. Acceptance criteria individually verified against `GenerateSalesReturnFromReturnOrderCommand.cs`. **(1) Grouping / fan-out:** items from all selected orders are grouped by `GroupBy(CustomerId, JenisRetur, SalesPersonId, DriverId)` — exactly the ADR-RO-007/INV-12 key — and each group produces one `ReturJual`, so a single Return Order fans out when its item Return Types differ (INV-12). **(2) Delegation:** the header is built through the existing `IReturJualBuilder`, `JenisRetur` maps directly to `ReturJual.JenisRetur` (`BAGUS`/`RUSAK`, IR-RO-02), each item is created through the existing `ICreateReturJualItemWorker` item-entry machinery receiving the recorded `Qty`/`SatId`, and header + items are persisted by the existing `IReturJualWriter.Save`. **(3) Warehouse:** the authoritative `BTR_Warehouse.WarehouseId` is resolved from the Return Order's `WarehouseCode` via the existing warehouse master (§8.5). **(4) No pricing/inventory/finance:** `HrgInputStr = "0"` never reaches the invoice-history price lookup, so generated documents carry zero price and remain completable in the existing RT1-Retur Jual flow (INV-13, DOMAIN §4). **(5) Lifecycle:** every non-`Synced` order is rejected before any write and consumed orders transition `Synced → Imported` inside one `TransHelper.NewScope()` transaction wrapping the generated documents (INV-10, Arch §10.6, P-04). **(6) INV-14:** no `ReturJual` persistence/DAL/domain file was modified; only the existing writer is invoked. **Authority:** complies with Arch §7.1/§8.5, ADR-RO-003/005/007, INV-09/10/12/13/14, IR-RO-02. **Scope:** exactly one new file under `InventoryContext/ReturnOrderAgg/` plus its `btr.application.csproj` registration; no unauthorized scope; nothing missing from the slice. **Build:** `MSBuild` Debug build of `btr.application.csproj` succeeded (0 warnings, 0 errors) and the dependent `btr.infrastructure`/`btr.distrib` chain succeeded (`btr.distrib.exe` produced). **INFO-001:** no automated tests were added; not required by the slice's acceptance criteria (`btr.test` has no DB-bound command harness; S1.1–S1.6 precedent). **INFO-002:** the recorded quantity is placed in the big-unit slot only when `SatId` equals the item's big `BTR_BrgSatuan` unit and otherwise in the small-unit slot, because the existing `ReturJual` item-entry machinery models only a big and a small unit; a Return Order item recorded in a third/middle unit would therefore be read in the small-unit slot. This is the accepted unit model of the existing machinery (ADR-RO-003 keeps conversion there), not a new Return Order behavior; the recorded `(int)Qty`/`SatId` are also preserved on the generated item as `SubQty`/`SubSatuan`. **INFO-003:** `ReturJualDate` is set to the generation time (`DateTimeProvider.Now`) and the `ReturJual` `Note` is left empty; the plan does not prescribe these header values. | None required. |

| S2.1 | 2026-09-17 | GO | None blocking. Acceptance criteria individually verified. **(1) `BTRADE_ReturnOrder` matches §6.2:** `ReturnOrderId VARCHAR(26)` + `ServerId VARCHAR(5)` clustered PK `(ReturnOrderId, ServerId)`; `ReturnOrderDate DATETIME NOT NULL` (verbatim — arch declares no default); `WarehouseCode VARCHAR(20)`; `CustomerId VARCHAR(6)` + `CustomerName VARCHAR(100)`; `SalesPersonId VARCHAR(5)` + `SalesPersonName VARCHAR(100)`; `DriverId VARCHAR(5)` + `DriverName VARCHAR(20)`; `Note VARCHAR(100)`; `StatusSync VARCHAR(10) DEFAULT 'TERKIRIM'` (IR-RO-08); `IX_BTRADE_ReturnOrder_ServerId_Status` on `(ServerId, StatusSync)`; all VARCHAR lengths/defaults confirmed via `sys.columns`/`sys.default_constraints`. **(2) `BTRADE_ReturnOrderItem` matches §6.2:** PK `(ReturnOrderId, NoUrut)`; `BrgId VARCHAR(6)`, `BrgCode VARCHAR(20)`, `BrgName VARCHAR(60)`, `Qty DECIMAL(18,2)`, `SatId VARCHAR(7)`, `JenisRetur VARCHAR(5)`; no index (arch declares none). **(3) Registration + migration:** both files registered in `btrade.sqldb.sqlproj` (folder `ReturnOrderContext` added); `Scripts/Create_BTRADE_ReturnOrder.sql` registered as `<None Include>` and re-runnable — executed twice against a throwaway LocalDB database, both runs exit 0 (idempotent). **Authority:** complies with Arch §6.2 verbatim (relay-only shaping: no audit columns, no soft delete, no FK, P-02), §10.1 (additive, guarded pattern, registered in sqlproj), IR-RO-01 (`VARCHAR(26)` ULID idempotency key), IR-RO-08 (`StatusSync` `TERKIRIM`/`DOWNLOADED`); no architecture decision contradicted. **Scope:** exactly two new table files under `ReturnOrderContext/`, one new guarded script under `Scripts/`, and the corresponding `btrade.sqldb.sqlproj` registrations; no existing table/column/index modified (GAP-002); nothing missing from the slice. **Cross-check:** no `BTR_ReturnOrder`/`ReturJual` object touched (INV-14); zero foreign keys on both tables. **Build:** SSDT `MSBuild /t:Build` of `btrade.sqldb.sqlproj` succeeded — `btrade.sqldb.dacpac` generated, 0 errors. **INFO-001:** no automated test project targets `btrade.sqldb` (SSDT project); build integrity plus guarded-script idempotency were verified by `MSBuild` and double execution against LocalDB, consistent with the S1.1 precedent. **INFO-002:** the DDL is duplicated between the SSDT table files and the idempotent upgrade script, mirroring the accepted Barcode Registry / S1.1 convention (Arch §10.1); the table files remain the source of truth and the script header cross-references them. | None required. |
| S2.2 | 2026-09-17 | GO | None blocking. Acceptance criteria individually verified against `btrade.domain/ReturnOrderFeature/ReturnOrderType.cs` and `ReturnOrderItemType.cs`. **(1) `ReturnOrderType`:** declared `ReturnOrderType : IReturnOrderKey, IServerId` with exactly the §5.2 member set — `ReturnOrderId`, `ServerId`, `ReturnOrderDate` (string `yyyy-MM-dd`, mirroring `OrderModel`'s string dates), `WarehouseCode`, `CustomerId`/`CustomerName`, `SalesPersonId`/`SalesPersonName`, `DriverId`/`DriverName`, `Note`, `StatusSync` (public setter — mutable for the `TERKIRIM → DOWNLOADED` download transition, IR-RO-08) and `ListItems`. **(2) `ReturnOrderItemType`:** the §5.2 record carries `ReturnOrderId`, `NoUrut`, `BrgId`, `BrgCode`, `BrgName`, `Qty` (`decimal`), `SatId`, `JenisRetur` and implements `IReturnOrderKey`. **(3) Transport-only:** no validation, guard clauses or business logic in either type (P-02); tenant scoping is via `IServerId`. Fields align with the S2.1 relay columns (§6.2), including the denormalized names. **Authority:** complies with Arch §5.2 verbatim (location `btrade.domain/ReturnOrderFeature/`, `OrderModel`/`OrderItemType` shape, feasibility §4.1) and P-02; no architecture/invariant contradiction (the office `IReturnOrderKey` from S1.2 lives in a different assembly/namespace, so there is no collision). **Scope:** exactly two new files under `ReturnOrderFeature/` (`ReturnOrderType.cs` with `ReturnOrderType` + `IReturnOrderKey`, `ReturnOrderItemType.cs`); `btrade.domain` is SDK-style with implicit compile globbing so no `.csproj` edit was needed; nothing missing from the slice. **Build:** `dotnet build btrade.domain.csproj` → 0 errors; dependent `btrade.application` / `btrade.infrastructure` / `btrade.webapi` → 0 errors (the `sqlproj` restore error is the pre-existing SSDT/dotnet-CLI limitation and is unrelated to this slice). **INFO-001:** the parameterless + full constructors and the static `Key(string)` factory are not literally enumerated in §5.2 but are part of the `OrderModel` shape the slice objective requires mirroring: the constructors are mechanical prerequisites to instantiate a class whose fields have private setters, and `Key(string)` is the identical delete/get-by-id key helper `OrderModel.Key` provides (used by the S2.6 submit delete-then-insert, `OrderUploadCommandHandler` precedent) — shape fidelity, not scope expansion or a new decision. **INFO-002:** 13 CS8618 nullable warnings arise from the parameterless constructor, identical in kind to the 15 pre-existing `OrderModel` warnings; this is the established convention for these relay models and produces no build failure. **INFO-003:** no automated tests were added and none are required by the slice's acceptance criteria (POCO transport model; S1.2 precedent). **TRACK-INFO-004:** the S2.1 progress-tracker row still read `IMPLEMENTED` although its review history records `GO`; corrected to `GO` as part of this review's tracker-accuracy pass. | None required. |

| S2.3 | 2026-09-17 | GO | None blocking. Acceptance criteria individually verified against `IReturnOrderDal.cs`/`IReturnOrderItemDal.cs` (`btrade.application/Contract/`) and `ReturnOrderDal.cs`/`ReturnOrderItemDal.cs` (`btrade.infrastructure/Repository/`). **(1) Generic composition:** `IReturnOrderDal` composes `IInsert<ReturnOrderType>`, `IUpdate<ReturnOrderType>`, `IDelete<IReturnOrderKey>`, `IGetDataMayBe<ReturnOrderType, IReturnOrderKey>`, `IListDataMayBe<ReturnOrderType, Periode, IServerId>` plus `Delete(IServerId)` — the exact `IOrderDal` shape with `OrderModel`→`ReturnOrderType`; `IReturnOrderItemDal` composes `IInsertBulk<ReturnOrderItemType>`, `IDelete<IReturnOrderKey>`, `IListData<ReturnOrderItemType, IReturnOrderKey>` — the exact `IOrderItemDal` shape. **(2) Auto-registration:** both classes are selected by the existing Scrutor scan in `InfrastructureService` (`IDelete<>`, `IListData<,>`, `IGetDataMayBe<,>`, `IListDataMayBe<,>`, `IInsert<>`, `IUpdate<>`) and registered `AsSelfWithInterfaces()`, so `IReturnOrderDal`/`IReturnOrderItemDal` resolve. **(3) Tenant boundary / idempotent upsert:** `ListData(Periode, IServerId)` filters `ServerId = @ServerId`; the `(ReturnOrderId, ServerId)` PK carries tenancy; `Delete(IReturnOrderKey)` deletes by `ReturnOrderId` (the §10.6 delete-then-insert resubmission path) and `Delete(IServerId)` purges a tenant. **(4) No drift copied:** the header SQL maps exactly the 12 `BTRADE_ReturnOrder` columns and the item SQL exactly the 8 `BTRADE_ReturnOrderItem` columns — no `OrderNote`/`FakturId`-style mismatch. **Authority:** complies with Arch §5.2, §6.2, §7.2, §10.6 and P-02 (no validation/business logic in the DAL); no architecture decision contradicted. **Scope:** exactly four new files + tracker update; no existing DAL modified; nothing missing. **Build:** `dotnet build btrade.infrastructure.csproj -t:Rebuild` → Build succeeded, 0 errors (28 warnings all pre-existing: `NETSDK1138` net6.0-EOL + S2.2 `CS8618` on `ReturnOrderType`/`OrderModel`); dependent `btrade.webapi` built 0 errors. **INFO-001:** DB round-trip not executed; there is no DB-bound automated harness for DAL slices (S1.1–S1.3/S2.1–S2.2 precedent) — column-to-§6.2 traceability verified by inspection. **INFO-002:** the plan AC phrase "all reads/writes scoped by `ServerId`" is realized as the `IOrderDal` mirror: `ListData` is `ServerId`-scoped, the PK carries `ServerId`, and `Delete(IServerId)` is tenant-scoped; `GetData`/`Delete(IReturnOrderKey)` are keyed by the globally-unique ULID only, exactly as `OrderDal` and the approved §5.2 `IReturnOrderKey` require (adding `ServerId` would alter the S2.2 key contract). **INFO-003:** `ReturnOrderDate` is a `DATETIME` column written/read as a `yyyy-MM-dd` `VarChar` (mirroring `OrderDal`'s date handling and the approved S2.2 string date shape); given all writers emit date-only values this does not affect the `Periode` bound. | None required. |

| S2.4 | 2026-09-17 | GO | None blocking. Acceptance criteria individually verified. **(1) Table + registration + seed:** `btrade.sqldb/WarehouseContext/BTR_WarehouseMapping.sql` declares `WarehouseCode VARCHAR(20) NOT NULL DEFAULT('')` + `ServerId VARCHAR(5) NOT NULL DEFAULT('')` with `PK_BTR_WarehouseMapping` clustered on `WarehouseCode` — verbatim Arch §6.3 (IR-RO-04); registered in `btrade.sqldb.sqlproj` (`<Build Include>`). `btrade.sqldb/Scripts/Create_BTR_WarehouseMapping.sql` is idempotent (`IF OBJECT_ID(...) IS NULL` + `NOT EXISTS` seed guards) and registered as `<None Include>`; executed twice against a throwaway LocalDB database — both runs exit 0, 3 rows, seed exactly `GAMPING`→`JOGJA`, `CONCAT`→`JOGJA`, `MAGELANG`→`MGL`; schema confirmed via `sys.columns`/`sys.default_constraints`/`sys.index_columns`; zero foreign keys. **(2) Login resolution + explicit failure:** `IssueTokenCommandHandler` resolves the tenant through `IWarehouseMappingDal.GetData(new WarehouseMappingKey(<selected warehouse code>))` and throws `ArgumentException` when the code is unmapped — `BTR_WarehouseMapping` is the sole resolution source, no `BTRADE_Location` lookup and no fallback remains (INV-03, R-03, P-11, ADR-RO-008). Mapped lookup (`GAMPING`) returns `JOGJA`; unmapped (`NOWHERE`) returns no row. **(3) Session carries both:** `IssueTokenResult` returns the resolved `WarehouseCode` (login response `LocationId`, stored by BGud as `warehouseCode`) and `ServerId` (stored as `officeCode`), and the JWT embeds both claims (§8.1, §9.2); no maintenance UI/CRUD was added — seed/migration only (ADR-RO-008). **Authority:** complies with Arch §6.3 verbatim, IR-RO-04 (Cloud-side table in `btrade.sqldb`, mirrors `BTRADE_Location`, seeded with the same initial rows), ADR-RO-008, P-11, R-03, INV-03; no architecture decision contradicted. **Scope:** one table file + one script + two `sqlproj` registrations + three supporting read components (see INFO-002) + the authorized `IssueTokenCommand` rewiring; no existing table/column/index modified, no BGud contract change, nothing missing from the slice. **Build:** SSDT `MSBuild /t:Build` of `btrade.sqldb.sqlproj` succeeded (`btrade.sqldb.dacpac` generated, 0 errors); `dotnet build btrade.webapi.csproj` succeeded (0 errors); `dotnet test btrade.webapi.Test` → 4/4 passed. **INFO-001:** the login request field is still named `LocationId` (pre-existing barcode-registry I-07 wire contract); its value is the warehouse code selected by BGud's warehouse selector, so treating it as the `WarehouseCode` lookup key introduces no wire-contract break and no rename beyond the slice's authority — informational, not a defect. **INFO-002:** `WarehouseMappingType`/`IWarehouseMappingKey`, `IWarehouseMappingDal` and `WarehouseMappingDal` are not named in the §3 impact inventory but are direct prerequisites for the mapping lookup, mirroring `LocationType`/`ILocationDal`/`LocationDal` and auto-registered by the existing Scrutor `IGetDataMayBe<,>` scan (same precedent as barcode-registry S3.6 INFO-001). **INFO-003:** `BTRADE_Location`/`ILocationDal`/`LocationDal`/`LocationType`/`LocationKey` are left untouched but are no longer on the login path; no other consumer exists in the current codebase (removal is out of scope). **INFO-004:** Arch §9.4 lists the Warehouse mapping owner as "Main Office (`BTR_WarehouseMapping`)" while §6.3/IR-RO-04 and this plan place it Cloud-side in `btrade.sqldb`; the implementation follows §6.3/IR-RO-04 and the slice's explicit location — raised for the Knowledge Curator (no code change). **INFO-005:** no automated test exercises the login mapping path (no DB-bound harness in `btrade.webapi.Test`); verification is build + script-execution + schema/lookup inspection, consistent with the S2.1–S2.3 precedent and not required by the slice's acceptance criteria. | None required. |

| S2.5 | 2026-09-17 | GO | None blocking. Acceptance criteria individually verified against `BTRADE_Driver.sql` (`btrade.sqldb/DriverContext/`), `DriverType.cs` (`btrade.domain/DriverFeature/`), `IDriverDal.cs` (`btrade.application/Contract/`), `DriverDal.cs` (`btrade.infrastructure/Repository/`), `DriverSyncCommand.cs` / `DriverListQuery.cs` (`btrade.application/UseCase/`) and `DriverController.cs` (`btrade.webapi/Controllers/`). **(1) Table matches §6.3:** `DriverId VARCHAR(5) NOT NULL DEFAULT('')`, `DriverName VARCHAR(20) NOT NULL DEFAULT('')`, `IsAktif BIT NOT NULL DEFAULT(1)`, `ServerId VARCHAR(5) NOT NULL DEFAULT('')`, clustered `PK_BTRADE_Driver (DriverId, ServerId)`; registered in `btrade.sqldb.sqlproj` (`DriverContext` folder + `<Build Include>`); `Scripts/Create_BTRADE_Driver.sql` is guarded (`IF OBJECT_ID(...) IS NULL`) and registered as `<None Include>`; executed twice against a throwaway LocalDB database — both runs exit 0 (idempotent); schema confirmed via `sys.columns`/`sys.default_constraints`/`sys.index_columns`/`sys.foreign_keys`, zero FKs. **(2) Domain type + DAL mirror SalesPerson:** `DriverType : IDriverKey, IServerId` with `DriverId`/`DriverName`/`IsAktif`/`ServerId`; `IDriverDal` composes `IInsert<DriverType>`, `IUpdate<DriverType>`, `IDelete<IDriverKey>`, `IGetDataMayBe<DriverType, IDriverKey>`, `IListDataMayBe<DriverType, IServerId>` + `Delete(IServerId)`; `DriverDal` maps exactly the 4 §6.3 columns in every statement, `Delete(IDriverKey)` scopes `DriverId`+`ServerId`, `Delete(IServerId)` purges a tenant, `ListData(IServerId)` filters `ServerId`; both classes auto-registered by the existing Scrutor scan (`AsSelfWithInterfaces`). **(3) Use cases + routes:** `DriverListDataQuery`/`DriverListDataHandler` and `DriverSyncCommand`/`DriverSyncHandler` mirror the SalesPerson pair; sync wraps `Delete(IServerId)` + per-row `Insert` in one `TransHelper.NewScope()` transaction (delete-then-insert upsert by `DriverId` + `ServerId`); `DriverController` exposes `GET /api/Driver/{serverId}` (I-RO-05) and `POST /api/Driver` (I-RO-06) with the `JSendOk` envelope. **Authority:** complies with Arch §5.3, §6.3, §7.2, §8.3, §8.4, §9.1, §9.4, P-02; GAP-013 closed. **Scope:** exactly the slice's files (one table + one script + `sqlproj` registration + `DriverType`/`IDriverKey` + `IDriverDal`/`DriverDal` + two use cases + one controller) + tracker; no existing DAL/controller/table modified; nothing missing. **Build:** `dotnet build btrade.webapi.csproj` → Build succeeded, 0 errors (only pre-existing `NETSDK1138`/`CS8618` warnings); SSDT `MSBuild /t:Build btrade.sqldb.sqlproj` → dacpac generated, 0 errors. **INFO-001:** `IDriverKey : IServerId` (rather than the bare `IDriverKey` in the §5.3 sketch) mirrors the `ISalesPersonKey : IServerId`/`ICustomerKey : IServerId` convention and is required so `Delete(IDriverKey)` remains tenant-scoped; the declared `DriverType : IDriverKey, IServerId` matches §5.3 literally. **INFO-002:** `[Authorize]` on `DriverController` realizes Arch §8.3/§9.1/§9.4 (JWT on I-RO-05/I-RO-06) and is required for S3.6's "401 without a token" acceptance; it is an authority realization, not scope expansion. **INFO-003:** no automated tests were added; the slice's acceptance criteria do not require them (S2.1–S2.4 precedent). | None required. |

| S2.6 | 2026-09-17 | GO | None blocking. Acceptance criteria individually verified against `ReturnOrderUploadCommand.cs` / `ReturnOrderIncrementalDownloadQuery.cs` (`btrade.application/UseCase/`). **(1) Idempotent submit:** `ReturnOrderUploadCommandHandler` builds the `ReturnOrderType` with `StatusSync = "TERKIRIM"`, copies the item payload, and in one `TransHelper.NewScope()` transaction performs `_returnOrderDal.Delete(ReturnOrderType.Key(id))` → `Insert(header)` → `_returnOrderItemDal.Delete(Key(id))` → `Insert(items)` → `Complete()`, i.e. delete-then-insert by `ReturnOrderId` (Arch §10.6) — a resubmission replaces the staged copy and cannot duplicate (ADR-RO-002); the ULID `ReturnOrderId` is the key, never the office number. **(2) Relay-not-authority:** the handler performs no business validation, numbering, warehouse mapping or office import — `ServerId` is carried for the `(ReturnOrderId, ServerId)` PK only and is resolved server-side in S2.7 (P-02, P-06). **(3) Acknowledged download:** the query parses the `Periode` from `Tgl1`/`Tgl2`, reads tenant-scoped `ListData(periode, request)`, filters `StatusSync == "TERKIRIM"`, and flips each returned order to `"DOWNLOADED"` + `Update` inside the same `TransHelper.NewScope()` transaction (IR-RO-08); a missing/empty result returns `Enumerable.Empty` and never writes. **(4) Items with parent, no silent drop:** each returned order is populated with its items via `_returnOrderItemDal.ListData(order)`; the Order precedent's `if (listItem.Count == 0) continue;` guard is deliberately absent, so a Return Order without item rows is returned with an empty `ListItems` rather than dropped. **Authority:** complies with Arch §7.2 (type names, locations, responsibilities), §8.1/§8.2 (submit → stage → download flow), §10.6, IR-RO-08, ADR-RO-002, P-02; no architecture decision contradicted. **Scope:** exactly two new files under `btrade.application/UseCase/` + tracker update; `btrade.application` uses implicit compile globbing so no `.csproj` edit (S2.3–S2.5 precedent); no existing type modified; nothing missing from the slice. **Cross-check:** no Main Office `BTR_ReturnOrder`/`ReturJual` object touched (INV-14); SCR-MOB/Desktop consumers, controllers (S2.7) and sync client (S3.x) are out of scope and untouched. **Build:** `dotnet build btrade.webapi.csproj -t:Rebuild` succeeded (0 errors); warnings are pre-existing only (`NETSDK1138` net6.0-EOL, `CS8618` on the S2.2 `ReturnOrderType` / existing `OrderModel`) — none from the new files. **INFO-001:** no automated tests were added; the slice's acceptance criteria do not require them and `btrade.application`/`btrade.test` has no DB-bound use-case harness (S1.1–S2.5 precedent); MediatR handlers are auto-registered by the existing assembly scan. **INFO-002:** the `ListItems` attachment relies on the S2.3 `ReturnOrderItemDal.ListData(IReturnOrderKey)`; the deliberate zero-item inclusion matches the S2.6 wording ("orders without items are not silently dropped") and does not contradict any authority statement. | None required. |

| S3.1 | 2026-09-17 | GO | None blocking. Acceptance criteria individually verified against `Model/ReturnOrderModel.cs` / `Model/ReturnOrderItemType.cs` (`j07-btrade-sync/Model/`). **(1)** `ReturnOrderModel : IReturnOrderKey` carries `ReturnOrderId`, `ReturnOrderDate` (string, mirroring cloud `ReturnOrderType` + `OrderModel` string-date convention), `WarehouseCode`, `CustomerId`/`CustomerName`, `SalesPersonId`/`SalesPersonName`, `DriverId`/`DriverName`, `Note`, `StatusSync`, `ListItems`, plus the static `Key(string)` factory (same mechanical key helper as `OrderModel.Key`, serving the S3.2/S3.3 delete-then-insert and staging paths — shape fidelity, not a new decision). **(2)** `ReturnOrderItemType : IReturnOrderKey` carries `ReturnOrderId`, `NoUrut`, `BrgId`, `BrgCode`, `BrgName`, `Qty` (decimal, ADR-RO-003), `SatId`, `JenisRetur` (ADR-RO-004). **(3)** Both files registered in `j07-btrade-sync.csproj` (`<Compile Include="Model\ReturnOrderItemType.cs" />`, `<Compile Include="Model\ReturnOrderModel.cs" />`); the non-SDK project compiled them (build produced `j07-btrade-sync.exe`). No `ServerId` on either sync-side type — mirrors `OrderModel`/`OrderItemType` (no tenant field); tenant binding stays server-side per P-06/S3.6. **Authority:** complies with Arch §4.3 (sync owns transport models), P-02 (transport-only, no validation), ADR-RO-003/004; no architecture decision contradicted. **Scope:** exactly two new files + two `csproj` lines + tracker update; no existing file modified; nothing missing. **Build:** `MSBuild` Debug build of `j07-btrade-sync.csproj` (`/p:SignManifests=false`) succeeded — `j07-btrade-sync.exe` produced, 0 errors (only pre-existing warnings: `CS0436 PackingOrderItemModel` conflict, `CS0108 KonfigurasiForm.CancelButton`; `MSB3482` signing error on Rebuild is a pre-existing environment issue — no signing certificate — unrelated to this slice). **INFO-001:** no automated tests were added; the slice's acceptance criteria do not require them (transport POCOs; S1.2/S2.2 precedent). | None required. |

### 8.3 Remediation History

Populated when a slice returns `NO-GO` (remediation record + re-review result).


