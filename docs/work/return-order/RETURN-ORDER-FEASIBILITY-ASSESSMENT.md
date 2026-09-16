# FEASIBILITY ASSESSMENT

# Return Order

| Field | Value |
| ----- | ----- |
| Deliverable | `RETURN-ORDER-FEASIBILITY-ASSESSMENT.md` |
| Business request | Implement the Return Order domain as defined in `docs/work/return-order/RETURN-ORDER-DOMAIN.md` |
| Primary business artifact | `docs/work/return-order/RETURN-ORDER-DOMAIN.md` |
| Implementation target | `src/BGud` (new Android warehouse app; first module was Barcode Registry) |
| Method | `docs/skills/feasibility-creation-skill.md` |
| Systems assessed | `src/BGud` (warehouse mobile), `j06-pkl-btrade-api` (Cloud API), `j07-btrade-sync` (synchronization), `j05-btr-distrib` (BTR Desktop / Main Office), `BTrade3` (stack reference), `j07-btr-gudang` (legacy warehouse client) |
| Author role | Analyst + Architect (Feasibility only) |
| Status | **Analysis — no implementation authorized** |

> This document is an analysis artifact. It does not define architecture, create
> implementation plans, create slices, or write code. It establishes planning
> readiness. Where a decision is required, it is recorded as an Open Question.
> No business decision is created here; every ambiguity is surfaced, never
> silently resolved.

---

## 1. Executive Summary

### Request

Implement the Return Order domain described in
`docs/work/return-order/RETURN-ORDER-DOMAIN.md`: a warehouse operational
capability, delivered through the BGud mobile application, that lets Warehouse
Officers record goods physically returned by Customers — with offline support
and item identification by Barcode Scan or Manual Item Search — and synchronize
those records to the Main Office, where they become the operational source
document for Sales Return processing in BTR Desktop.

Return Order is deliberately narrow: it records physical goods movement only. It
does **not** perform inventory valuation, financial processing, customer
balance adjustment, or accounting (DOMAIN §1, §15). Sales Return creation,
pricing determination, inventory impact, and financial processing remain in the
existing Office systems.

### Recommendation

Implement Return Order as a new warehouse-mobility domain that reuses the
existing, proven infrastructure rather than creating new synchronization or
reference-data plumbing:

1. **BGud (mobile)** — new offline Return Order capture (create / update /
   delete / search / synchronize) built on the existing Room + Retrofit +
   WorkManager stack, reusing Barang/Barcode lookup (Barcode Scan + Manual Item
   Search) already delivered by the Barcode Registry module.
2. **Cloud (`j06-pkl-btrade-api`)** — a `ReturnOrder` staging relay, mirroring
   the existing CheckIn/Order mobile-originated pattern (BGud → Cloud →
   `j07-btrade-sync` → Main Office), *not* the Barcode master-data snapshot
   pattern.
3. **`j07-btrade-sync`** — an incremental download service staging Return
   Orders into the Main Office, mirroring `CheckInIncrementalDownloadService`
   / `OrderIncrementalDownloadService`.
4. **Main Office (`j05-btr-distrib`)** — a new authoritative Return Order
   aggregate and table, distinct from the existing `ReturJual` (Sales Return),
   plus Return Order Import and Generate Sales Return from Return Order. No
   dedicated review/approval screen (GAP-006/ADR-RO-007).
5. **Reference data** — reuse the existing Customer / SalesPerson cloud sync
   for BGud lookup; add a Driver projection if needed (GAP-005); resolve the
   Warehouse dimension through a centralized Warehouse Mapping (GAP-012).

The capability is implementable within the current ecosystem. All business and
technical decisions are now made and recorded as ADR-RO-001 … ADR-RO-008, so
implementation planning can proceed.

### Feasibility Result

```text
FEASIBLE
```

All required technical patterns already exist and are proven in this
repository: offline Room cache + WorkManager sync (BGud Barcode Registry),
mobile-originated relay (CheckIn/Order), authoritative Office processing
(`ReturJual`), and shared reference-data sync (Customer/SalesPerson/Barang).
No new synchronization infrastructure, no new client technology, and no new
authority model are required.

### Planning Readiness Result

```text
READY
```

All identified gaps (GAP-001 … GAP-014) are closed and all blocking questions
(BQ-1 … BQ-7, TQ-1 … TQ-5) are resolved, recorded as ADR-RO-001 … ADR-RO-008
(Section 11). Non-blocking Architecture items (INFO-001, INFO-002) remain
documented in Section 9 and do not block planning. The Implementation Plan may
now be produced.

---

## 2. Request Understanding

### Requested Capabilities (as stated in the domain artifact)

| Capability | Reference |
| ---------- | --------- |
| Create Return Order | DOMAIN §5 BC-001 |
| Update Return Order (pre-sync only) | DOMAIN §5 BC-002, BR-017 |
| Delete Return Order (pre-sync only) | DOMAIN §5 BC-003, BR-019 |
| Search Return Order | DOMAIN §5 BC-004 |
| Synchronize Return Order | DOMAIN §5 BC-005 |
| Item identification by Barcode Scan or Manual Item Search | DOMAIN BR-009 |
| Offline warehouse operation | DOMAIN BG-003 |

### Stated Environment and Constraints

| Aspect | Stated position |
| ------ | --------------- |
| Business area | Warehouse (Inventory) — DOMAIN §1, §13 |
| Target client | BGud Mobile App (Warehouse Officers) |
| Offline | Must operate with no network; sync later (BG-003) |
| Downstream | Sales Return in BTR Desktop; separate entity (DOMAIN §13) |
| Out of scope | Inventory update/valuation, customer balance, credit note, accounting, approval/rejection workflow, principal claim, disposal, financial settlement, invoice-level tracking (DOMAIN §15) |
| Source document | Return Order is the source for Sales Return (BG-004) |

### Requested Outcome

A BGud-delivered, offline-capable warehouse document that records physically
returned goods (Customer, Warehouse, Items with `BrgId`/`Qty`/`SatId` and
Return Type `BAGUS`/`RUSAK`, optional Salesman/Driver, Notes) and synchronizes
them to the Main Office, where they are grouped (Customer + Return Type +
Salesman + Driver) and consumed into Sales Return processing.

---

## 3. Current State Analysis

### 3.1 Existing Business Flow

There is **no Return Order business flow today**. The current return workflow is
fully office-centric: Office Admin records a Sales Return directly in BTR
Desktop through the existing `ReturJual` capability (menu `RT1-Retur Jual`).
There is no warehouse-side, mobile-originated receiving document, and no
pathway for a warehouse device to feed Sales Return creation.

Existing adjacent flows (evidence):

```text
Mobile-originated operational data (CheckIn / Order) — the closest analog:
  BTrade3 ── POST /api/CheckIn, POST /api/Order ──▶ Cloud
       ↓ (incremental download by j07-btrade-sync)
  BTR Desktop (BTR_CheckIn, BTR_Order)

Master data (Barang / Barcode / Customer / SalesPerson) — reference only:
  BTR Desktop ── j07-btrade-sync ──▶ Cloud ──▶ mobile caches

Sales Return (existing, downstream):
  BTR Desktop (ReturJualForm) ── BTR_ReturJual / BTR_ReturJualItem
```

Evidence: `j07-btrade-sync/Service/CheckInIncrementalDownloadService.cs`,
`OrderIncrementalDownloadService.cs`; `j05-btr-distrib/btr.domain/InventoryContext/ReturJualAgg/ReturJualModel.cs`;
`btr.distrib/SharedForm/MainForm.cs` (`RT1ReturJualButton_Click`).

### 3.2 Existing Components

