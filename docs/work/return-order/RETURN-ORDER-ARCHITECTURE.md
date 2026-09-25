# ARCHITECTURE

# Return Order

| Field | Value |
| ----- | ----- |
| Deliverable | `RETURN-ORDER-ARCHITECTURE.md` |
| Location | `docs/work/return-order/RETURN-ORDER-ARCHITECTURE.md` |
| Method | `docs/skills/architecture-creation-skill.md` |
| Systems | `j05-btr-distrib` (BTR Desktop / Main Office), `j06-pkl-btrade-api` (Cloud API), `j07-btrade-sync` (synchronization), `BGud` (Android warehouse app) |
| Author role | Architect (Architecture only) |
| Status | **DRAFT — see §24** |

> This document is a realization artifact. It translates the approved business
> decisions into a technical blueprint. It does not define business rules,
> implementation plans, implementation slices, migrations, deployment
> procedures, or testing procedures.

---

## 1. Executive Summary

### Purpose

Realize the Return Order business capability: a warehouse operational document,
recorded offline by Warehouse Officers in the BGud Android application, that
captures goods physically returned by Customers, and that is relayed through
Cloud staging and `j07-btrade-sync` into the Main Office, where it becomes the
authoritative source document for Sales Return generation in BTR Desktop.

The architecture realizes four concurrent realizations:

1. **Authoritative aggregate** on the Main Office Server (`j05-btr-distrib`) —
   a new `ReturnOrder` aggregate and tables, distinct from the existing
   `ReturJual` (Sales Return), holding `Draft → Synced → Imported` lifecycle
   semantics and owning numbering and Warehouse resolution.
2. **Cloud staging relay** in `j06-pkl-btrade-api` — a `ReturnOrder` relay
   (submit + incremental download + acknowledgement) that mirrors the existing
   CheckIn/Order mobile-originated pattern and is never an authority.
3. **Synchronization** in `j07-btrade-sync` — an incremental download service
   that stages Return Orders into the Main Office and invokes the office import
   command in-process.
4. **BGud capture** (primary target) — offline create/update/delete/search/
   synchronize on the existing Room + Retrofit + WorkManager stack, reusing
   Barang/Barcode item identification (Barcode Scan + Manual Item Search) and
   Customer/SalesPerson/Driver reference caches.

Return Order records physical goods movement only. Inventory valuation,
financial processing, customer balance adjustment, and accounting remain out of
scope (DOMAIN §1, §15); pricing, inventory, and finance stay in the existing
`ReturJual` capability (ADR-RO-007).

### Status

```text
DRAFT
```

### Inputs

| Input | Artifact |
| ----- | -------- |
| DOMAIN | `docs/work/return-order/RETURN-ORDER-DOMAIN.md` |
| WORKFLOW | `docs/work/return-order/RETURN-ORDER-DOMAIN.md` §14 (High-Level Workflow) and §5 (BC-001…BC-005) — no separate `RETURN-ORDER-WORKFLOW.md` exists; see §24 |
| GAP-ANALYSIS | `docs/work/return-order/RETURN-ORDER-FEASIBILITY-ASSESSMENT.md` §5 (GAP-001…GAP-014, all CLOSED), §9 (BQ-1…BQ-7, TQ-1…TQ-5, OQ-1/OQ-2, all RESOLVED) |
| ADRs | `ADR-RO-001`…`ADR-RO-008` (feasibility §11) |
| UX | Office "Generate Return Order" workflow under the `Retur Penjualan` menu (see §3.1 IR-RO-05) |
| Supporting evidence | `src/j05-btr-distrib`, `src/j06-pkl-btrade-api`, `src/j07-btrade-sync`, `src/BGud` |

### Architecture Stance

Architecture consumes the decisions above. It does **not** reinterpret them.
Where an input artifact leaves a *technical* realization open, this document
selects one realization and records the evidence used (see §3.1 Interpretation
Register) so reviewers can verify objectively. Non-blocking realization items
recorded by the feasibility assessment as INFO-001/INFO-002 and C-1…C-6 are
settled here as technical selections, never as new business decisions.

---

## 2. Architecture Principles

### P-01 — Single Authority

The Main Office Server database (`btr.sql`) is the only place where a Return
Order becomes authoritative. The Cloud and BGud stage or capture data but never
author or mutate authoritative Return Order state.

*Traceable to:* ADR-RO-001; GAP-001; DOMAIN §13.

### P-02 — Cloud Is a Relay, Never an Authority

The Cloud stores Return Orders for transport only (`StatusSync` `TERKIRIM` →
`DOWNLOADED`). It performs no business validation and authors no authoritative
state.

*Traceable to:* ADR-RO-001; GAP-003.

### P-03 — Operational Pull, Never Snapshot

Return Order is operational data. It reaches the Main Office by incremental
**pull** (`j07-btrade-sync` download), never by full-snapshot replacement.
Snapshot/replace semantics are never applied to Return Orders.

*Traceable to:* GAP-003; feasibility §3.4 "Critical property".

### P-04 — One Transaction Per Authoritative Change

An import or a Sales Return generation is committed in a single Main Office
transaction (`TransHelper.NewScope()`), consistent with `ReturJualWriter.Save`.

*Evidence:* `btr.application/InventoryContext/ReturJualAgg/Workers/ReturJualWriter.cs`.

### P-05 — Mirror the `ReturJual` Layering

The Return Order aggregate is layered exactly like the `ReturJual` aggregate:
domain model + key interface → application builder/writer/DAL contract/queries →
infrastructure Dapper DAL → WinForms form.

*Traceable to:* GAP-001; feasibility §4.1.

### P-06 — No Client-Supplied Tenant

Return Order commands and endpoints never accept `ServerId` from a caller.
Tenant is resolved server-side from the authenticated session (ADR-007
precedent from the Barcode Registry).

*Traceable to:* feasibility §4.4, §4.5; §9.

### P-07 — BGud Is an Offline Capture Device

BGud writes Return Orders locally first; the Cloud is a synchronization target,
not a runtime dependency. No capture step requires a network call.

*Traceable to:* DOMAIN BG-003; GAP-003.

### P-08 — Identity and Numbering Are Separate

`ReturnOrderId` is a **ULID** generated by BGud and is the globally unique
technical identity and synchronization/idempotency key. `ReturnOrderNo`
is a separate, human-readable business document number assigned by the Main
Office at import. The two serve different purposes and are never merged; BGud
generates no business document numbers.

*Traceable to:* ADR-RO-002; GAP-009.

### P-09 — Unit Fidelity Is Preserved

Return Order Item stores `BrgId`, `Qty`, `SatId`. BGud performs no small-unit
normalization; the recorded physical unit is preserved end-to-end. Unit
conversion remains the responsibility of existing BTR item/unit rules.

*Traceable to:* ADR-RO-003; GAP-007.

### P-10 — Ownership Transfers After Synchronization

After successful synchronization, ownership transfers to the Main Office. The
device may not modify or delete a `Synced` Return Order, and no import outcome
or `ReturnOrderNo` is propagated back to the device.

*Traceable to:* ADR-RO-006; GAP-011; GAP-014.

### P-11 — Warehouse Resolution Is Centralized

A centralized Warehouse Mapping reference table maps a `WarehouseCode` (the
physical warehouse selected at BGud login) to its `ServerId` (tenant/office).
It is populated by database seed/migration scripts only — no Warehouse
maintenance UI exists. BGud never hardcodes the mapping and never guesses it.

*Traceable to:* ADR-RO-008; GAP-012.

### P-12 — Minimal New Surface

No new synchronization infrastructure, no new unit model, no new Customer/Sales
Person domain model, no new pricing/inventory/finance capability, no migration
framework.

*Traceable to:* GAP-004; GAP-007; DOMAIN §15; ADR-RO-003.

### P-13 — Return Order Is a Source Document Only

Return Order exists only as the source for Sales Return generation. There is no
standalone Return Order review, approval, rejection, or management workflow and
no such states. The single Office surface is "Generate Sales Return from Return
Order" (menu `Retur Penjualan → Generate Return Order`).

*Traceable to:* GAP-006; ADR-RO-007; DOMAIN §13, §15.

---

## 3. Decision Traceability

Decisions consumed from the feasibility assessment and ADRs.

| Source | Decision | Realized in |
| ------ | -------- | ----------- |
| GAP-001 | Return Order is a new Warehouse-domain aggregate, distinct from `ReturJual` | §4.1, §5.1 |
| GAP-002 | New `BTR_ReturnOrder` / `BTR_ReturnOrderItem`; `ReturJual` persistence untouched | §6.1 |
| GAP-003 | Mobile → Cloud → `j07-btrade-sync` → Main Office; incremental transaction sync | §8.1, §8.2 |
| GAP-004 | Reuse `BTRADE_Customer` + `GET /api/Customer/{serverId}`; BGud local Customer cache | §6.4, §8.3 |
| GAP-005 | Salesman/Driver optional; local caches; Driver projection if absent | §6.2, §6.3, §8.3 |
| GAP-006 | BGud List/Create/Detail/Edit/Delete/Sync; Office single "Generate Return Order" surface; no review/approval screens | §11, §12 |
| GAP-007 | Reuse `BTR_BrgSatuan`; item stores `BrgId`/`Qty`/`SatId`; no normalization | §5.1, §6.1, §6.4 |
| GAP-008 | Return Type = `JenisRetur` `BAGUS`/`RUSAK` | §5.1 VO-01, §6.1, §6.4 |
| GAP-009 | `ReturnOrderId` (ULID) by BGud; `ReturnOrderNo` by Main Office at import | §5.1, §6.1, §8.2 |
| GAP-010 | Main Office Generate Sales Return from Return Order (grouping) | §7.1, §8.5 |
| GAP-011 | `Imported` office-internal; device shows `Draft`/`Synced` only; no sync-back | §5.1, §14, §18 |
| GAP-012 | Centralized Warehouse Mapping (`WarehouseCode` → `ServerId`), seed/migration only, no maintenance UI | §6.3, §9.2, §10.1 |
| GAP-013 | `BTRADE_Driver` projection added | §6.3, §8.3, §8.4 |
| GAP-014 | Device modify/delete only while `Draft`; pre-sync delete local-only | §5.1, §14, §16 |
| ADR-RO-001 | Relay through Cloud staging; direct Mobile→Main Office prohibited; incremental sync | §4.2, §8.1, §8.2 |
| ADR-RO-002 | `ReturnOrderId` (ULID) sync/idempotency key; `ReturnOrderNo` (business number) at import; never merged | §5.1, §6.1, §8.2, §10.6 |
| ADR-RO-003 | Reuse `BTR_BrgSatuan`; `BrgId`/`Qty`/`SatId`; no normalization | §5.1, §6.1 |
| ADR-RO-004 | Return Type = `BAGUS`/`RUSAK` (replaces Good/Broken) | §5.1, §6.1 |
| ADR-RO-005 | Salesman/Driver optional; local caches; Driver projection if absent | §5.1, §6.3, §6.4 |
| ADR-RO-006 | `Imported` office-internal; device `Draft`/`Synced`; ownership transfers | §5.1, §14, §16 |
| ADR-RO-007 | Generate Sales Return in Main Office; grouping Customer+ReturnType+Salesman+Driver; pricing in `ReturJual` | §7.1, §8.5 |
| ADR-RO-008 | Centralized Warehouse Mapping (`WarehouseCode` → `ServerId`); seed-only; not hardcoded in BGud | §6.3, §9.2, §10.1 |

### 3.1 Interpretation Register

Technical realizations that the input artifacts left open. Each is a *technical*
selection, not a business decision. Each is recorded here so a reviewer can
accept or reject it without re-deriving the architecture.

