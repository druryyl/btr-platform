# FEASIBILITY ASSESSMENT

# Barcode Registry

| Field | Value |
| ----- | ----- |
| Deliverable | `BARCODE-REGISTRY-FEASIBILITY-ASSESMENT.md` |
| Business request | Implement the Barcode Registry domain as defined in `docs/work/barcode-registry/BARCODE-REGISTRY-DOMAIN.md` |
| Primary business artifact | `docs/work/barcode-registry/BARCODE-REGISTRY-DOMAIN.md` |
| Decision records | `adrs/ADR-001-desktop-keyboard-wedge-scanning.md`; `adrs/ADR-002-authenticated-jwt-write-endpoints.md`; `adrs/ADR-003-mobile-authentication-against-cloud-api.md`; `adrs/ADR-004-barcode-uniqueness-scope.md`; `adrs/ADR-005-barang-master-shared-reference-dataset.md`; `adrs/ADR-006-cache-first-mobile-barcode-lookup.md`; `adrs/ADR-007-tenant-isolation-from-authenticated-identity.md` |
| Method | `docs/skills/feasibility-creation-skill.md` |
| Systems assessed | `j05-btr-distrib` (BTR Desktop / Main Office), `j06-pkl-btrade-api` (Cloud API), `j07-btrade-sync` (synchronization), `j07-btr-gudang` (warehouse client), `BTrade3` (Android sales app), and the planned **Warehouse Mobile App** (BQ-1) |
| Author role | Analyst + Architect (Feasibility only) |
| Status | **Analysis — no implementation authorized** |

> This document is an analysis artifact. It does not define architecture, create
> implementation plans, create slices, or write code. It establishes planning
> readiness. Where a decision is required, it is recorded as an Open Question.

---

## 1. Executive Summary

### Request

Implement the Barcode Registry domain described in
`docs/work/barcode-registry/BARCODE-REGISTRY-DOMAIN.md`: a centralized,
authoritative repository of barcode-to-Item mappings owned by the Main Office
Server (BTR Desktop), exposing barcode registration and maintenance from Desktop
and Mobile, barcode lookup for operational applications, activation/deactivation,
and a one-way synchronization of active barcode data to the Cloud API as a
read-only lookup model.

### Recommendation

Implement the Barcode Registry end to end across the four tiers, using existing
patterns and the decisions recorded in this assessment:

1. **Authoritative registry on the Main Office Server** in `j05-btr-distrib`
   (greenfield; `Brg`-style layering; dedicated table with normalized
   `BarcodeValue`; optional `BrgSatId`) plus a Desktop maintenance screen and an
   Item Barcode tab.
2. **Cloud read model** in `j06-pkl-btrade-api`, populated by an **incremental**
   one-way sync from `j07-btrade-sync` (Active upsert; deactivate → remove).
3. **Mobile-originated registration** via a Cloud `BarcodeRegistrationRequest`
   integration queue, transferred by `j07-btrade-sync` to the Main Office for
   authoritative validation; the Cloud never serves lookups from pending requests.
4. **New Warehouse Mobile App** (first module: Barcode Registry) reusing the
   `BTrade3` stack plus CameraX, ML Kit barcode scanning, and WorkManager;
   cache-first offline lookup and offline request capture.

No new synchronization infrastructure is introduced. All business and technical
questions are resolved; see the ADRs and Section 9.

### Feasibility Result

```text
FEASIBLE
```

The domain is implementable within the BTR ecosystem. The authoritative registry,
Desktop capability, Cloud read model, incremental barcode sync, and offline mobile
capability are all feasible using existing patterns and infrastructure, and the
previously uncertain mobile registration/security/tenancy mechanisms are decided
(ADR-002/003/007, GAP-007). Feasibility is contingent on implementing the decided
ADRs, including creating the new Warehouse Mobile App.

### Planning Readiness Result

```text
READY
```

All identified gaps (GAP-001..GAP-016) are closed and all blocking questions are
resolved. Planning and ARCHITECTURE.md creation may proceed. Non-blocking open
items are listed in Section 9.

---

## 2. Request Understanding

The request asks for a new master data domain, "Barcode Registry", whose
capability is to own the relationship between physical barcode values and Items.

### Requested Capabilities (as stated in the domain artifact)

| Capability | Reference |
| ---------- | --------- |
| Barcode registration | DOMAIN §3.1, UC-002 |
| Barcode maintenance / correction | DOMAIN §3.2, UC-003, BR-010 |
| Barcode lookup | DOMAIN §3.3, UC-001, BR-007 |
| Activation / deactivation | DOMAIN §3.4, lifecycle §8 |
| Barcode-to-Item mapping (1 Item → many Barcodes; 1 Barcode → 1 Item) | DOMAIN §3.5, BR-002, BR-003 |
| Optional Packaging Level | DOMAIN §3.6, BR-005 — **superseded** by GAP-009 (removed; optional `BrgSatId` instead) |
| One-way synchronization to Cloud lookup | DOMAIN §3.7 |

### Stated Environment and Constraints

| Aspect | Stated position |
| ------ | --------------- |
| Item authority | Product Master (Master Barang) in `j05-btr-distrib`; Item data authoritative there |
| Barcode authority | Main Office Server in `j05-btr-distrib` is System of Record |
| Cloud | `j06-pkl-btrade-api` serves mobile; lookup/read model only; must never be authoritative |
| Synchronization | `j07-btrade-sync` currently synchronizes Item data Main Office → Cloud; barcode must join this model |
| Mobile | Must support Barcode Lookup, Barcode Registration, and offline operation |
| Conflict rule | Main Office wins; `btr.distrib` authoritative; Cloud converges to Main Office |

### Requested Outcome

A single authoritative source of barcode-to-Item mappings supporting current and
future warehouse mobility modules (Sales Return, Stock Opname, Receiving,
Picking, Warehouse Transfer), where mobile applications can resolve scans to
Items and can register new barcodes, including while offline.

---

## 3. Current State Analysis

### 3.1 Existing Business Flow

There is **no barcode business flow today**. A repository-wide search for
`barcode`/`barkod` across `src/` returns no code or SQL matches — only
documentation. `BrgCode` (the Item business code, `VARCHAR(20)`) is a product
code and is **not** a scan identity.

Existing relevant flows are Item master maintenance and Item synchronization:

```text
BTR Desktop (j05)                 Cloud API (j06)               Mobile / Warehouse
  Item master (BTR_Brg)
        │
        │  j07-btrade-sync : BrgSyncService
        │  POST /api/Brg  (full snapshot, replace-by-ServerId)
        ▼
                             BTRADE_Brg (read model)
                                    │
                                    │  GET /api/Brg/{serverId}
                                    ▼
                                                        BTrade3 (Android, offline Room cache)
                                                        BTR Gudang (WinForms, packing only)

Mobile-originated data (Order, CheckIn) flows the other way only by Main Office polling:
  BTrade3 ── POST /api/Order, POST /api/CheckIn ──▶ Cloud ── GET incremental ──▶ j07-btrade-sync ──▶ BTR
```

### 3.2 Existing Components

| Component | System | Relevance to Barcode Registry |
| --------- | ------ | ----------------------------- |
| Item master aggregate (`BrgModel`, `BrgBuilder`, `BrgWriter`, `BrgDal`, `BrgForm`) | `j05-btr-distrib` | Pattern to mirror for a barcode aggregate; Item is the external reference |
| Item units (`BTR_BrgSatuan`: `Satuan`, `Conversion`, `SatuanPrint`) | `j05-btr-distrib` | Reused for unit-specific barcodes via optional `BrgSatId` (GAP-009); Packaging Level removed |
| Cloud Item read model (`BrgType`, `BrgDal`, `BrgListDataQuery`, `BrgSyncCommand`, `BrgController`) | `j06-pkl-btrade-api` | Pattern to mirror for cloud barcode lookup |
| Master-data uploader (`BrgSyncService`, `KategoriSyncService`, `SalesPersonSyncService`, `WilayahSyncService`) | `j07-btrade-sync` | Exact repeatable pattern for Main Office → Cloud barcode sync |
| Mobile submission relay (Order/CheckIn incremental download) | `j07-btrade-sync` + `j06` | The only existing remote-data → Main Office path; pull-based, not push |
| Android mobile client (`BTrade3`: Room, Retrofit, sync repositories) | `BTrade3` | Salesman application; **not** the target for warehouse barcode scope (BQ-1) |
| Warehouse mobile client (**new Warehouse Mobile App**) | planned | Target client for barcode lookup/registration and future warehouse modules (BQ-1); stack undecided (TQ-7) |
| Warehouse client (`j07-btr-gudang`: WinForms, Packing Order only) | `j07-btr-gudang` | **Not a mobile app**; no scanning; only packing-order download/print |

### 3.3 Existing Database

**Main Office Server (`j05-btr-distrib`, SSDT project `btr.sql`):**