| Component | System | Relevance to Return Order |
| --------- | ------ | ------------------------- |
| `ReturJual` aggregate + `ReturJualForm` + reports | `j05-btr-distrib` | Existing downstream Sales Return; Return Order must remain a *separate* entity that feeds it. Already models Customer, Warehouse, Salesman, Driver, `JenisRetur`, items, pricing, void. |
| `BTR_Driver` + `DriverModel` + `IM4-Driver` menu | `j05-btr-distrib` | Driver master exists in Main Office; referenced by `ReturJual.DriverId`. |
| `BTR_SalesPerson` / `BTR_Customer` / `BTR_Brg` / `BTR_BrgSatuan` / `BTR_Warehouse` | `j05-btr-distrib` | Reference data for Return Order attributes. |
| Barang + Barcode offline caches, barcode scan (CameraX + ML Kit), manual Item search | `src/BGud` | Directly reused for item identification (BR-009) and Unit display. |
| Offline intent queue (`barcode_registration_request_entity`) + `BarcodeSyncWorker` | `src/BGud` | Pattern to mirror for offline Return Order capture and sync. |
| Session binding (`SessionPreferencesDataSource`, `SessionBinding`, `warehouseCode`) | `src/BGud` | Resolves the Warehouse/operational-location dimension for a Return Order. |
| `BTRADE_Customer`, `BTRADE_SalesPerson`, `BTRADE_Brg`, `BTRADE_Wilayah` + `GET /api/Customer/{serverId}`, `GET /api/SalesPerson/{serverId}` | `j06-pkl-btrade-api` | Reusable reference download for BGud Customer/Salesman lookup. |
| `BTRADE_CheckIn`, `BTRADE_Order` + incremental download endpoints | `j06-pkl-btrade-api` | The mobile-originated relay pattern Return Order will follow. |
| `CheckInIncrementalDownloadService`, `OrderIncrementalDownloadService`, `MainOfficeCommandExecutor` | `j07-btrade-sync` | Reusable office-side download/relay pattern. |
| `WrhDownloadPackingOrderCmd` (Customer/Driver/Warehouse/Office denormalized) | `j06-pkl-btrade-api` | Shows Customer/Driver/Warehouse already flow to warehouse clients via Packing Order download. |

### 3.3 Existing Database

**Main Office (`j05-btr-distrib`, SSDT `btr.sql`):**

- Sales Return already exists: `BTR_ReturJual` / `BTR_ReturJualItem` (evidence:
  `PrincipalReturnEvidenceDal.cs` `FROM BTR_ReturJual r INNER JOIN BTR_ReturJualItem ri`),
  with `JenisRetur`, `CustomerId`, `WarehouseId`, `SalesPersonId`, `DriverId`,
  item quantity/pricing, and void columns (`ReturJualModel.cs`).
- Reference masters exist: `BTR_Customer`, `BTR_SalesPerson`, `BTR_Driver`,
  `BTR_Brg`, `BTR_BrgSatuan`, `BTR_Warehouse`.
- **No Return Order table exists.** There is no `BTR_ReturnOrder` /
  `BTR_ReturnOrderItem`.

**Cloud (`j06-pkl-btrade-api`, SSDT `btrade.sqldb`):**

- Reference read models: `BTRADE_Customer`, `BTRADE_SalesPerson`,
  `BTRADE_Brg`, `BTRADE_Wilayah`, `BTRADE_Kategori`.
- Operational relays: `BTRADE_CheckIn`, `BTRADE_Order`, `BTRADE_OrderItem`.
- Barcode Registry: `BTRADE_BrgBarcode`, `BTRADE_BarcodeRegistrationRequest`,
  `BTRADE_User`, `BTRADE_Location`.
- **No `BTRADE_Driver` projection** (Driver is not yet synced to Cloud).
- **No `BTRADE_ReturnOrder` relay table.**

**BGud (Room, `AppDatabase`):**

- `barang_entity` (Barang reference cache), `barcode_entity` (Active barcode
  cache), `barcode_registration_request_entity` (offline intent queue).
- DataStore `session_preferences` holds token / userId / `warehouseCode` /
  `officeCode` / last-sync timestamps.
- **No Customer, Salesman, Driver, or Return Order entities.**

### 3.4 Existing Integrations

- `j07-btrade-sync` → Cloud: `POST /api/Brg`, `/api/Kategori`, `/api/SalesPerson`,
  `/api/Wilayah`, `/api/Customer`, `/api/PackingOrder/bulk`, `/api/Barcode/sync`,
  `/api/User`; GET `api/Order/incremental/...`, `api/CheckIn/incremental/...`,
  `api/BarcodeRegistration/pending`.
- Cloud → mobile: `GET /api/Brg/{serverId}`, `GET /api/Customer/{serverId}`,
  `GET /api/SalesPerson/{serverId}`, `GET /api/barcodes/sync`.
- Mobile → Cloud (operational): `POST /api/Order`, `POST /api/CheckIn`,
  `POST /api/barcode-registration`.

**Critical property.** Operational data (CheckIn/Order) reaches the Main Office
by **pull** (`j07-btrade-sync` incremental download), while master data is
**full-snapshot replace-by-`ServerId`**. Return Order is operational data and
must follow the pull/incremental pattern, never the snapshot pattern.

### 3.5 Existing Security Model

BGud already authenticates against `pkl.btrade.api` (`POST api/Auth/login`) and
attaches a JWT on every request (`AuthInterceptor`), with tenant resolved
server-side from the bound operational location (ADR-007 from the Barcode
Registry work). No new security mechanism is required for Return Order; the
existing authenticated write + session-bound location model applies.

---

## 4. Impact Analysis

### 4.1 Backend Impact

| Area | Impact |
| ---- | ------ |
| BGud (`src/BGud`) | New Return Order domain entities + DAOs + repository + sync worker integration; new ViewModels/Screens. Reuse Barang/Barcode DAOs, session, and sync worker pattern. |
| Cloud domain (`btrade.domain`) | New `ReturnOrderType` / `ReturnOrderItemType` relay read/write models. |
| Cloud application (`btrade.application`) | New commands/queries: submit, incremental-download-by-`ServerId`, acknowledge. Mirror `CheckIn`/`Order`. |
| Cloud infrastructure (`btrade.infrastructure`) | New DAL(s) following `CheckInDal`/`OrderDal`. |
| `j07-btrade-sync` | New `ReturnOrderIncrementalDownloadService` + `ReturnOrderDal` + `ReturnOrderModel`/`ReturnOrderItemModel`; `SyncForm` wiring. |
| Main Office domain (`btr.domain`) | New `ReturnOrder` aggregate/entity (distinct from `ReturJual`), key interface. |
| Main Office application (`btr.application`) | New builder/writer/validator + queries; consume-into-`ReturJual` boundary (or expose to Desktop). |
| Main Office infrastructure (`btr.infrastructure`) | New Dapper DAL for `BTR_ReturnOrder`/`BTR_ReturnOrderItem`. |

### 4.2 Database Impact

| Store | Impact |
| ----- | ------ |
| Main Office (`btr.sql`) | New `BTR_ReturnOrder` + `BTR_ReturnOrderItem` tables (`ReturnOrderId` GUID PK generated by BGud, `ReturnOrderNo` assigned at import, Customer, Warehouse, Salesman?, Driver?, Notes, Status `Draft`/`Synced`/`Imported`, audit columns, `RowVer`). |
| Cloud (`btrade.sqldb`) | New `BTRADE_ReturnOrder` + `BTRADE_ReturnOrderItem` relay tables keyed with `ServerId` + idempotency key; status/acknowledgement columns. |
| BGud (Room) | New `return_order_entity` + `return_order_item_entity`; new `customer_entity`, `salesperson_entity`, `driver_entity` reference caches; migration (version bump). |

### 4.3 Frontend Impact

| Client | Impact |
| ------ | ------ |
| BGud | New Return Order List, Create, Detail, Edit, Delete, and Synchronization surfaces (Customer picker, Item line entry via scan/search, Qty/SatId/Return Type `BAGUS`/`RUSAK`); Home/navigation wiring. |
| BTR Desktop (`j05-btr-distrib`) | Return Order Import and Generate Sales Return from Return Order (grouping Customer + Return Type + Salesman + Driver; pricing via invoice history). No dedicated review/approval screen (GAP-006/ADR-RO-007). |

### 4.4 Integration Impact