| ID | Open point left by inputs | Selected realization | Evidence / traceability |
| -- | ------------------------- | -------------------- | ----------------------- |
| IR-RO-01 | Feasibility §4.2/C-3: physical storage type for the BGud-generated `ReturnOrderId` (ADR-RO-002 says "GUID"; the NunaId proposal was revoked) | `ReturnOrderId` is a **ULID** — a 26-character Crockford Base32 string generated by BGud via a Kotlin ULID implementation matching the platform's `Ulid.NewUlid().ToString()` format — stored as `VARCHAR(26)` identically across Main Office, Cloud, and Room. This is the architect's selected realization, consistent with the platform's established ULID-keyed technical-id convention. No `UNIQUEIDENTIFIER` and no GUID-to-ULID conversion is introduced. | `Ulid` package (`btr.domain.csproj`, `btr.application.csproj`, `btrade.domain.csproj`); `BrgBarcodeWriter` (`Ulid.NewUlid().ToString()`); `BTR_VisitPlanException` (`VARCHAR(26)`) |
| IR-RO-02 | Column naming for the per-item Return Type (domain "Return Type" vs existing `ReturJual.JenisRetur`) | The item column is named `JenisRetur` (`VARCHAR(5)`, values `BAGUS`/`RUSAK`) to match the existing `ReturJual.JenisRetur` and make the Generate-Sales-Return mapping direct. | ADR-RO-004 ("reuse `ReturJual.JenisRetur` values"); `BTR_ReturJual.JenisRetur VARCHAR(5)` |
| IR-RO-03 | Item unit and quantity physical shape (ADR-RO-003 mandates `BrgId`/`Qty`/`SatId`; no type given) | `SatId` references `BTR_BrgSatuan.Satuan` (`VARCHAR(7)`, matching the barcode registry `Satuan` column); `Qty` is `DECIMAL(18,2)` so the recorded physical-unit quantity is preserved without small-unit normalization. | ADR-RO-003; `BTR_BrgSatuan` (PK `(BrgId, Satuan)`) |
| IR-RO-04 | Where the centralized Warehouse Mapping lives and what it maps (GAP-012) | A Cloud-side table `BTR_WarehouseMapping` (`WarehouseCode` → `ServerId`) in `btrade.sqldb`, mirroring the existing `BTRADE_Location` (location → `ServerId`) and seeded with the same initial rows. It is authoritative for warehouse→tenant resolution, consumed by the login flow (Cloud token issuance + BGud session), and maintained by seed/migration scripts only — no maintenance UI. | ADR-RO-008; `BTRADE_Location` precedent (feasibility §3.5) |
| IR-RO-05 | INFO-002/C-2: import mechanics and the Salesman/Driver completion surface | Import is an **in-process** Main Office command invoked by `j07-btrade-sync` via `MainOfficeCommandExecutor` (feasibility §12). The single Desktop surface **"Generate Return Order"** (menu `Retur Penjualan → Generate Return Order`) is where Office Admin views synchronized (`Synced`, not yet `Imported`) orders, completes optional Salesman/Driver inline, selects one or more orders, and executes Generate — so the grouping key (Customer + Return Type + Salesman + Driver) is complete at generation time. There is no separate review/approval/import screen. | INFO-002; feasibility §12; ADR-RO-007 |
| IR-RO-06 | GAP-013/C-5: `BTRADE_Driver` projection population path | A `j07-btrade-sync` master-data uploader (`DriverSyncService`) publishes `BTR_Driver` → `BTRADE_Driver` (upsert by `DriverId` + `ServerId`), mirroring `SalesPersonSyncService` / `CustomerUploadService`. | GAP-013; `j07-btrade-sync/Service/SalesPersonSyncService.cs` |
| IR-RO-07 | C-4: `ReturnOrderNo` generation mechanism | `ReturnOrderNo` is generated by `INunaCounterBL` following the `ReturJualWriter` convention. The concrete prefix/format is a data/configuration decision confirmed at implementation (carried as C-1 in §24); the mechanism is fixed now. | ADR-RO-002; `ReturJualWriter` (`_counter.Generate(...)`, `GenerateReturJualCode`) |
| IR-RO-08 | Cloud relay status vocabulary | Mirror CheckIn/Order: `StatusSync` transitions `TERKIRIM` → `DOWNLOADED` (download-coupled acknowledgement). | `OrderUploadCommand` (`"TERKIRIM"`); `OrderIncrementalDownloadQueryHandler` (`"DOWNLOADED"`) |
| IR-RO-09 | Office lifecycle vocabulary (domain lists `Draft/Synced/Imported`; only synced orders are relayed) | The office `Status` column holds `Synced` and `Imported` only; `Draft` is a device-only state and is never staged. Device status is `Draft`/`Synced` (ADR-RO-006). | ADR-RO-006; GAP-011; GAP-014 |
| IR-RO-10 | Return Order Item identity | Composite `(ReturnOrderId, NoUrut)`, mirroring `ReturJualItemId = {ReturJualId}-{NoUrut:D2}` and the Order relay's `OrderItemType.NoUrut`. No separate surrogate item id is introduced. | `BTR_ReturJualItem.ReturJualItemId`; `OrderItemType.NoUrut` |
| IR-RO-11 | Referential-integrity convention | New Return Order tables declare **no** foreign keys, following the existing BTR convention (feasibility §3.3); integrity is enforced at the application layer. This avoids the FK-deployment risk of the barcode registry and matches `BTR_ReturJual`. | `BTR_ReturJual.sql` (no FK); feasibility §3.3 |

---

# PART A — BACKEND ARCHITECTURE

---

## 4. Bounded Context Realization

### 4.1 Return Order (Main Office) — Authoritative Context

System: `j05-btr-distrib` (`btr.domain`, `btr.application`, `btr.infrastructure`, `btr.distrib`, `btr.sql`)

#### Responsibilities

* Own the authoritative Return Order aggregate and its lifecycle
  (`Synced → Imported` office-side; `Draft` is device-only).
* Own numbering: assign `ReturnOrderNo` at import (ADR-RO-002).
* Persist the `WarehouseCode` captured under the selected Warehouse context
  (ADR-RO-008).
* Accept synced Return Orders relayed by `j07-btrade-sync` and import them
  idempotently.
* Own "Generate Sales Return from Return Order" (grouping Customer + Return
  Type + Salesman + Driver), delegating to the existing `ReturJual` capability
  (ADR-RO-007).
* Expose queries and the single Desktop "Generate Return Order" surface
  (ADR-RO-007).

#### Owns

* `ReturnOrder` aggregate (§5.1).
* `BTR_ReturnOrder`, `BTR_ReturnOrderItem` tables (§6.1).

#### Depends On

* Customer (`BTR_Customer`) — external reference by `CustomerId` only.
* Warehouse (`BTR_Warehouse`) — external reference by `WarehouseId` only.
* Sales Person (`BTR_SalesPerson`), Driver (`BTR_Driver`) — optional references.
* Item (`BTR_Brg`) and Item units (`BTR_BrgSatuan`) — external references by
  `BrgId` and `(BrgId, Satuan)` only.
* Sales Return (`ReturJual` aggregate) for the Generate boundary.

#### Exposes

* Application commands and queries (§7.1) to `btr.distrib` and, in-process, to
  `j07-btrade-sync` (`MainOfficeCommandExecutor`).

#### Explicitly Does Not

* Accept `ServerId` (no tenancy concept exists in the Main Office database).
* Perform pricing, inventory update, valuation, customer balance adjustment,
  or accounting (DOMAIN §15).
* Provide a Return Order review, approval, or rejection workflow (GAP-006,
  ADR-RO-007, P-13).

---

### 4.2 Return Order Relay (Cloud) — Transport Only

System: `j06-pkl-btrade-api` (`btrade.domain`, `btrade.application`, `btrade.infrastructure`, `btrade.webapi`, `btrade.sqldb`)

#### Responsibilities

* Stage mobile-originated Return Orders idempotently (`StatusSync = TERKIRIM`).
* Serve incremental download to `j07-btrade-sync`, flipping `TERKIRIM` →
  `DOWNLOADED` in the same transaction (acknowledgement).
* Publish the `BTRADE_Driver` reference projection for BGud lookup.
* Resolve the Warehouse Mapping (`WarehouseCode` → `ServerId`) at login so the
  session carries both values.
* Enforce authentication and tenant binding at the API boundary.

#### Owns

* `ReturnOrderType` / `ReturnOrderItemType` relay models (§5.2).
* `BTRADE_ReturnOrder`, `BTRADE_ReturnOrderItem`, `BTRADE_Driver`,
  `BTR_WarehouseMapping` tables (§6.2, §6.3).

#### Depends On

* Main Office, via `j07-btrade-sync`, for all authoritative content.
* Platform JWT configuration.

#### Exposes

* Cloud HTTP contract (§8.3, §8.4).

#### Explicitly Does Not

* Validate Return Order business rules.
* Author or mutate authoritative Return Order state.
* Accept `ServerId` from a caller on the write path (it is resolved from the
  JWT; the incremental download route carries `serverId` in the path by
  convention, mirroring CheckIn/Order).

---

### 4.3 Return Order Synchronization

System: `j07-btrade-sync` (authoritative sync client; `j05-btr-distrib\btr.sync` excluded)

#### Responsibilities

* Download Return Orders incrementally from the Cloud and stage them into
  `BTR_ReturnOrder` (upsert by `ReturnOrderId`).
* Invoke the office `ImportReturnOrderCommand` in-process for numbering and
  Warehouse resolution.
* Publish the `BTRADE_Driver` projection (IR-RO-06).

#### Owns

* `ReturnOrderModel`/`ReturnOrderItemType` transport models, `ReturnOrderDal`/
  `ReturnOrderItemDal` (staging), `ReturnOrderIncrementalDownloadService`,
  `DriverSyncService`.

#### Depends On

* Main Office database (staging) and `btr.application` (in-process import).
* Cloud API (download + Driver upload).

#### Exposes

* Nothing to other business contexts. Operated by Office Admin.

---

### 4.4 Warehouse Mobility (BGud) — Offline Capture Client

System: `BGud` (Android application)

#### Responsibilities

* Capture Return Orders offline into Room (create/update/delete/search).
* Resolve items by Barcode Scan or Manual Item Search from the local
  Barang/Barcode caches.
* Select the mandatory Customer from a local Customer cache; capture optional
  Salesman/Driver.
* Synchronize (`Draft` → `Synced`) via WorkManager on Login Sync and Manual
  Sync Now.

#### Owns

* Room database (§6.4), DataStore session state, local Return Order queue.

#### Depends On

* Cloud API for reference downloads (Customer/SalesPerson/Driver) and Return
  Order submission.

#### Explicitly Does Not

* Decide numbering, warehouse authority, or item/unit authority.
* Hold `ServerId` as an input to any command.
* Act as an authority for any data.

---

## 5. Domain Model Realization

### 5.1 Aggregate: `ReturnOrder` (Main Office, Aggregate Root)

Location: `btr.domain/InventoryContext/ReturnOrderAgg/`

```text
ReturnOrderModel : IReturnOrderKey
    ReturnOrderId     string      // ULID, VARCHAR(26) (IR-RO-01)
    ReturnOrderNo     string      // assigned at import (IR-RO-07)
    ReturnOrderDate   DateTime    // captured receiving date
    WarehouseCode     string      // selected Warehouse context (Warehouse Mapping key)
    CustomerId        string      // external reference → BTR_Customer
    SalesPersonId     string      // optional
    DriverId          string      // optional
    Note              string
    Status            string      // Synced | Imported (IR-RO-09)
    CreatedBy         string
    CreatedDate       DateTime
    ModifiedBy        string
    ModifiedDate      DateTime

    ListItem : List<ReturnOrderItemModel>
```

Key interface: `IReturnOrderKey { string ReturnOrderId { get; } }` — mirrors
`IReturJualKey`.

#### Purpose

Preserve a single warehouse receiving event (one Customer, one Warehouse, one
or more items) as the operational source for Sales Return generation.

#### Aggregate Root

`ReturnOrder`. Items are child entities of the aggregate root.