- Item: `BTR_Brg` (`BrgId VARCHAR(6)` PK, `BrgName`, `BrgCode VARCHAR(20)`, `IsAktif`, `SupplierId`, `JenisBrgId`, `KategoriId`, `Hpp`, …). No barcode column. No FK constraints; integrity is application-level.
- Units: `BTR_BrgSatuan` (`BrgId`, `Satuan`, `Conversion`, `SatuanPrint`).
- **No barcode/EAN/GTIN/packaging/scan table exists.**
- Schema is applied via SSDT publish (`.sqlproj` includes every table) plus hand-written idempotent upgrade scripts under `btr.sql\Scripts\`. No migration framework.

**Cloud (`j06-pkl-btrade-api`, SSDT project `btrade.sqldb`):**

- Item: `BTRADE_Brg` (`BrgId`, `BrgName`, `BrgCode`, `KategoriName`, `SatBesar`, `SatKecil`, `Konversi`, `HrgSat`, `Stok`, `ServerId`; PK `BrgId, ServerId`).
- **No barcode table or column exists.**
- Also note existing schema gaps: `BTRADE_FakturItem.sql` is empty, and `BTRADE_ParamNo`/`APTOL_ParamNo` are referenced by code but absent from the project.

**Warehouse client (`j07-btr-gudang`, SSDT project):** local SQL Server database `btrgd`; tables `BTRG_PackingOrder`, `BTRG_PackingOrderItem`, `BTRG_PrintLog`. No barcode column.

**Android (`BTrade3`):** Room database with `barang`, `customer`, `order`, `check_in`, `sales_person` entities. No barcode entity.

### 3.4 Existing Integrations

- `j07-btrade-sync` → Cloud: `POST /api/Brg`, `POST /api/Kategori`, `POST /api/SalesPerson`, `POST /api/Wilayah`, `POST /api/Customer`, `POST /api/PackingOrder/bulk`.
- Cloud → `j07-btrade-sync`: `GET /api/Order/incremental/...`, `GET /api/CheckIn/incremental/...`, `GET /api/Customer/Updated/{serverId}`, `PATCH /api/Customer/ClearUpdatedFlag`.
- Cloud → `BTrade3`: `GET /api/Brg/{serverId}`, `GET /api/Customer/{serverId}`, `GET /api/SalesPerson/{serverId}`; `BTrade3` → Cloud: `POST /api/Order`, `PATCH /api/Customer`, `POST /api/CheckIn`.
- Cloud → `j07-btr-gudang`: `GET /api/PackingOrder/...`, `GET /api/PackingOrder/pending/...`.

**Critical property:** Master data is **full-snapshot, replace-all by `ServerId`** on the cloud (for example `BrgSyncCommand` deletes all `BTRADE_Brg` rows for the `ServerId` then inserts the received list). There is **no incremental/change-flag mechanism** for master data, and **no Cloud → Main Office push** of any kind; remote data reaches the Main Office only by polling.

### 3.5 Existing Security Model

- **Desktop (`j05`)**: Windows user context; menu-driven; no API exposure.
- **Cloud (`j06`)**: JWT Bearer is configured but **not enforced** — no `[Authorize]` attribute exists on any controller; there is **no token-issuance/login endpoint**; secrets are committed; CORS is `AllowAnyOrigin`; mobile and sync clients use **cleartext HTTP**.
- **`j07-btrade-sync`**: DB credentials are hardcoded; DB server/database and tenant (`ServerTargetID`) are read from the Windows registry and are client-controlled.
- **`j07-btr-gudang`**: no API authentication; local DB credentials hardcoded; requires a local SQL Server instance.
- **`BTrade3`**: Google Sign-In is used only as a local gate and is **not** sent as an API credential.

---

## 4. Impact Analysis

### 4.1 Backend Impact

| Area | Impact |
| ---- | ------ |
| Main Office domain (`btr.domain`) | New barcode aggregate/entity (`BarcodeModel`/key); relationship to `Brg` (external reference). |
| Main Office application (`btr.application`) | New builder/writer/validator, DAL contracts, and queries for register / correct / activate / deactivate / lookup / list-by-item. |
| Main Office infrastructure (`btr.infrastructure`) | New Dapper DAL(s); unique-index-aware persistence. |
| Cloud domain (`btrade.domain`) | New `BarcodeType` read model (and, if mobile registration is approved, a staged registration-request type). |
| Cloud application (`btrade.application`) | New MediatR query (`Barcode` lookup/list) and sync command(s); possibly a registration-request submission handler. |
| Cloud infrastructure (`btrade.infrastructure`) | New DAL(s) following the `BrgDal` pattern. |
| `j07-btrade-sync` | New syncer (`BarcodeSyncService` + `BarcodeDal` + `BarcodeType`), wired into `SyncForm` and the master-data pattern. |
| `j07-btr-gudang` | Only if this client participates; today it has no item-lookup endpoint or scan handling. |
| `j07-btr-gudang` | Warehouse operations today; a candidate to be superseded/complemented by the new Warehouse Mobile App (BQ-1). |
| **New Warehouse Mobile App** | New client (first module: Barcode Registry); barcode data layer, API contract, lookup/registration UI, offline cache and request queue (BQ-1). |

### 4.2 Database Impact

| Store | Impact |
| ----- | ------ |
| Main Office (`btr.sql`) | New `BTR_BrgBarcode` table (naming to be confirmed) with PK, normalized-text `BarcodeValue` and case-insensitive unique constraint (within the Product Master authority — GAP-010; BQ-5), index on `BrgId`, activation flag, audit columns; register in `btr.sql.sqlproj`; add idempotent upgrade script. |
| Cloud (`btrade.sqldb`) | New `BTRADE_BrgBarcode` read-model table keyed by `ServerId` with unique `(ServerId, BarcodeValue)`; register in `btrade.sqldb.sqlproj`. `BarcodeRegistrationRequest` staging table and/or delivery/acknowledgement columns (decided, GAP-007). |
| `j07-btr-gudang` (`btrgd`) | New local barcode cache table only if this client must look up barcodes offline. |
| `BTrade3` (Room) | **Not in scope** for barcode (Salesman app, BQ-1). |
| **New Warehouse Mobile App** (local store) | New barcode + Barang cache, offline registration-request queue, migrations (stack per TQ-7). |

### 4.3 Frontend Impact

| Client | Impact |
| ------ | ------ |
| BTR Desktop (`j05`) | New barcode maintenance screen (register/correct/activate/deactivate), barcode list on the Item screen, menu seed, browser. |
| **New Warehouse Mobile App** | Barcode lookup and registration screens; camera scan input; offline cache-first lookup; registration-request queue and sync status (BQ-1). |
| `j07-btr-gudang` | Only if in scope; currently no scan widget or item master screen. |
| `BTrade3` | **Not in scope** for barcode (Salesman app, BQ-1). |

### 4.4 Integration Impact

| Integration | Impact |
| ----------- | ------ |
| Main Office → Cloud | New barcode sync (new endpoint + sync service), following the existing full-snapshot replace-by-`ServerId` pattern. |
| Cloud → Mobile | New bulk sync endpoint `GET /api/barcodes/sync` (active barcodes for the authenticated context); no point lookup for MVP. |
| Mobile → Main Office | **New mechanism required** for registration if mobile-originated registrations must become authoritative (existing pattern is cloud-staged polling by `j07-btrade-sync`). |
| Product Master | Barcode references `BrgId`; no modification of Product Master (BR-008). |
| Sync client duplication | There are **two** sync codebases (`j07-btrade-sync` and the older `j05-btr-distrib\btr.sync` with matching class names). The authoritative client must be confirmed before adding a syncer. |

### 4.5 Security Impact

Any path that allows a barcode to be written as authoritative raises the current
gaps to release-level concerns: unenforced JWT, no token issuer, cleartext
transport, client-selected tenant (`ServerId`), and committed secrets. A
read-only lookup endpoint is lower risk but still inherits unauthenticated
access to item/barcode data.

**Decided (GAP-008 / ADR-002 / ADR-003 / ADR-007):** JWT authentication is
enforced on all cloud write endpoints, anonymous writes are prohibited, mobile
authenticates against `pkl.btrade.api` before submitting operational commands,
and the user selects an operational location at authentication so that
`ServerId` is resolved server-side (never supplied by client commands). Still
unresolved: transport security and coordinated client updates.

---

## 5. Gap Analysis

| Gap ID | Type | Description |
| ------ | ---- | ----------- |
| GAP-001 | Functional | **CLOSED** — No Barcode Registry domain exists anywhere (greenfield; no legacy barcode data to migrate). Resolution: implement Barcode Registry as a new greenfield domain in `btr.distrib`; no migration strategy required; no legacy compatibility constraints. |
| GAP-002 | Data | **CLOSED** — No barcode table/column in the Main Office schema (`btr.sql`). Resolution: create a dedicated Barcode table in `btr.sql` with a unique `BarcodeValue` constraint and an FK to Item Master (`BTR_Brg`); no migration or backfill required. |
| GAP-003 | Data | **CLOSED** — No barcode table/model in the Cloud read model (`btrade.sqldb`, `btrade.domain`). Resolution: create a Barcode read model in `btrade.sqldb` and `btrade.domain`; synchronize active barcode mappings from the Main Office through the sync client; Cloud remains lookup-only and never becomes authoritative. |
| GAP-004 | Integration | **CLOSED** — No barcode synchronization path between Main Office and Cloud. Resolution: extend the existing sync process to replicate Barcode Registry data Main Office → Cloud; reuse the existing synchronization architecture and introduce no new synchronization infrastructure. |
| GAP-005 | Functional | **CLOSED** — No Desktop UI for barcode registration, maintenance, activation, or Item-barcode listing. Resolution: add a Barcode Registry maintenance screen and an Item Barcode tab in Master Barang; no approval workflow, dashboard, or migration tooling required. |
| GAP-006 | Technical | **CLOSED** — No barcode scanning capability in any client (`BTrade3`, `j07-btr-gudang`); no scanner SDK, camera, or keyboard-wedge handling. Resolution: add barcode acquisition to Desktop and Mobile clients — Desktop uses keyboard-wedge scanners, Mobile uses camera-based scanning; domain and business logic remain scanner-technology agnostic. |
| GAP-007 | Integration | **CLOSED** — No path for a mobile-originated registration to be validated and accepted by the Main Office given the Cloud is read-only and the Main Office is not publicly reachable. Resolution: introduce a BarcodeRegistrationRequest staging area in the Cloud; Mobile submits requests to the Cloud API; the sync client transfers requests to the Main Office for validation and creation of authoritative Barcode records; the Cloud never becomes authoritative. |
| GAP-008 | Technical | **CLOSED** — Cloud write endpoints are unauthenticated and unauthenticated-writable; JWT is configured but not enforced and has no issuer. Resolution: enforce JWT authentication on all cloud write endpoints and introduce a token-issuing mechanism using existing BTR user accounts; authentication is mandatory for all mobile-originated writes, including Barcode Registration Requests. |
| GAP-009 | Data/Definition | **CLOSED** — Domain used `ItemId`/`ItemCode`/`ItemName` and an undefined "Packaging Level". Resolution: align terminology with Product Master (`ItemId` → `BrgId`, `ItemCode` → `BrgCode`, `ItemName` → `BrgName`); **remove PackagingLevel** from the domain; reuse `BTR_BrgSatuan` via an optional `BrgSatId` in the barcode mapping when unit-specific identification is required; do not create a second unit-classification model. |
| GAP-010 | Data | **CLOSED** — Uniqueness scope of `BarcodeValue` was undefined across tenants. Resolution: uniqueness is tenant-scoped, not platform-global; Main Office enforces uniqueness within its Product Master authority; Cloud read models enforce uniqueness on `(ServerId, BarcodeValue)`. Domain rule updated from "globally unique" to "unique within an authoritative Product Master scope". |
| GAP-011 | Functional | **CLOSED** — Offline registration conflict semantics were undefined. Resolution: offline registrations are requests, not authoritative updates; during synchronization the Main Office validates each request against the authoritative registry; on conflict the authoritative state wins and the request is rejected; no merge strategy. Recorded as domain rules BR-012 and BR-013. |
| GAP-012 | Technical | **CLOSED** — No migration framework; both schemas are SSDT projects. Resolution: use the existing SSDT deployment approach; add Barcode Registry schema objects to `btr.sqlproj` and `btrade.sqlproj`; provide idempotent upgrade scripts for existing installations; introduce no migration framework. |
| GAP-013 | Integration | **CLOSED** — No standalone Item lookup endpoint for the warehouse client; item data reached `j07-btr-gudang` only embedded in packing orders. Resolution: expose standalone Barang lookup to warehouse clients by reusing the existing Barang master-data synchronization and API infrastructure; mobile applications may search and select Barang independently of Packing Orders and keep a local offline cache. Recorded as ADR-005. |
| GAP-014 | UX | **CLOSED** — No barcode-enabled screens existed and lookup had to work offline. Resolution: introduce barcode-enabled screens for Desktop and Mobile; mobile barcode lookup uses a locally synchronized barcode cache as the primary lookup mechanism, enabling resolution without network connectivity; unknown barcodes may be captured as offline registration requests for later synchronization. Recorded as ADR-006. |
| GAP-015 | Operational | **CLOSED** — Duplicate synchronization clients existed (`j07-btrade-sync` and `j05-btr-distrib\btr.sync`) with unconfirmed ownership. Resolution: `j07-btrade-sync` is the authoritative and maintained synchronization client; all Barcode Registry synchronization is implemented there; `j05-btr-distrib\btr.sync` is excluded from the target architecture. |
| GAP-016 | Integration | **CLOSED** — Master-data sync is full-snapshot replace-by-`ServerId`, which could erase remotely created barcode data. Resolution: Barcode master data continues to use the existing synchronization strategy; `BarcodeRegistrationRequest` is an integration queue artifact and must not participate in snapshot replacement synchronization; master-data replication and request processing are separate synchronization patterns. Recorded as domain rule BR-014. |

### Closed Gaps

| Gap ID | Type | Resolution | Status |
| ------ | ---- | ---------- | ------ |
| GAP-001 | Functional | Implement Barcode Registry as a new greenfield domain in `btr.distrib`. No migration strategy required. No legacy compatibility constraints. | CLOSED |
| GAP-002 | Data | Create a dedicated Barcode table in `btr.sql` with a unique `BarcodeValue` constraint and an FK to Item Master (`BTR_Brg`). No migration or backfill required. | CLOSED |
| GAP-003 | Data | Create a Barcode read model in `btrade.sqldb` and `btrade.domain`. Synchronize active barcode mappings from the Main Office through the sync client. Cloud remains lookup-only and never becomes authoritative. | CLOSED |
| GAP-004 | Integration | Extend the existing sync process to replicate Barcode Registry data from Main Office to Cloud. Reuse the existing synchronization architecture; introduce no new synchronization infrastructure. | CLOSED |
| GAP-005 | Functional | Add a Barcode Registry maintenance screen and an Item Barcode tab in Master Barang. No approval workflow, dashboard, or migration tooling required. | CLOSED |
| GAP-006 | Technical | Add barcode acquisition capability to Desktop and Mobile clients. Desktop uses keyboard-wedge barcode scanners. Mobile uses camera-based barcode scanning. Domain and business logic remain scanner-technology agnostic. | CLOSED |
| GAP-007 | Integration | Introduce a BarcodeRegistrationRequest staging area in the Cloud. Mobile submits registration requests to the Cloud API. The sync client transfers requests to the Main Office for validation and creation of authoritative Barcode records. The Cloud never becomes authoritative for Barcode Registry data. | CLOSED |
| GAP-008 | Technical | Enforce JWT authentication on all cloud write endpoints and introduce a token-issuing mechanism using existing BTR user accounts. Authentication becomes mandatory for all mobile-originated write operations, including Barcode Registration Requests. | CLOSED |
| GAP-009 | Data/Definition | Align terminology with Product Master (`ItemId` → `BrgId`, `ItemCode` → `BrgCode`, `ItemName` → `BrgName`); remove `PackagingLevel`; reuse `BTR_BrgSatuan` via optional `BrgSatId` for unit-specific barcodes; no second unit-classification model. | CLOSED |
| GAP-010 | Data | BarcodeValue uniqueness is tenant-scoped, not platform-global. Main Office enforces uniqueness within its Product Master authority; Cloud read models enforce `(ServerId, BarcodeValue)`. Domain rule updated from "globally unique" to "unique within an authoritative Product Master scope". | CLOSED |
| GAP-011 | Functional | Offline registrations are requests, not authoritative updates. Main Office validates each request during synchronization; on conflict the authoritative state wins and the request is rejected; no merge strategy. Recorded as domain rules BR-012 and BR-013. | CLOSED |
| GAP-012 | Technical | Use the existing SSDT deployment approach. Add Barcode Registry schema objects to `btr.sqlproj` and `btrade.sqlproj`; provide idempotent upgrade scripts for existing installations; introduce no migration framework. | CLOSED |
| GAP-013 | Integration | Expose standalone Barang lookup to warehouse clients by reusing the existing Barang master-data synchronization and API infrastructure. Mobile applications may search and select Barang independently of Packing Orders and maintain a local offline cache. Recorded as ADR-005. | CLOSED |
| GAP-014 | UX | Introduce barcode-enabled screens for Desktop and Mobile. Mobile barcode lookup uses a locally synchronized barcode cache as the primary lookup mechanism (no network required); unknown barcodes may be captured as offline registration requests for later synchronization. Recorded as ADR-006. | CLOSED |
| GAP-015 | Operational | `j07-btrade-sync` is the authoritative and maintained synchronization client; all Barcode Registry synchronization is implemented there; `j05-btr-distrib\btr.sync` is excluded from the target architecture. | CLOSED |
| GAP-016 | Integration | Barcode master data continues to use the existing synchronization strategy. `BarcodeRegistrationRequest` is an integration queue artifact and must not participate in snapshot replacement synchronization. Master-data replication and request processing are separate synchronization patterns. Recorded as domain rule BR-014. | CLOSED |

---

## 6. Solution Options

### GAP-005 — Realizing the authoritative registry (CLOSED; GAP-001/002/003 closed: greenfield)

**Resolution (decided).** Add a Barcode Registry maintenance screen and an Item
Barcode tab in Master Barang. No approval workflow, dashboard, or migration
tooling required.

**Recommended realization (below, retained for context).** Add a standalone
Barcode aggregate in the Main Office following the existing `Brg` layering
(domain → application → infrastructure → WinForms), with its own table and a
Desktop maintenance screen. Item remains an external reference.

- Advantages: Reuses a proven, auto-discovered pattern; respects BR-008 (does not touch Product Master); clean ownership boundary; greenfield (no legacy migration).
- Disadvantages: Introduces a second aggregate that must be kept consistent with Item lifecycle (inactive items).
- Risk: LOW.

### GAP-004 — Main Office → Cloud synchronization (CLOSED)

**Resolution (decided).** Extend the existing sync process to replicate Barcode
Registry data Main Office → Cloud, reusing the existing synchronization
architecture and introducing no new synchronization infrastructure.

**Recommended realization (retained for context).** A dedicated barcode syncer
using the authoritative client (`j07-btrade-sync`) but **incremental** per TQ-3:
Main Office change feed → `BarcodeType` → Cloud upsert/remove → Cloud holds only
Active barcodes for the `ServerId`; mobile consumes `GET /api/barcodes/sync`.

- Advantages: Reuses the existing sync client and API infrastructure; low effort.
- Disadvantages / watch-outs: incremental change detection must be reliable; keep request processing (GAP-016/BR-014) separate from master-data replication.
- Risk: MEDIUM (change detection/idempotency).

### GAP-007 — Mobile-originated registration (CLOSED)

**Resolution (decided).** Introduce a `BarcodeRegistrationRequest` staging area
in the Cloud. Mobile submits registration requests to the Cloud API. The sync
client transfers requests to the Main Office for validation and creation of
authoritative Barcode records. The Cloud never becomes authoritative for
Barcode Registry data.

**Rationale.** This is the previously assessed **Option B**, now decided. It
satisfies BO-003 (mobile registration) and offline capture (a request can be
stored on the device and uploaded later) while preserving Main Office authority
and BR-011 (all uniqueness validation on the Main Office). The staging record is
transport only — the Cloud must never serve lookup from unvalidated requests.

**Options considered (retained for context).**

**Option A.** Office-only registration for the first release; mobile is lookup-only. Fully consistent with the one-way model and lowest risk. Does not satisfy BO-003's "Mobile registration" objective.

**Option B (selected).** Mobile submits a **registration request** to the Cloud as a staging record only; the sync client polls and relays it to the Main Office, which validates uniqueness and persists the authoritative record; the authoritative record then syncs back down. The Cloud staging record is transport, never a lookup source, so the Cloud never becomes authoritative.

- Advantages: Satisfies BO-003 and offline capture; preserves Main Office authority and BR-011.
- Disadvantages: New relay + acknowledgement flow; must not reuse the existing "acknowledge during GET" pattern; needs authentication.
- Risk: HIGH (new integration mechanism + security).

**Option C.** Direct authenticated mobile → Main Office endpoint. Contradicts the stated constraint that the Main Office is not publicly accessible. Not selected.

### GAP-006 — Scanning capability (CLOSED)

**Resolution (decided).** Add barcode acquisition capability to Desktop and
Mobile clients. Desktop uses keyboard-wedge barcode scanners. Mobile uses
camera-based barcode scanning. Domain and business logic remain
scanner-technology agnostic.

**Decision record:** `adrs/ADR-001-desktop-keyboard-wedge-scanning.md` (Desktop
shall use keyboard-wedge scanners; no vendor-specific scanner SDK supported).

**Options considered (retained for context).**

**Option A.** Keyboard-wedge (HID) input captured by the existing text-entry screens; no SDK required.

**Option B.** Integrated camera scanning via a platform library (e.g., ZXing/ML Kit on Android). Higher effort, additional dependency, not currently present.

### GAP-008 — Authentication (CLOSED)

**Resolution (decided).** Enforce JWT authentication on all cloud write
endpoints and introduce a token-issuing mechanism using existing BTR user
accounts. Authentication becomes a mandatory prerequisite for all
mobile-originated write operations, including Barcode Registration Requests.

**Decision records:**

- `adrs/ADR-002-authenticated-jwt-write-endpoints.md` — all cloud write
  endpoints must require authenticated JWT identity; anonymous writes prohibited.
- `adrs/ADR-003-mobile-authentication-against-cloud-api.md` — mobile applications
  authenticate against `pkl.btrade.api` before submitting operational commands.

**Note.** This decides the authentication mechanism. Tenant binding — mapping the
authenticated identity to a `ServerId` rather than trusting a client-supplied
value — is resolved separately by **ADR-007** (TQ-2).

### GAP-009 — Definitions (CLOSED)

**Resolution (decided).**

1. Align Barcode Registry terminology with existing Product Master terminology:
   `ItemId` → `BrgId`, `ItemCode` → `BrgCode`, `ItemName` → `BrgName`.
2. **Remove `PackagingLevel`** from the Barcode Registry domain.
3. Reuse the existing `BTR_BrgSatuan` model by introducing `BrgSatId` (optional)
   in the barcode mapping when barcode identification must be unit-specific.
4. Do not create a second unit-classification model inside Barcode Registry.

**Notes for downstream artifacts.**

- The repository's Item Master column is `BrgName` (`BTR_Brg.BrgName`); the
  decision names `BrgNama` semantically, but the existing identifier is
  `BrgName`. Architecture should use the existing name, not introduce a variant.
- `BTR_BrgSatuan` has no surrogate key; its key is `(BrgId, Satuan)`. The
  meaning of `BrgSatId` (the `Satuan` value, or a new surrogate) must be settled
  during architecture.
- Removing `PackagingLevel` supersedes parts of
  `docs/work/barcode-registry/BARCODE-REGISTRY-DOMAIN.md` (§3.6, §5, BR-005,
  UC-002/UC-003). The domain artifact must be updated so knowledge stays
  synchronized; this assessment does not rewrite the domain.

### GAP-010 — Uniqueness scope across tenants (CLOSED)

**Resolution (decided).** `BarcodeValue` uniqueness is tenant-scoped, not
platform-global. The Main Office enforces uniqueness within its Product Master
authority. Cloud read models enforce uniqueness on `(ServerId, BarcodeValue)`.
The domain rule is updated from "globally unique" to "unique within an
authoritative Product Master scope".

**Decision record:** `adrs/ADR-004-barcode-uniqueness-scope.md`.

**Notes for downstream artifacts.**

- The Main Office `btr.sql` unique constraint on `BarcodeValue` (GAP-002) is
  correct for a single-authority database; it is scoped to that Product Master.
- The Cloud read model must use a composite unique key `(ServerId, BarcodeValue)`,
  not `BarcodeValue` alone, so distinct tenants may hold the same value.
- Removing the "globally unique" wording supersedes
  `docs/work/barcode-registry/BARCODE-REGISTRY-DOMAIN.md` BR-001 and the
  aggregate consistency boundary (line 201). The domain artifact must be updated
  so knowledge stays synchronized; this assessment does not rewrite the domain.

### GAP-011 — Offline registration conflict semantics (CLOSED)

**Resolution (decided).** Offline registrations are treated as requests, not
authoritative updates. During synchronization the Main Office validates each
request against the authoritative Barcode Registry. If a conflict exists, the
authoritative state wins and the request is rejected. No merge strategy is
implemented.

**Recorded as domain rules** in
`docs/work/barcode-registry/BARCODE-REGISTRY-DOMAIN.md`:

- **BR-012** — Offline barcode registrations are treated as registration
  requests; authoritative validation occurs in the Main Office during
  synchronization.
- **BR-013** — On synchronization conflict, the Main Office authoritative state
  wins; conflicting offline requests are rejected and do not modify the
  authoritative registry.

This resolves **BQ-2**. It means a rejected request must be reported back to the
requesting device/user (outcome/status), which is an integration detail to
settle in architecture.

### GAP-012 — Schema deployment (CLOSED)

**Resolution (decided).** Use the existing SSDT deployment approach. Add Barcode
Registry schema objects to the Main Office and Cloud database projects
(`btr.sql.sqlproj` and `btrade.sqldb.sqlproj`; referred to in the decision as
`btr.sqlproj` and `btrade.sqlproj`). Provide idempotent upgrade scripts for
existing installations. No migration framework will be introduced.

### GAP-013 — Warehouse/mobile Barang lookup (CLOSED)

**Resolution (decided).** Expose standalone Barang lookup capability to
warehouse clients by reusing the existing Barang master-data synchronization and
API infrastructure. Mobile applications may search and select Barang
independently of Packing Orders and maintain a local offline cache for offline
operation.

**Decision record:** `adrs/ADR-005-barang-master-shared-reference-dataset.md` —
Barang Master is a shared reference dataset for warehouse mobility and may be
synchronized to mobile clients for offline lookup.

### GAP-014 — Barcode-enabled screens and offline lookup (CLOSED)

**Resolution (decided).** Introduce barcode-enabled screens for Desktop and
Mobile applications. Mobile barcode lookup uses a locally synchronized barcode
cache as the primary lookup mechanism, enabling barcode resolution without
network connectivity. Unknown barcodes may be captured as offline registration
requests for later synchronization.

**Decision record:** `adrs/ADR-006-cache-first-mobile-barcode-lookup.md` — mobile
barcode lookup is cache-first; Cloud APIs are synchronization sources, not
runtime dependencies for barcode scanning.

### GAP-015 — Authoritative synchronization client (CLOSED)

**Resolution (decided).** `j07-btrade-sync` is the authoritative and maintained
synchronization client. All Barcode Registry synchronization capabilities will be
implemented there. `j05-btr-distrib\btr.sync` is excluded from the target
architecture.

This also resolves **TQ-4**.

### GAP-016 — Request queue vs snapshot replacement (CLOSED)

**Resolution (decided).** Barcode master data continues to use the existing
synchronization strategy. `BarcodeRegistrationRequest` is treated as an
integration queue artifact and must not participate in snapshot replacement
synchronization. Master-data replication and request processing are separate
synchronization patterns.

**Recorded as domain rule** in
`docs/work/barcode-registry/BARCODE-REGISTRY-DOMAIN.md`:

- **BR-014** — Barcode Registration Requests are integration artifacts and are not
  subject to master-data synchronization replacement rules.

This removes the earlier data-loss risk where a full-snapshot publish could
erase remotely created data.

---

## 7. Recommended Approach

The preferred approach, at solution level:

1. **Own the registry on the Main Office.** A dedicated Barcode aggregate referencing Item by `BrgId`, with server-side uniqueness validation (BR-011), lifecycle (Active/Inactive), optional `BrgSatId` for unit-specific mappings (`BTR_BrgSatuan`), and audit fields. Item remains owned by Product Master; Packaging Level is removed (GAP-009).
2. **Desktop first.** Deliver barcode registration, maintenance, activation/deactivation, and Item-barcode listing in BTR Desktop, mirroring existing master-data screen conventions.
3. **Cloud read model (bulk sync).** Add a barcode read-model table published one-way from the Main Office with only Active barcodes; mobile consumes `GET /api/barcodes/sync` and resolves barcodes locally (cache-first). No point-lookup API for MVP.
4. **Mobile registration is confirmed** (GAP-007): the Cloud holds an unvalidated `BarcodeRegistrationRequest`; the sync client transfers it to the Main Office, which is the only validating authority. Offline requests are validated at synchronization; conflicting requests are rejected (BR-012/BR-013). The Cloud must never serve a lookup from pending requests.
5. **Security as a precondition**, not a follow-up, for any barcode write path.

This keeps the recommended solution implementation-neutral and does not select a
final architecture; it establishes what must change.

---

## 8. Risks

| Risk | Impact | Probability | Mitigation |
| ---- | ------ | ----------- | ---------- |
| Cloud becomes de facto authority because mobile writes land there | High — violates the authority model | Medium | Stage requests only; never serve lookups from pending; Main Office validates (BR-011) |
| Barcode uniqueness not enforced → duplicate mappings | High | Medium | **Scope decided** (GAP-010 / ADR-004): Main Office enforces uniqueness within its Product Master authority (unique `BarcodeValue`); Cloud uses unique `(ServerId, BarcodeValue)`; plus server validation (BR-011) |
| Full-snapshot barcode sync erases remotely registered barcodes | High — data loss | Medium | **Resolved** (GAP-016 / BR-014, reinforced by TQ-3): sync is incremental (upsert/remove), not snapshot replacement; `BarcodeRegistrationRequest` never participates in snapshot sync |
| Unauthenticated/cleartext cloud write path | Critical — data integrity and access | High (already present) | **Mitigation decided** (GAP-008 / ADR-002 / ADR-003): enforce JWT on all write endpoints, prohibit anonymous writes, mobile authenticates against `pkl.btrade.api`. Residual: transport security and tenant binding must still be implemented. |
| Offline registration conflicts | Medium — inconsistent data | Medium | **Mitigation decided** (GAP-011 / BR-012 / BR-013): offline registrations are validated by the Main Office at synchronization; conflicts are rejected and the authoritative state wins; no merge. Residual: return request outcomes to the device. |
| Wrong app assumed for mobility (`j07-btr-gudang` is WinForms, not mobile) | High — scope and effort mis-estimated | High | ~~Confirm the target client (BQ-1)~~ **Resolved:** a new Warehouse Mobile App is the target (BQ-1); its stack is TQ-7 |
| Inventory reconciliation impact from lookup errors | High — operational | Low–Medium | Lookup is read-only; no stock mutation in this domain |
| Stale lookups / sync not running | Medium — stale lookups | Medium | **Decided** (OQ-2): cache-first with sync at login and on manual request; BGud syncs only the selected Warehouse's office data; new registrations propagate on next sync |
| Schema deployment on existing databases | Medium | Medium | **Decided** (GAP-012): existing SSDT approach; register schema objects in `btr.sql.sqlproj` and `btrade.sqldb.sqlproj` and provide idempotent upgrade scripts; no migration framework |
| Product Master changes after barcode mapping (item deactivated) | Low–Medium | Medium | Define lookup behavior for inactive Items as an Open Question |

---

## 9. Open Questions

Questions marked **[BLOCKING]** change data cardinality, tenancy, schema, or
integration topology and must be resolved before planning.

### Business Questions

- ~~**BQ-1 [BLOCKING]** — Which application is the "Mobile App" for barcode lookup and registration?~~ **RESOLVED:** create a new **Warehouse Mobile App**. Barcode Registry, Sales Return Capture, Stock Opname, Receiving, Picking, and future warehouse operations belong to a warehouse mobility domain and must not be coupled to the Salesman application (`BTrade3`). Barcode Registry is the first module of the new app and provides shared lookup/registration for future warehouse modules.
- ~~**BQ-2 [BLOCKING]** — What does "offline barcode registration" mean precisely?~~ **RESOLVED (GAP-011 / BR-012 / BR-013):** offline registrations are requests, not authoritative updates; the Main Office validates at synchronization; on conflict the authoritative state wins and the request is rejected; no merge strategy.
- ~~**BQ-3 [BLOCKING]** — How should "Packaging Level" be modeled?~~ **RESOLVED (GAP-009):** Packaging Level is removed from the domain; unit-specific barcode identification uses an optional `BrgSatId` referencing the existing `BTR_BrgSatuan` model.
- ~~**BQ-4 [BLOCKING]** — Is a `BarcodeValue` globally unique across all tenants/companies, or unique per `ServerId`?~~ **RESOLVED (GAP-010 / ADR-004):** tenant-scoped. Main Office enforces uniqueness within its Product Master authority; Cloud uses `(ServerId, BarcodeValue)`.
- ~~**BQ-5** — Which barcode symbologies and lengths must be supported, and what normalization applies?~~ **RESOLVED:** Barcode Registry stores barcode values as normalized text and is symbology-agnostic. Phase 1 supports any barcode that can be scanned or entered as text (EAN-13, UPC, Code128, QR, internal barcodes). Normalization: trim whitespace; remove CR/LF and tab characters; preserve leading zeros; compare case-insensitively. No barcode-format or check-digit validation in Phase 1.
- ~~**BQ-6** — Confirm the role matrix; is any approval required?~~ **RESOLVED:** no approval workflow. Warehouse Officer: lookup, register, correct mapping. Office Admin: lookup, register, correct mapping, activate, deactivate. System Administrator: all functions. Registrations and corrections take effect immediately after successful Main Office validation; accountability comes from audit fields (`CreatedBy`, `CreatedDate`, `ModifiedBy`, `ModifiedDate`) rather than approvals.
- ~~**BQ-7** — May a barcode be registered against an Item not present on the device, or against an inactive Item?~~ **RESOLVED:** a barcode may only be registered against a Barang that exists in the mobile device's local cache **and is currently Active**. Registration against an unknown or inactive Barang is not permitted on mobile. The Main Office performs authoritative validation during synchronization and may still reject a registration if Barang status changed after the last mobile synchronization.

### Technical Questions

- ~~**TQ-1 [BLOCKING]** — Which mechanism carries a mobile registration to the Main Office?~~ **RESOLVED (GAP-007):** Cloud `BarcodeRegistrationRequest` staging, transferred by `j07-btrade-sync` to the Main Office.
- ~~**TQ-2 [BLOCKING]** — Bind the tenant (`ServerId`) to the authenticated identity rather than accepting a client-supplied value.~~ **RESOLVED (ADR-007):** users are not permanently bound to one `ServerId`; during authentication the user selects an operational location (Office or Gudang) and the session is bound to it. `ServerId` is resolved server-side from the selected location and is **never** supplied by application commands. Example mapping: Gudang Gamping → `JOGJA`, Gudang Concat → `JOGJA`, Gudang Magelang → `MGL`. Subsequent API requests operate within the authenticated location context. Legacy route-based `ServerId` endpoints may migrate later.
- ~~**TQ-3 [BLOCKING]** — Is barcode synchronization full-snapshot or incremental, and how is "publish only Active" applied?~~ **RESOLVED:** incremental synchronization, not full-snapshot replacement. Active created → upsert; Active modified → upsert; deactivated → remove from Cloud. Cloud and mobile caches contain only Active barcodes. The "publish only Active" rule is enforced by the synchronization process, not by downstream clients.
- ~~**TQ-4** — Which sync client is authoritative: `j07-btrade-sync` or the older `j05-btr-distrib\btr.sync`?~~ **RESOLVED (GAP-015):** `j07-btrade-sync` is authoritative; `j05-btr-distrib\btr.sync` is excluded.
- ~~**TQ-5** — What is the barcode lookup contract on the Cloud?~~ **RESOLVED:** the Cloud contract is **bulk synchronization, not operational lookup**. The mobile app keeps a local barcode cache and resolves barcodes locally. Required APIs: `GET /api/barcodes/sync` (all active barcodes for the authenticated operational context) and `POST /api/barcode-registration` (submit registration requests). `ServerId` is derived from the authenticated location context, never client-supplied. A point-lookup API (`GET /api/barcodes/{barcode}`) is **not required for MVP** and may be added later if a proven need emerges.
- ~~**TQ-6** — Should the Cloud read model retain Inactive barcodes for reference, or only Active ones?~~ **RESOLVED (TQ-3):** only Active barcodes are published; caches contain only Active.
- ~~**TQ-7 [BLOCKING]** — What platform/technology stack will the new Warehouse Mobile App use?~~ **RESOLVED:** reuse the `BTrade3` stack — Kotlin, Jetpack Compose, Material 3, Navigation Compose, Room, Retrofit, OkHttp, Gson, DataStore — plus CameraX (camera), Google ML Kit Barcode Scanning (recognition), and WorkManager (background sync). This satisfies scanning, offline operation, local caching, synchronization queues, and authenticated API access.

### Operational Questions

- ~~**OQ-1** — What scanning hardware is used?~~ **RESOLVED (GAP-006 / ADR-001):** Desktop uses keyboard-wedge (HID) scanners with no vendor SDK; Mobile uses camera-based scanning. Domain/business logic stays scanner-agnostic.
- ~~**OQ-2** — What synchronization cadence and availability are required for barcode lookup to be acceptable offline?~~ **RESOLVED:** cache-first offline model. Synchronization occurs during login and when manually requested by the user; the app remains fully operational offline using its local RoomDB cache. A BGud client synchronizes only the Barang and Barcode data belonging to the Office (`ServerId`) associated with the selected Warehouse. Newly registered barcodes may not be immediately visible on other devices and become available after the next successful synchronization; this delay is acceptable for Barcode Registry.
- ~~**OQ-3** — Is there any external source of existing barcode data to import?~~ **RESOLVED:** no external barcode source exists. The registry is populated entirely through user registration in BTR Desktop and BGud Mobile. No migration, import, or Principal-supplied barcode feed in Phase 1. Future import capability may be added if Principals/suppliers provide structured barcode master data, but that is outside the current initiative.
- ~~**OQ-4** — Who operates/maintains the registry?~~ **RESOLVED:** the Office Admin function in each Office operationally maintains the Barcode Registry: review registration issues reported by warehouse users; correct mappings; activate/deactivate records; run daily master-data synchronization using `j07-btrade-sync`; verify successful Barang/Barcode sync to Cloud; coordinate with IT when failures are not operationally resolvable. Each Office Admin runs `j07-btrade-sync` at the start of each business day as part of the existing master-data synchronization SOP. Office JOGJA Admin maintains JOGJA data; Office MGL Admin maintains MGL data. IT/System Administrator responsibilities are limited to application deployment, database maintenance, technical troubleshooting, and defect resolution.

---

## 10. Implementation Impact Inventory

### Backend

- `src/j05-btr-distrib/btr.domain` — barcode aggregate/entity and key.
- `src/j05-btr-distrib/btr.application` — builder, writer, validator, DAL contract(s), queries (lookup, list-by-item, maintenance).
- `src/j05-btr-distrib/btr.infrastructure` — barcode DAL(s).
- `src/j06-pkl-btrade-api/btrade.domain` — barcode read model; `BarcodeRegistrationRequest` staging model (decided, GAP-007; integration queue, excluded from snapshot replacement per BR-014/GAP-016).
- `src/j06-pkl-btrade-api/btrade.application` — barcode lookup query, barcode sync command, and registration-request submission (decided, GAP-007).
- `src/j06-pkl-btrade-api/btrade.infrastructure` — barcode DAL(s), including staging-request DAL.
- `src/j07-btrade-sync` — authoritative sync client (GAP-015); barcode sync service, DAL, model, and `SyncForm` wiring; transfer of staged registration requests to the Main Office (decided, GAP-007). `src/j05-btr-distrib/btr.sync` is excluded from the target architecture.
- **New Warehouse Mobile App** (new project; stack per TQ-7) — barcode data layer, repository, API contract, offline cache and request queue, and UI (BQ-1: Barcode Registry is its first module).
- `src/j07-btr-gudang` — standalone Barang lookup + local offline cache reusing the existing Barang sync/API (decided, GAP-013/ADR-005); only if this client remains a target rather than being superseded by the new app.
- `src/BTrade3` — **not in scope** for barcode (Salesman application, BQ-1).

### Database

- `src/j05-btr-distrib/btr.sql` — new `BTR_BrgBarcode` (or equivalent) table: PK, `BarcodeValue` stored as normalized text with a case-insensitive unique constraint (BQ-5), index on `BrgId`, optional `BrgSatId`, `IsActive`, audit columns; `.sqlproj` registration; idempotent upgrade script. (Packaging Level removed — GAP-009.)
- `src/j06-pkl-btrade-api/btrade.sqldb` — new barcode read-model table keyed with `ServerId` and a unique `(ServerId, BarcodeValue)` constraint (GAP-010/ADR-004); `.sqlproj` registration; staging/acknowledgement structures for `BarcodeRegistrationRequest` (decided, GAP-007).
- `src/j07-btr-gudang/BtrGudang.SqlDb` — local Barang cache table (offline lookup, GAP-013/ADR-005) and barcode cache; scoped to the Office (`ServerId`) of the selected Warehouse (OQ-2).
- `src/BTrade3` Room — **not in scope** for barcode (Salesman app, BQ-1).
- **New Warehouse Mobile App** local store — barcode + Barang cache and offline registration-request queue (cache-first per ADR-006; stack per TQ-7).

### Frontend

- `src/j05-btr-distrib/btr.distrib` — barcode maintenance screen, Item barcode list, menu seed (`BTR_Menu`), browser.
- **New Warehouse Mobile App** — barcode lookup/registration screens with cache-first scan resolution and offline request capture (ADR-006), camera scan input, sync status (BQ-1).
- `src/BTrade3` — **not in scope** for barcode (Salesman app, BQ-1).
- `src/j07-btr-gudang/BtrGudang.Winform` — standalone Barang search/select screen and offline cache, if in scope (GAP-013/ADR-005).

### Integration

- New sync/publish path Main Office → Cloud, surfaced to mobile as `GET /api/barcodes/sync` (all active barcodes for the authenticated operational context, TQ-3/TQ-5).
- New mobile registration submission endpoint `POST /api/barcode-registration` (Cloud staging; `ServerId` derived from authenticated location, TQ-5).
- No point-lookup endpoint for MVP (`GET /api/barcodes/{barcode}` deferred).
- Reuse of `j07-btrade-sync` (authoritative client) and the existing Barang sync/Barang-lookup infrastructure.

### Security

- Enforced JWT authentication on Cloud write endpoints; anonymous writes prohibited (ADR-002).
- Mobile authentication against `pkl.btrade.api` before submitting operational commands (ADR-003).
- Tenant binding of `ServerId` to authenticated session (ADR-007 — user selects an operational location at authentication; `ServerId` resolved server-side, never client-supplied).
- Transport security for mobile and sync clients.
- Access control per role matrix (BQ-6): Warehouse Officer — lookup/register/correct; Office Admin — plus activate/deactivate; System Administrator — all; no approval step.

---

## 11. Planning Readiness

### Status

```text
READY
```

### Blocking Issues

None. All identified gaps **GAP-001 through GAP-016 are CLOSED** and all blocking
questions are resolved.

Resolved since earlier revisions: **TQ-1** (mobile → Main Office transport,
GAP-007), **BQ-3** (Packaging Level, GAP-009 — removed in favor of optional
`BrgSatId`), **BQ-4** (uniqueness scope, GAP-010 / ADR-004 — tenant-scoped),
**BQ-2** (offline/conflict semantics, GAP-011 / BR-012 / BR-013), **TQ-4**
(authoritative sync client, GAP-015), **BQ-1** (target mobile application —
new Warehouse Mobile App), **BQ-5** (barcode storage/normalization —
symbology-agnostic normalized text), **BQ-6** (roles/approval — no approval;
immediate effect with audit fields), **BQ-7** (mobile registration only
against cached Active Barang; Main Office re-validates at synchronization),
**TQ-2** (tenant binding — user selects an operational location at
authentication; `ServerId` resolved server-side, ADR-007), **TQ-3/TQ-6**
(incremental sync; only Active barcodes published), **TQ-7** (Warehouse
Mobile App stack — `BTrade3` stack + CameraX + ML Kit + WorkManager), **TQ-5**
(Cloud contract = bulk sync + registration submission; no point lookup
for MVP), **OQ-2** (cache-first offline model; sync at login and on manual
request; BGud syncs only the selected Warehouse's Office data), **OQ-3**
(no external barcode source; registry populated by user registration only), and
**OQ-4** (Office Admin maintains the registry and runs daily `j07-btrade-sync`).

**Architecture input note.** The Architecture Creation Skill lists `DOMAIN.md`,
`WORKFLOW.md`, and `GAP-ANALYSIS.md` as mandatory inputs. The Domain exists
(`BARCODE-REGISTRY-DOMAIN.md`); this assessment serves as the closed gap analysis
(GAP-001..016); the domain use cases UC-001..UC-004 describe the workflows. If a
dedicated `WORKFLOW.md` is required by the architecture process, producing it is
the first planning action, not a feasibility blocker.

### Closed Since Previous Revision

- **GAP-001 — CLOSED.** Implement Barcode Registry as a new greenfield domain in
  `btr.distrib`; no migration strategy required; no legacy compatibility
  constraints. GAP-001 was already recorded as low risk and was not a planning
  blocker, so its closure does not change the planning readiness status — but it
  removes the only open gap on the feasible Main Office registry workstream.
- **GAP-002 — CLOSED.** Create a dedicated Barcode table in `btr.sql` with a
  unique `BarcodeValue` constraint and an FK to Item Master (`BTR_Brg`); no
  migration or backfill required. This settles the Main Office persistence
  question. BQ-4 (uniqueness scope) was subsequently closed by GAP-010/ADR-004:
  the `btr.sql` unique constraint applies within the Main Office Product Master
  authority, while the Cloud read model uses `(ServerId, BarcodeValue)`.
- **GAP-003 — CLOSED.** Create a Barcode read model in `btrade.sqldb` and
  `btrade.domain`, populated by synchronizing **active** barcode mappings from
  the Main Office through the sync client; the Cloud stays lookup-only and never
  authoritative. This decides that only active mappings are published and
  reinforces the one-way model. Sync granularity was subsequently decided
  incremental (TQ-3), and TQ-6 is resolved (Cloud holds only Active). The
  resolution's "btrade.sync" is the authoritative client `j07-btrade-sync`
  (confirmed by GAP-015); `j05-btr-distrib\btr.sync` is excluded.
- **GAP-004 — CLOSED.** Extend the existing sync process to replicate Barcode
  Registry data Main Office → Cloud, reusing the existing synchronization
  architecture and introducing no new synchronization infrastructure. This
  settles the transport approach (no new plumbing) and confirms `j07-btrade-sync`
  as the client (GAP-015). Sync granularity was subsequently decided incremental
  (TQ-3).
- **GAP-005 — CLOSED.** Add a Barcode Registry maintenance screen and an Item
  Barcode tab in Master Barang; no approval workflow, dashboard, or migration
  tooling. This settles the Desktop UI surface and confirms BQ-6's "no approval"
  position for the Desktop channel. The mobile UI surface is provided by the new
  Warehouse Mobile App (BQ-1).
- **GAP-006 — CLOSED.** Add barcode acquisition to Desktop (keyboard-wedge) and
  Mobile (camera-based); domain and business logic stay scanner-technology
  agnostic. This resolves OQ-1's acquisition model. The mobile camera path
  belongs to the new Warehouse Mobile App (BQ-1), whose stack is TQ-7. Desktop
  acquisition is
  recorded in `adrs/ADR-001-desktop-keyboard-wedge-scanning.md`.
- **GAP-007 — CLOSED.** Introduce a `BarcodeRegistrationRequest` staging area in
  the Cloud; Mobile submits requests to the Cloud API; the sync client transfers
  them to the Main Office for validation and authoritative creation; the Cloud
  never becomes authoritative. This closes **TQ-1** and selects the previously
  assessed Option B. Tenant binding was subsequently closed by ADR-007 (TQ-2).
  Offline request conflict semantics were subsequently closed by
  GAP-011/BR-012/BR-013. The staged request must use explicit post-commit
  acknowledgement, not the existing "acknowledge during GET" pattern.
- **GAP-008 — CLOSED.** Enforce JWT authentication on all cloud write endpoints
  and introduce a token-issuing mechanism using existing BTR user accounts;
  authentication is mandatory for all mobile-originated writes, including
  Barcode Registration Requests. Decision records: ADR-002 (authenticated JWT
  write endpoints; anonymous writes prohibited), ADR-003 (mobile authenticates
  against `pkl.btrade.api`), and ADR-007 (tenant derived from authenticated
  identity; TQ-2). Authentication was the security prerequisite for the write
  path; transport security and coordinated client updates remain.
- **GAP-009 — CLOSED.** Align terminology with Product Master (`BrgId`,
  `BrgCode`, `BrgName`), remove `PackagingLevel`, and reuse `BTR_BrgSatuan` via
  an optional `BrgSatId`. This resolves **BQ-3** and removes it from the blocker
  list. Follow-up: `docs/work/barcode-registry/BARCODE-REGISTRY-DOMAIN.md`
  must be updated to remove Packaging Level and align names, so domain knowledge
  stays synchronized with the decision.
- **GAP-010 — CLOSED.** Uniqueness is tenant-scoped, not platform-global: Main
  Office enforces uniqueness within its Product Master authority; Cloud read
  models use `(ServerId, BarcodeValue)`. Domain rule restated from "globally
  unique" to "unique within an authoritative Product Master scope". Decision
  record: ADR-004. This resolves **BQ-4** and removes it from the blocker list.
  Follow-up: update `BARCODE-REGISTRY-DOMAIN.md` (BR-001 and the aggregate
  consistency boundary).
- **GAP-011 — CLOSED.** Offline registrations are requests, not authoritative
  updates; the Main Office validates each at synchronization; on conflict the
  authoritative state wins and the request is rejected; no merge strategy.
  Recorded as **BR-012** and **BR-013** in
  `docs/work/barcode-registry/BARCODE-REGISTRY-DOMAIN.md`. This resolves
  **BQ-2** and removes it from the blocker list. Follow-up for architecture: a
  rejected request must be reported back to the requesting device/user.
- **GAP-012 — CLOSED.** Use the existing SSDT deployment approach; add Barcode
  Registry schema objects to the Main Office and Cloud database projects and
  provide idempotent upgrade scripts for existing installations; no migration
  framework. This removes schema-deployment uncertainty and confirms the
  implementation impact inventory's database entries.
- **GAP-013 — CLOSED.** Expose standalone Barang lookup to warehouse clients by
  reusing the existing Barang synchronization/API; mobile may search and select
  Barang independently of Packing Orders and keep a local offline cache.
  Decision record: ADR-005 (Barang Master is a shared reference dataset for
  warehouse mobility and may be synced to mobile for offline lookup). This
  supports GAP-006/GAP-007 and adds a Barang offline cache to the impact
  inventory.
- **GAP-014 — CLOSED.** Introduce barcode-enabled screens for Desktop and
  Mobile; mobile lookup is cache-first against a locally synchronized barcode
  cache (no network required); unknown barcodes are captured as offline
  registration requests. Decision record: ADR-006 (mobile barcode lookup is
  cache-first; Cloud APIs are synchronization sources, not runtime
  dependencies). This also addresses OQ-2 (offline lookup availability) at the
  design level; cache refresh/staleness behavior remains an architectural detail.
- **GAP-015 — CLOSED.** `j07-btrade-sync` is the authoritative and maintained
  synchronization client; all Barcode Registry synchronization is implemented
  there; `j05-btr-distrib\btr.sync` is excluded from the target architecture.
  This resolves **TQ-4** and removes duplicate-client ambiguity from the impact
  scope.
- **GAP-016 — CLOSED.** Barcode master data keeps the existing sync strategy;
  `BarcodeRegistrationRequest` is an integration queue artifact that never
  participates in snapshot replacement; master-data replication and request
  processing are separate synchronization patterns. Recorded as **BR-014** in
  `docs/work/barcode-registry/BARCODE-REGISTRY-DOMAIN.md`. This eliminates the
  data-loss risk where a full-snapshot publish could erase remote registrations.
- **BQ-1 — RESOLVED.** Create a new **Warehouse Mobile App** as the target client
  for barcode lookup/registration and future warehouse modules; Barcode Registry
  is its first module. Warehouse mobility must not be coupled to the Salesman
  application (`BTrade3`). This removes the largest scope ambiguity and changes
  the impact inventory from `BTrade3` to a new client. New blocking question
  **TQ-7** (platform/stack of the new app) is added.
- **BQ-5 — RESOLVED.** Store barcode values as normalized text, symbology-agnostic:
  trim whitespace; remove CR/LF and tab; preserve leading zeros; compare
  case-insensitively. Phase 1 supports any scannable/typed barcode (EAN-13, UPC,
  Code128, QR, internal). No format or check-digit validation in Phase 1. This
  confirms storage as text and a case-insensitive uniqueness comparison; it
  should be captured as a domain rule for the registry.
- **BQ-6 — RESOLVED.** No approval workflow. Warehouse Officer may lookup,
  register, and correct mappings; Office Admin additionally may activate and
  deactivate; System Administrator may perform all functions. Registrations and
  corrections are effective immediately after Main Office validation, with
  accountability via audit fields (`CreatedBy`, `CreatedDate`, `ModifiedBy`,
  `ModifiedDate`). This matches `BARCODE-REGISTRY-DOMAIN.md` §4 and its "no
  approval workflow" statement.
- **BQ-7 — RESOLVED.** Mobile registration is allowed only against a Barang that
  exists in the device's local cache and is currently Active; unknown or inactive
  Barang is not permitted on mobile. The Main Office re-validates at
  synchronization and may reject if status changed since the last sync. This
  extends BR-004 (registration requires an existing Item) with an Active-state
  and mobile-cache constraint; it should be captured as a domain rule and
  enforced in both the new Warehouse Mobile App and the Main Office.
- **TQ-2 — RESOLVED.** `ServerId` is not supplied by application commands. During
  authentication the user selects an operational location (Office or Gudang); the
  authenticated session is bound to that location; `ServerId` is resolved
  server-side and used for all tenant-scoped operations. Example mapping:
  Gudang Gamping → `JOGJA`, Gudang Concat → `JOGJA`, Gudang Magelang → `MGL`.
  Users are not permanently bound to a single `ServerId`. Subsequent API requests
  operate within the authenticated location context. Legacy endpoints that still
  require `ServerId` in the route may remain and migrate later. Decision record:
  ADR-007.
- **TQ-3 — RESOLVED.** Barcode synchronization is **incremental**, not
  full-snapshot replacement: Active created → upsert; Active modified → upsert;
  deactivated → remove from Cloud. Cloud and mobile caches contain only Active
  barcodes, enforced by the sync process rather than downstream clients. This
  also resolves **TQ-6** (Cloud retains only Active) and confirms that the
  barcode sync deliberately differs from the `Brg` full-snapshot pattern.
- **TQ-7 — RESOLVED.** The new Warehouse Mobile App reuses the `BTrade3`
  technology stack (Kotlin, Jetpack Compose, Material 3, Navigation Compose,
  Room, Retrofit, OkHttp, Gson, DataStore) plus approved libraries CameraX
  (camera), Google ML Kit Barcode Scanning (recognition), and WorkManager
  (background synchronization). This satisfies barcode scanning, offline
  operation, local caching, sync queues, and authenticated API access. This was
  the final planning blocker; Planning Readiness is now **READY**.
- **TQ-5 — RESOLVED.** The Cloud barcode contract is bulk synchronization, not
  operational lookup. Required APIs: `GET /api/barcodes/sync` (all active
  barcodes for the authenticated operational context) and
  `POST /api/barcode-registration` (submit registration requests). `ServerId` is
  derived from the authenticated location context. No point-lookup API is
  required for MVP. Note: these routes are distinct from the existing
  PascalCase `/api/Brg` convention and should be aligned with the API's routing
  conventions during architecture.
- **OQ-2 — RESOLVED.** Cache-first offline model; sync at login and on manual
  request; the app is fully operational offline on its RoomDB cache. A BGud
  client synchronizes only the Barang and Barcode data for the Office
  (`ServerId`) of the selected Warehouse. Newly registered barcodes reach other
  devices on the next successful sync (eventual consistency is acceptable). This
  defines the cache refresh policy left open by ADR-006.
- **OQ-3 — RESOLVED.** No external barcode source exists; the registry is
  populated entirely by user registration in BTR Desktop and BGud Mobile. No
  migration, import, or Principal-supplied feed in Phase 1; future import may be
  added if Principals/suppliers provide structured barcode master data. This
  confirms GAP-001's greenfield/no-backfill position.
- **OQ-4 — RESOLVED.** The Office Admin in each Office maintains the registry
  (review registration issues, correct mappings, activate/deactivate, run daily
  `j07-btrade-sync`, verify Barang/Barcode sync to Cloud, escalate to IT).
  JOGJA Admin maintains JOGJA data; MGL Admin maintains MGL data. IT/System
  Administrator scope is limited to deployment, database maintenance,
  troubleshooting, and defect resolution. Follow-up: capture the daily sync
  procedure in the existing master-data synchronization SOP.

### Planner Guidance

- **All workstreams are unblocked.** Main Office registry, Desktop maintenance,
  Cloud read model, incremental barcode sync, mobile registration request queue,
  and the new Warehouse Mobile App can all be planned.
- **Major dependencies:** Item identity (`BrgId`), existing Item/unit model
  (`BTR_Brg`, `BTR_BrgSatuan`), the authoritative sync client
  (`j07-btrade-sync`), the Cloud read model, and enforced JWT auth with
  session-bound location (ADR-002/003/007).
- **Sequencing concerns (not phases):** Main Office registry before barcode sync;
  request processing is an integration queue separate from incremental snapshot
  sync (GAP-016/BR-014); authentication and tenant resolution precede any write
  path; the new mobile app precedes mobile lookup/registration.
- **Review concerns:** prove the Cloud never serves lookup from unvalidated
  registration data; prove uniqueness is enforced in the Main Office; prove only
  Active barcodes are published and cached; prove no Product Master data is
  modified (BR-008); prove commands do not accept a client-supplied `ServerId`.

Do not create implementation phases or slices in this assessment.

---

## Appendix A — Evidence Index

| Claim | Evidence |
| ----- | -------- |
| No barcode code/SQL exists in the repository | Repository-wide search for `barcode`/`barkod` returns only documentation |
| Item master shape | `src/j05-btr-distrib/btr.sql/Tables/BrgContext/BTR_Brg.sql`; `btr.domain/BrgContext/BrgAgg/BrgModel.cs` |
| Item units | `src/j05-btr-distrib/btr.sql/Tables/BrgContext/BTR_BrgSatuan.sql` |
| Main Office registry pattern | `btr.application/BrgContext/BrgAgg/{BrgBuilder,BrgWriter,GetBrgQuery}.cs`; `btr.infrastructure/BrgContext/BrgAgg/BrgDal.cs`; `btr.distrib/InventoryContext/BrgAgg/BrgForm.cs` |
| Cloud item read model | `src/j06-pkl-btrade-api/btrade.domain/SalesFeature/BrgType.cs`; `btrade.sqldb/SalesContext/BTRADE_Brg.sql` |
| Cloud replace-by-`ServerId` sync | `btrade.application/UseCase/BrgSyncCommand.cs`; `btrade.infrastructure/Repository/BrgDal.cs` (`Delete(IServerId)`) |
| Sync upload pattern | `src/j07-btrade-sync/j07-btrade-sync/Service/BrgSyncService.cs`; `Repository/BrgDal.cs`; `Model/BrgType.cs` |
| Mobile client (offline) | `src/BTrade3/.../database/AppDatabase.kt`; `repository/SyncRepository.kt`; `network/ApiService.kt` |
| Warehouse client is WinForms, packing only | `src/j07-btr-gudang` project files; `BtrGudang.Winform/Forms/*`; `BtrGudang.Winform/Services/PackingOrderDownloaderSvc.cs` |
| Auth not enforced in Cloud | `btrade.webapi` controllers (no `[Authorize]`); `Configurations/PresentationService.cs`; `appsettings.json` |
| Schema application model | `src/j05-btr-distrib/btr.sql/btr.sql.sqlproj`; `src/j06-pkl-btrade-api/btrade.sqldb/btrade.sqldb.sqlproj` |
| Domain authority and rules | `docs/work/barcode-registry/BARCODE-REGISTRY-DOMAIN.md` §1, §7, §8 |
| Architecture prerequisites | `docs/skills/architecture-creation-skill.md` (Required Inputs) |