| Integration | Impact |
| ----------- | ------ |
| BGud → Cloud | New `POST /api/return-order` submission (JWT; `ServerId` resolved server-side). |
| Cloud → BGud | Optional sync-back of status/import outcome (TQ-4); Customer/Salesman/Driver reference downloads. |
| Cloud → `j07-btrade-sync` | New incremental download endpoint `GET /api/ReturnOrder/incremental/...` + acknowledge. |
| `j07-btrade-sync` → Main Office | New staging of Return Orders into `BTR_ReturnOrder`. |
| Return Order → `ReturJual` | Main Office Generate Sales Return from Return Order — grouping Customer + Return Type + Salesman + Driver (DOMAIN §13; ADR-RO-007). |

### 4.5 Security Impact

Low relative to the current state. Return Order writes reuse the existing
JWT-authenticated, tenant-bound model (ADR-002/ADR-003/ADR-007). The only new
security consideration is the role gate for Warehouse Officer vs Office Admin
functions (already established by the Barcode Registry role matrix precedent).

---

## 5. Gap Analysis

| Gap ID | Type | Description |
| ------ | ---- | ----------- |
| GAP-001 | Functional | **CLOSED** — No Return Order domain, entity, or capability exists anywhere (greenfield). Resolution: introduce a new Warehouse-domain aggregate named **Return Order**. Return Order and `ReturJual` are separate business entities with separate responsibilities, lifecycles, and ownership. `ReturJual` remains the Office-domain document generated from synchronized Return Orders. |
| GAP-002 | Data | **CLOSED** — No `BTR_ReturnOrder` / `BTR_ReturnOrderItem` table in the Main Office schema; no schema for Return Order at all. Resolution: create new tables `BTR_ReturnOrder` and `BTR_ReturnOrderItem`. Existing `BTR_ReturJual` / `BTR_ReturJualDetail` are **not** modified. Return Order persistence is separated from Sales Return persistence. |
| GAP-003 | Integration | **CLOSED** — No mobile-originated relay for Return Orders. Resolution: use the existing **Mobile → Cloud → `j07-btrade-sync` → Main Office** pattern. A Cloud staging relay will be introduced; `j07-btrade-sync` remains the single synchronization authority responsible for downloading and importing Return Orders into the Main Office. Synchronization model: **Incremental Transaction Sync**. Recorded as **ADR-RO-001**. |
| GAP-004 | Data | **CLOSED** — BGud had no Customer reference cache, yet Customer is mandatory (BR-001/BR-002). Resolution: reuse existing `BTRADE_Customer` and `GET /api/Customer/{serverId}`; BGud maintains a **local Customer cache** to support offline Return Order creation. Customer remains mandatory and is selected from the local cache. No new Customer API or Customer domain model is required. |
| GAP-005 | Data | **CLOSED** — BGud had no Salesman or Driver reference cache. Resolution: Salesman and Driver remain optional; BGud may capture both fields when known, and both may be left empty and completed later by Office Admin. BGud maintains **local caches for available Salesman and Driver master data**; if Driver master data is not yet available in Cloud, a **Driver projection** shall be added. Neither field is required to create a Return Order. |
| GAP-006 | UX | **CLOSED** — No Return Order screens existed. Resolution: BGud implements Return Order List, Create, Detail, Edit, Delete, and Synchronization; Main Office implements Return Order Import and Generate Sales Return from Return Order. Dedicated Return Order review/approval screens are not required for MVP because the domain contains no approval or rejection workflow. |
| GAP-007 | Data/Definition | **CLOSED** — Unit model undefined. Resolution: Return Order reuses the existing `BTR_BrgSatuan` model. Return Order Item stores `BrgId`, `Qty`, `SatId`. BGud shall not introduce a new unit model and shall **not** normalize quantities into a small-unit representation. Unit conversion remains the responsibility of existing BTR item/unit rules. Warehouse users record quantities using the physical unit received. Recorded as **ADR-RO-003**. |
| GAP-008 | Definitional | **CLOSED** — Return Type vocabulary aligned. Resolution: Return Order reuses the existing `ReturJual.JenisRetur` values **`BAGUS`** and **`RUSAK`**; these become the canonical domain values for Return Type. The previously proposed `Good` / `Broken` values are replaced to eliminate unnecessary mapping and maintain consistency with the existing BTR domain model. Recorded as **ADR-RO-004**. |
| GAP-009 | Functional | **CLOSED** — `ReturnOrderNo` numbering undefined. Resolution: `ReturnOrderId` (**GUID**) is generated by BGud and used as the synchronization and idempotency key. `ReturnOrderNo` is assigned by Main Office during import; number format and sequencing are owned by Main Office. BGud does not generate business document numbers. This eliminates offline numbering collisions and keeps numbering authority centralized. Recorded as **ADR-RO-002**. |
| GAP-010 | Integration | **CLOSED** — Sales Return generation from Return Orders is now in scope: the **Main Office shall implement "Generate Sales Return from Return Order"** (grouping Customer + ReturnType + Salesman + Driver, per DOMAIN §13), alongside Return Order Import. Pricing continues to be determined by Office Admin using invoice history (DOMAIN §1, §4). Recorded as **ADR-RO-007**. |
| GAP-011 | Functional | **CLOSED** — Lifecycle state `Imported` has no defined propagation path. Resolution: `Imported` remains an **internal Main Office lifecycle state**. BGud displays only **`Draft`** and **`Synced`**. No propagation path from Main Office back to BGud is required. Ownership of a Return Order transfers to the Main Office after successful synchronization. Recorded as **ADR-RO-006**. |
| GAP-012 | Technical | **CLOSED** — Warehouse resolution undefined. Resolution: introduce a **centralized Warehouse Mapping** model. BGud users authenticate against a `WarehouseCode` representing the physical warehouse; `WarehouseCode` is mapped to the authoritative `BTR_Warehouse.WarehouseId` used by `ReturJual`. The mapping is maintained centrally and shall not be hardcoded in BGud. Recorded as **ADR-RO-008**. |
| GAP-013 | Data | **CLOSED** — `BTRADE_Driver` does not exist in the Cloud; Customer/SalesPerson already sync, but Driver does not. Resolution: BGud maintains local Salesman and Driver caches; **if Driver master data is not yet available in Cloud, a Driver projection shall be added** (GAP-005). |
| GAP-014 | Functional | **CLOSED** — Offline delete semantics. Resolution (confirmed by the GAP-011 ownership rule): deletion and modification are permitted on BGud only while the Return Order is `Draft` (pre-synchronization, BR-017–BR-020). Because pre-sync deletion is local-only, "delete" never needs to propagate. After successful synchronization ownership transfers to the Main Office, so the device may not modify or delete it. |

### Closed Gaps