#### Child Entities

`ReturnOrderItemModel`:

```text
ReturnOrderItemModel : IReturnOrderKey, IBrgKey
    ReturnOrderId   string      // parent id (IR-RO-10)
    NoUrut          int
    BrgId           string
    BrgCode         string      // denormalized read convenience
    Qty             decimal     // physical-unit quantity, DECIMAL(18,2) (IR-RO-03)
    SatId           string      // → BTR_BrgSatuan.Satuan (IR-RO-03)
    JenisRetur      string      // BAGUS | RUSAK (IR-RO-02)
```

#### Value Objects

**VO-01 — `JenisRetur` (Return Type)**

Per-item value, vocabulary exactly `BAGUS` / `RUSAK` (ADR-RO-004). A single
Return Order may mix both types across items (DOMAIN §9). `BrgName` is resolved
at read (not persisted), mirroring `ReturJualItemModel`.

#### Invariants

* **INV-01** — `ReturnOrderId` is mandatory, is a ULID, and is the technical
  identity and idempotency key. It is distinct from `ReturnOrderNo` (the
  business document number) and is never merged with it (ADR-RO-002).
* **INV-02** — Customer is mandatory and must reference an existing
  `BTR_Customer` (BR-001, BR-002, BR-008).
* **INV-03** — Warehouse is mandatory (BR-005, BR-006). The `WarehouseCode` is
  the selected Warehouse context and must exist in the centralized
  `BTR_WarehouseMapping` (`WarehouseCode` → `ServerId`); an unmapped code fails
  explicitly (ADR-RO-008).
* **INV-04** — A Return Order contains at least one item (BR-012).
* **INV-05** — Each item references an existing `BTR_Brg` (BR-007, BR-008).
* **INV-06** — Each item's `Qty` is greater than zero (BR-010).
* **INV-07** — Each item's `SatId` references a valid `(BrgId, Satuan)` in
  `BTR_BrgSatuan` (BR-011, ADR-RO-003).
* **INV-08** — Each item's `JenisRetur` is `BAGUS` or `RUSAK` (ADR-RO-004).
* **INV-09** — Salesman and Driver are optional; both may be empty at import
  and completed later (BR-013…BR-016, ADR-RO-005).
* **INV-10** — Office `Status` transitions only `Synced → Imported` (IR-RO-09,
  ADR-RO-006). `Draft` is device-only and never staged.
* **INV-11** — Import is idempotent by `ReturnOrderId`; re-import of an
  already-imported order never duplicates or re-numbers (ADR-RO-002).
* **INV-12** — Generate Sales Return may fan out a single Return Order into
  multiple `ReturJual` documents when the grouping key (Customer + Return Type
  + Salesman + Driver) differs across items (ADR-RO-007, DOMAIN §13).
* **INV-13** — Pricing, inventory, and finance are never performed by the
  Return Order commands (DOMAIN §15).
* **INV-14** — `ReturJual` persistence is never modified by this feature
  (GAP-002).

#### Repository

`IReturnOrderDal` (application contract), implemented by `ReturnOrderDal`
(infrastructure, Dapper). Composition:

```text
IInsert<ReturnOrderModel>
IUpdate<ReturnOrderModel>
IGetData<ReturnOrderModel, IReturnOrderKey>
IListData<ReturnOrderModel>
```

Specialized members:

```text
ReturnOrderModel  GetByReturnOrderId(IReturnOrderKey key)
IEnumerable<ReturnOrderModel> ListByStatus(string status)
bool Exists(IReturnOrderKey key)
```

Item persistence mirrors `ReturJualItemDal` (delete-by-parent then bulk insert).

---

### 5.2 Cloud Relay Models: `ReturnOrderType` / `ReturnOrderItemType`

Location: `btrade.domain/ReturnOrderFeature/`

```text
public class ReturnOrderType : IReturnOrderKey, IServerId
    ReturnOrderId     string
    ServerId          string
    ReturnOrderDate   string      // yyyy-MM-dd (mirror OrderModel string dates)
    WarehouseCode     string
    CustomerId        string
    CustomerName      string      // denormalized for display
    SalesPersonId     string
    SalesPersonName   string
    DriverId          string
    DriverName        string
    Note              string
    StatusSync        string      // TERKIRIM | DOWNLOADED (IR-RO-08)
    ListItems         List<ReturnOrderItemType>

public record ReturnOrderItemType(
    string ReturnOrderId,
    int NoUrut,
    string BrgId,
    string BrgCode,
    string BrgName,
    decimal Qty,
    string SatId,
    string JenisRetur) : IReturnOrderKey;

public interface IReturnOrderKey { string ReturnOrderId { get; } }
```

Mirrors the `OrderModel`/`OrderItemType` shape (feasibility §4.1). These types
carry no validation logic; they are transport-only (P-02).

---

### 5.3 Cloud Reference Projection: `DriverType`

Location: `btrade.domain/DriverFeature/`

```text
public class DriverType : IDriverKey, IServerId
    DriverId     string
    DriverName   string
    IsAktif      bool
    ServerId     string

public interface IDriverKey { string DriverId { get; } }
```

Mirrors `SalesPersonType`. Published by `j07-btrade-sync` (IR-RO-06), consumed
by BGud for its optional Driver cache.

---

## 6. Persistence Model

### 6.1 Main Office — `BTR_ReturnOrder`, `BTR_ReturnOrderItem`

Project: `src/j05-btr-distrib/btr.sql`
Files: `btr.sql/Tables/InventoryContext/BTR_ReturnOrder.sql`,
`BTR_ReturnOrderItem.sql` (registered in `btr.sql.sqlproj`)

#### `BTR_ReturnOrder`

```sql
CREATE TABLE BTR_ReturnOrder(
    ReturnOrderId   VARCHAR(26)  NOT NULL CONSTRAINT DF_BTR_ReturnOrder_ReturnOrderId   DEFAULT(''),
    ReturnOrderNo   VARCHAR(20)  NOT NULL CONSTRAINT DF_BTR_ReturnOrder_ReturnOrderNo   DEFAULT(''),
    ReturnOrderDate DATETIME     NOT NULL CONSTRAINT DF_BTR_ReturnOrder_ReturnOrderDate DEFAULT('3000-01-01'),
    WarehouseCode   VARCHAR(20)  NOT NULL CONSTRAINT DF_BTR_ReturnOrder_WarehouseCode   DEFAULT(''),
    CustomerId      VARCHAR(6)   NOT NULL CONSTRAINT DF_BTR_ReturnOrder_CustomerId      DEFAULT(''),
    SalesPersonId   VARCHAR(5)   NOT NULL CONSTRAINT DF_BTR_ReturnOrder_SalesPersonId   DEFAULT(''),
    DriverId        VARCHAR(5)   NOT NULL CONSTRAINT DF_BTR_ReturnOrder_DriverId        DEFAULT(''),
    Note            VARCHAR(100) NOT NULL CONSTRAINT DF_BTR_ReturnOrder_Note            DEFAULT(''),
    Status          VARCHAR(10)  NOT NULL CONSTRAINT DF_BTR_ReturnOrder_Status          DEFAULT('Synced'),

    CreatedBy       VARCHAR(50)  NOT NULL CONSTRAINT DF_BTR_ReturnOrder_CreatedBy       DEFAULT(''),
    CreatedDate     DATETIME     NOT NULL CONSTRAINT DF_BTR_ReturnOrder_CreatedDate     DEFAULT('3000-01-01'),
    ModifiedBy      VARCHAR(50)  NOT NULL CONSTRAINT DF_BTR_ReturnOrder_ModifiedBy      DEFAULT(''),
    ModifiedDate    DATETIME     NOT NULL CONSTRAINT DF_BTR_ReturnOrder_ModifiedDate    DEFAULT('3000-01-01'),

    RowVer          ROWVERSION   NOT NULL,

    CONSTRAINT PK_BTR_ReturnOrder PRIMARY KEY CLUSTERED (ReturnOrderId)
)
GO

CREATE INDEX IX_BTR_ReturnOrder_Status
    ON BTR_ReturnOrder(Status, ReturnOrderDate)
    WITH(FILLFACTOR=75)
GO

CREATE INDEX IX_BTR_ReturnOrder_CustomerId
    ON BTR_ReturnOrder(CustomerId, ReturnOrderId)
    WITH(FILLFACTOR=75)
GO
```

#### `BTR_ReturnOrderItem`

```sql
CREATE TABLE BTR_ReturnOrderItem(
    ReturnOrderId VARCHAR(26)     NOT NULL CONSTRAINT DF_BTR_ReturnOrderItem_ReturnOrderId DEFAULT(''),
    NoUrut        INT             NOT NULL CONSTRAINT DF_BTR_ReturnOrderItem_NoUrut        DEFAULT(0),
    BrgId         VARCHAR(6)      NOT NULL CONSTRAINT DF_BTR_ReturnOrderItem_BrgId         DEFAULT(''),
    BrgCode       VARCHAR(20)     NOT NULL CONSTRAINT DF_BTR_ReturnOrderItem_BrgCode       DEFAULT(''),
    Qty           DECIMAL(18,2)   NOT NULL CONSTRAINT DF_BTR_ReturnOrderItem_Qty           DEFAULT(0),
    SatId         VARCHAR(7)      NOT NULL CONSTRAINT DF_BTR_ReturnOrderItem_SatId         DEFAULT(''),
    JenisRetur    VARCHAR(5)      NOT NULL CONSTRAINT DF_BTR_ReturnOrderItem_JenisRetur    DEFAULT(''),

    CONSTRAINT PK_BTR_ReturnOrderItem PRIMARY KEY CLUSTERED (ReturnOrderId, NoUrut)
)
GO
```

#### Primary Keys

* `BTR_ReturnOrder.ReturnOrderId` — ULID string `VARCHAR(26)` (IR-RO-01).
* `BTR_ReturnOrderItem.(ReturnOrderId, NoUrut)` — composite (IR-RO-10).

#### Foreign Keys

None (IR-RO-11). Referential integrity (Customer, Warehouse, SalesPerson,
Driver, Brg, unit) is enforced at the application layer, matching the existing
`BTR_ReturJual` convention. The `WarehouseCode` is a reference to the
centralized Warehouse Mapping (`§6.3`), not a foreign key.

#### Unique Constraints

`PK_BTR_ReturnOrder` enforces the `ReturnOrderId` uniqueness that backs
import idempotency (INV-11, ADR-RO-002).

#### Indexes

* `IX_BTR_ReturnOrder_Status` — Generate worklist (`Synced` orders).
* `IX_BTR_ReturnOrder_CustomerId` — Customer-scoped listing.

#### Soft Delete Strategy

None. Return Orders are never deleted from the Main Office. The device may
delete a `Draft` locally before synchronization (GAP-014); once synced, the
record is immutable office-side (ADR-RO-006).

#### Audit Strategy

`CreatedBy`, `CreatedDate`, `ModifiedBy`, `ModifiedDate` (DOMAIN §8) plus
`RowVer` for optimistic concurrency. `CreatedBy` holds the `BTR_User.UserId`.

---

### 6.2 Cloud Relay — `BTRADE_ReturnOrder`, `BTRADE_ReturnOrderItem`

Project: `src/j06-pkl-btrade-api/btrade.sqldb`
Files: `btrade.sqldb/ReturnOrderContext/BTRADE_ReturnOrder.sql`,
`BTRADE_ReturnOrderItem.sql` (registered in `btrade.sqldb.sqlproj`)