| Gap ID | Type | Resolution | Status |
| ------ | ---- | ---------- | ------ |
| GAP-001 | Functional | Introduce a new Warehouse-domain aggregate **Return Order**. Return Order and `ReturJual` are separate business entities with separate responsibilities, lifecycles, and ownership. `ReturJual` remains the Office-domain document generated from synchronized Return Orders. | CLOSED |
| GAP-002 | Data | Create new tables `BTR_ReturnOrder` and `BTR_ReturnOrderItem`. Existing `BTR_ReturJual` / `BTR_ReturJualDetail` are not modified. Return Order persistence is separated from Sales Return persistence. | CLOSED |
| GAP-003 | Integration | Use the existing Mobile → Cloud → `j07-btrade-sync` → Main Office pattern with a Cloud staging relay. `j07-btrade-sync` remains the single synchronization authority. Synchronization model: Incremental Transaction Sync. Recorded as ADR-RO-001. | CLOSED |
| GAP-004 | Data | Reuse existing `BTRADE_Customer` and `GET /api/Customer/{serverId}`; BGud maintains a local Customer cache for offline Return Order creation. Customer remains mandatory and is selected from the local cache. No new Customer API or domain model. | CLOSED |
| GAP-005 | Data | Salesman and Driver remain optional; BGud may capture both when known, and both may be left empty and completed later by Office Admin. BGud maintains local Salesman/Driver caches; a Driver projection is added if Driver master data is not yet in Cloud. Neither field is required to create a Return Order. | CLOSED |
| GAP-006 | UX | BGud implements Return Order List, Create, Detail, Edit, Delete, and Synchronization; Main Office implements Return Order Import and Generate Sales Return from Return Order. No dedicated review/approval screens (domain has no approval/rejection workflow). | CLOSED |
| GAP-010 | Integration | Sales Return generation from Return Orders is in scope: Main Office implements "Generate Sales Return from Return Order" (grouping Customer + ReturnType + Salesman + Driver). Pricing stays with Office Admin using invoice history. Recorded as ADR-RO-007. | CLOSED |
| GAP-007 | Data/Definition | Reuse existing `BTR_BrgSatuan`. Return Order Item stores `BrgId`, `Qty`, `SatId`. No new unit model; no small-unit normalization in BGud; conversion stays with existing BTR item/unit rules. Warehouse users record the physical unit received. Recorded as ADR-RO-003. | CLOSED |
| GAP-008 | Definitional | Return Type reuses existing `ReturJual.JenisRetur` values `BAGUS` and `RUSAK` as canonical domain values; `Good`/`Broken` are replaced to eliminate mapping. Recorded as ADR-RO-004. | CLOSED |
| GAP-009 | Functional | `ReturnOrderId` (GUID) generated by BGud is the synchronization/idempotency key; `ReturnOrderNo` is assigned by Main Office during import, with format and sequencing owned by Main Office. BGud generates no business document numbers. Recorded as ADR-RO-002. | CLOSED |
| GAP-011 | Functional | `Imported` remains an internal Main Office lifecycle state; BGud displays only `Draft` and `Synced`; no propagation path back to BGud. Ownership transfers to Main Office after successful synchronization. Recorded as ADR-RO-006. | CLOSED |
| GAP-014 | Functional | Device-side delete/modify is permitted only while `Draft` (pre-sync, BR-017–BR-020); pre-sync delete is local-only and never propagates. After synchronization ownership transfers to Main Office (confirmed by GAP-011). | CLOSED |
| GAP-012 | Technical | Introduce a centralized Warehouse Mapping: `WarehouseCode` (BGud login) → authoritative `BTR_Warehouse.WarehouseId` used by `ReturJual`. Mapping maintained centrally, never hardcoded in BGud. Recorded as ADR-RO-008. | CLOSED |

> **INFO-001 (naming observation, non-blocking).** The GAP-002 decision names the
> existing Sales Return detail table `BTR_ReturJualDetail`; that table does not
> exist in the repository. The observed existing Sales Return persistence is
> `BTR_ReturJual` with `BTR_ReturJualItem` (and child tables
> `BTR_ReturJualItemDisc`, `BTR_ReturJualItemQtyHrg`) under
> `btr.sql/Tables/InventoryContext/`. The decision's intent — do not modify
> existing Sales Return persistence — is unaffected; the exact existing table
> list should be reconciled during Architecture. No business decision is created
> here.

> **INFO-002 (completion point, non-blocking).** GAP-005 allows optional
> Salesman/Driver to "be completed later by Office Admin", while GAP-006 decides
> no dedicated review/approval screen is required. The exact Main Office surface
> on which Office Admin completes Salesman/Driver before grouping/generation
> (the Return Order Import surface or the Generate Sales Return surface) is an
> Architecture/UX realization detail, not a business decision. It must be
> settled during Architecture so grouping (Customer + Return Type + Salesman +
> Driver) has the completed values available.

---

## 6. Solution Options

### GAP-003 — Return Order transport (mobile → office) (CLOSED; ADR-RO-001)

**Resolution (decided).** Return Order shall use the existing
**Mobile → Cloud → `j07-btrade-sync` → Main Office** pattern. A Cloud staging
relay (`BTRADE_ReturnOrder`) is introduced; `j07-btrade-sync` remains the single
synchronization authority responsible for downloading and importing Return
Orders into the Main Office. Synchronization model: **Incremental Transaction
Sync**. Direct Mobile → Main Office communication is prohibited.

**Decision record:** **ADR-RO-001** — *Mobile-originated Return Orders shall be
relayed through Cloud staging and imported by `j07-btrade-sync`; direct
Mobile → Main Office communication is prohibited.*

**Recommended realization (retained for context).** Cloud `BTRADE_ReturnOrder`
staging relay + `j07-btrade-sync` incremental download + explicit
acknowledgement, exactly mirroring the existing CheckIn/Order pattern
(delete-then-insert by a stable idempotency key on resubmission; `StatusSync`
relayed; office upserts by id).

- Advantages: proven pattern (`CheckInIncrementalDownloadService`,
  `OrderIncrementalDownloadService`, `MainOfficeCommandExecutor`); no new
  infrastructure; offline-safe (device queues locally, uploads when connected).
- Disadvantages: requires new Cloud tables/endpoints and a new sync service.
- Risk: LOW–MEDIUM (idempotency and acknowledgement must be implemented
  correctly, but the pattern is established).

**Rejected alternative.** Direct BGud → Main Office (prohibited by ADR-RO-001;
Main Office not publicly reachable) and snapshot-based sync (Return Order is
operational data, not master data; snapshot would erase remote records).

### GAP-004 — Customer reference data to BGud (CLOSED; BQ-5)

**Resolution (decided).** Reuse existing `BTRADE_Customer` and
`GET /api/Customer/{serverId}`. BGud maintains a local Customer cache to support
offline Return Order creation. Customer remains mandatory and is selected from
the local cache. No new Customer API or Customer domain model is required. This
resolves **BQ-5**.

### GAP-005 — Salesman/Driver reference data to BGud (CLOSED; BQ-3, GAP-013)

**Resolution (decided).** Salesman and Driver remain optional. BGud may capture
both fields when known; both may be left empty and completed later by Office
Admin. BGud maintains local caches for available Salesman and Driver master
data. If Driver master data is not yet available in Cloud, a Driver projection
shall be added. Neither field is required to create a Return Order. This
resolves **BQ-3** and **GAP-013**.

### GAP-007 — Unit model (CLOSED; ADR-RO-003)

**Resolution (decided).** Return Order reuses the existing `BTR_BrgSatuan`
model. Return Order Item stores `BrgId`, `Qty`, and `SatId`. BGud shall not
introduce a new unit model and shall not normalize quantities into a small-unit
representation; unit conversion remains the responsibility of existing BTR
item/unit rules. Warehouse users record quantities using the physical unit
received. This resolves **BQ-2** and **TQ-5**, and closes **GAP-007** (recorded
as **ADR-RO-003**).

**Options considered (retained for context).**

- **Option A (selected).** Reuse `BTR_BrgSatuan` — the domain's "Dus, Pak, Pcs"
  are the Item's own packaging units, referenced by `SatId`. No second
  unit-classification model.
- **Option B.** Free-text/arbitrary packaging unit per line. Not selected.

### GAP-008 — Return Type vocabulary (CLOSED; ADR-RO-004)

**Resolution (decided).** Return Order reuses the existing `ReturJual.JenisRetur`
values **`BAGUS`** and **`RUSAK`**. These become the canonical domain values for
Return Type. The previously proposed `Good` / `Broken` values are replaced to
eliminate unnecessary mapping and maintain consistency with the existing BTR
domain model. This resolves **BQ-6** and closes **GAP-008** (recorded as
**ADR-RO-004**).

> Follow-up (Knowledge Curator): `docs/work/return-order/RETURN-ORDER-DOMAIN.md`
> §3, §5, §9 and BR-013/BR-014 use "Good"/"Broken"; the domain artifact should be
> updated to the canonical `BAGUS`/`RUSAK` values so knowledge stays synchronized.
> This assessment does not rewrite the domain.

### GAP-009 — Return Order identity and numbering (CLOSED; ADR-RO-002)

**Resolution (decided).** `ReturnOrderId` (**GUID**) is generated by BGud and
used as the synchronization and idempotency key. `ReturnOrderNo` is assigned by
Main Office during import; number format and sequencing are owned by Main
Office. BGud does not generate business document numbers. This eliminates
offline numbering collisions and keeps numbering authority centralized. This
resolves **BQ-1** and **TQ-2**, and closes **GAP-009** (recorded as
**ADR-RO-002**).

> **INFO-003 (device visibility of `ReturnOrderNo`) — RESOLVED (GAP-011).**
> Because no propagation path from Main Office back to BGud is required, the
> device does not receive the Main Office-assigned `ReturnOrderNo`. BGud displays
> only its local state (`Draft` / `Synced`); the business document number is an
> Office-side concern.

### GAP-011 — `Imported` lifecycle visibility (CLOSED; ADR-RO-006)

**Resolution (decided).** The `Imported` state remains an internal Main Office
lifecycle state. BGud displays only **`Draft`** and **`Synced`**. No propagation
path from Main Office back to BGud is required. Ownership of a Return Order
transfers to the Main Office after successful synchronization. This resolves
**BQ-7** and **TQ-4**, and closes **GAP-011** (recorded as **ADR-RO-006**). The
ownership rule also confirms **GAP-014** (device delete/modify only while
`Draft`).

### GAP-010 — Sales Return generation boundary (CLOSED; ADR-RO-007)

**Resolution (decided).** Sales Return generation from Return Orders is in
scope. The **Main Office shall implement** Return Order Import and **Generate
Sales Return from Return Order**. Imported Return Orders are grouped using
Customer + Return Type + Salesman + Driver (DOMAIN §13). Pricing continues to be
determined by Office Admin using invoice history (DOMAIN §1, §4). Dedicated
Return Order review/approval screens are not required for MVP. This resolves
**BQ-4** and closes **GAP-010** (recorded as **ADR-RO-007**).

**Options considered (retained for context).**

- **Option A (selected).** Deliver Return Order plus a Main Office
  import/generation surface; Sales Return generation reuses the existing
  `ReturJual` capability. Downstream pricing/posting stays in existing
  `ReturJual` code. Return Order remains a source document, not a pricing
  engine.
- **Option B.** Build a standalone Return-Order→`ReturJual` engine with its own
  pricing model. Not selected; pricing/inventory/finance remain out of scope
  (DOMAIN §15).

### GAP-012 — Warehouse resolution (CLOSED; ADR-RO-008)

**Resolution (decided).** Introduce a **centralized Warehouse Mapping** model.
BGud users authenticate against a `WarehouseCode` representing the physical
warehouse; `WarehouseCode` is mapped to the authoritative
`BTR_Warehouse.WarehouseId` used by `ReturJual`. The mapping is maintained
centrally and shall not be hardcoded in BGud. This resolves **TQ-1** and closes
**GAP-012** (recorded as **ADR-RO-008**).

**Options considered (retained for context).**

- **Option A (selected).** A centralized mapping layer resolves the
  authenticated `WarehouseCode` to `BTR_Warehouse.WarehouseId`, complementing the
  existing `BTRADE_Location` (location → `ServerId`).
- **Option B.** Treat the bound `warehouseCode` as the Warehouse identity
  directly. Not selected; `ReturJual` uses authoritative
  `BTR_Warehouse.WarehouseId`.

---

## 7. Recommended Approach

At solution level (implementation-neutral):

1. **Own Return Order as a new Warehouse-domain aggregate** in the Main Office,
   distinct from `ReturJual`. It references Customer and Warehouse; carries
   optional Salesman/Driver, Notes, and a lifecycle `Draft → Synced → Imported`.
2. **BGud captures offline** Return Orders into a Room store (Return Order +
   items + a Customer reference cache), with item identification by Barcode
   Scan and Manual Item Search reusing the Barcode Registry module, and sync via
   WorkManager.
3. **Cloud acts as a staging relay** for Return Orders (never an authority),
   mirroring CheckIn/Order. Submission is idempotent; acknowledgement is
   explicit; the Cloud never authors authoritative Return Order state.
4. **`j07-btrade-sync` downloads** Return Orders incrementally into the Main
   Office, mirroring `CheckInIncrementalDownloadService`.
5. **Main Office consumes** synced Return Orders: Office Admin completes any
   missing optional Salesman/Driver within the import/generation flow, then
   generates the Sales Return from the Return Order (existing `ReturJual`),
   grouping by Customer + Return Type + Salesman + Driver and determining
   pricing from invoice history.
6. **Security** is inherited (JWT + session-bound location); no new
   authentication mechanism.

This establishes *what must change* without selecting a final architecture or
creating slices.

---

## 8. Risks

| Risk | Impact | Probability | Mitigation |
| ---- | ------ | ----------- | ---------- |
| Return Order and Sales Return (`ReturJual`) become conflated, breaking existing return behavior | High — duplicate/divergent return data | Medium | **Resolved** (GAP-001/GAP-010/ADR-RO-007): separate entities and tables; Return Order is a source document, never a pricing/posting authority (DOMAIN §1, §13); Main Office generates the Sales Return. |
| Offline `ReturnOrderNo` collisions across devices | High — duplicate document numbers | Medium | **Resolved** (GAP-009/ADR-RO-002): BGud generates no business number; `ReturnOrderId` (GUID) is the idempotency key and `ReturnOrderNo` is assigned by Main Office at import. |
| Unit/quantity misinterpreted (Dus vs Pcs vs `BTR_BrgSatuan`) | High — wrong quantities reach Sales Return | Medium | **Resolved** (GAP-007/ADR-RO-003): reuse `BTR_BrgSatuan`; store `BrgId`/`Qty`/`SatId`; no small-unit normalization in BGud; conversion stays with existing BTR item/unit rules. |
| Customer/Salesman/Driver reference data missing on device | Medium — capture blocked offline | Medium | **Resolved** (GAP-004/GAP-005): local Customer cache (mandatory), local Salesman/Driver caches (optional), Driver Cloud projection added if needed. |
| Full-snapshot sync erases remote Return Orders | High — data loss | Low | Use the operational pull/incremental pattern, never snapshot replacement (GAP-003). |
| `Imported` state never reaches BGud, so device shows stale status | Low–Medium | Medium | **Resolved** (GAP-011/ADR-RO-006): `Imported` is Main Office-internal; BGud shows only `Draft`/`Synced`; no sync-back by design. |
| Warehouse/location mismatch between BGud session and `BTR_Warehouse` | Medium — Return Order lands in wrong warehouse | Medium | **Resolved** (GAP-012/ADR-RO-008): centralized Warehouse Mapping maps `WarehouseCode` → `BTR_Warehouse.WarehouseId`; mapping maintained centrally, never hardcoded in BGud. |
| Scope creep into pricing/inventory/finance | High — violates DOMAIN §15 | Medium | Enforce out-of-scope boundary; pricing stays in existing `ReturJual`. |

---

## 9. Open Questions

Questions marked **[BLOCKING]** change data shape, tenancy, schema, or
integration topology and must be resolved (and recorded as ADRs) before
implementation planning.

### Business Questions

- ~~**BQ-1 [BLOCKING]** — `ReturnOrderNo` numbering.~~ **RESOLVED (GAP-009 /
  ADR-RO-002 / APPROVED):** `ReturnOrderId` (GUID) is generated by BGud and used
  as the synchronization/idempotency key; `ReturnOrderNo` is assigned by Main
  Office during import. Number format and sequencing are owned by Main Office.
  BGud does not generate business document numbers.
- ~~**BQ-2 [BLOCKING]** — Unit model.~~ **RESOLVED (GAP-007 / ADR-RO-003 /
  APPROVED):** Return Order reuses the existing `BTR_BrgSatuan` model; Return
  Order Item stores `BrgId`, `Qty`, `SatId`. BGud shall not introduce a new unit
  model and shall not normalize quantities into a small-unit representation;
  unit conversion remains the responsibility of existing BTR item/unit rules.
  Warehouse users record quantities using the physical unit received.
- ~~**BQ-3 [BLOCKING]** — Salesman/Driver mobile scope.~~ **RESOLVED (GAP-005 /
  GAP-013 / APPROVED):** Salesman and Driver remain optional; BGud may capture
  both when known, and both may be left empty and completed later by Office
  Admin. BGud maintains local Salesman/Driver caches; a Driver projection is
  added if Driver master data is not yet in Cloud. Neither field is required to
  create a Return Order.