```sql
CREATE TABLE BTRADE_ReturnOrder(
    ReturnOrderId   VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_ReturnOrderId DEFAULT(''),
    ServerId        VARCHAR(5)   NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_ServerId      DEFAULT(''),
    ReturnOrderDate DATETIME     NOT NULL,
    WarehouseCode   VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_WarehouseCode DEFAULT(''),
    CustomerId      VARCHAR(6)   NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_CustomerId    DEFAULT(''),
    CustomerName    VARCHAR(100) NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_CustomerName  DEFAULT(''),
    SalesPersonId   VARCHAR(5)   NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_SalesPersonId DEFAULT(''),
    SalesPersonName VARCHAR(100) NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_SalesPersonName DEFAULT(''),
    DriverId        VARCHAR(5)   NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_DriverId      DEFAULT(''),
    DriverName      VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_DriverName    DEFAULT(''),
    Note            VARCHAR(100) NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_Note          DEFAULT(''),
    StatusSync      VARCHAR(10)  NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_StatusSync    DEFAULT('TERKIRIM'),

    CONSTRAINT PK_BTRADE_ReturnOrder PRIMARY KEY CLUSTERED (ReturnOrderId, ServerId)
)
GO

CREATE TABLE BTRADE_ReturnOrderItem(
    ReturnOrderId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRADE_ReturnOrderItem_ReturnOrderId DEFAULT(''),
    NoUrut        INT           NOT NULL CONSTRAINT DF_BTRADE_ReturnOrderItem_NoUrut        DEFAULT(0),
    BrgId         VARCHAR(6)    NOT NULL CONSTRAINT DF_BTRADE_ReturnOrderItem_BrgId         DEFAULT(''),
    BrgCode       VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRADE_ReturnOrderItem_BrgCode       DEFAULT(''),
    BrgName       VARCHAR(60)   NOT NULL CONSTRAINT DF_BTRADE_ReturnOrderItem_BrgName       DEFAULT(''),
    Qty           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRADE_ReturnOrderItem_Qty           DEFAULT(0),
    SatId         VARCHAR(7)    NOT NULL CONSTRAINT DF_BTRADE_ReturnOrderItem_SatId         DEFAULT(''),
    JenisRetur    VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRADE_ReturnOrderItem_JenisRetur    DEFAULT(''),

    CONSTRAINT PK_BTRADE_ReturnOrderItem PRIMARY KEY CLUSTERED (ReturnOrderId, NoUrut)
)
GO

CREATE INDEX IX_BTRADE_ReturnOrder_ServerId_Status
    ON BTRADE_ReturnOrder(ServerId, StatusSync)
GO
```

#### Primary Keys

* `(ReturnOrderId, ServerId)` — mirrors `PK_BTRADE_Order`.
* Items: `(ReturnOrderId, NoUrut)` — scoped through the parent (globally unique
  ULID, IR-RO-01).

#### Idempotency

`StatusSync` `TERKIRIM`/`DOWNLOADED` (IR-RO-08). `PK_BTRADE_ReturnOrder` backs
submit idempotency: re-submission of the same `ReturnOrderId` replaces the
staged copy (delete-then-insert by `ReturnOrderId`), never duplicates.

#### Soft Delete / Audit

None. The relay is transport-only; authoritative audit lives in the Main Office.

---

### 6.3 Cloud Reference Projections — `BTRADE_Driver`, `BTR_WarehouseMapping`

Project: `src/j06-pkl-btrade-api/btrade.sqldb`
File: `btrade.sqldb/DriverContext/BTRADE_Driver.sql`

```sql
CREATE TABLE BTRADE_Driver(
    DriverId   VARCHAR(5)  NOT NULL CONSTRAINT DF_BTRADE_Driver_DriverId   DEFAULT(''),
    DriverName VARCHAR(20) NOT NULL CONSTRAINT DF_BTRADE_Driver_DriverName DEFAULT(''),
    IsAktif    BIT         NOT NULL CONSTRAINT DF_BTRADE_Driver_IsAktif    DEFAULT(1),
    ServerId   VARCHAR(5)  NOT NULL CONSTRAINT DF_BTRADE_Driver_ServerId   DEFAULT(''),

    CONSTRAINT PK_BTRADE_Driver PRIMARY KEY CLUSTERED (DriverId, ServerId)
)
GO
```

#### `BTR_WarehouseMapping` — Centralized Warehouse Mapping (GAP-012)

File: `btrade.sqldb/WarehouseContext/BTR_WarehouseMapping.sql`

```sql
CREATE TABLE BTR_WarehouseMapping(
    WarehouseCode VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_WarehouseMapping_WarehouseCode DEFAULT(''),
    ServerId      VARCHAR(5)  NOT NULL CONSTRAINT DF_BTR_WarehouseMapping_ServerId      DEFAULT(''),

    CONSTRAINT PK_BTR_WarehouseMapping PRIMARY KEY CLUSTERED (WarehouseCode)
)
GO
```

**Authority.** `BTR_WarehouseMapping` is the single authority that maps a
physical Warehouse (`WarehouseCode`, selected at BGud login) to its tenant/office
(`ServerId`). It mirrors the existing `BTRADE_Location` (location → `ServerId`)
and is populated **by database seed/migration scripts only** (no maintenance UI,
no Warehouse CRUD screen, no Warehouse administration module).

**Initial seed data** (idempotent migration script):

| WarehouseCode | ServerId |
| ------------- | -------- |
| `GAMPING` | `JOGJA` |
| `CONCAT` | `JOGJA` |
| `MAGELANG` | `MGL` |

This table resolves the tenant at login (Cloud token issuance + BGud session),
which is the only resolution the Return Order transport requires. The
authoritative `BTR_Warehouse.WarehouseId` used by `ReturJual` is resolved
office-side at Generate time from the `WarehouseCode` via the existing
warehouse master (see §8.5); no Warehouse maintenance UI is required.

---

### 6.4 Mobile Local Store — BGud Room Database

Database: `bgud_database` (Room, `AppDatabase` v2)

| Entity (table) | Purpose | Key |
| -------------- | ------- | --- |
| `return_order_entity` | Offline Return Order capture (`DRAFT`/`SYNCED`) | `returnOrderId` |
| `return_order_item_entity` | Return Order line items | `(returnOrderId, noUrut)` |
| `customer_entity` | Mandatory Customer reference cache (GAP-004) | `customerId` |
| `salesperson_entity` | Optional Salesman reference cache (ADR-RO-005) | `salesPersonId` |
| `driver_entity` | Optional Driver reference cache (ADR-RO-005) | `driverId` |
| existing `barang_entity`, `barcode_entity`, `barcode_registration_request_entity` | unchanged | — |
| DataStore `session_preferences` | token, user, `warehouseCode`, `serverId`, `officeCode`, last-sync timestamps | — |

```text
return_order_entity
    returnOrderId     TEXT PK     // ULID string (ADR-RO-002)
    customerId        TEXT
    customerCode      TEXT
    customerName      TEXT
    warehouseCode     TEXT        // selected Warehouse context (BR-005/006, IR-RO-04)
    salesPersonId     TEXT        // optional ('' when absent)
    salesPersonName   TEXT
    driverId          TEXT        // optional
    driverName        TEXT
    note              TEXT
    status            TEXT        // DRAFT | SYNCED
    createdAt         INTEGER
    createdBy         TEXT

return_order_item_entity
    returnOrderId     TEXT        // parent (composite PK part)
    noUrut            INTEGER
    brgId             TEXT
    brgCode           TEXT
    brgName           TEXT
    qty               REAL        // physical-unit quantity (IR-RO-03)
    satId             TEXT
    jenisRetur        TEXT        // BAGUS | RUSAK

customer_entity
    customerId        TEXT PK
    customerCode      TEXT
    customerName      TEXT
    address           TEXT

salesperson_entity
    salesPersonId     TEXT PK
    salesPersonName   TEXT

driver_entity
    driverId          TEXT PK
    driverName        TEXT
    isAktif           INTEGER
```

Indexes: `return_order_entity(status)`, `return_order_entity(customerName)`
(search); `return_order_item_entity(returnOrderId)` (already covered by PK).
The three reference caches are full-replace downloads (`deleteAll` +
`upsertAll`) scoped to the bound Office (GAP-004, GAP-005).

---

## 7. Application Layer Design

### 7.1 Main Office — `btr.application/InventoryContext/ReturnOrderAgg/`

#### Commands

| Command | Responsibility |
| ------- | -------------- |
| `ImportReturnOrderCommand` | Idempotently import one relayed Return Order: assign `ReturnOrderNo` (INunaCounterBL, IR-RO-07), persist header + items (including the `WarehouseCode` captured under the selected Warehouse context) with `Status = Synced` in one transaction. Skips/returns the existing order if `ReturnOrderId` already imported (INV-11). |
| `CompleteReturnOrderCommand` | Office Admin completes optional Salesman/Driver on a `Synced` order inline on the Generate Return Order surface (IR-RO-05). Does not re-number or alter items. |
| `GenerateSalesReturnFromReturnOrderCommand` | Take one or more selected `Synced` orders; group by Customer + Return Type + Salesman + Driver; delegate to the existing `ReturJual` builder/writer to create `ReturJual` header + item records; mark consumed orders `Imported`. Never performs pricing/inventory/finance (INV-13). |

Supporting components mirroring `ReturJual`:

| Component | Responsibility |
| --------- | -------------- |
| `ReturnOrderBuilder` | `Create()`, `Load(IReturnOrderKey)`, `Attach()`, field setters; resolves `BrgCode`/`BrgName` and `WarehouseName`/`CustomerName`/`SalesPersonName`/`DriverName` on read. |
| `ReturnOrderValidator` | FluentValidation `AbstractValidator<ReturnOrderModel>` enforcing INV-02…INV-09 at import. |
| `ReturnOrderWriter` | Single transaction per save (P-04), mirroring `ReturJualWriter.Save`. |

#### Queries

| Query | Responsibility |
| ----- | -------------- |
| `GetReturnOrderQuery` | Load one Return Order (header + items) by id. |
| `ListReturnOrderQuery` | List `Synced` (not yet `Imported`) orders by date/Customer for the Generate Return Order surface. |
| `ListReturnOrderByCustomerQuery` | Optional Customer-scoped listing (Generate surface). |

#### Use Cases

| Workflow capability | Application service |
| ------------------- | ------------------- |
| BC-005 Synchronize → import | `ImportReturnOrderCommand` (via `j07-btrade-sync`) |
| Complete optional Salesman/Driver (Generate surface) | `CompleteReturnOrderCommand` |
| Generate Sales Return from Return Order | `GenerateSalesReturnFromReturnOrderCommand` |
| BC-004 Search (Desktop) | `ListReturnOrderQuery`, `GetReturnOrderQuery` |

---

### 7.2 Cloud — `btrade.application`

| Type | Name | Responsibility |
| ---- | ---- | -------------- |
| Command | `ReturnOrderUploadCommand` | Stage one Return Order (`StatusSync = TERKIRIM`) idempotently by `ReturnOrderId` (delete-then-insert header + items in one transaction). No business validation (P-02). |
| Query | `ReturnOrderIncrementalDownloadQuery` | Return `TERKIRIM` orders scoped to `ServerId` + date periode; flip them to `DOWNLOADED` in the same transaction (acknowledgement, IR-RO-08). |
| Command | `DriverSyncCommand` | Upsert the `BTRADE_Driver` projection (delete-then-insert by `DriverId` + `ServerId`), mirroring `SalesPersonSyncCommand`. |
| Query | `DriverListDataQuery` | Return the Driver list for `GET /api/Driver/{serverId}`. |

DAL contracts: `IReturnOrderDal`, `IReturnOrderItemDal`, `IDriverDal` in
`btrade.application/Contract/`, keyed by `IReturnOrderKey`/`IDriverKey`,
auto-registered by the existing Scrutor scan.

---

## 8. Integration Design

### 8.1 BGud → Cloud: Return Order Submission

* Endpoint: `POST /api/return-order` (I-RO-01). JWT required; `ServerId`
  resolved server-side from the JWT (P-06).
* Session context: BGud stores both `WarehouseCode` (the Warehouse selected at
  login) and `ServerId` (resolved from the Warehouse Mapping) in the session
  context (ADR-RO-008). A Return Order is created under the selected
  `WarehouseCode`.