- ~~**BQ-4 [BLOCKING]** — Sales Return generation boundary.~~ **RESOLVED
  (GAP-006 / GAP-010 / ADR-RO-007 / APPROVED):** generating Sales Return from a
  Return Order is **in scope** and implemented in the **Main Office** (Return
  Order Import + Generate Sales Return from Return Order), grouping by Customer +
  Return Type + Salesman + Driver. Pricing continues via Office Admin using
  invoice history; pricing/inventory/finance remain in existing `ReturJual`
  (DOMAIN §15). No dedicated review/approval screens (no approval workflow).
- ~~**BQ-5** — Customer selection.~~ **RESOLVED (GAP-004 / APPROVED):** reuse
  existing `BTRADE_Customer` and `GET /api/Customer/{serverId}`; BGud maintains a
  local Customer cache for offline Return Order creation. Customer remains
  mandatory and is selected from the local cache. No new Customer API or domain
  model is required.
- ~~**BQ-6** — Return Type vocabulary.~~ **RESOLVED (GAP-008 / ADR-RO-004 /
  APPROVED):** Return Order reuses the existing `ReturJual.JenisRetur` values
  `BAGUS` and `RUSAK` as the canonical Return Type values; the proposed
  `Good`/`Broken` values are replaced. A single Return Order may mix both types.
  Follow-up: update the domain artifact vocabulary.
- ~~**BQ-7 [BLOCKING]** — `Imported` visibility.~~ **RESOLVED (GAP-011 /
  ADR-RO-006 / APPROVED):** `Imported` remains an internal Main Office lifecycle
  state; BGud displays only `Draft` and `Synced`; no propagation path back to
  BGud. Ownership transfers to the Main Office after synchronization.

### Technical Questions

- ~~**TQ-1 [BLOCKING]** — Warehouse resolution.~~ **RESOLVED (GAP-012 /
  ADR-RO-008 / APPROVED):** introduce a centralized Warehouse Mapping model.
  BGud users authenticate against a `WarehouseCode` representing the physical
  warehouse; `WarehouseCode` is mapped to the authoritative
  `BTR_Warehouse.WarehouseId` used by `ReturJual`. The mapping is maintained
  centrally and shall not be hardcoded in BGud.
- ~~**TQ-2** — Return Order identity mechanics.~~ **RESOLVED (GAP-009 /
  ADR-RO-002):** `ReturnOrderId` (GUID) generated by BGud is the
  synchronization/idempotency key; `ReturnOrderNo` is assigned by Main Office
  during import. Idempotent resubmission is keyed by `ReturnOrderId`.
- ~~**TQ-3** — Reference data download to BGud.~~ **RESOLVED (GAP-004 /
  GAP-005):** reuse `GET /api/Customer/{serverId}` and
  `GET /api/SalesPerson/{serverId}`; add a `BTRADE_Driver` projection + route if
  Driver master data is not yet available in Cloud (GAP-013). No new Customer
  API or domain model.
- ~~**TQ-4 [BLOCKING]** — Lifecycle sync.~~ **RESOLVED (GAP-011 / ADR-RO-006):**
  device status is only `Draft` / `Synced`; the Main Office lifecycle
  (`Synced` → `Imported`) is internal and is **not** propagated back to the
  device. No import outcome (nor `ReturnOrderNo`) is sync-backed to BGud.
- ~~**TQ-5** — Unit conversion.~~ **RESOLVED (GAP-007 / ADR-RO-003):** the
  persistence shape is `BrgId`, `Qty`, `SatId` on the Return Order Item; no
  small-unit normalization is performed by BGud. Unit conversion remains with
  existing BTR item/unit rules.

### Operational Questions

- ~~**OQ-1** — Sync cadence.~~ **RESOLVED (APPROVED):** Return Order
  synchronization mirrors Barcode Registry — **Login Sync** and **Manual Sync
  Now** only. No background, scheduled, or real-time synchronization will be
  implemented. Offline operation is fully supported.
- ~~**OQ-2** — Offline accumulation.~~ **RESOLVED (APPROVED):** offline volume
  is assumed to be bounded by normal device storage and warehouse operational
  practices; no special retention or batching rule is required.

---

## 10. Implementation Impact Inventory

### Backend

- `src/BGud` — Return Order entities/DAOs/repository, Customer reference cache,
  sync worker extension, ViewModels, Navigation.
- `src/j06-pkl-btrade-api/btrade.domain` — `ReturnOrderType`,
  `ReturnOrderItemType`.
- `src/j06-pkl-btrade-api/btrade.application` — submit / incremental-download /
  acknowledge commands + queries.
- `src/j06-pkl-btrade-api/btrade.infrastructure` — `ReturnOrderDal`.
- `src/j07-btrade-sync` — `ReturnOrderModel`/`ReturnOrderItemModel`,
  `ReturnOrderDal`, `ReturnOrderIncrementalDownloadService`, `SyncForm` wiring.
- `src/j05-btr-distrib/btr.domain` — `ReturnOrder` aggregate + key.
- `src/j05-btr-distrib/btr.application` — builder/writer/validator/queries;
  Return Order Import and Generate Sales Return from Return Order
  (ADR-RO-007).
- `src/j05-btr-distrib/btr.infrastructure` — `ReturnOrderDal`.
- Centralized Warehouse Mapping (GAP-012/ADR-RO-008) — a centrally maintained
  `WarehouseCode` → `BTR_Warehouse.WarehouseId` mapping consumed by the office
  processing/import path; not hardcoded in BGud.

### Database

- Main Office `btr.sql` — new `BTR_ReturnOrder`, `BTR_ReturnOrderItem`
  (`ReturnOrderId` GUID PK generated by BGud, `ReturnOrderNo` assigned at
  import, Customer, Warehouse, optional Salesman/Driver, Notes, Status, audit
  columns, `RowVer`; item stores `BrgId`, `Qty`, `SatId`); `.sqlproj`
  registration + idempotent upgrade script. (Physical storage type for the GUID
  is an Architecture detail; the Main Office also uses a ULID `VARCHAR(26)`
  convention elsewhere.)
- Centralized Warehouse Mapping — a centrally maintained mapping store
  (`WarehouseCode` → `BTR_Warehouse.WarehouseId`), alongside the existing
  `BTRADE_Location` (GAP-012/ADR-RO-008).
- Cloud `btrade.sqldb` — new `BTRADE_ReturnOrder`, `BTRADE_ReturnOrderItem`
  (keyed with `ServerId` + idempotency key; status/ack columns); new
  `BTRADE_Driver` projection if Driver master data is not yet available in
  Cloud (GAP-005/GAP-013).
- BGud Room — new `return_order_entity`, `return_order_item_entity`,
  `customer_entity`, `salesperson_entity`, `driver_entity`; version bump +
  migration.

### Frontend

- BGud — Return Order List / Create / Detail / Edit / Delete / Synchronization
  screens; Home + Navigation wiring; Customer picker; item line entry
  (scan/search); Qty / SatId / Return Type (`BAGUS`/`RUSAK`) entry.
- BTR Desktop — Return Order Import and Generate Sales Return from Return Order
  (grouping Customer + Return Type + Salesman + Driver; pricing via invoice
  history). No dedicated review/approval screen (GAP-006/ADR-RO-007).

### Integration

- BGud → Cloud: `POST /api/return-order` (JWT; `ServerId` server-resolved).
- Cloud → `j07-btrade-sync`: `GET /api/ReturnOrder/incremental/...` + acknowledge.
- `j07-btrade-sync` → Main Office: Return Order staging (upsert by id).
- Cloud → BGud reference: reuse `GET /api/Customer/{serverId}`,
  `GET /api/SalesPerson/{serverId}`; new `GET /api/Driver/{serverId}` if Driver
  master data is not yet available in Cloud (GAP-005/GAP-013).
- Return Order → `ReturJual`: Main Office Generate Sales Return from Return
  Order (grouping Customer + Return Type + Salesman + Driver; ADR-RO-007).