* Payload: header (ReturnOrderId, ReturnOrderDate, WarehouseCode, CustomerId +
  denormalized names, optional Salesman/Driver, Note) + items
  (`BrgId`/`BrgCode`/`BrgName`, `Qty`, `SatId`, `JenisRetur`). **No `ServerId`**
  in the body — the Cloud derives it from the authenticated session.
* Idempotency: `ReturnOrderId` is the key; resubmission replaces the staged
  copy (delete-then-insert).

### 8.2 Cloud → Main Office: Incremental Download + Import

```text
ReturnOrderIncrementalDownloadService.Execute(periode)
    ↓
GET /api/ReturnOrder/incremental/{tgl1}/{tgl2}/{serverId}   (I-RO-02)
    ↓  (Cloud flips TERKIRIM → DOWNLOADED in the same transaction)
ReturnOrderDal / ReturnOrderItemDal stage into BTR_ReturnOrder (upsert by id)
    ↓
MainOfficeCommandExecutor → ImportReturnOrderCommand (in-process, I-RO-08)
    ↓  (assigns ReturnOrderNo, persists WarehouseCode, Status = Synced)
```

Mirrors `OrderIncrementalDownloadService` and `CheckInIncrementalDownloadService`.
`Periode` bounds the download by `ReturnOrderDate`.

### 8.3 Cloud → BGud: Reference Data Download

| Data | Endpoint | Status |
| ---- | -------- | ------ |
| Customer | `GET /api/Customer/{serverId}` | Existing, reused (GAP-004) |
| Sales Person | `GET /api/SalesPerson/{serverId}` | Existing, reused (GAP-005) |
| Driver | `GET /api/Driver/{serverId}` | New (I-RO-05) |

BGud replaces the local cache per reference type; timestamps advance only on a
committed download.

### 8.4 `j07-btrade-sync` → Cloud: Driver Projection Upload

* Endpoint: `POST /api/Driver` (I-RO-06), mirroring `SalesPersonSyncService`.
  Upsert by `DriverId` + `ServerId` (IR-RO-06).

### 8.5 Main Office: Generate Sales Return from Return Order

The single Office surface (menu `Retur Penjualan → Generate Return Order`)
realizes this flow:

```text
GenerateReturnOrderForm lists Synced orders (not yet Imported)
    ↓ Office Admin completes optional Salesman/Driver inline, selects orders
GenerateSalesReturnFromReturnOrderCommand(selectedReturnOrderIds)
    ↓ group selected orders by (CustomerId, JenisRetur, SalesPersonId, DriverId)
    ↓ delegate to existing ReturJualBuilder/ReturJualWriter → create ReturJual
      header + item records (BTR_ReturJual / BTR_ReturJualItem; pricing via
      invoice history stays in the existing ReturJual flow; DOMAIN §15)
    ↓ mark consumed orders Status = Imported
```

Return Type maps to `ReturJual.JenisRetur` (`BAGUS`/`RUSAK`) directly (IR-RO-02);
recorded `Qty`/`SatId` flow into the existing `ReturJual` item-entry machinery,
which owns unit conversion (ADR-RO-003). The authoritative
`BTR_Warehouse.WarehouseId` for the generated `ReturJual` is resolved
office-side from the Return Order's `WarehouseCode` using the existing
warehouse master (a data relationship, not a maintenance UI).

### 8.6 Integration Inventory

| # | Direction | Endpoint / mechanism | Pattern | Auth |
| - | --------- | -------------------- | ------- | ---- |
| I-RO-01 | BGud → Cloud | `POST /api/return-order` | Idempotent submit (ADR-RO-002) | JWT |
| I-RO-02 | `j07-btrade-sync` → Cloud | `GET /api/ReturnOrder/incremental/{tgl1}/{tgl2}/{serverId}` | Incremental download + ack (mirror CheckIn/Order) | JWT |
| I-RO-03 | Cloud → BGud | `GET /api/Customer/{serverId}` | Reference download (existing) | Existing |
| I-RO-04 | Cloud → BGud | `GET /api/SalesPerson/{serverId}` | Reference download (existing) | Existing |
| I-RO-05 | Cloud → BGud | `GET /api/Driver/{serverId}` | Reference download (new) | JWT |
| I-RO-06 | `j07-btrade-sync` → Cloud | `POST /api/Driver` | Projection upload (IR-RO-06) | JWT |
| I-RO-07 | `j07-btrade-sync` → Main Office | `ReturnOrderDal` staging (upsert by id, `Status = Synced`) | Staging (not authority) | in-process |
| I-RO-08 | `j07-btrade-sync` → Main Office | `MainOfficeCommandExecutor` → `ImportReturnOrderCommand` | In-process command (numbering + mapping) | in-process |
| I-RO-09 | Main Office (Desktop) | `GenerateSalesReturnFromReturnOrderCommand` → `ReturJual` | Office consumption | — |

> Sync triggers: **Login Sync** and **Manual Sync Now** only; no background,
> scheduled, or real-time synchronization (OQ-1). Offline operation is fully
> supported.

---

## 9. Security Design

### 9.1 Authentication

| Client | Mechanism |
| ------ | --------- |
| BTR Desktop | Existing `BTR_User` login and role/menu authorization. Unchanged. |
| `j07-btrade-sync` | Presents a JWT from `pkl.btrade.api` on write endpoints (I-RO-02, I-RO-06). |
| BGud | Authenticates against `pkl.btrade.api`; presents the JWT on every request (`AuthInterceptor`). |

No new authentication mechanism is introduced (feasibility §4.5).

### 9.2 Authorization and Tenant Boundaries

* **Tenant.** `ServerId` is resolved server-side from the authenticated
  session; it is never read from a Return Order write payload (P-06). The
  incremental download route carries `serverId` in the path by convention,
  mirroring CheckIn/Order (I-RO-02).
* **Warehouse selection and mapping.** At login, the BGud user selects a
  Warehouse (`WarehouseCode`). The Cloud resolves its `ServerId` from the
  centralized `BTR_WarehouseMapping`, and the session stores both
  `WarehouseCode` and `ServerId`. The mapping is seed data — never client input
  and never hardcoded in BGud (ADR-RO-008).
* **Role gate.** Warehouse Officer: create/update/delete/sync (BGud). Office
  Admin: complete Salesman/Driver and generate Sales Return (Desktop). No
  approval/rejection workflow exists (GAP-006).

### 9.3 Permission Boundaries

| Function | Warehouse Officer | Office Admin |
| -------- | :---------------: | :----------: |
| Create/Update/Delete Return Order (pre-sync) | Yes | — |
| Synchronize Return Order | Yes | — |
| Search Return Order (device) | Yes | — |
| Generate Return Order (complete Salesman/Driver + generate) | — | Yes |

### 9.4 Security Ownership

| Concern | Owner |
| ------- | ----- |
| Credential store of record | Main Office (`BTR_User`) |
| Token issuance and tenant binding | Cloud API (server-side resolution) |
| Authoritative validation and numbering | Main Office application layer |
| Warehouse mapping | Main Office (`BTR_WarehouseMapping`, centrally maintained) |
| Endpoint enforcement | Cloud API middleware/attributes |

---

## 10. Operational Architecture

### 10.1 Migration Strategy

Existing SSDT approach; no migration framework (GAP-012, P-12).

* New objects (`BTR_ReturnOrder`, `BTR_ReturnOrderItem`, `BTR_WarehouseMapping`,
  `BTRADE_ReturnOrder`, `BTRADE_ReturnOrderItem`, `BTRADE_Driver`) are
  registered in `btr.sql.sqlproj` / `btrade.sqldb.sqlproj`.
* Idempotent upgrade scripts using the guarded pattern
  (`IF OBJECT_ID(...) IS NULL ...`, `IF COL_LENGTH(...) IS NULL ...`).
* All additions are additive; no existing table, column, or index is modified
  (GAP-002).

### 10.2 Data Backfill Strategy

**None required.** There is no legacy Return Order data (GAP-001 greenfield).
The Warehouse Mapping table is populated centrally as configuration
(ADR-RO-008).

### 10.3 Rollback Strategy

* **Schema:** additions are additive; removing the new objects removes the
  feature's persistence without affecting `BTR_ReturJual` or any existing table.
* **Cloud relay:** it is a projection; it can be re-downloaded by re-running
  the incremental download (idempotent by `ReturnOrderId`).
* **Authoritative data:** the Main Office `BTR_ReturnOrder` is the source; no
  downstream store can reconstruct it.

### 10.4 Performance Considerations

* The Generate worklist reads `BTR_ReturnOrder` via
  `IX_BTR_ReturnOrder_Status`.
* The incremental download is a periode-bounded, `ServerId`-scoped scan
  (mirror CheckIn/Order); it is a sync-run operation, never per-capture.
* BGud capture is local Room only; no capture step performs a network call
  (P-07).

### 10.5 Concurrency Considerations

* Import is idempotent by `ReturnOrderId` (PK + `Exists` check); a duplicate
  download stages via upsert and re-import is a no-op (INV-11).
* Download and import run sequentially within one sync run (download → stage →
  import).
* Mobile edits are local and single-user; no device-level concurrency.

### 10.6 Idempotency Requirements

| Operation | Idempotency guarantee |
| --------- | --------------------- |
| Submit (I-RO-01) | `PK (ReturnOrderId, ServerId)`; delete-then-insert by `ReturnOrderId`. |
| Download + ack (I-RO-02) | `TERKIRIM → DOWNLOADED` terminal transition; re-download of a `DOWNLOADED` row returns nothing. |
| Stage (I-RO-07) | Upsert by `ReturnOrderId`; re-download never duplicates items. |
| Import (I-RO-08) | `Exists(ReturnOrderId)` check; re-import never re-numbers or duplicates. |
| Generate (I-RO-09) | Consumed orders transition `Synced → Imported`; regeneration of an `Imported` order is prevented. |

---

# PART B — FRONTEND ARCHITECTURE

---

## 11. Screen Inventory

### 11.1 BGud (Android)

#### SCR-MOB-RO-001 — Return Order List

* **Purpose:** Search and list local Return Orders as a work queue (BC-004).
* **Primary Actor:** Warehouse Officer
* **Workflow Reference:** BC-004
* **Domain Reference:** DOMAIN §5, §11
* **Status filter:** `Semua` / `Draft` / `Synced`; default `Semua` (GAP-010).
* **Row routing:** Draft row → Edit/Resume (SCR-MOB-RO-004); Synced row →
  read-only Detail (SCR-MOB-RO-003) (GAP-003, GAP-005).
* **Updated (BGUD-RETURN-ORDER-NAV-001):** Date-section grouping added (TODAY /
  YESTERDAY / EARLIER); row routing is now status-conditional.

#### SCR-MOB-RO-002 — Create Return Order

* **Purpose:** Record returned goods (BC-001): mandatory Customer, optional
  Salesman/Driver, Notes, item lines by Barcode Scan or Manual Item Search.
* **Primary Actor:** Warehouse Officer
* **Workflow Reference:** BC-001
* **Domain Reference:** DOMAIN §6, §8, §9; BR-001…BR-012

#### SCR-MOB-RO-003 — Return Order Detail

* **Purpose:** Read-only view of a **Synced** Return Order.
* **Primary Actor:** Warehouse Officer
* **Workflow Reference:** BC-004 (view)
* **Domain Reference:** DOMAIN §8, §11; BR-018, BR-020
* **Routing note:** Reachable from the list only for `Synced` orders. Draft
  orders open Edit/Resume directly (GAP-003). Delete action has been relocated
  to the Edit screen (GAP-003).
* **Updated (BGUD-RETURN-ORDER-NAV-001):** Screen is now exclusively for Synced
  orders; Delete action removed from this screen.

#### SCR-MOB-RO-004 — Edit / Resume Return Order

* **Purpose:** Modify a `Draft` Return Order (BC-002); host the Delete Draft
  action (BC-003).
* **Primary Actor:** Warehouse Officer
* **Workflow Reference:** BC-002, BC-003
* **Domain Reference:** DOMAIN §5; BR-017, BR-018, BR-019, BR-020
* **Routing note:** Reachable directly from a Draft row in the list (GAP-003).
  Delete action is available on this screen when `status == DRAFT` (BR-019).
* **Updated (BGUD-RETURN-ORDER-NAV-001):** Delete action relocated here from
  Detail (GAP-003). Layout re-prioritized per §12.4.

#### SCR-MOB-RO-005 — Synchronization (extended)

* **Purpose:** Show Return Order sync state and trigger Manual Sync Now
  (BC-005).
* **Primary Actor:** Warehouse Officer
* **Workflow Reference:** BC-005
* **Domain Reference:** DOMAIN §11, §12

> **Delete (BC-003)** is a status-gated action on Edit (SCR-MOB-RO-004), not a
> separate screen. `Draft` only; local-only, never propagates (GAP-014).
> **Updated (BGUD-RETURN-ORDER-NAV-001):** Delete was previously on Detail
> (SCR-MOB-RO-003); it is now on Edit (SCR-MOB-RO-004). Draft Detail is no
> longer reachable from the list (GAP-003).

### 11.2 BTR Desktop (`j05-btr-distrib/btr.distrib`)

#### SCR-DESK-RO-001 — Generate Return Order

* **Screen Name:** `GenerateReturnOrderForm`
* **Purpose:** The single Return Order surface. Office Admin views synchronized
  (`Synced`, not yet `Imported`) Return Orders, completes optional
  Salesman/Driver inline, selects one or more orders, and executes Generate
  Sales Return. The system groups by Customer + Return Type + Salesman +
  Driver, creates `ReturJual` (header + items), and marks the source orders
  `Imported`.
* **Primary Actor:** Office Admin
* **Workflow Reference:** DOMAIN §13; ADR-RO-007
* **Domain Reference:** DOMAIN §4, §8, §13
* **Menu:** `Retur Penjualan → Generate Return Order`

> Return Order has no dedicated review, approval, rejection, or management
> workflow and no such states (GAP-006, ADR-RO-007, P-13). The existing
> `RT1-Retur Jual` form remains the downstream pricing/posting surface. No
> other Return Order management screens exist.

---

## 12. Screen Layout Architecture

### 12.1 SCR-MOB-RO-001 — Return Order List (Work Queue)

> **Updated (BGUD-RETURN-ORDER-NAV-001):** Date-section grouping and
> status-conditional routing added.

```text
Search Bar             (Customer name / date, min 3 chars, 300 ms debounce)
Status Filter Chips    ([ Semua ] [ Draft ] [ Synced ]; default Semua)
Result List (grouped):
  TODAY
    ├─ Draft row  → tap → EditReturnOrderScreen (Edit / Resume)
    └─ Synced row → tap → ReturnOrderDetailScreen (read-only)
  YESTERDAY
    ├─ Draft row  → Edit / Resume
    └─ Synced row → Detail
  EARLIER
    ├─ Draft row  → Edit / Resume
    └─ Synced row → Detail
New Return Action      (FAB → return_order_create)
```

Row displays: Customer, date (`dd MMM yyyy`), item count, status badge.
Ordering within each date section: most-recent-first.

### 12.2 SCR-MOB-RO-002 — Create Return Order

> **Updated (BGUD-RETURN-ORDER-NAV-001):** Layout re-prioritized; Item Entry /
> Item List is now the dominant working area. Salesman, Driver, Notes moved to
> a secondary position below item entry. Capture workflow and barcode scanning
> are unchanged.

```text
Transaction Context  (Customer picker [mandatory], Warehouse [read-only,
                     session-bound])
Item Entry / List    (Barcode Scanner [BarcodeScannerView], Item Search,
                     Qty, Unit [SatId], Return Type [BAGUS/RUSAK];
                     added-item list — each row: Item, Qty, Unit, Return Type)
Additional Info      (Salesman picker [optional], Driver picker [optional],
                     Notes)
Action Region        (Save, Cancel)
```

Item identification reuses `BarcodeScannerView` (scan) and existing manual
Item Search. Warehouse is session-bound, not user-selectable (BR-005/006).

### 12.3 SCR-MOB-RO-003 — Return Order Detail (Synced Only)

> **Updated (BGUD-RETURN-ORDER-NAV-001):** Screen is now exclusively for Synced
> orders. Delete action removed. Edit button removed (Synced orders are
> non-editable, BR-018).

```text
Header Region     (Customer, Warehouse, Salesman/Driver, Notes, status)
Item Region       (Item, Qty, Unit, Return Type)
Action Region     (read-only; no Edit, no Delete for Synced orders — BR-018, BR-020)
```

### 12.4 SCR-MOB-RO-004 — Edit / Resume Return Order

> **Updated (BGUD-RETURN-ORDER-NAV-001):** Delete action relocated here from
> Detail (GAP-003). Layout re-prioritized to mirror Create (§12.2).

```text
Transaction Context  (Customer picker [editable], Warehouse [read-only,
                     session-bound])
Item Entry / List    (Barcode Scanner, Item Search, Qty, Unit, Return Type;
                     added-item list — each row: Item, Qty, Unit, Return Type)
Additional Info      (Salesman picker [optional], Driver picker [optional],
                     Notes)
Action Region        (Save, Cancel, Delete [enabled only when status == DRAFT])
```

Delete is visible and enabled only when `status == DRAFT` (BR-019).
Synced orders are non-editable and non-deletable (BR-018, BR-020).

### 12.5 SCR-DESK-RO-001 — Generate Return Order

```text
Toolbar            (Refresh, Generate)
Filter Panel       (date, Customer)
Worklist           (selectable: ReturnOrderNo, Customer, Warehouse, status,
                   Salesman, Driver)
Detail Panel       (items grid; inline Salesman/Driver completion)
Result Region      (generated ReturJual documents, openable in RT1-Retur Jual)
```

---

## 13. Navigation Architecture

### 13.1 BGud (Navigation Compose)

> **Updated (BGUD-RETURN-ORDER-NAV-001):** Home restructured as work launcher;
> status-conditional list routing; Draft Detail removed; Register relocated to
> Barcode Registry; More section introduced; Sync card becomes nav entry.

```text
home
  ├─ New Return ───────────────▶ return_order_create
  ├─ Return Orders ────────────▶ return_order_list
  ├─ Synchronization Status ───▶ synchronization   (card tap)
  └─ More
       ├─ Barcode Registry ────▶ barcode_registry
       └─ Settings ────────────▶ settings

return_order_list
  ├─ Draft row ────────────────▶ return_order_edit?returnOrderId={id}
  ├─ Synced row ───────────────▶ return_order_detail?returnOrderId={id}
  └─ New Return (FAB) ─────────▶ return_order_create

return_order_detail  (Synced only)
  └─ (read-only; no Edit, no Delete)

return_order_create
  ├─ Save ────▶ local write ──▶ back
  └─ Cancel ──▶ back

return_order_edit
  ├─ Save ────▶ local write ──▶ back
  ├─ Cancel ──▶ back
  └─ Delete (Draft only) ──▶ confirm ──▶ back (list refreshed)

barcode_registry
  ├─ row Edit ────────────────▶ edit?barcodeId={id}
  └─ Register Barcode ────────▶ register           (no pre-filled barcode)
```

| Source | Target | Conditions |
| ------ | ------ | ---------- |
| `home` New Return | `return_order_create` | User selects New Return |
| `home` Return Orders | `return_order_list` | User selects Return Orders |
| `home` Sync Status card | `synchronization` | User taps the card |
| `home` More → Barcode Registry | `barcode_registry` | User selects Barcode Registry in More |
| `home` More → Settings | `settings` | User selects Settings in More |
| `return_order_list` Draft row | `return_order_edit` | Row status == `DRAFT` |
| `return_order_list` Synced row | `return_order_detail` | Row status == `SYNCED` |
| `return_order_list` New Return FAB | `return_order_create` | User taps FAB |
| `return_order_edit` Delete | local Room delete | Status is `Draft` (BR-019); confirm then back |
| `barcode_registry` Register | `register` | No pre-filled barcode |

### 13.2 BTR Desktop

```text
MainForm (role-gated ribbon)
    ↓ Retur Penjualan ── Generate Return Order
GenerateReturnOrderForm (SCR-DESK-RO-001)
    ↓ open generated document
ReturJualForm (existing RT1-Retur Jual)
```

The single surface opens as an MDI child via the DI container
(`BringMdiChildToFrontIfLoaded<T>()`), gated by a `BTR_Menu` row under the
`Retur Penjualan` parent and its `BTR_RoleMenu` grant. No other Return Order
Desktop screen exists (GAP-006, ADR-RO-007).

---

## 14. UI State Architecture

### 14.1 SCR-MOB-RO-002 — Create Return Order

```text
Empty
  ↓ Customer selected (mandatory)
Header Ready
  ↓ add first item line (scan/search → Qty → Unit → Return Type)
Items Present
  ↓ all lines valid (Qty > 0, Unit set, Return Type set)
Ready To Save
  ↓ Save
Saving (local write only, no network)
  └─ success → Saved → back
```

Transition rules:
* `Save` is enabled only when Customer is set and at least one valid item line
  exists (BR-001, BR-012).
* Item identification never performs a network call (P-07); Barcode Scan and
  Manual Item Search resolve against the local caches (BR-009).
* Warehouse is fixed to the session binding and never editable (BR-005/006).

### 14.2 SCR-MOB-RO-003 — Detail (Synced Only)

> **Updated (BGUD-RETURN-ORDER-NAV-001):** Detail is now Synced-only;
> no Edit or Delete on this screen.

```text
Loaded (Synced)
  └─ read-only; no Edit, no Delete (BR-018, BR-020)
```

### 14.2b SCR-MOB-RO-004 — Edit / Resume (Draft)

> **Updated (BGUD-RETURN-ORDER-NAV-001):** Edit now hosts the Delete action
> (relocated from Detail per GAP-003).

```text
Loaded
  ├─ Draft   → editable; Delete enabled (BR-017, BR-019)
  └─ Synced  → non-editable; Delete disabled (BR-018, BR-020)
Edit: Loaded → Dirty → Saving → Saved → back
Delete: Loaded (Draft) → Confirm → local Room delete → back
```

### 14.3 SCR-MOB-RO-005 — Synchronization

```text
Idle
  ↓ Sync Now
Synchronizing (submit Draft → Synced, then reference downloads)
  ├─ success → Synchronized (counts + timestamps refreshed)
  └─ failure → Failed (retry)
```

### 14.4 Device Lifecycle Display

```text
Device status: Draft | Synced   (ADR-RO-006; never Imported)
```

---

## 15. Workspace Modes

### 15.1 BGud — Return Order Workspace

#### Browse Mode

* **Purpose:** Search and view local Return Orders.
* **Available Actions:** Search, open Detail, Create.
* **Restrictions:** No field is editable from the list.

#### Capture Mode

* **Purpose:** Record a new Return Order in the field.
* **Available Actions:** Select Customer, capture items (scan/search), set
  Qty/Unit/Return Type, Save locally.
* **Restrictions:** Customer mandatory; Warehouse fixed; at least one item
  (BR-012); no network required (BG-003).

#### Maintenance Mode (Draft only)

* **Purpose:** Modify or delete a `Draft` order.
* **Available Actions:** Change Customer/Salesman/Driver/Notes/items; Delete.
* **Restrictions:** `Synced` orders are read-only (BR-018/020); pre-sync delete
  is local-only (GAP-014).

#### Synchronization Mode

* **Purpose:** Make sync state visible and trigger it.
* **Available Actions:** Sync Now; inspect pending/synced counts and last-sync
  timestamps.