- Sync triggers: **Login Sync** and **Manual Sync Now** only; no background,
  scheduled, or real-time synchronization (OQ-1). Offline operation fully
  supported.

### Security

- Reuse existing JWT + session-bound location model (no new mechanism).
- Role gate: Warehouse Officer (create/update/delete/sync) vs Office Admin
  (complete Salesman/Driver, generate Sales Return).

---

## 11. Recommended ADRs

### Recorded (approved)

| ADR | Decision | Resolves |
| --- | -------- | -------- |
| ADR-RO-001 | Mobile-originated Return Orders shall be relayed through Cloud staging and imported by `j07-btrade-sync`; direct Mobile → Main Office communication is prohibited. Synchronization model: Incremental Transaction Sync. | GAP-003 (CLOSED) |
| ADR-RO-002 | `ReturnOrderId` (GUID) is generated by BGud and used as the synchronization and idempotency key. `ReturnOrderNo` is assigned by Main Office during import; number format and sequencing are owned by Main Office. BGud does not generate business document numbers. | GAP-009, BQ-1, TQ-2 (CLOSED) |
| ADR-RO-003 | Return Order reuses the existing `BTR_BrgSatuan` model. Return Order Item stores `BrgId`, `Qty`, `SatId`. No new unit model; no small-unit normalization in BGud; unit conversion remains with existing BTR item/unit rules. Warehouse users record quantities using the physical unit received. | GAP-007, BQ-2, TQ-5 (CLOSED) |
| ADR-RO-004 | Return Type reuses the existing `ReturJual.JenisRetur` values `BAGUS` and `RUSAK` as canonical domain values; the proposed `Good`/`Broken` values are replaced to eliminate unnecessary mapping and maintain consistency with the existing BTR domain model. | GAP-008, BQ-6 (CLOSED) |
| ADR-RO-005 | Salesman and Driver remain optional; BGud may capture both when known and may leave both empty for Office Admin completion. BGud maintains local Salesman/Driver caches; a Driver projection is added if Driver master data is not yet in Cloud. Neither field is required to create a Return Order. | GAP-005, GAP-013, BQ-3 (CLOSED) |
| ADR-RO-006 | `Imported` remains an internal Main Office lifecycle state; BGud displays only `Draft` and `Synced`; no propagation path from Main Office back to BGud. Ownership of a Return Order transfers to the Main Office after successful synchronization. | GAP-011 (and confirms GAP-014), BQ-7, TQ-4 (CLOSED) |
| ADR-RO-007 | Sales Return generation from a Return Order is in scope and implemented in the Main Office (Return Order Import + Generate Sales Return from Return Order), grouping by Customer + Return Type + Salesman + Driver. Pricing continues via Office Admin using invoice history; pricing/inventory/finance remain in existing `ReturJual`. No dedicated review/approval screens. | GAP-006, GAP-010, BQ-4 (CLOSED) |
| ADR-RO-008 | Introduce a centralized Warehouse Mapping model. BGud users authenticate against a `WarehouseCode` representing the physical warehouse; `WarehouseCode` is mapped to the authoritative `BTR_Warehouse.WarehouseId` used by `ReturJual`. The mapping is maintained centrally and shall not be hardcoded in BGud. | GAP-012, TQ-1 (CLOSED) |

### Required before implementation planning

None. All identified gaps (GAP-001 … GAP-014) and all blocking questions
(BQ-1 … BQ-7, TQ-1 … TQ-5) are resolved and the corresponding ADRs
(ADR-RO-001 … ADR-RO-008) are recorded above. Planning may proceed.

---

## 12. Assumptions and Dependencies

### Assumptions (explicit, not decisions)

- Return Order follows the same transport, security, and offline patterns as the
  existing Barcode Registry and CheckIn/Order features; nothing below assumes a
  new mechanism.
- "Warehouse" in the domain ultimately resolves to `BTR_Warehouse` for the
  downstream `ReturJual`, but the resolution mechanism is a TQ, not assumed.
- The existing `ReturJual` capability (Sales Return) is the intended downstream
  consumer and remains authoritative for pricing/inventory/finance.
- BGud's existing Barang/Barcode caches and scan/search surfaces are reused
  unchanged for item identification.

### Dependencies

- `BTR_Customer`, `BTR_SalesPerson`, `BTR_Driver`, `BTR_Warehouse`,
  `BTR_BrgSatuan` reference data (Main Office) and their Cloud projections.
- The Barcode Registry security/tenancy decisions (ADR-002/003/007) already
  implemented in BGud and the Cloud API.
- `j07-btrade-sync` as the authoritative synchronization client (GAP-015 of the
  Barcode Registry assessment); `j05-btr-distrib\btr.sync` excluded.
- `MainOfficeCommandExecutor` (in-process command execution) for office-side
  consumption, mirroring Barcode Registry IR-08.

---

## 13. Planning Readiness

### Status

```text
READY
```

### Blocking Issues

None. All identified gaps (**GAP-001 … GAP-014**) and all blocking questions
(**BQ-1 … BQ-7**, **TQ-1 … TQ-5**) are resolved, and the corresponding decision
records are recorded in Section 11:

- **ADR-RO-001** — transport (Mobile → Cloud → `j07-btrade-sync` → Main Office).
- **ADR-RO-002** — identity/numbering (`ReturnOrderId` GUID; `ReturnOrderNo`
  assigned at import).
- **ADR-RO-003** — unit model (`BTR_BrgSatuan`; `BrgId`/`Qty`/`SatId`).
- **ADR-RO-004** — Return Type vocabulary (`BAGUS`/`RUSAK`).
- **ADR-RO-005** — optional Salesman/Driver and reference caches.
- **ADR-RO-006** — lifecycle (`Imported` office-internal; device `Draft`/`Synced`).
- **ADR-RO-007** — Sales Return generation boundary (Main Office import/generate).
- **ADR-RO-008** — centralized Warehouse Mapping.

Non-blocking items remain for Architecture (INFO-001 table-name reconciliation
and INFO-002 Salesman/Driver completion surface); none prevent planning.
OQ-1/OQ-2 (cadence and offline volume) are resolved.

### Resolved Since Previous Revision

- **GAP-001 — CLOSED (APPROVED).** A new Warehouse-domain aggregate named
  **Return Order** shall be introduced. Return Order and `ReturJual` are separate
  business entities with separate responsibilities, lifecycles, and ownership;
  `ReturJual` remains the Office-domain document generated from synchronized
  Return Orders. GAP-001 was already recorded as low risk and was not a planning
  blocker, so its closure does not change the planning readiness status — but it
  settles the domain-separation question that GAP-010 (Sales Return generation
  boundary) and ADR-RO-007 build upon.
- **GAP-002 — CLOSED (APPROVED).** Create new Main Office tables
  `BTR_ReturnOrder` and `BTR_ReturnOrderItem`. Existing `BTR_ReturJual` /
  `BTR_ReturJualDetail` are **not** modified; Return Order persistence is
  separated from Sales Return persistence. This settles the persistence question
  and confirms the DB impact inventory; the exact column shape remains subject
  to the open ADRs (numbering, unit, lifecycle).
- **GAP-003 — CLOSED (APPROVED; ADR-RO-001).** Return Order uses the existing
  **Mobile → Cloud → `j07-btrade-sync` → Main Office** pattern with a Cloud
  staging relay. `j07-btrade-sync` remains the single synchronization authority;
  the synchronization model is **Incremental Transaction Sync**; direct
  Mobile → Main Office communication is prohibited. This removes the transport
  uncertainty from the integration impact inventory. The remaining
  acknowledgement/status detail is carried by the open lifecycle question (BQ-7,
  ADR-RO-006).
- **GAP-004 — CLOSED (APPROVED).** Reuse existing `BTRADE_Customer` and
  `GET /api/Customer/{serverId}`; BGud maintains a local Customer cache for
  offline Return Order creation. Customer remains mandatory and is selected from
  the local cache. No new Customer API or domain model is required. This
  resolves **BQ-5** and settles the mandatory-attribute reference path.