* **Restrictions:** Requires connectivity; Login Sync and Manual Sync Now only
  (OQ-1).

### 15.2 BTR Desktop — Generate Return Order Workspace

#### Generate Mode

* **Purpose:** View synchronized (not yet `Imported`) Return Orders, complete
  optional Salesman/Driver, select one or more orders, and generate Sales
  Returns.
* **Available Actions:** Refresh, complete Salesman/Driver inline, select
  orders, Generate, open generated `ReturJual` documents.
* **Restrictions:** Only `Synced` orders are selectable; `Imported` orders are
  read-only and cannot be regenerated (ADR-RO-006). No review/approval mode
  exists (P-13).

---

## 16. Interaction Rules

| # | Condition | Enabled | Disabled | Notes |
| - | --------- | ------- | -------- | ----- |
| IR-M1 | Mobile, no Customer selected | Cancel | Save | BR-001/002 |
| IR-M2 | Mobile, no valid item line | Cancel | Save | BR-012 |
| IR-M3 | Mobile, item not in local cache | — | Save that line | BR-008 |
| IR-M4 | Mobile, Qty ≤ 0 | — | Save that line | BR-010 |
| IR-M5 | Mobile, Unit not set | — | Save that line | BR-011 |
| IR-M6 | Mobile, Return Type not BAGUS/RUSAK | — | Save that line | ADR-RO-004 |
| IR-M7 | Mobile Edit screen, status `Synced` | View | Edit, Delete | BR-018/020 — Edit screen still loaded for navigation purposes |
| IR-M8 | Mobile Edit screen, status `Draft` | Edit, Delete, Save | — | BR-017/019; Delete relocated from Detail per BGUD-RETURN-ORDER-NAV-001 |
| IR-M9 | Mobile, offline | Create/Edit/Delete (local) | Sync Now progress | BG-003 |
| IR-D1 | Desktop, Generate surface, no order selected | Refresh | Generate | selection required (ADR-RO-007) |
| IR-D2 | Desktop, Generate surface, Salesman/Driver empty on a selected order | Generate | — | optional fields (ADR-RO-005) |
| IR-D3 | Desktop, order `Imported` | View | Generate again | ADR-RO-006 |

---

## 17. UI Commands

### 17.1 BGud

| UI Action | Command / API |
| --------- | ------------- |
| Save (create/edit) | local Room write (`DRAFT`); later `POST /api/return-order` (I-RO-01) |
| Delete (Draft) | local Room delete (never propagates, GAP-014) |
| Scan / manual item search | local Room query on `barcode_entity` / `barang_entity` — **no API** (BR-009) |
| Customer / Salesman / Driver pickers | local Room query on reference caches — **no API** |
| Sync Now / Login Sync | submit `DRAFT` orders (I-RO-01), then reference downloads (I-RO-03/04/05) |

### 17.2 BTR Desktop

| UI Action | Command / API |
| --------- | ------------- |
| Refresh / view synced orders | `ListReturnOrderQuery` |
| Complete Salesman/Driver (inline) | `CompleteReturnOrderCommand` |
| Generate Sales Return | `GenerateSalesReturnFromReturnOrderCommand` |
| Open generated document | existing `ReturJualForm` |

### 17.3 Synchronization Client (`j07-btrade-sync`)

| UI Action | Command / API |
| --------- | ------------- |
| Process Return Order | `ReturnOrderIncrementalDownloadService` (I-RO-02) → stage → `ImportReturnOrderCommand` (I-RO-08) |
| Sync Driver projection | `DriverSyncService` → `POST /api/Driver` (I-RO-06) |

---

## 18. Validation Ownership

### 18.1 Main Office (Authoritative — always applied)

* Customer existence (INV-02, BR-008).
* Warehouse mapping resolution — unmapped `WarehouseCode` fails (INV-03,
  ADR-RO-008).
* At least one item (INV-04, BR-012).
* Item existence (`BrgId` exists) (INV-05, BR-008).
* `Qty > 0` (INV-06, BR-010).
* Unit validity — `(BrgId, Satuan)` exists in `BTR_BrgSatuan` (INV-07,
  ADR-RO-003).
* Return Type `BAGUS`/`RUSAK` (INV-08, ADR-RO-004).
* Import idempotency and numbering authority (INV-11, ADR-RO-002).
* Non-mutation of `ReturJual` persistence and no pricing/inventory/finance
  (INV-13/14, DOMAIN §15).

### 18.2 Cloud API (boundary only)

* JWT presence and validity; `ServerId` resolved server-side (P-06).
* Request-shape validation: `ReturnOrderId`, `CustomerId`, and items present.
* **No Return Order business validation** — the Cloud never decides existence,
  unit, or lifecycle (P-02).

### 18.3 BGud Client (usability + offline guardrails)

* Customer mandatory and from the local cache (BR-001/002, GAP-004).
* At least one item; `Qty > 0`; Unit from the item's cached unit data; Return
  Type `BAGUS`/`RUSAK` (BR-010…BR-012, ADR-RO-003/004).
* Item must exist in the local cache (BR-008).
* Modify/delete gated by `Draft` status (BR-017…BR-020).
* No small-unit normalization anywhere (ADR-RO-003).

The Main Office is authoritative; client checks are usability optimizations.

---

## 19. View Models

### 19.1 BGud View Models (`viewmodel/`)

| ViewModel | Purpose | Key Fields | Source |
| --------- | ------- | ---------- | ------ |
| `ReturnOrderListViewModel` | Searchable local list | `query`, `results`, `isEmpty`, `statusFilter` | Room `return_order_entity` |
| `CreateReturnOrderViewModel` | Capture header + item lines | `customer`, `salesman`, `driver`, `note`, `items`, `saveState` | Room caches + `return_order_entity` |
| `ReturnOrderDetailViewModel` | Read-only detail + status-gated actions | `order`, `items`, `isDraft` | Room `return_order_entity` |
| `EditReturnOrderViewModel` | Draft modification | `order`, `items`, `saveState` | Room |
| `ReturnOrderSyncViewModel` (extension) | Sync state + Manual Sync Now | `pendingCount`, `syncedCount`, `lastRefSync`, `isSyncing` | Room + I-RO-01/03/04/05 |

Each screen follows the existing `ViewModel` + `…ViewModelFactory` convention.

### 19.2 Desktop DTOs

#### `ReturnOrderFormDto`

* **Purpose:** Row model for the Generate Return Order worklist.
* **Key Fields:** `ReturnOrderId`, `ReturnOrderNo`, `ReturnOrderDate`,
  `CustomerId`, `CustomerName`, `WarehouseId`, `WarehouseName`,
  `SalesPersonId`, `SalesPersonName`, `DriverId`, `DriverName`, `Status`
* **Source Query:** `ListReturnOrderQuery`

#### `ReturnOrderItemFormDto`

* **Purpose:** Item grid row.
* **Key Fields:** `BrgCode`, `BrgName`, `Qty`, `SatId`, `JenisRetur`
* **Source Query:** `GetReturnOrderQuery`

### 19.3 Cloud Contracts

* `ReturnOrderSubmitRequest` (I-RO-01): header + items; **no `ServerId`**.
* `ReturnOrderIncrementalResponse` (I-RO-02): `ReturnOrderType[]`.
* `DriverListResponse` (I-RO-05): `DriverType[]`.

---

## 20. Frontend Performance Architecture

### BGud — Return Order List

```text
Default load:           50 rows
Paging:                 Incremental (load more on scroll end)
Minimum characters:     3
Auto search:            Yes, debounced 300 ms
Source:                 Local Room only (no network)
```

### BGud — Create / Edit

```text
Item identification:    Local Room query (scan + manual search) — no network
Reference pickers:      Local Room caches — no network
Save:                   Local write only (offline-first, BG-003)
```

### BGud — Synchronization

```text
Trigger:                Login Sync + Manual Sync Now only (OQ-1)
Ordering:               submit Draft orders → reference downloads
Worker:                 One sync run at a time (KEEP policy), connectivity
                        constraint, no periodic scheduling
```

---

# PART C — TRACEABILITY

---

## 21. Traceability Matrix

### 21.1 Business Rules

| Domain Rule | Workflow | Gap / Decision | Architecture Element |
| ----------- | -------- | -------------- | -------------------- |
| BR-001/002 Customer mandatory | BC-001 | GAP-004 | §5.1 INV-02; §18.1/18.3; §6.4 `customer_entity` |
| BR-003/004 No invoice required; multi-invoice items | BC-001 | — | §5.1 (no invoice reference in the model) |
| BR-005/006 Warehouse mandatory, single | BC-001 | GAP-012, ADR-RO-008 | §5.1 INV-03; §6.1 `BTR_WarehouseMapping` |
| BR-007/008 Item mandatory, must exist | BC-001 | — | §5.1 INV-05; §18.1/18.3 |
| BR-009 Barcode or Search | BC-001 | GAP-004 | §17.1 (local Room query); §14.1 |
| BR-010 Qty > 0 | BC-001 | — | §5.1 INV-06; §18.3 |
| BR-011 Unit mandatory | BC-001 | GAP-007, ADR-RO-003 | §5.1 INV-07; §6.1 `SatId` |
| BR-012 At least one item | BC-001 | — | §5.1 INV-04; §14.1 |
| BR-013/014 Salesman optional, completed later | BC-001 | GAP-005, ADR-RO-005 | §5.1 INV-09; §7.1 `CompleteReturnOrderCommand` |
| BR-015/016 Driver optional, completed later | BC-001 | GAP-005, ADR-RO-005 | §5.1 INV-09; §7.1 `CompleteReturnOrderCommand` |
| BR-017/018 Modify pre-sync only | BC-002 | GAP-014 | §14.2; §16 IR-M7/M8 |
| BR-019/020 Delete pre-sync only | BC-003 | GAP-014 | §14.2; §16 IR-M7/M8; §17.1 |
| Return Types (Good/Broken → BAGUS/RUSAK) | BC-001 | GAP-008, ADR-RO-004 | §5.1 VO-01; §6.1 `JenisRetur` |
| Lifecycle Draft → Synced → Imported | BC-005 | GAP-011, ADR-RO-006 | §5.1 INV-10; §14.4 |
| Grouping for Sales Return | DOMAIN §13 | GAP-010, ADR-RO-007 | §7.1; §8.5 |
| Out of scope (pricing/inventory/finance) | DOMAIN §15 | ADR-RO-007 | §5.1 INV-13; §8.5 |

### 21.2 Workflows

| Workflow | Screens | Commands / APIs | Integration |
| -------- | ------- | --------------- | ----------- |
| BC-001 Create | SCR-MOB-RO-002 | local write; later I-RO-01 | I-RO-01 |
| BC-002 Update | SCR-MOB-RO-004 | local write (Draft) | — (pre-sync) |
| BC-003 Delete | SCR-MOB-RO-003 | local delete (Draft) | — (never propagates) |
| BC-004 Search | SCR-MOB-RO-001, SCR-DESK-RO-001 | local query / `ListReturnOrderQuery` | — |
| BC-005 Synchronize | SCR-MOB-RO-005 | I-RO-01, I-RO-02, I-RO-07, I-RO-08 | I-RO-01/02/07/08 |
| Generate Sales Return | SCR-DESK-RO-001 | `CompleteReturnOrderCommand`, `GenerateSalesReturnFromReturnOrderCommand` | I-RO-09 |

### 21.3 Screens → Workflow → Domain

| Screen | Workflow Capability | Domain Capability |
| ------ | ------------------- | ----------------- |
| SCR-MOB-RO-001 | BC-004 | §5 Search |
| SCR-MOB-RO-002 | BC-001 | §5 Create, §6 Domain Concepts |
| SCR-MOB-RO-003 | BC-003/004 | §8, §11 Lifecycle |
| SCR-MOB-RO-004 | BC-002 | §5 Update |
| SCR-MOB-RO-005 | BC-005 | §12 Domain Events |
| SCR-DESK-RO-001 | Generate Sales Return (complete + group + generate) | §4 Actors, §13 |

---

## 22. Architecture Risks

### R-01 — Return Order and `ReturJual` Become Conflated

* **Risk:** Implementation reads or writes `BTR_ReturJual` from the Return
  Order path, or reuses `ReturJual` numbering/authority for Return Orders.
* **Impact:** High — divergent or duplicated return data.
* **Mitigation:** Separate aggregate (§5.1), separate tables (§6.1), separate
  DAL (§7.1), no shared numbering prefix, and INV-14 (never modify
  `ReturJual` persistence).
* **Residual Risk:** Low.

### R-02 — Unit/Quantity Misinterpretation

* **Risk:** A quantity recorded in a physical unit is silently normalized into
  a small unit before reaching Sales Return.
* **Impact:** High — wrong quantities in downstream Sales Return.
* **Mitigation:** Store `BrgId`/`Qty`/`SatId` and forbid normalization (P-09,
  ADR-RO-003); conversion stays in existing `ReturJual` item-entry logic.
* **Residual Risk:** Low — requires a deliberate review violation.

### R-03 — Warehouse Mapping Failure

* **Risk:** A `WarehouseCode` with no mapping row is silently resolved to a
  fallback warehouse.
* **Impact:** Medium — Return Orders land in the wrong warehouse.
* **Mitigation:** `BTR_WarehouseMapping` PK lookup; unmapped code fails the
  import explicitly (INV-03, ADR-RO-008).
* **Residual Risk:** Low.

### R-04 — Cloud Becomes a De Facto Authority

* **Risk:** The Cloud relay is treated as authoritative, or business validation
  creeps into the relay commands.
* **Impact:** High — violates ADR-RO-001 and the authority model.
* **Mitigation:** Relay-only `StatusSync` (IR-RO-08), no validation (§18.2),
  never authored locally (P-02).
* **Residual Risk:** Low.

### R-05 — Snapshot Replacement of Return Orders

* **Risk:** Return Order transport reuses the master-data snapshot pattern,
  erasing remote records.
* **Impact:** High — data loss.
* **Mitigation:** Incremental pull only (P-03, GAP-003); download is
  periode-bounded and status-gated (`TERKIRIM`).
* **Residual Risk:** Low.

### R-06 — Domain Artifact Out of Sync

* **Risk:** `RETURN-ORDER-DOMAIN.md` still uses `Good`/`Broken` (§3, §5, §9,
  BR-013/BR-014) while the closed decision is `BAGUS`/`RUSAK`.
* **Impact:** Medium — implementers following the domain artifact build the
  wrong vocabulary.
* **Mitigation:** This architecture consumes ADR-RO-004; the domain artifact
  update is carried as a Knowledge Curator pass (C-3 in §24).
* **Residual Risk:** Medium until the domain artifact is updated.

### R-07 — Driver Projection Duplication / Wrong Source

* **Risk:** The Driver reference is synced from the wrong place or duplicated
  with a divergent shape.
* **Impact:** Medium — optional Driver lookup breaks or diverges.
* **Mitigation:** `BTRADE_Driver` mirrors `BTRADE_SalesPerson`; populated by
  `DriverSyncService` upsert by `DriverId` + `ServerId` (IR-RO-06).
* **Residual Risk:** Low.

### R-08 — Identifier Storage-Type Mismatch

* **Risk:** `ReturnOrderId` is stored as one type in one store and another type
  elsewhere (e.g., GUID vs ULID), breaking idempotency.
* **Impact:** High — duplicate imports or failed re-submission.
* **Mitigation:** `VARCHAR(26)` ULID string end-to-end (IR-RO-01), matching the
  platform's ULID-keyed convention.
* **Residual Risk:** Low.

### R-09 — ReturnOrderNo Format Undefined

* **Risk:** The `ReturnOrderNo` prefix/format is invented during implementation.
* **Impact:** Low-Medium — inconsistency with existing numbering.
* **Mitigation:** Mechanism fixed (`INunaCounterBL`, IR-RO-07); the concrete
  prefix/format is a data decision carried as C-1 (§24), not silently chosen.
* **Residual Risk:** Medium until C-1 is confirmed.

---

## 23. Implementation Guidance

### 23.1 Backend Constraints

* Place the aggregate in `InventoryContext/ReturnOrderAgg` and mirror the
  `ReturJual` layering: `ReturnOrderModel` + `IReturnOrderKey` (domain);
  `ReturnOrderBuilder`, `ReturnOrderWriter`, `ReturnOrderValidator`,
  `IReturnOrderDal`, queries (application); `ReturnOrderDal` with Dapper
  (infrastructure).
* Every authoritative write is a single `TransHelper.NewScope()` transaction.
* Store `ReturnOrderId` as a `VARCHAR(26)` ULID string (IR-RO-01); never
  convert to GUID or `UNIQUEIDENTIFIER`, and never use it as the business
  document number (that is `ReturnOrderNo`).
* The Main Office has no tenancy concept. Do not add one.
* New schema objects declare no FK (IR-RO-11); enforce integrity in the
  application layer.
* Register every new schema object in its `.sqlproj` and ship an idempotent
  upgrade script.
* The Cloud relay must not validate Return Order business rules (P-02).

### 23.2 Frontend Constraints

* BGud reuses the existing structure (`dao` / `database` / `model` / `network` /
  `repository` / `ui/screen` / `ui/component` / `viewmodel` +
  `…ViewModelFactory` / `datastore` / `ui/Navigation.kt`).
* No capture step may trigger a network call (P-07).
* All BGud writes are local-first; submission happens only during Login Sync or
  Manual Sync Now (OQ-1).
* The device must not store or send `ServerId` as a command input (P-06).
* Delete and edit are gated by `Draft` status and are local-only (GAP-014).

### 23.3 Critical Invariants

```text
INV-01  ReturnOrderId is a ULID and the idempotency key (distinct from ReturnOrderNo)
INV-02  Customer mandatory and existing
INV-03  WarehouseCode must exist in BTR_WarehouseMapping (→ ServerId)
INV-04  At least one item
INV-05  Item must exist in BTR_Brg
INV-06  Qty > 0
INV-07  (BrgId, SatId) valid in BTR_BrgSatuan
INV-08  JenisRetur is BAGUS | RUSAK
INV-09  Salesman/Driver optional, completable later
INV-10  Office Status: Synced → Imported (Draft device-only)
INV-11  Import idempotent by ReturnOrderId
INV-12  Generate may fan out by grouping key
INV-13  No pricing/inventory/finance in Return Order commands
INV-14  ReturJual persistence never modified
```

### 23.4 Prohibited Shortcuts

* Do not modify `BTR_ReturJual` / `BTR_ReturJualItem` / child tables (GAP-002).
* Do not perform small-unit normalization in BGud or on import (ADR-RO-003).
* Do not apply snapshot/replace semantics to Return Orders (GAP-003).
* Do not let the Cloud validate Return Order business rules (ADR-RO-001).
* Do not accept a client-supplied `ServerId` on any Return Order write
  (P-06).
* Do not generate `ReturnOrderNo` in BGud (ADR-RO-002).
* Do not merge `ReturnOrderId` (ULID) with `ReturnOrderNo` (business number);
  they serve different purposes (ADR-RO-002).
* Do not hardcode the warehouse mapping in BGud (ADR-RO-008).
* Do not propagate `Imported` status or `ReturnOrderNo` back to BGud
  (ADR-RO-006).
* Do not implement background/scheduled/realtime synchronization (OQ-1).
* Do not introduce a migration framework (P-12).
* Do not introduce a Return Order review, approval, rejection, or validation
  workflow or states (GAP-006, ADR-RO-007, P-13).

---

## 24. Architecture Readiness

### Ready For Planning

```text
YES
```

All fourteen gaps (GAP-001 … GAP-014) are CLOSED and all business and technical
questions (BQ-1 … BQ-7, TQ-1 … TQ-5, OQ-1/OQ-2) are RESOLVED. No blocking
business ambiguity remains that would prevent a planner from slicing the work
described in §4 through §20.

### Blocking Issues

None for planning.

### Must Be Confirmed Before Implementation

Recorded as confirmations, not blockers:

| # | Item | Why | Reference |
| - | ---- | --- | --------- |
| C-1 | Confirm the `ReturnOrderNo` prefix/format and sequencing configuration (`INunaCounterBL` mechanism is fixed; the concrete prefix is a data decision). | Numbering is owned by the Main Office (ADR-RO-002); the format is business data. | IR-RO-07, R-09 |
| C-2 | Confirm the Desktop menu identifiers and role grants for the single `Generate Return Order` surface under the `Retur Penjualan` parent. | `BTR_Menu.MenuId` / `BTR_RoleMenu` values are data decisions. | §13.2, §9.3 |
| C-3 | Knowledge Curator pass: update `RETURN-ORDER-DOMAIN.md` vocabulary from `Good`/`Broken` to `BAGUS`/`RUSAK` (§3, §5, §9, BR-013/BR-014). | Code and knowledge must remain consistent (ADR-RO-004). | R-06 |
| C-4 | Produce `RETURN-ORDER-WORKFLOW.md` if the artifact chain requires a standalone workflow artifact. | This architecture consumed the workflow from DOMAIN §14 and §5. | §1 Inputs |

### Planner Guidance

**Expected implementation areas**

1. Main Office domain + application + infrastructure for the `ReturnOrder`
   aggregate, `ImportReturnOrderCommand`, `CompleteReturnOrderCommand`, and
   `GenerateSalesReturnFromReturnOrderCommand` (§5.1, §7.1).
2. Main Office schema objects `BTR_ReturnOrder`, `BTR_ReturnOrderItem`,
   `BTR_WarehouseMapping` plus sqlproj registration and idempotent upgrade
   scripts (§6.1, §10.1).
3. Desktop "Generate Return Order" surface (menu `Retur Penjualan → Generate
   Return Order`) with menu seed and role grants (§11.2, §12.5, §13.2).
4. Cloud relay models/tables/DALs and use cases (`ReturnOrderUploadCommand`,
   `ReturnOrderIncrementalDownloadQuery`, Driver projection + routes) (§5.2,
   §5.3, §6.2, §6.3, §7.2).
5. Synchronization client: incremental download service, staging DALs, import
   executor extension, Driver uploader, and `SyncForm` wiring (§4.3, §8.2,
   §8.4).
6. BGud capture: Room entities/DAOs, reference caches, capture repository,
   sync worker, and List/Create/Detail/Edit/Synchronization screens (§6.4,
   §11.1, §12, §14, §19.1).

**Major dependencies**

* Customer/Barang/Barcode reference data and existing sync.
* The Barcode Registry security model (JWT + session-bound location).
* `j07-btrade-sync` and the in-process `MainOfficeCommandExecutor`.
* The existing `ReturJual` aggregate and `BTR_Warehouse`, `BTR_BrgSatuan`
  reference data.
* `BTR_Driver` (Main Office) for the Driver projection.

**Sequencing considerations** (not phases, not slices)

* Customer/SalesPerson/Driver reference caches precede Return Order capture.
* The centralized Warehouse Mapping precedes office import.
* The Cloud relay precedes the `j07-btrade-sync` download.
* The Main Office Return Order table and import command precede Generate Sales
  Return.
* Import runs after download within a sync run (download → stage → import).

**Review concerns**

* Prove Return Order never posts inventory/valuation (INV-13).
* Prove it stays a distinct entity from `ReturJual` (INV-14).
* Prove quantities retain the recorded `SatId` unit (no normalization).
* Prove idempotent resubmission keyed by `ReturnOrderId` and explicit
  acknowledgement.
* Prove no snapshot replacement is applied to Return Orders (P-03).
* Prove `ServerId` is never client-supplied (P-06).
* Prove the warehouse mapping is not hardcoded in BGud (ADR-RO-008).