- **GAP-005 — CLOSED (APPROVED; ADR-RO-005).** Salesman and Driver remain
  optional; BGud may capture both when known, and both may be left empty and
  completed later by Office Admin. BGud maintains local Salesman/Driver caches;
  a Driver projection is added if Driver master data is not yet in Cloud.
  Neither field is required to create a Return Order. This resolves **BQ-3** and
  removes the optional Salesman/Driver scope ambiguity from the mobile and
  Desktop impact inventory.
- **GAP-013 — CLOSED (APPROVED).** Driver has no Cloud projection today;
  `BTRADE_Driver` (or equivalent) shall be added if Driver master data is not
  yet available in Cloud, so BGud can hold a local Driver cache (resolved with
  GAP-005).
- **GAP-006 — CLOSED (APPROVED).** BGud implements Return Order List, Create,
  Detail, Edit, Delete, and Synchronization; the Main Office implements Return
  Order Import and Generate Sales Return from Return Order. Dedicated
  review/approval screens are not required for MVP (the domain has no
  approval/rejection workflow). This settles the full screen inventory for both
  clients.
- **GAP-010 — CLOSED (APPROVED; ADR-RO-007), resolving BQ-4.** Generating Sales
  Return from a Return Order is in scope and implemented in the Main Office
  (import + generation), grouped by Customer + Return Type + Salesman + Driver;
  pricing remains with Office Admin using invoice history. The remaining
  scope question is removed from the planning blocker list.
- **GAP-007 — CLOSED (APPROVED; ADR-RO-003), resolving BQ-2 and TQ-5.** Return
  Order reuses the existing `BTR_BrgSatuan` model; Return Order Item stores
  `BrgId`, `Qty`, `SatId`. BGud introduces no new unit model and performs no
  small-unit normalization; unit conversion remains with existing BTR item/unit
  rules, and warehouse users record the physical unit received. This removes the
  unit-model blocker from the item-line data shape.
- **GAP-008 — CLOSED (APPROVED; ADR-RO-004), resolving BQ-6.** Return Type
  reuses the existing `ReturJual.JenisRetur` values `BAGUS` and `RUSAK` as the
  canonical domain values; `Good`/`Broken` are replaced to eliminate mapping.
  Follow-up: update the domain artifact vocabulary (`RETURN-ORDER-DOMAIN.md`
  §3/§5/§9, BR-013/BR-014) so knowledge stays synchronized.
- **GAP-009 — CLOSED (APPROVED; ADR-RO-002), resolving BQ-1 and TQ-2.**
  `ReturnOrderId` (GUID) is generated by BGud and used as the synchronization and
  idempotency key; `ReturnOrderNo` is assigned by Main Office during import,
  with format and sequencing owned by Main Office. BGud generates no business
  document numbers. This removes the numbering-collision risk and the last
  blocking business question.
- **GAP-011 — CLOSED (APPROVED; ADR-RO-006), resolving BQ-7 and TQ-4.**
  `Imported` remains an internal Main Office lifecycle state; BGud displays only
  `Draft`/`Synced`; no propagation path back to BGud is required. Ownership
  transfers to the Main Office after successful synchronization.
- **GAP-014 — CLOSED (confirmed by GAP-011).** Device modify/delete is permitted
  only while `Draft` (pre-sync, BR-017–BR-020); pre-sync delete is local-only and
  never propagates. This removes device-authority ambiguity after synchronization.
- **GAP-012 — CLOSED (APPROVED; ADR-RO-008), resolving TQ-1.** Introduce a
  centralized Warehouse Mapping: BGud authenticates a `WarehouseCode` mapped to
  the authoritative `BTR_Warehouse.WarehouseId`; the mapping is maintained
  centrally and never hardcoded in BGud. This removes the last planning blocker;
  Planning Readiness is now **READY**.
- **OQ-1 / OQ-2 — RESOLVED (APPROVED).** Return Order synchronization mirrors
  Barcode Registry: Login Sync and Manual Sync Now only; no background,
  scheduled, or real-time synchronization. Offline operation is fully supported,
  and offline volume is assumed bounded by normal device storage and warehouse
  operational practices. No ADR required (non-blocking operational clarification).

### Planner Guidance

- **Scope is well-bounded.** Return Order is operational data capture + sync +
  Main Office Import + Generate Sales Return from Return Order. Pricing,
  inventory, and finance stay in existing `ReturJual` (DOMAIN §15).
- **Major dependencies:** Customer/Barang/Barcode reference data and existing
  sync; the Barcode Registry security model; `j07-btrade-sync`; the existing
  `ReturJual` aggregate.
- **Sequencing concerns (not phases):** Customer/Salesman/Driver reference
  caches precede Return Order capture; the centralized Warehouse Mapping
  precedes office import; the Cloud relay precedes the `j07-btrade-sync`
  download; the Main Office Return Order table precedes Generate Sales Return
  from Return Order.
- **Review concerns:** prove Return Order never posts inventory/valuation;
  prove it stays a distinct entity from `ReturJual`; prove quantities retain the
  recorded `SatId` unit (no small-unit normalization in BGud); prove idempotent
  resubmission keyed by `ReturnOrderId` and explicit acknowledgement; prove no
  snapshot replacement is applied to Return Orders; prove `ServerId` is never
  client-supplied; prove the warehouse mapping is not hardcoded in BGud.

Do not create implementation phases or slices in this assessment.

---

## Appendix A — Evidence Index

| Claim | Evidence |
| ----- | -------- |
| Sales Return already exists in Main Office | `src/j05-btr-distrib/btr.domain/InventoryContext/ReturJualAgg/ReturJualModel.cs`, `ReturJualItemModel.cs`; `btr.distrib/SharedForm/MainForm.cs` (`RT1ReturJualButton_Click`) |
| `JenisRetur` values BAGUS/RUSAK | `docs/features/btr-portal/btr-portal-kpi-catalog.md` (PRN-RET-001/002 formulas); `ReturJualModel.cs` |
| Driver master exists in Main Office | `btr.domain/InventoryContext/DriverAgg/DriverModel.cs`, `IDriverKey.cs`; `MainForm.cs` (`IM4DriverButton_Click`) |
| Customer/SalesPerson synced to Cloud | `j06-pkl-btrade-api/btrade.sqldb/SalesContext/BTRADE_Customer.sql`, `BTRADE_SalesPerson.sql`; `btrade.webapi/Controllers/CustomerController.cs`, `SalesPersonController.cs` |
| No `BTRADE_Driver` projection | `j06-pkl-btrade-api/btrade.sqldb` table inventory (no Driver table) |
| Mobile-originated relay pattern (CheckIn/Order) | `j07-btrade-sync/Service/CheckInIncrementalDownloadService.cs`, `OrderIncrementalDownloadService.cs`; `j06-pkl-btrade-api/btrade.sqldb/LocationContext/BTRADE_CheckIn.sql` |
| BGud offline cache + sync pattern | `src/BGud/app/src/main/java/com/elsasa/bgud/database/AppDatabase.kt`, `repository/BarcodeSyncRepository.kt`, `sync/BarcodeSyncWorker.kt` |
| BGud Barang/Barcode scan + search | `src/BGud/.../dao/BarangDao.kt`, `BarcodeDao.kt`, `ui/component/BarcodeScannerView.kt`, `ui/screen/ScanScreen.kt` |
| Session binding / warehouseCode | `src/BGud/.../datastore/SessionPreferencesDataSource.kt`, `SessionBinding.kt` |
| Operational location → tenant mapping (precedent for centralized mapping) | `j06-pkl-btrade-api/btrade.sqldb/BarcodeContext/BTRADE_Location.sql` (location → `ServerId`) |
| Warehouse mobility direction (BGud) | `docs/work/barcode-registry/BARCODE-REGISTRY-ARCHITECTURE.md` §4.4; `docs/features/stock-opname/mobile-stok-opname-domain.md` |
| Master-data snapshot vs operational pull distinction | `docs/work/barcode-registry/BARCODE-REGISTRY-FEASIBILITY-ASSESMENT.md` §3.4, GAP-016/BR-014 |
| Feasibility skill structure | `docs/skills/feasibility-creation-skill.md` |
