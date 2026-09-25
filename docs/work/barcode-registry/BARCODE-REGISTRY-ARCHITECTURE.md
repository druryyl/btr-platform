# ARCHITECTURE

# Barcode Registry

| Field | Value |
| ----- | ----- |
| Deliverable | `BARCODE-REGISTRY-ARCHITECTURE.md` |
| Location | `docs/work/barcode-registry/BARCODE-REGISTRY-ARCHITECTURE.md` |
| Method | `docs/skills/architecture-creation-skill.md` |
| Systems | `j05-btr-distrib` (BTR Desktop / Main Office), `j06-pkl-btrade-api` (Cloud API), `j07-btrade-sync` (synchronization), `BGud` (new Android warehouse app) |
| Author role | Architect (Architecture only) |
| Status | **DRAFT — see §24** |

> This document is a realization artifact. It translates approved business
> decisions into a technical blueprint. It does not define business rules,
> implementation plans, implementation slices, migrations, deployment
> procedures, or testing procedures.

---

## 1. Executive Summary

### Purpose

Realize the Barcode Registry business capability: a single authoritative
repository of barcode-to-Item (Barang) mappings, owned by the Main Office
Server, maintained from BTR Desktop and from the new BGud Android warehouse
application, and published one-way to a Cloud read model so that warehouse
mobile clients can resolve scanned barcodes offline.

The architecture realizes five concurrent realizations:

1. **Authoritative registry** on the Main Office Server (`j05-btr-distrib`),
   following the existing `Brg` master-data layering.
2. **Desktop maintenance surface** in BTR Desktop (registration, correction,
   activation, deactivation, Item barcode listing).
3. **Cloud read model + integration queue** in `j06-pkl-btrade-api`
   (active-only barcode read model; `BarcodeRegistrationRequest` staging).
4. **Synchronization** implemented in `j07-btrade-sync` only
   (incremental Main Office → Cloud publish; pull-and-relay of registration
   requests Cloud → Main Office).
5. **BGud**, a new Android application (Kotlin / Jetpack Compose / Room /
   Retrofit / CameraX / ML Kit / WorkManager) providing cache-first offline
   barcode lookup and offline registration request capture.

### Status

```text
DRAFT
```

### Inputs

| Input | Artifact |
| ----- | -------- |
| DOMAIN | `docs/work/barcode-registry/BARCODE-REGISTRY-DOMAIN.md` |
| WORKFLOW | `docs/work/barcode-registry/BARCODE-REGISTRY-DOMAIN.md` §10 (UC-001..UC-004 and the Barcode Synchronization Workflow) — no separate `BARCODE-REGISTRY-WORKFLOW.md` exists; see §24 |
| GAP-ANALYSIS | `docs/work/barcode-registry/BARCODE-REGISTRY-FEASIBILITY-ASSESMENT.md` §5 (GAP-001..GAP-016, all CLOSED), §9 (BQ-1..BQ-7, TQ-1..TQ-7, OQ-1..OQ-4, all RESOLVED) |
| ADRs | `adrs/ADR-001`…`adrs/ADR-007` |
| UX | `docs/work/barcode-registry/BARCODE-REGISTRY-UX-BLUEPRINT.md` |
| Supporting evidence | `src/j05-btr-distrib`, `src/j06-pkl-btrade-api`, `src/j07-btrade-sync`, `src/BTrade3` (stack reference only) |

### Architecture Stance

Architecture consumes the decisions above. It does **not** reinterpret them.
Where an input artifact leaves a *technical* realization open, this document
selects one realization and records the evidence used (see §3.1
Interpretation Register) so reviewers can verify objectively.

---

## 2. Architecture Principles

### P-01 — Single Authority

The Main Office Server database (`btr.sql`) is the only place where a Barcode
mapping becomes authoritative. No other store may validate, mint, or mutate
authoritative barcode state.

*Traceable to:* DOMAIN §1 Authority Model; BR-011; BR-013; GAP-003; GAP-007.

### P-02 — Cloud Is a Read Model Plus a Transport Queue

The Cloud stores two things and nothing else for this feature:

1. an **active-only** barcode read model (published, never authored), and
2. a **staging queue** of unvalidated registration requests.

The Cloud never serves lookup from the staging queue.

*Traceable to:* DOMAIN §1; GAP-003; GAP-007; BR-012; BR-014; TQ-6.

### P-03 — One Transaction Per Authoritative Change

A barcode registration, correction, activation, or deactivation is committed
in a single Main Office transaction (`TransHelper.NewScope()`), consistent with
`BrgWriter.Save`.

*Evidence:* `btr.application/BrgContext/BrgAgg/BrgWriter.cs`.

### P-04 — Persistence Follows Existing Conventions

New schema objects follow existing SSDT conventions: `CREATE TABLE` with named
`DF_` default constraints, clustered `PK_`, `IX_` indexes, `UX_` unique
indexes, no migration framework, idempotent upgrade scripts.

*Evidence:* `btr.sql/Tables/BrgContext/BTR_Brg.sql`; `btr.sql/Scripts/Upgrade_*.sql`;
GAP-012.

### P-05 — Mirror the `Brg` Layering

The barcode aggregate is layered exactly like the Item (`Brg`) aggregate:
domain model + key interface → application builder/writer/validator/DAL
contract/queries → infrastructure Dapper DAL → WinForms form.

*Traceable to:* GAP-005; feasibility §7.1.

### P-06 — No Client-Supplied Tenant

Barcode Registry commands and endpoints never accept `ServerId` from a caller.
Tenant is resolved server-side from the authenticated session's bound
operational location.

*Traceable to:* ADR-007; §9.

### P-07 — Cache-First Mobile

A scan is resolved against the local Room cache. No scan triggers a network
call. The Cloud is a synchronization source, not a runtime dependency.

*Traceable to:* ADR-006; GAP-014; UX-001.

### P-08 — Offline Writes Are Queued Intent, Not State

Offline registration and correction are local queue rows. They become
authoritative only when the Main Office accepts them during synchronization.

*Traceable to:* BR-012; BR-013; GAP-011; GAP-016.

### P-09 — Master-Data Replication and Request Processing Are Separate Flows

Barcode replication (Main Office → Cloud publish) and registration request
processing (Cloud → Main Office relay) are two distinct synchronization
patterns with distinct endpoints, payloads, and lifecycles. Neither may reuse
the other's semantics. Snapshot/replace logic is never applied to the request
queue.

*Traceable to:* BR-014; GAP-016; TQ-3.

### P-10 — Scanner-Agnostic Domain

The domain and application layers accept a barcode value string. Acquisition
mechanism (keyboard wedge on Desktop, camera on Mobile) is isolated in the
client.

*Traceable to:* ADR-001; GAP-006; BQ-5.

### P-11 — Minimal New Surface

No new synchronization infrastructure, no new Item identity model, no new unit
classification model, no new point-lookup endpoint, no migration framework.

*Traceable to:* GAP-004; GAP-009; GAP-012; TQ-5.

---

## 3. Decision Traceability

Decisions consumed from the feasibility assessment and ADRs.

| Source | Decision | Realized in |
| ------ | -------- | ----------- |
| GAP-001 | Greenfield domain in `btr.distrib`; no legacy migration/backfill | §4.1, §10.2 |
| GAP-002 | Dedicated barcode table in `btr.sql`; unique `BarcodeValue`; FK to `BTR_Brg` | §6.1 |
| GAP-003 | Cloud read model in `btrade.sqldb`/`btrade.domain`; active-only; never authoritative | §4.2, §6.2 |
| GAP-004 | Reuse existing sync architecture; no new sync infrastructure | §8.1 |
| GAP-005 | Desktop maintenance screen + Item Barcode tab in Master Barang; no approval workflow, no dashboard | §11.1 |
| GAP-006 | Desktop keyboard-wedge; Mobile camera; domain scanner-agnostic | §11, §19, §23 |
| GAP-007 | Cloud `BarcodeRegistrationRequest` staging; sync client relays to Main Office; Cloud never authoritative; explicit post-commit acknowledgement | §4.2, §8.2, §8.3 |
| GAP-008 | Enforce JWT on all cloud write endpoints; token issuance from existing BTR user accounts; no anonymous writes | §9, §4.2 |
| GAP-009 | Terminology aligned to Product Master (`BrgId`, `BrgCode`, `BrgName`); `PackagingLevel` removed; optional unit via `BTR_BrgSatuan`; no second unit model | §3.1 IR-01, §5.1, §6.1 |
| GAP-010 | Uniqueness tenant-scoped; Main Office unique `BarcodeValue`; Cloud unique `(ServerId, BarcodeValue)` | §5.1, §6.1, §6.2 |
| GAP-011 | Offline registration = request; Main Office validates; conflict → authoritative wins, request rejected; no merge | §5.2, §8.2, §8.3 |
| GAP-012 | SSDT deployment; register objects in `btr.sql.sqlproj` and `btrade.sqldb.sqlproj`; idempotent upgrade scripts; no migration framework | §10.1 |
| GAP-013 | Standalone Barang lookup for warehouse clients; reuse existing Barang sync/API; mobile offline cache | §8.4, §11.2 |
| GAP-014 | Barcode-enabled screens Desktop + Mobile; cache-first mobile lookup; unknown barcode → offline registration request | §11.2, §13.2, §14 |
| GAP-015 | `j07-btrade-sync` is the authoritative sync client; `j05-btr-distrib\btr.sync` excluded | §4.3, §23.4 |
| GAP-016 | `BarcodeRegistrationRequest` is an integration queue artifact; never participates in snapshot replication | §5.3, §8.2, P-09 |
| BQ-1 | New Warehouse Mobile App (**BGud**); Barcode Registry is its first module; not coupled to `BTrade3` | §4.4, §11.2 |
| BQ-2 | Offline/conflict semantics (see GAP-011) | §8.3 |
| BQ-3 | Packaging Level removed in favour of optional unit (see GAP-009) | §3.1 IR-01 |
| BQ-4 | Tenant-scoped uniqueness (see GAP-010) | §6 |
| BQ-5 | Barcode stored as normalized text; symbology-agnostic; trim, strip CR/LF/TAB, preserve leading zeros, compare case-insensitively; no format/check-digit validation | §5.1 VO-01, §6.1 |
| BQ-6 | No approval workflow. Warehouse Officer: lookup/register/correct. Office Admin: + activate/deactivate. System Administrator: all. Immediate effect; accountability via audit fields | §9.3, §16 |
| BQ-7 | Mobile registration only against a Barang present in the device cache and currently Active; Main Office re-validates at synchronization | §18, §14.2 |
| TQ-1 | Mobile registration transport = Cloud staging + sync relay (see GAP-007) | §8.2 |
| TQ-2 | Tenant from authenticated identity + selected operational location; `ServerId` never client-supplied | §9.2 |
| TQ-3 | Incremental sync; Active create → upsert; Active update → upsert; deactivate → remove; only Active published | §8.1, §6.2 |
| TQ-4 | Authoritative sync client = `j07-btrade-sync` | §4.3 |
| TQ-5 | Cloud contract = `GET /api/barcodes/sync` + `POST /api/barcode-registration`; no point-lookup for MVP | §8.5, §17.2 |
| TQ-6 | Cloud and mobile caches hold only Active barcodes | §6.2, §8.1 |
| TQ-7 | BGud stack: Kotlin, Jetpack Compose, Navigation Compose, Room, Retrofit, OkHttp, Gson, DataStore + CameraX, ML Kit Barcode Scanning, WorkManager | §4.4, §11.2 |
| OQ-1 | Desktop keyboard-wedge; Mobile camera | §11 |
| OQ-2 | Cache-first; sync at login and on manual request; BGud syncs only the selected Warehouse's Office data | §8.4, §15.2 |
| OQ-3 | No external barcode source; registry populated only by user registration | §10.2 |
| OQ-4 | Office Admin maintains the registry and runs daily `j07-btrade-sync`; IT provides technical support only | §10.3, §9.3 |
| ADR-001 | Desktop keyboard-wedge scanning; no vendor SDK | §11.1, §19.1 |
| ADR-002 | JWT enforced on all cloud write endpoints; anonymous writes prohibited | §9.1, §9.2 |
| ADR-003 | Mobile authenticates against `pkl.btrade.api` before operational commands | §9.1, §13.2 |
| ADR-004 | Uniqueness scope: Main Office authority vs Cloud `(ServerId, BarcodeValue)` | §6 |
| ADR-005 | Barang Master is a shared reference dataset; may be synced to mobile for offline lookup | §8.4 |
| ADR-006 | Cache-first mobile barcode lookup; Cloud is a sync source, not a runtime dependency | §7.2, §11.2 |
| ADR-007 | Tenant isolation from authenticated identity; location selected at authentication; mapping is data, not client input | §9.2, §6.3 |

### 3.1 Interpretation Register

Technical realizations that the input artifacts left open. Each is a *technical*
selection, not a business decision. Each is recorded here so a reviewer can
accept or reject it without re-deriving the architecture.

| ID | Open point left by inputs | Selected realization | Evidence / traceability |
| -- | ------------------------- | -------------------- | ----------------------- |
| IR-01 | GAP-009: "The meaning of `BrgSatId` (the `Satuan` value, or a new surrogate) must be settled during architecture." Closed decision restates Packaging Level as optional with values Small Unit / Big Unit. | `BrgSatId` is realized as the existing `BTR_BrgSatuan.Satuan` value. No surrogate key is introduced. The barcode mapping carries an optional `Satuan` column referencing `BTR_BrgSatuan(BrgId, Satuan)`. The two supported packaging levels are resolved from the mapped Item's own `BTR_BrgSatuan` rows: **Small Unit** = the row with `Conversion = 1`; **Big Unit** = the row with `Conversion > 1`. | `btr.sql/Tables/BrgContext/BTR_BrgSatuan.sql` (PK `(BrgId, Satuan)`, no surrogate); `j07-btrade-sync/Repository/BrgDal.cs` (`cc.Conversion > 1` → `SatBesar`; `dd.Conversion = 1` → `SatKecil`); GAP-009 §3 "no second unit-classification model" |
| IR-02 | Primary key type for the new barcode table | `BrgBarcodeId VARCHAR(26)` populated with a ULID. Matches the existing ULID-keyed table convention and the `Ulid` package already present in the platform. | `btr.sql/.../BTR_VisitPlanException.sql` (`VARCHAR(26)`); `Ulid` package in `j07-btrade-sync` and `btrade.application` |
| IR-03 | Case-insensitive uniqueness mechanism (BQ-5) | A persisted computed column `BarcodeValueKey AS UPPER(LTRIM(RTRIM(BarcodeValue)))` carries the unique index, so case-insensitive uniqueness does not depend on database collation. | BQ-5 ("compare case-insensitively"); GAP-002 (unique `BarcodeValue`); `UX_` index convention in `btr.sql` |
| IR-04 | Incremental change detection (TQ-3) — no change-flag mechanism exists for master data | `BTR_BrgBarcode.RowVer ROWVERSION` provides a monotonic change feed; the sync client persists the last processed watermark using the existing `RegistryHelper` pattern. | TQ-3 ("incremental"); feasibility §3.4 ("no incremental/change-flag mechanism for master data"); `j07-btrade-sync/Shared/RegistryHelper.cs` (existing `WriteString`/`ReadString` state store) |
| IR-05 | How the Cloud verifies "existing BTR user accounts" when it has no user store | A credential projection (`BTRADE_User`) is replicated Main Office → Cloud by `j07-btrade-sync` using the existing master-data uploader pattern; the Cloud verifies SHA-256(password) against the projection exactly as BTR Desktop does, then issues the JWT. | GAP-008 ("token-issuing mechanism using existing BTR user accounts"); `btr.distrib/SharedForm/LoginForm.cs` (`PasswrodText.Text.HashSha256() == user.Password`); `BTR_User(UserId VARCHAR(50), Password VARCHAR(64), RoleId VARCHAR(5))`; existing `BrgSyncService` uploader pattern |
| IR-06 | Where activation/deactivation is realized | On the BTR Desktop maintenance screen (SCR-DESK-001). The BGud screen inventory follows `BARCODE-REGISTRY-UX-BLUEPRINT.md` §4 (Navigation Structure) and §16 (Out of Scope), which define no mobile activate/deactivate screen. | GAP-005 (Desktop maintenance screen); UX Blueprint §4, §9, §10; BQ-6 (role matrix, client-agnostic). See §22 R-07 and §24 |
| IR-07 | Route casing for the two mandated Cloud endpoints vs the API's PascalCase convention | The two mandated routes (`GET /api/barcodes/sync`, `POST /api/barcode-registration`) are implemented verbatim as explicit attribute routes. All other new Barcode Registry routes use the existing PascalCase controller convention (`/api/Barcode`, `/api/BarcodeRegistration`). Two conventions coexist deliberately. | TQ-5 note ("should be aligned with the API's routing conventions during architecture"); ADR-007 §8 (legacy/new convention coexistence accepted) |
| IR-08 | How `j07-btrade-sync` performs authoritative validation without duplicating business logic | The sync client references `btr.application` / `btr.infrastructure` directly and invokes the Main Office application command in-process. Both target .NET Framework 4.8, so the reference is viable. Authoritative validation therefore exists in exactly one place. | `j07-btrade-sync.csproj` and `btr.application.csproj` both `v4.8`; BR-011 ("all barcode uniqueness validation must be performed by Main Office Server"); P-01 |
| IR-09 | Mobile activation state of the local cache after a warehouse change | A queued registration request is submitted only by a session bound to the same operational location under which it was captured. The device stores `warehouseCode` (not `ServerId`) on the queued request for matching. Switching warehouse requires re-authentication; the local cache is replaced for the new tenant and previously queued requests are held, never re-homed. | ADR-007 ("queued offline registrations belong to the location under which they were captured and must not be silently re-homed"); ADR-007 §4 (no client-supplied `ServerId`) |

---

# PART A — BACKEND ARCHITECTURE

---

## 4. Bounded Context Realization

### 4.1 Barcode Registry (Main Office) — Authoritative Context

System: `j05-btr-distrib` (`btr.domain`, `btr.application`, `btr.infrastructure`, `btr.distrib`, `btr.sql`)

#### Responsibilities

* Own the authoritative barcode-to-Item mapping.
* Validate barcode uniqueness within the Product Master authority (BR-011).
* Own the barcode lifecycle (Active / Inactive).
* Own barcode audit fields.
* Accept registration requests relayed from the synchronization client and
  accept or reject them (BR-012, BR-013).
* Expose authoritative queries for Desktop screens and for the
  synchronization change feed.

#### Owns

* `BrgBarcode` aggregate (§5.1).
* `BTR_BrgBarcode` table (§6.1).

#### Depends On

* Product Master (`Brg` aggregate, `BTR_Brg`) — external reference by `BrgId` only.
* Item units (`BTR_BrgSatuan`) — external reference by `(BrgId, Satuan)` only.
* Platform user identity (`BTR_User`) for audit fields.

#### Exposes

* Application commands and queries (§7.1) to `btr.distrib` and to
  `j07-btrade-sync`.
* A change feed (`ListChanged`) consumed by the synchronization client.

#### Explicitly Does Not

* Modify Product Master data in any way (BR-008).
* Accept `ServerId` (no tenancy concept exists in the Main Office database).
* Serve remote lookup.

---

### 4.2 Barcode Lookup (Cloud) — Read Model + Integration Queue

System: `j06-pkl-btrade-api` (`btrade.domain`, `btrade.application`, `btrade.infrastructure`, `btrade.webapi`, `btrade.sqldb`)

#### Responsibilities

* Store the **active-only** barcode read model published from the Main Office.
* Serve bulk barcode download to authenticated mobile clients.
* Stage incoming `BarcodeRegistrationRequest` records without validating them.
* Serve staged requests to the synchronization client and record their outcome.
* Enforce authentication and tenant binding at the API boundary.

#### Owns

* `BarcodeType` read model (§5.2) and `BTRADE_BrgBarcode` table (§6.2).
* `BarcodeRegistrationRequest` integration entity (§5.3) and
  `BTRADE_BarcodeRegistrationRequest` table (§6.3).
* `BTRADE_User` credential projection and `BTRADE_Location` operational
  location mapping (§6.3, IR-05, ADR-007).

#### Depends On

* Main Office, via the synchronization client, for all authoritative content.
* Platform JWT configuration (`Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`).

#### Exposes

* Cloud HTTP contract (§8.5).

#### Explicitly Does Not

* Validate barcode uniqueness.
* Serve barcode lookup from unvalidated staged requests.
* Accept `ServerId` from any caller (ADR-007).
* Author or mutate authoritative barcode state.

---

### 4.3 Barcode Synchronization

System: `j07-btrade-sync` (authoritative; `j05-btr-distrib\btr.sync` excluded per GAP-015)

#### Responsibilities

* Publish Main Office barcode changes to the Cloud incrementally (§8.1).
* Pull staged registration requests from the Cloud and relay them to the Main
  Office for authoritative validation (§8.2).
* Acknowledge each request explicitly after the Main Office transaction commits
  (§8.3).
* Replicate the credential projection used for token issuance (IR-05).

#### Owns

* `BrgBarcodeType` transport model, `BrgBarcodeDal` (Main Office read),
  `BarcodeSyncService`, `BarcodeRegistrationRelayService`.
* The incremental watermark state (IR-04).

#### Depends On

* Main Office database (read) and `btr.application` (in-process command
  execution, IR-08).
* Cloud API.

#### Exposes

* Nothing to other business contexts. Operated by Office Admin (OQ-4).

---

### 4.4 Warehouse Mobility (BGud) — Offline Client

System: `BGud` (new Android application)

#### Responsibilities

* Authenticate the user and establish the operational context (Warehouse →
  Office).
* Maintain a local cache of Barang and Active barcodes for the bound Office.
* Resolve scans cache-first (ADR-006).
* Capture registration and correction intent locally for later submission.
* Present synchronization state to the user.
* Expose Register Barcode from within **Barcode Registry** (administrative
  destination); Register Barcode is no longer a Home-level quick action
  (BGUD-RETURN-ORDER-NAV-001, GAP-002).

#### BGud Navigation (Updated — BGUD-RETURN-ORDER-NAV-001)

> The BGud Home screen was restructured from a feature launcher to a work
> launcher. The navigation hierarchy below reflects the approved target state.

```text
Home
├─ New Return
├─ Return Orders
├─ Synchronization Status  (clickable → Synchronization screen)
└─ More
     ├─ Barcode Registry
     │    ├─ Search Barcode
     │    ├─ Register Barcode   ← moved from Home (GAP-002)
     │    └─ Edit Barcode
     └─ Settings
```

**Previous Home navigation (pre-BGUD-RETURN-ORDER-NAV-001):**

```text
Home
├─ Scan Barcode
├─ Barcode Registry  (Search, Edit)
├─ Synchronization
└─ Settings
```

Barcode Registry is now a **More** destination, not a primary Home action.
Register Barcode is accessed from within Barcode Registry, not from Home.

#### Owns

* Room database (§6.4), DataStore session state, local request queue.

#### Depends On

* Cloud API for authentication, Barang download, barcode download, and request
  submission.

#### Explicitly Does Not

* Decide barcode validity or uniqueness.
* Hold `ServerId` as an input to any command.
* Act as an authority for any data.
* Expose Register Barcode as a Home-level quick action
  (BGUD-RETURN-ORDER-NAV-001).

---

## 5. Domain Model Realization

### 5.1 Aggregate: `BrgBarcode` (Main Office, Aggregate Root)

Location: `btr.domain/BrgContext/BrgBarcodeAgg/`

```text
BrgBarcodeModel : IBrgBarcodeKey
    BrgBarcodeId      string     // ULID, VARCHAR(26)
    BarcodeValue      string     // normalized text, as captured
    BrgId             string     // external reference → BTR_Brg
    BrgCode           string     // denormalized read convenience from Product Master
    BrgName           string     // denormalized read convenience from Product Master
    Satuan            string     // optional; '' when no packaging level
    IsAktif           bool       // maps to domain "IsActive"
    CreatedBy         string
    CreatedDate       DateTime
    ModifiedBy        string
    ModifiedDate      DateTime
```

Key interface: `IBrgBarcodeKey { string BrgBarcodeId { get; } }` — mirrors
`IBrgKey`.

#### Purpose

Preserve the authoritative and unique relationship between one physical barcode
value and one Item within the Product Master authority.

#### Aggregate Root

`BrgBarcode`. There is exactly one root; there are no child entities. The unit
(packaging level) is an **attribute** of the mapping, not a child entity,
because `BTR_BrgSatuan` is owned by Product Master and is only referenced
(GAP-009, IR-01).

#### Child Entities

None.

#### Value Objects

**VO-01 — `BarcodeValue` (normalized text)**

Normalization applied by the domain before persistence and before every
comparison (BQ-5):

```text
1. Remove CR, LF and TAB characters
2. Trim leading/trailing whitespace
3. Preserve leading zeros
4. Preserve internal characters unchanged
5. Comparison key = UPPER(normalized value)
```

No symbology, length, or check-digit validation is performed (BQ-5).
The domain object is scanner-agnostic (ADR-001, GAP-006).

**VO-02 — `Satuan` (optional packaging level)**

Optional. `''` means "no packaging level recorded" (BR-005). When present, it
is one of the mapped Item's own `BTR_BrgSatuan.Satuan` values. The two
supported packaging levels are:

| Packaging Level | Source |
| --------------- | ------ |
| Small Unit | the Item's `BTR_BrgSatuan` row with `Conversion = 1` |
| Big Unit | the Item's `BTR_BrgSatuan` row with `Conversion > 1` |

(IR-01; evidence `j07-btrade-sync/Repository/BrgDal.cs`.)

#### Invariants

* **INV-01** — `BarcodeValue` is mandatory and non-empty after normalization.
* **INV-02** — Uniqueness of `BarcodeValue` holds within the Product Master
  authority **across all lifecycle states** (Active and Inactive). Rationale:
  BR-001/ADR-004 define uniqueness within the authority, and BR-006 retains
  inactive barcodes for historical reference. Re-registering an existing value
  against a different Item is therefore not a new registration; the existing
  mapping is surfaced instead, which is exactly the "Duplicate Barcode → View
  Existing Mapping" behaviour specified by the UX blueprint.
* **INV-03** — A barcode maps to exactly one Item (BR-003).
* **INV-04** — The referenced `BrgId` must exist (BR-004).
* **INV-05** — When `Satuan` is present, `(BrgId, Satuan)` must exist in
  `BTR_BrgSatuan`.
* **INV-06** — Registration creates the barcode in the Active state (DOMAIN §8).
* **INV-07** — Correction changes `BrgId` and/or `Satuan` only; it never changes
  activation state (DOMAIN §8).
* **INV-08** — Deactivate moves Active → Inactive; Activate moves Inactive →
  Active. Inactive barcodes are never deleted (BR-006).
* **INV-09** — Product Master data is never modified (BR-008).

#### Repository

`IBrgBarcodeDal` (application layer contract), implemented by
`BrgBarcodeDal` (infrastructure, Dapper). Composition:

```text
IInsert<BrgBarcodeModel>
IUpdate<BrgBarcodeModel>
IGetData<BrgBarcodeModel, IBrgBarcodeKey>
IListData<BrgBarcodeModel>
```

Specialized members:

```text
BrgBarcodeModel  GetByValue(string barcodeValueKey)
IEnumerable<BrgBarcodeModel> ListByBrg(IBrgKey brg)
IEnumerable<BrgBarcodeModel> ListChanged(byte[] watermark)   // RowVer > watermark
bool ExistsByValue(string barcodeValueKey)
```

---

### 5.2 Cloud Read Model: `BarcodeType`

Location: `btrade.domain/BarcodeFeature/`

```text
public record BarcodeType(
    string BrgBarcodeId,
    string BarcodeValue,
    string BrgId,
    string BrgCode,
    string BrgName,
    string Satuan,
    string ServerId) : IBarcodeKey;

public interface IBarcodeKey : IServerId { string BrgBarcodeId { get; } }
```

#### Purpose

Denormalized, active-only, tenant-partitioned copy of the authoritative
mapping, shaped for bulk download and local caching. It mirrors the existing
`BrgType` shape and the existing `IBrgKey : IServerId` composition.

#### Invariants

* Only Active barcodes exist in this model (TQ-3, TQ-6, BR-007).
* Uniqueness is `(ServerId, BarcodeValue)` (ADR-004).
* It is never authored locally; it is only upserted or removed by the
  synchronization command.

---

### 5.3 Cloud Integration Entity: `BarcodeRegistrationRequest`

Location: `btrade.domain/BarcodeFeature/`

```text
BarcodeRegistrationRequestType : IBarcodeRegistrationRequestKey
    BarcodeRegistrationId   string    // ULID, PK
    ClientRequestId         string    // device-generated idempotency key
    ServerId                string    // resolved server-side from JWT context
    BarcodeValue            string
    BrgId                   string
    Satuan                  string    // '' when absent
    RequestedBy             string    // user id from JWT
    RequestedAt             DateTime
    Status                  string    // PENDING | ACCEPTED | REJECTED
    ProcessedAt             DateTime?
    ProcessedNote           string
```

Key interface, mirroring `IBarcodeKey` (§5.2). It is declared because the DAL
composition mandated in §7.2 (`IDelete<>`, `IGetDataMayBe<>`) requires a key
type for this entity:

```csharp
public interface IBarcodeRegistrationRequestKey
{
    string BarcodeRegistrationId { get; }
}
```

#### Purpose

Transport-only staging of a mobile-originated registration intent
(GAP-007). It is an **integration queue artifact**, not master data (BR-014).

#### Invariants

* **INV-10** — A request is never a lookup source. No query that serves
  lookups reads this entity.
* **INV-11** — `Status` transitions only `PENDING → ACCEPTED` or
  `PENDING → REJECTED`. It is terminal afterwards.
* **INV-12** — `(ServerId, ClientRequestId)` is unique, making submission
  idempotent (§10.6).
* **INV-13** — The Main Office, not the Cloud, decides `ACCEPTED` or
  `REJECTED` (BR-011, BR-013).
* **INV-14** — The request queue never participates in snapshot replacement
  (BR-014, GAP-016).

---

## 6. Persistence Model

### 6.1 Main Office — `BTR_BrgBarcode`

Project: `src/j05-btr-distrib/btr.sql`
File: `btr.sql/Tables/BrgContext/BTR_BrgBarcode.sql` (registered in `btr.sql.sqlproj`)

#### Tables

```sql
CREATE TABLE BTR_BrgBarcode(
    BrgBarcodeId   VARCHAR(26) NOT NULL CONSTRAINT DF_BTR_BrgBarcode_BrgBarcodeId   DEFAULT(''),
    BarcodeValue   VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_BrgBarcode_BarcodeValue   DEFAULT(''),
    BarcodeValueKey AS UPPER(LTRIM(RTRIM(BarcodeValue))) PERSISTED,
    BrgId          VARCHAR(6)  NOT NULL CONSTRAINT DF_BTR_BrgBarcode_BrgId          DEFAULT(''),
    Satuan         VARCHAR(7)  NOT NULL CONSTRAINT DF_BTR_BrgBarcode_Satuan         DEFAULT(''),
    IsAktif        BIT         NOT NULL CONSTRAINT DF_BTR_BrgBarcode_IsAktif        DEFAULT(0),

    CreatedBy      VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_BrgBarcode_CreatedBy      DEFAULT(''),
    CreatedDate    DATETIME    NOT NULL CONSTRAINT DF_BTR_BrgBarcode_CreatedDate    DEFAULT('3000-01-01'),
    ModifiedBy     VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_BrgBarcode_ModifiedBy     DEFAULT(''),
    ModifiedDate   DATETIME    NOT NULL CONSTRAINT DF_BTR_BrgBarcode_ModifiedDate   DEFAULT('3000-01-01'),

    RowVer         ROWVERSION     NOT NULL,

    CONSTRAINT PK_BTR_BrgBarcode PRIMARY KEY CLUSTERED (BrgBarcodeId)
)
GO

CREATE UNIQUE INDEX UX_BTR_BrgBarcode_BarcodeValueKey
    ON BTR_BrgBarcode(BarcodeValueKey)
    WITH(FILLFACTOR=75)
GO

CREATE INDEX IX_BTR_BrgBarcode_BrgId
    ON BTR_BrgBarcode(BrgId, BrgBarcodeId)
    WITH(FILLFACTOR=75)
GO

CREATE INDEX IX_BTR_BrgBarcode_RowVer
    ON BTR_BrgBarcode(RowVer)
GO

ALTER TABLE BTR_BrgBarcode
    ADD CONSTRAINT FK_BTR_BrgBarcode_BTR_Brg
    FOREIGN KEY (BrgId) REFERENCES BTR_Brg(BrgId)
GO
```

#### Primary Keys

`BrgBarcodeId` — ULID `VARCHAR(26)` (IR-02).

#### Foreign Keys

`BrgId → BTR_Brg(BrgId)`, as required by GAP-002.

> Note: existing BTR tables declare no FK constraints and enforce integrity at
> the application level (feasibility §3.3). The FK above is declared because
> GAP-002 requires it. Application-level existence validation (INV-04) remains
> mandatory and is the primary defence; recorded as risk R-05.

#### Unique Constraints

Unique index on the persisted computed `BarcodeValueKey` — case-insensitive,
collation-independent, scoped to the Product Master authority (ADR-004, BQ-5,
IR-03).

#### Indexes

* `UX_BTR_BrgBarcode_BarcodeValueKey` — uniqueness and lookup by value.
* `IX_BTR_BrgBarcode_BrgId` — Item barcode listing (UC-004).
* `IX_BTR_BrgBarcode_RowVer` — incremental change feed (IR-04).

#### Soft Delete Strategy

None. Inactivation is a state change (`IsAktif = 0`), never a delete. Inactive
barcodes are retained permanently for historical reference (BR-006).

#### Audit Strategy

`CreatedBy`, `CreatedDate`, `ModifiedBy`, `ModifiedDate` — as defined by
DOMAIN §5 and relied upon by BQ-6 for accountability in the absence of an
approval workflow. `CreatedBy` / `ModifiedBy` hold `BTR_User.UserId`
(`VARCHAR(50)`).

---

### 6.2 Cloud Read Model — `BTRADE_BrgBarcode`

Project: `src/j06-pkl-btrade-api/btrade.sqldb`
File: `btrade.sqldb/BarcodeContext/BTRADE_BrgBarcode.sql` (registered in `btrade.sqldb.sqlproj`)

```sql
CREATE TABLE [dbo].[BTRADE_BrgBarcode]
(
    BrgBarcodeId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRADE_BrgBarcode_BrgBarcodeId DEFAULT(''),
    BarcodeValue VARCHAR(50) NOT NULL CONSTRAINT DF_BTRADE_BrgBarcode_BarcodeValue DEFAULT(''),
    BrgId        VARCHAR(6)  NOT NULL CONSTRAINT DF_BTRADE_BrgBarcode_BrgId        DEFAULT(''),
    BrgCode      VARCHAR(20) NOT NULL CONSTRAINT DF_BTRADE_BrgBarcode_BrgCode      DEFAULT(''),
    BrgName      VARCHAR(60) NOT NULL CONSTRAINT DF_BTRADE_BrgBarcode_BrgName      DEFAULT(''),
    Satuan       VARCHAR(7)  NOT NULL CONSTRAINT DF_BTRADE_BrgBarcode_Satuan       DEFAULT(''),
    ServerId     VARCHAR(5)  NOT NULL CONSTRAINT DF_BTRADE_BrgBarcode_ServerId     DEFAULT(''),

    CONSTRAINT PK_BTRADE_BrgBarcode PRIMARY KEY CLUSTERED (BrgBarcodeId, ServerId),
    CONSTRAINT UX_BTRADE_BrgBarcode_ServerId_BarcodeValue UNIQUE (ServerId, BarcodeValue)
)
GO

CREATE INDEX IX_BTRADE_BrgBarcode_ServerId_BrgId
    ON [dbo].[BTRADE_BrgBarcode] (ServerId, BrgId)
GO
```

#### Primary Keys

Composite `(BrgBarcodeId, ServerId)` — mirrors `PK_BTRADE_Brg (BrgId, ServerId)`.

#### Foreign Keys

None. The Cloud is a read model; referential integrity is not owned here
(consistent with `BTRADE_Brg`).

#### Unique Constraints

`UX_BTRADE_BrgBarcode_ServerId_BarcodeValue` — the Cloud-side expression of
ADR-004: distinct tenants may hold the same `BarcodeValue`.

#### Soft Delete Strategy

None. Deactivation is a physical removal from the Cloud read model
(TQ-3: "deactivate → remove"). The authoritative record remains in the Main
Office.

#### Audit Strategy

None. The Cloud read model is a projection; audit lives with the authority.

---

### 6.3 Cloud Integration and Security Tables

Project: `src/j06-pkl-btrade-api/btrade.sqldb`

**`BTRADE_BarcodeRegistrationRequest`** — `btrade.sqldb/BarcodeContext/`

```sql
CREATE TABLE [dbo].[BTRADE_BarcodeRegistrationRequest]
(
    BarcodeRegistrationId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRADE_BRR_Id DEFAULT(''),
    ClientRequestId       VARCHAR(36) NOT NULL CONSTRAINT DF_BTRADE_BRR_ClientRequestId DEFAULT(''),
    ServerId              VARCHAR(5)  NOT NULL CONSTRAINT DF_BTRADE_BRR_ServerId DEFAULT(''),
    BarcodeValue          VARCHAR(50) NOT NULL CONSTRAINT DF_BTRADE_BRR_BarcodeValue DEFAULT(''),
    BrgId                 VARCHAR(6)  NOT NULL CONSTRAINT DF_BTRADE_BRR_BrgId DEFAULT(''),
    Satuan                VARCHAR(7)  NOT NULL CONSTRAINT DF_BTRADE_BRR_Satuan DEFAULT(''),
    RequestedBy           VARCHAR(50) NOT NULL CONSTRAINT DF_BTRADE_BRR_RequestedBy DEFAULT(''),
    RequestedAt           DATETIME    NOT NULL,
    Status                VARCHAR(10) NOT NULL CONSTRAINT DF_BTRADE_BRR_Status DEFAULT('PENDING'),
    ProcessedAt           DATETIME    NULL,
    ProcessedNote         VARCHAR(200) NOT NULL CONSTRAINT DF_BTRADE_BRR_ProcessedNote DEFAULT(''),

    CONSTRAINT PK_BTRADE_BarcodeRegistrationRequest PRIMARY KEY CLUSTERED (BarcodeRegistrationId),
    CONSTRAINT UX_BTRADE_BRR_ServerId_ClientRequestId UNIQUE (ServerId, ClientRequestId)
)
GO

CREATE INDEX IX_BTRADE_BRR_ServerId_Status
    ON [dbo].[BTRADE_BarcodeRegistrationRequest] (ServerId, Status)
GO
```

Soft delete: none; terminal statuses are retained so the mobile client can
display Rejected outcomes (UX Blueprint §11, §15).

**`BTRADE_Location`** — operational location → `ServerId` mapping (ADR-007:
"mapping … is configuration/data, not client input").

```sql
CREATE TABLE [dbo].[BTRADE_Location]
(
    LocationId   VARCHAR(10) NOT NULL CONSTRAINT DF_BTRADE_Location_LocationId   DEFAULT(''),
    LocationName VARCHAR(50) NOT NULL CONSTRAINT DF_BTRADE_Location_LocationName DEFAULT(''),
    ServerId     VARCHAR(5)  NOT NULL CONSTRAINT DF_BTRADE_Location_ServerId     DEFAULT(''),

    CONSTRAINT PK_BTRADE_Location PRIMARY KEY CLUSTERED (LocationId)
)
```

Seed data (ADR-007):

| LocationId | LocationName | ServerId |
| ---------- | ------------ | -------- |
| `GAMPING` | Gudang Gamping | `JOGJA` |
| `CONCAT` | Gudang Concat | `JOGJA` |
| `MAGELANG` | Gudang Magelang | `MGL` |

**`BTRADE_User`** — credential projection (IR-05, GAP-008).

```sql
CREATE TABLE [dbo].[BTRADE_User]
(
    UserId   VARCHAR(50) NOT NULL CONSTRAINT DF_BTRADE_User_UserId   DEFAULT(''),
    UserName VARCHAR(50) NOT NULL CONSTRAINT DF_BTRADE_User_UserName DEFAULT(''),
    Password VARCHAR(64) NOT NULL CONSTRAINT DF_BTRADE_User_Password DEFAULT(''),
    RoleId   VARCHAR(5)  NOT NULL CONSTRAINT DF_BTRADE_User_RoleId   DEFAULT(''),
    IsAktif  BIT         NOT NULL CONSTRAINT DF_BTRADE_User_IsAktif  DEFAULT(0),
    ServerId VARCHAR(5)  NOT NULL CONSTRAINT DF_BTRADE_User_ServerId DEFAULT(''),

    CONSTRAINT PK_BTRADE_User PRIMARY KEY CLUSTERED (UserId)
)
```

`Password` holds the same SHA-256 hash as `BTR_User.Password`
(`LoginForm.cs`: `PasswrodText.Text.HashSha256() == user.Password`).

---

### 6.4 Mobile Local Store — BGud Room Database

Database: `bgud_database` (Room, `AppDatabase`)

| Entity (table) | Purpose | Key |
| -------------- | ------- | --- |
| `barcode_entity` | Active barcode cache for the bound Office | `brgBarcodeId` |
| `barang_entity` | Barang reference cache (ADR-005) | `brgId` |
| `barcode_registration_request_entity` | Offline registration intent queue | `clientRequestId` |
| DataStore `session_preferences` | token, user, warehouseCode, officeCode, last sync timestamps | — |

```text
barcode_entity
    brgBarcodeId     TEXT PK
    barcodeValue     TEXT
    barcodeValueKey  TEXT   // UPPER(normalized) — lookup key
    brgId            TEXT
    brgCode          TEXT
    brgName          TEXT
    satuan           TEXT   // '' when absent

barcode_registration_request_entity
    clientRequestId  TEXT PK
    barcodeValue     TEXT
    brgId            TEXT
    satuan           TEXT
    status           TEXT   // PENDING | SYNCED | REJECTED
    warehouseCode    TEXT   // IR-09 — never a ServerId
    createdAt        INTEGER
    serverNote       TEXT   // rejection reason returned by the Cloud
```

Indexes: `barcode_entity(barcodeValueKey)` unique; `barcode_entity(brgId)`;
`barcode_registration_request_entity(status)`.

> The local cache holds **only Active** barcodes (TQ-3, TQ-6) and is scoped to
> the single Office bound to the session (OQ-2, IR-09).

---

## 7. Application Layer Design

### 7.1 Main Office — `btr.application/BrgContext/BrgBarcodeAgg/`

#### Commands

| Command | Responsibility |
| ------- | -------------- |
| `RegisterBrgBarcodeCommand` | Normalize the barcode value; validate uniqueness (INV-02) and Item existence (INV-04) and unit validity (INV-05); create the mapping in the Active state; stamp audit fields. Used by Desktop and by the registration relay. |
| `CorrectBrgBarcodeCommand` | Change `BrgId` and/or `Satuan` on an existing mapping; validate unit validity; preserve activation state (INV-07); stamp `ModifiedBy`/`ModifiedDate`. |
| `ActivateBrgBarcodeCommand` | Inactive → Active (INV-08). |
| `DeactivateBrgBarcodeCommand` | Active → Inactive (INV-08). |
| `ProcessBarcodeRegistrationRequestCommand` | Accept or reject one relayed request (BR-012/BR-013). On accept it delegates to `RegisterBrgBarcodeCommand`; on conflict it rejects without mutation. Returns `ACCEPTED`/`REJECTED` plus a reason. |

Supporting components, mirroring the `Brg` pattern:

| Component | Responsibility |
| --------- | -------------- |
| `BrgBarcodeBuilder` | `Create()`, `Load(IBrgBarcodeKey)`, `Attach()`, `BarcodeValue()`, `Brg(IBrgKey)`, `Satuan()`, `Activate()`, `Deactivate()`. Resolves and denormalizes `BrgCode`/`BrgName` from Product Master. |
| `BrgBarcodeValidator` | FluentValidation `AbstractValidator<BrgBarcodeModel>` enforcing INV-01, INV-04, INV-05. |
| `BrgBarcodeWriter` | Single transaction per save (P-03), mirroring `BrgWriter.Save`. |

#### Queries

| Query | Responsibility |
| ----- | -------------- |
| `GetBrgBarcodeQuery` | Load one mapping by id. |
| `GetBrgBarcodeByValueQuery` | Resolve a normalized barcode value to its mapping (UC-001). Returns only Active mappings for operational lookup (BR-007). |
| `ListBrgBarcodeByBrgQuery` | All mappings for one Item, all lifecycle states (UC-004). |
| `ListBrgBarcodeQuery` | Search/filter across the registry (barcode value, Item code, Item name). |
| `ListBrgBarcodeChangedQuery` | Change feed for the synchronization client: rows with `RowVer` greater than the supplied watermark, ordered by `RowVer` (IR-04). |

#### Use Cases

| Workflow capability | Application service |
| ------------------- | ------------------- |
| UC-001 Barcode Lookup | `GetBrgBarcodeByValueQuery` |
| UC-002 Register Barcode | `RegisterBrgBarcodeCommand` |
| UC-003 Maintain Barcode | `CorrectBrgBarcodeCommand`, `ActivateBrgBarcodeCommand`, `DeactivateBrgBarcodeCommand` |
| UC-004 View Item Barcodes | `ListBrgBarcodeByBrgQuery` |
| Barcode Synchronization Workflow | `ListBrgBarcodeChangedQuery` (publish) + `ProcessBarcodeRegistrationRequestCommand` (relay) |

### 7.2 Cloud — `btrade.application`

| Type | Name | Responsibility |
| ---- | ---- | -------------- |
| Command | `BarcodeSyncCommand` | Apply one incremental batch: upsert Active barcodes, remove deactivated ones, all scoped to the JWT-resolved `ServerId`. Never replaces the tenant snapshot (P-09). |
| Query | `BarcodeSyncListQuery` | Return all Active barcodes for the JWT-resolved `ServerId` (serves `GET /api/barcodes/sync`). |
| Command | `BarcodeRegistrationSubmitCommand` | Stage one request with `Status = PENDING`; enforce `(ServerId, ClientRequestId)` idempotency (INV-12). |
| Query | `BarcodeRegistrationPendingQuery` | Return `PENDING` requests for the JWT-resolved `ServerId` to the synchronization client. |
| Command | `BarcodeRegistrationAckCommand` | Explicit post-commit acknowledgement: set `ACCEPTED`/`REJECTED`, `ProcessedAt`, `ProcessedNote` (GAP-007). |
| Command | `UserSyncCommand` | Replicate the credential projection (`BTRADE_User`) — existing master-data uploader pattern (IR-05). |
| Command | `IssueTokenCommand` | Verify SHA-256(password) against `BTRADE_User`; bind the selected operational location; issue a JWT carrying user id, location, and resolved `ServerId` (GAP-008, ADR-007). |

DAL contracts: `btrade.application/Contract/IBarcodeDal.cs`,
`IBarcodeRegistrationDal.cs`, following `IBrgDal` composition
(`IInsert`, `IUpdate`, `IDelete`, `IGetDataMayBe`, `IListDataMayBe`). The
contracts are keyed by `IBarcodeKey` (§5.2) and
`IBarcodeRegistrationRequestKey` (§5.3) respectively.

---

## 8. Integration Design

### 8.1 Main Office → Cloud: Incremental Barcode Publish

#### Purpose

Replicate Active barcode mappings to the Cloud read model.

#### Trigger

Executed by `j07-btrade-sync` as part of the operator-initiated master-data
synchronization run (OQ-4: Office Admin runs it at the start of each business
day).

#### Flow

```text
BrgBarcodeDal.ListChanged(watermark)        -- RowVer > watermark, ordered
        ↓
map to BrgBarcodeType
   IsAktif = 1  → Upsert list
   IsAktif = 0  → Remove list (by BrgBarcodeId)
        ↓
POST api/Barcode/sync   (JWT; ServerId resolved server-side)
        ↓
Cloud: transaction { upsert each; delete each }  -- scoped to ServerId
        ↓
HTTP 200 → persist new watermark = max(RowVer) of the batch
```

#### Payload

```text
BarcodeSyncCommand
    ListUpsert : BarcodeType[]
    ListRemove : string[]        // BrgBarcodeId values
    // no ServerId field — resolved from the JWT (ADR-007)
```

#### Error Handling

Non-2xx or transport failure: the run is reported as failed in the sync UI, the
watermark is **not** advanced, and the batch is retried on the next run.
Because publish is idempotent upsert/remove, replay is safe (§10.6).

#### Retry Strategy

Operator-initiated retry (`Sync Now`). No automatic retry loop.

---

### 8.2 Cloud → Main Office: Registration Request Relay

#### Purpose

Move mobile-originated registration intent to the only authority that may
validate it (BR-011, GAP-007).

#### Trigger

Same synchronization run as §8.1, executed **before** it, so that newly
accepted registrations are published by the immediately following publish step.

#### Flow

```text
GET api/BarcodeRegistration/pending          (JWT; ServerId from context)
        ↓
for each request:
    ProcessBarcodeRegistrationRequestCommand (in-process, btr.application)
        ├─ accept → new authoritative BrgBarcode (Active)
        └─ reject → no mutation; reason recorded
        ↓
POST api/BarcodeRegistration/ack             (explicit post-commit, GAP-007)
        { BarcodeRegistrationId, Status, ProcessedNote }
        ↓
run §8.1 publish
```

#### Payload

```text
BarcodeRegistrationPendingQuery  →  BarcodeRegistrationRequestType[]
BarcodeRegistrationAckCommand    →  { BarcodeRegistrationId, Status, ProcessedNote }
```

Rejection reasons (produced by the Main Office, transported verbatim):

| Reason | Cause |
| ------ | ----- |
| `DUPLICATE_BARCODE` | The value already exists in the authority (INV-02) |
| `ITEM_NOT_FOUND` | `BrgId` no longer exists (BR-004) |
| `ITEM_INACTIVE` | The referenced Item is not Active (BQ-7 re-validation) |
| `INVALID_UNIT` | `(BrgId, Satuan)` not found in `BTR_BrgSatuan` (INV-05) |

#### Error Handling

Relay and acknowledgement are separate steps. If acknowledgement fails after a
successful Main Office commit, the request remains `PENDING` in the Cloud and
is re-relayed on the next run. Re-relay is safe because
`ProcessBarcodeRegistrationRequestCommand` re-validates and will reject a
duplicate with `DUPLICATE_BARCODE`, at which point the request is acknowledged
`ACCEPTED` only if the authoritative mapping matches the request; otherwise it
is acknowledged `REJECTED` with `DUPLICATE_BARCODE`. No partial state is
possible because the Main Office commit is a single transaction (P-03).

#### Retry Strategy

Operator-initiated retry. Idempotency is carried by `ClientRequestId`
(INV-12).

---

### 8.3 Registration Request Outcomes

The rejection outcome must reach the requesting device (feasibility GAP-011
follow-up). It is delivered through the Cloud staging record:

```text
BGud submits        → Status = PENDING     (device shows "Pending")
Main Office accepts → Status = ACCEPTED    (device shows "Synced")
Main Office rejects → Status = REJECTED    (device shows "Rejected" + reason)
```

The device learns the outcome on its next successful synchronization
(login sync or manual sync) by requesting the status of its own queued
requests. This satisfies UX Blueprint §11 (Pending / Success / Rejected) and
§15 without introducing push notification (explicitly out of scope in
UX Blueprint §16).

### 8.4 Cloud → BGud: Master Data Download

| Data | Endpoint | Convention |
| ---- | -------- | ---------- |
| Barang | `GET /api/Brg/{serverId}` | Existing endpoint reused unchanged (ADR-005, GAP-013). Legacy route-based `ServerId` is permitted to remain (ADR-007 §8). |
| Barcodes | `GET /api/barcodes/sync` | New; derives `ServerId` from the JWT. Mandated route (TQ-5). |

The Barang `serverId` used by BGud is the Office resolved and returned by the
authentication response; it is used for display and for this legacy read
route only.

### 8.5 Mobile → Cloud: Registration Submission

* `POST /api/barcode-registration` — mandated route (TQ-5). Requires JWT
  (ADR-002/ADR-003). Payload carries no `ServerId` (ADR-007).

### 8.6 Integration Inventory

| # | Direction | Endpoint | Pattern | Auth |
| - | --------- | -------- | ------- | ---- |
| I-01 | `j07-btrade-sync` → Cloud | `POST api/Barcode/sync` | Incremental publish (TQ-3) | JWT (write) |
| I-02 | `j07-btrade-sync` → Cloud | `GET api/BarcodeRegistration/pending` | Incremental download (mirrors `GET api/Order/incremental`) | JWT |
| I-03 | `j07-btrade-sync` → Cloud | `POST api/BarcodeRegistration/ack` | Explicit ack (GAP-007) | JWT (write) |
| I-04 | BGud → Cloud | `POST /api/barcode-registration` | Request submission | JWT (write) |
| I-05 | Cloud → BGud | `GET /api/barcodes/sync` | Bulk active download | JWT |
| I-06 | Cloud → BGud | `GET /api/Brg/{serverId}` | Existing Barang download (ADR-005) | Existing |
| I-07 | BGud → Cloud | `POST api/Auth/login` | Token issuance (GAP-008, IR-05) | Credentials |
| I-08 | `j07-btrade-sync` → Cloud | `POST api/User` | Credential projection publish (IR-05) | JWT (write) |
| I-09 | BGud → Cloud | `GET api/BarcodeRegistration/status` | Device learns its own request outcomes (§8.3) | JWT |

> No point-lookup endpoint (`GET /api/barcodes/{barcode}`) is built for MVP
> (TQ-5).

---

## 9. Security Design

### 9.1 Authentication

| Client | Mechanism |
| ------ | --------- |
| BTR Desktop | Existing Windows application context plus the existing `BTR_User` login (`LoginForm`) and role/menu authorization (`BTR_RoleMenu`). No API surface. Unchanged by this feature. |
| `j07-btrade-sync` | Must present a JWT obtained from `pkl.btrade.api` using an operator account, because ADR-002 requires authenticated identity on **all** cloud write endpoints, including I-01, I-03 and I-08. |
| BGud | Authenticates against `pkl.btrade.api` before any operational command (ADR-003). Presents the JWT on every subsequent request. |

Token issuance (GAP-008, IR-05):

```text
POST api/Auth/login  { userId, password, locationId }
    ↓
verify SHA-256(password) against BTRADE_User
    ↓
resolve ServerId from BTRADE_Location(locationId)
    ↓
issue JWT { sub = userId, role, locationId, serverId }
```

Read endpoint protection follows the platform's overall endpoint-protection
policy (ADR-002 §5). Barcode Registry's own endpoints (I-01…I-05) are all
authenticated.

### 9.2 Authorization and Tenant Boundaries

* **Tenant.** `ServerId` is resolved server-side from the authenticated
  session's bound operational location. It is never read from a route, query
  string, header, or body on any Barcode Registry endpoint (P-06, ADR-007).
* **Write endpoints** reject unauthenticated callers (ADR-002).
* **Legacy endpoints** that already take `ServerId` in the route (for example
  `GET /api/Brg/{serverId}`) may remain unchanged (ADR-007 §8) and are the only
  place where BGud supplies a tenant value.
* **Location switching.** A user is not permanently bound to one `ServerId`.
  Changing the operational location requires re-authentication (ADR-007).
  Queued offline registrations are held until a session bound to their original
  location submits them (IR-09).

### 9.3 Permission Boundaries (BQ-6)

| Function | Warehouse Officer | Office Admin | System Administrator |
| -------- | :---------------: | :----------: | :------------------: |
| Barcode lookup | Yes | Yes | Yes |
| Register barcode | Yes | Yes | Yes |
| Correct barcode mapping | Yes | Yes | Yes |
| Activate / Deactivate barcode | No | Yes | Yes |

No approval workflow exists. Every change takes effect immediately after
successful Main Office validation. Accountability is provided by
`CreatedBy`/`CreatedDate`/`ModifiedBy`/`ModifiedDate` (BQ-6).

Desktop enforcement: `BTR_Menu` seed row for the barcode maintenance form plus
`BTR_RoleMenu` grants, consumed by the existing role-gated ribbon
(`MainForm.SetupUserMenu`).

Mobile enforcement: the role claim in the JWT gates activation/deactivation
surfaces. Because the BGud MVP screen inventory has no activate/deactivate
screen (IR-06), the mobile role gate is realized as: registration and
correction available to all authenticated warehouse roles; the Cloud rejects
any activation/deactivation command that is not from an Office Admin or System
Administrator.

### 9.4 Security Ownership

| Concern | Owner |
| ------- | ----- |
| Credential store of record | Main Office (`BTR_User`) |
| Credential projection and token issuance | Cloud API |
| Tenant binding | Cloud API (server-side resolution) |
| Authoritative validation | Main Office application layer |
| Endpoint enforcement | Cloud API middleware/attributes |
| Transport security | Platform/IT — unresolved by the feasibility assessment; carried as risk R-03 |

---

## 10. Operational Architecture

### 10.1 Migration Strategy

The existing SSDT approach is used; no migration framework is introduced
(GAP-012).

* New objects are added to `btr.sql` (`BTR_BrgBarcode`) and to
  `btrade.sqldb` (`BTRADE_BrgBarcode`, `BTRADE_BarcodeRegistrationRequest`,
  `BTRADE_Location`, `BTRADE_User`) and are registered in
  `btr.sql.sqlproj` and `btrade.sqldb.sqlproj`.
* Idempotent upgrade scripts are provided for existing installations using the
  existing guarded pattern
  (`IF COL_LENGTH(...) IS NULL BEGIN ... END`, `IF OBJECT_ID(...) IS NULL ...`)
  as seen in `btr.sql/Scripts/Upgrade_*.sql`.
* All additions are additive: new tables and indexes only. No existing table,
  column, or index is modified or dropped.

> This section states strategy only. Deployment execution is out of scope for
> this artifact.

### 10.2 Data Backfill Strategy

**None required.** There is no legacy barcode data anywhere in the platform
(GAP-001, feasibility §3.1 repository-wide search result) and no external
barcode source exists (OQ-3). The registry is populated exclusively through
user registration in BTR Desktop and BGud.

### 10.3 Rollback Strategy

* **Schema:** additions are additive; removal of the new objects removes the
  feature's persistence without affecting existing tables.
* **Data:** because the Cloud read model is a projection, it can be rebuilt
  from the Main Office by resetting the incremental watermark and re-running
  the publish step (§8.1).
* **Authoritative data:** the Main Office registry is the only source; no
  rollback path can or should reconstruct it from any downstream store.
* **Operational ownership:** Office Admin runs daily synchronization and
  escalates unresolved failures to IT (OQ-4). IT owns application deployment,
  database maintenance, troubleshooting, and defect resolution only.

### 10.4 Performance Considerations

* Lookup is a single indexed unique-key hit on `BarcodeValueKey` (Main Office)
  or `barcodeValueKey` (Room). No scan performs a network call (ADR-006).
* The Main Office change feed is an ordered range scan on `IX_..._RowVer`.
* Bulk download (`GET /api/barcodes/sync`) returns the full Active set for one
  tenant; it is a login-time or manual operation, never a per-scan operation.
* The Cloud read model is denormalized (`BrgCode`, `BrgName` carried inline) so
  the mobile cache needs no join at lookup time — mirroring `BTRADE_Brg`.

### 10.5 Concurrency Considerations

* Two operators registering the same value concurrently: the unique index
  `UX_BTR_BrgBarcode_BarcodeValueKey` makes the second transaction fail. The
  application pre-check (INV-02) is a usability optimization; the index is the
  guarantee.
* Publish and relay run sequentially within one synchronization run, with
  relay first, so a newly accepted barcode is published in the same run.
* Mobile edits are local and single-user; no device-level concurrency exists.

### 10.6 Idempotency Requirements

| Operation | Idempotency guarantee |
| --------- | --------------------- |
| Barcode publish (I-01) | Upsert by `BrgBarcodeId`; removal by id. Replaying a batch is a no-op. |
| Registration submission (I-04) | Unique `(ServerId, ClientRequestId)`; a duplicate submission is acknowledged, not re-staged. |
| Registration relay (I-02/I-03) | Re-relay re-validates in the Main Office; an already-accepted request yields `DUPLICATE_BARCODE` rather than a second mapping. |
| Acknowledgement (I-03) | Terminal status transition only (INV-11); repeated acks are no-ops. |

---

# PART B — FRONTEND ARCHITECTURE

---

## 11. Screen Inventory

### 11.1 BTR Desktop (`j05-btr-distrib/btr.distrib`)

#### SCR-DESK-001 — Barcode Registry Maintenance

* **Screen Name:** `BrgBarcodeForm`
* **File:** `btr.distrib/InventoryContext/BrgBarcodeAgg/BrgBarcodeForm.cs`
* **Purpose:** Register, correct, activate, and deactivate barcode mappings;
  search the registry.
* **Primary Actor:** Office Admin, System Administrator (BQ-6, GAP-005)
* **Workflow Reference:** UC-002, UC-003
* **Domain Reference:** DOMAIN §3.1, §3.2, §3.4, §8
* **Menu:** new `BTR_Menu` row + `BTR_RoleMenu` grants; opened as an MDI child
  through the existing `BringMdiChildToFrontIfLoaded<T>()` pattern in
  `MainForm`. Form resolved from the DI container.

#### SCR-DESK-002 — Item Barcode Tab (Master Barang)

* **Screen Name:** Barcode tab on the existing `BrgForm`
* **File:** `btr.distrib/InventoryContext/BrgAgg/BrgForm.cs` (+ `BrgFormBarcodeDto.cs`)
* **Purpose:** Display and maintain all barcodes belonging to the Item being
  edited.
* **Primary Actor:** Warehouse Officer, Office Admin
* **Workflow Reference:** UC-004, UC-003
* **Domain Reference:** DOMAIN §3.2, §3.5, BR-002
* **Notes:** Added as a tab alongside the existing Satuan / Harga / Stok tabs,
  using the same `BindingList<Dto>` + grid pattern (`BrgFormSatuanDto`,
  `InitGridSatuan`).

#### SCR-DESK-003 — Barcode Browser

* **Screen Name:** `BrgBarcodeBrowser` (`IBrowser<BrgBarcodeBrowserView>`)
* **File:** `btr.distrib/Browsers/`
* **Purpose:** Read-only search and selection of an existing barcode mapping;
  supports barcode lookup and "View Existing Mapping" on duplicate.
* **Primary Actor:** Warehouse Officer, Office Admin
* **Workflow Reference:** UC-001, UC-004
* **Domain Reference:** DOMAIN §3.3, §3.5
* **Notes:** Reuses the existing `IBrowser<T>` infrastructure already used by
  `BrgForm` (`IBrowser<BrgBrowserView>`).

> Barcode acquisition on Desktop is a keyboard-wedge text entry into the
> existing input controls (ADR-001). No scanner SDK, no separate capture
> screen, no device configuration UI.

---

### 11.2 BGud (Android)

#### SCR-MOB-001 — Login

* **Purpose:** Authenticate and establish the operational context.
* **Primary Actor:** Warehouse Officer, Office Admin
* **Workflow Reference:** all; required before any operational command
  (ADR-003)
* **Domain Reference:** DOMAIN §4; ADR-007
* **Reference:** UX Blueprint §5

#### SCR-MOB-002 — Home

* **Purpose:** Entry point to barcode operations; shows context and sync state.
* **Primary Actor:** all
* **Workflow Reference:** navigation hub
* **Domain Reference:** —
* **Reference:** UX Blueprint §6

#### SCR-MOB-003 — Scan Barcode

* **Purpose:** Resolve a scanned barcode to an Item (cache-first).
* **Primary Actor:** Warehouse Officer, Office Admin
* **Workflow Reference:** UC-001
* **Domain Reference:** DOMAIN §3.3, BR-007
* **Reference:** UX Blueprint §7

#### SCR-MOB-004 — Register Barcode

* **Purpose:** Create a barcode registration request.
* **Primary Actor:** Warehouse Officer, Office Admin
* **Workflow Reference:** UC-002
* **Domain Reference:** DOMAIN §3.1, BR-004, BR-005
* **Reference:** UX Blueprint §8

#### SCR-MOB-005 — Barcode Registry

* **Purpose:** Browse and search barcode mappings in the local cache.
* **Primary Actor:** Warehouse Officer, Office Admin
* **Workflow Reference:** UC-004
* **Domain Reference:** DOMAIN §3.2, §3.5
* **Reference:** UX Blueprint §9

#### SCR-MOB-006 — Edit Barcode

* **Purpose:** Correct an existing barcode mapping.
* **Primary Actor:** Warehouse Officer, Office Admin
* **Workflow Reference:** UC-003
* **Domain Reference:** DOMAIN §3.2, BR-010
* **Reference:** UX Blueprint §10

#### SCR-MOB-007 — Synchronization

* **Purpose:** Show synchronization state and trigger a manual sync.
* **Primary Actor:** all
* **Workflow Reference:** Barcode Synchronization Workflow (DOMAIN §10)
* **Domain Reference:** DOMAIN §3.7
* **Reference:** UX Blueprint §11

#### SCR-MOB-008 — Settings

* **Purpose:** Application settings surface defined by the UX blueprint.
* **Primary Actor:** all
* **Workflow Reference:** navigation (UX Blueprint §4)
* **Domain Reference:** —
* **Reference:** UX Blueprint §4

> **Not in the BGud inventory:** activate/deactivate. See IR-06, §22 R-07 and
> §24. No screen exists without a workflow trace; the UX blueprint's
> navigation structure is treated as the authoritative mobile surface.

---

## 12. Screen Layout Architecture

### 12.1 SCR-DESK-001 — Barcode Registry Maintenance

```text
Toolbar               (New, Save, Delete/Deactivate, Activate, Search, Refresh)
Identity Panel        (BrgBarcodeId [read-only], Item selector + browser button,
                       Item Code [read-only], Item Name [read-only])
Barcode Panel         (Barcode value input — keyboard-wedge target)
Packaging Panel       (Unit — optional; Small Unit / Big Unit from the Item's
                       own BTR_BrgSatuan rows)
Status Panel          (IsAktif indicator, Created*, Modified* read-only)
Worklist Grid         (search results: Barcode, Item Code, Item Name, Unit, Status)
```

Layout notes:
* Follows the `BrgForm` convention: header panels + tabular detail grid +
  bottom action buttons.
* The barcode value input is a plain text box and is the keyboard-wedge target
  (ADR-001); Enter on the scanner terminator triggers lookup.
* Status and audit fields are read-only; they are system-owned.

### 12.2 SCR-DESK-002 — Item Barcode Tab

```text
Tab Body
    Toolbar      (Add Barcode, Edit, Activate, Deactivate)
    Grid         (Barcode, Unit, Status, Modified)
```
Layout notes: embedded in `BrgForm`; participates in the form's single save
transaction via `BrgBarcodeWriter`.

### 12.3 SCR-MOB-001 — Login

```text
Header          (application identity)
Form            (Username, Password, Warehouse selector)
Action          (Login)
Feedback        (error message region)
```
Warehouse selector values: Gudang Gamping, Gudang Concat, Gudang Magelang
(UX Blueprint §5, ADR-007).

### 12.4 SCR-MOB-002 — Home

```text
Context Header   (User, Warehouse, Office)
Quick Actions    (Scan Barcode, Search Barcode, Register Barcode)
Sync Status Card (Online/Offline, Last Sync Time, Pending Upload Count)
Navigation       (Barcode Registry, Synchronization, Settings)
```

### 12.5 SCR-MOB-003 — Scan Barcode

```text
Camera Viewfinder   (CameraX + ML Kit region)
Manual Entry        (fallback text input + Search)
Result Region       (Barcode, Item Code, Item Name, Unit) — Scenario A
Not Found Region    ("Barcode Not Found" + Register Barcode / Cancel) — Scenario B
```
Layout notes: single-screen, minimal transitions (UX-004). Scan-first (UX-002);
manual typing is a fallback only.

### 12.6 SCR-MOB-004 — Register Barcode

```text
Barcode Field    (read-only, captured from scanner)
Item Search      (by Item Code or Item Name; local Barang cache)
Item Result List (selectable)
Unit Selector    (optional; Small Unit / Big Unit from the selected Item)
Actions          (Save, Cancel)
```

### 12.7 SCR-MOB-005 — Barcode Registry

```text
Search Bar       (Barcode, Item Code, Item Name)
Result List      (Barcode, Item Code, Item Name, Unit)
Row Action       (Edit Barcode)
Empty State
```

### 12.8 SCR-MOB-006 — Edit Barcode

```text
Barcode Field    (read-only)
Item Search      (local Barang cache)
Unit Selector    (optional)
Actions          (Save, Cancel)
```

### 12.9 SCR-MOB-007 — Synchronization

```text
Master Data Card    (Last Barang Sync, Last Barcode Sync)
Queue Card          (Pending, Success, Rejected)
Actions             (Sync Now)
Connectivity State  (Online / Offline)
```

---

## 13. Navigation Architecture

### 13.1 BTR Desktop

```text
MainForm (role-gated ribbon)
    ↓ menu click (BTR_Menu / BTR_RoleMenu)
Barcode Registry Maintenance  (SCR-DESK-001, MDI child)
    ↓ Item selector → browser button
BrgBarcodeBrowser (SCR-DESK-003)
    ↓ double-click row
Barcode Registry Maintenance (loaded for editing)

MainForm
    ↓ Master Barang
BrgForm
    ↓ Barcode tab
Item Barcode Tab (SCR-DESK-002)
```

| Source | Target | Conditions |
| ------ | ------ | ---------- |
| `MainForm` | `BrgBarcodeForm` | User's role has the barcode menu grant (BQ-6) |
| `BrgBarcodeForm` | `BrgBarcodeBrowser` | Search / Item selection requested |
| `BrgBarcodeBrowser` | `BrgBarcodeForm` | A row is selected |
| `BrgForm` | Barcode tab | Item is loaded (`BrgId` present) |

### 13.2 BGud (Navigation Compose)

```text
login
  ├─ success ──▶ office resolution ──▶ master data synchronization ──▶ home
  └─ failure ──▶ login (error)

home
  ├─ Scan Barcode ────────▶ scan
  ├─ Search Barcode ──────▶ barcode_registry
  ├─ Register Barcode ────▶ register  (barcode argument optional)
  ├─ Barcode Registry ────▶ barcode_registry
  ├─ Synchronization ─────▶ synchronization
  └─ Settings ────────────▶ settings

scan
  ├─ found ───────────────▶ scan (result region) ── Close ──▶ back
  ├─ not found + Register ─▶ register?barcode={value}
  └─ not found + Cancel ───▶ back

register
  ├─ Save ────▶ local queue ── success message ──▶ back
  └─ Cancel ──▶ back

barcode_registry
  └─ Edit Barcode ────────▶ edit?barcodeId={id}

edit
  ├─ Save ────▶ local queue ──▶ back
  └─ Cancel ──▶ back

synchronization
  └─ Sync Now ──▶ sync worker ──▶ synchronization (refreshed)
```

| Source | Target | Conditions |
| ------ | ------ | ---------- |
| any | `login` | No valid JWT, token expired, or warehouse change requested |
| `login` | `home` | Authentication succeeded **and** login synchronization completed |
| `scan` | `register` | Barcode not found in the local cache AND the user chose Register |
| `register` | back | Save persisted a local request (no network required) |
| `barcode_registry` | `edit` | A cached mapping is selected |
| `synchronization` | `synchronization` | Sync completed or failed; state refreshed |

Start destination: `login` when no valid session exists, otherwise `home`.
Route naming follows the existing `BTrade3` string-route + `navArgument`
convention (`ui/Navigation.kt`).

---

## 14. UI State Architecture

### 14.1 SCR-DESK-001 — Barcode Registry Maintenance

```text
Idle
  ↓ New
New Entry
  ↓ Barcode value captured
Value Captured
  ↓ Item selected
Ready To Save
  ↓ Save
Saving
  ├─ success → Saved
  └─ duplicate → Duplicate Detected
  ↓ (Duplicate) View Existing Mapping
Existing Loaded
  ↓ Edit fields
Dirty
  ↓ Save
Saving → Saved
```

Transition rules:
* `Save` is enabled only in `Ready To Save` and `Dirty`.
* `Activate` is enabled only when the loaded mapping is Inactive and the user's
  role permits it (BQ-6).
* `Deactivate` is enabled only when the loaded mapping is Active and the user's
  role permits it.
* `Duplicate Detected` never mutates state; it navigates to the existing
  mapping (UX Blueprint §14).
* `Saved` returns to `Idle` after the grid is refreshed.

### 14.2 SCR-MOB-003 — Scan Barcode

```text
Scanning
  ↓ barcode recognized / manual submit
Resolving            (local cache lookup, no network)
  ├─ found ──────▶ Found
  └─ not found ───▶ Not Found
Found
  ↓ Close / scan again
Scanning

Not Found
  ├─ Register Barcode ──▶ (navigate to register with barcode)
  └─ Cancel ────────────▶ Scanning
```

Transition rules:
* `Resolving` never waits on the network (P-07, ADR-006).
* `Found` displays Barcode, Item Code, Item Name, Unit (UX Blueprint §7).
* Offline behaviour is identical to online behaviour in this screen.

### 14.3 SCR-MOB-004 — Register Barcode

```text
Initial             (barcode captured, read-only)
  ↓ Item search text entered
Searching           (local Barang cache)
  ├─ results ──▶ Results
  └─ none ─────▶ Empty
Results
  ↓ Item selected
Item Selected
  ↓ (optional) Unit selected
Ready To Save
  ↓ Save
Saving              (local write only)
  └─ success ──▶ Saved (success message) ──▶ back
```

Transition rules:
* `Save` is enabled only when Barcode is present and an **Active** Item from
  the local cache is selected (UX Blueprint §8; BQ-7).
* Unit is optional and never gates `Save` (BR-005).
* `Saving` performs a local write only; no network is required (UX Blueprint §8
  "No immediate server communication is required").
* An Item that is not Active is rejected in the client with
  "Item sudah tidak aktif." (UX Blueprint §14).

### 14.4 SCR-MOB-006 — Edit Barcode

```text
Loaded
  ↓ change Item or Unit
Dirty
  ↓ Save
Saving (local write only)
  └─ success ──▶ Saved ──▶ back
```

Barcode is never editable (UX Blueprint §10). Changes are stored locally and
synchronized later.

### 14.5 SCR-MOB-007 — Synchronization

```text
Idle
  ↓ Sync Now
Synchronizing
  ├─ success ──▶ Synchronized (timestamps + queue counts refreshed)
  └─ failure ──▶ Failed ("Gagal sinkronisasi." + Retry)
```

### 14.6 Global Connectivity and Request Status States

```text
Connectivity:  Online | Offline           (UX Blueprint §15)
Request:       Pending | Synced | Rejected (UX Blueprint §15)
```

`Pending` → `Synced` or `Pending` → `Rejected` only. A `Rejected` request is
terminal on the device; correcting it means registering a different mapping.

---

## 15. Workspace Modes

### 15.1 BTR Desktop — Barcode Maintenance Workspace

#### Browse Mode

* **Purpose:** Find and review existing barcode mappings.
* **Available Actions:** Search, Refresh, open a mapping, open the Barcode
  Browser.
* **Restrictions:** No field is editable; Save is disabled.

#### Edit Mode

* **Purpose:** Create or change a mapping.
* **Available Actions:** Enter barcode value, select Item, select optional
  Unit, Save, Cancel, Activate, Deactivate.
* **Restrictions:** `BrgBarcodeId` and audit fields are not editable.
  Activate/Deactivate require the Office Admin or System Administrator role
  (BQ-6).

### 15.2 BGud — Operational Workspace Modes

#### Scan Mode (primary)

* **Purpose:** Continuous identification of Items by scanning, standing at
  shelves/cartons (UX-004).
* **Available Actions:** Scan, manual entry, view result, proceed to Register
  when unknown.
* **Restrictions:** No network call; no editing; no data entry beyond the
  barcode itself.

#### Registration Mode

* **Purpose:** Capture a new barcode mapping while in the field.
* **Available Actions:** Search local Barang, select Item, select optional
  Unit, Save to local queue, Cancel.
* **Restrictions:** Item must exist in the local cache and be Active (BQ-7).
  Save writes locally; it does not require connectivity.

#### Maintenance Mode

* **Purpose:** Correct an existing cached mapping.
* **Available Actions:** Change Item, change Unit, Save locally, Cancel.
* **Restrictions:** Barcode value is not editable. Activation/deactivation is
  not available on mobile (IR-06).

#### Synchronization Mode

* **Purpose:** Make synchronization state visible and let the user trigger it.
* **Available Actions:** Sync Now, inspect queue counts, inspect last sync
  timestamps.
* **Restrictions:** Requires connectivity to make progress. While
  synchronizing, queued submissions and downloads run in a defined order:
  submit pending requests → download barcodes → download Barang.

> Switching operational Warehouse is not a mode; it requires re-authentication
> (ADR-007) and replaces the local cache for the newly bound Office (IR-09).

---

## 16. Interaction Rules

| # | Condition | Enabled | Disabled | Notes |
| - | --------- | ------- | -------- | ----- |
| IR-D1 | Desktop, Browse Mode | Search, Refresh, Open | Save, Activate, Deactivate | No unsaved edits exist |
| IR-D2 | Desktop, Edit Mode, no Item selected | Cancel | Save, Activate, Deactivate | Item is mandatory (BR-004) |
| IR-D3 | Desktop, existing Active mapping loaded | Deactivate, Correct, Save (when dirty) | Activate | Lifecycle (DOMAIN §8) |
| IR-D4 | Desktop, existing Inactive mapping loaded | Activate, Correct | Deactivate | Lifecycle (DOMAIN §8) |
| IR-D5 | Desktop, user role = Warehouse Officer | Lookup, Register, Correct | Activate, Deactivate | BQ-6 |
| IR-D6 | Desktop, duplicate barcode value entered | View Existing Mapping | Save | UX Blueprint §14; INV-02 |
| IR-M1 | Mobile, no cached barcode match | Register Barcode, Cancel | any lookup action | UX Blueprint §7 Scenario B |
| IR-M2 | Mobile, Item not found in local cache | Close | Save | "Item tidak ditemukan." (UX Blueprint §14) |
| IR-M3 | Mobile, selected Item is not Active | Close | Save | "Item sudah tidak aktif." (UX Blueprint §14); BQ-7 |
| IR-M4 | Mobile, barcode already exists in local cache | View Existing Mapping | Save registration | "Barcode sudah terdaftar." (UX Blueprint §14) |
| IR-M5 | Mobile, Offline | Scan, Register, Edit | Sync Now (progress impossible) | UX-001; ADR-006 |
| IR-M6 | Mobile, synchronization in progress | Cancel/Close | Sync Now, duplicate submissions | One sync run at a time |
| IR-M7 | Mobile, queued requests captured under a different Warehouse | — | Submission of those requests | IR-09; ADR-007 |
| IR-M8 | Any, no valid JWT | Login | All operational commands | ADR-003 |

---

## 17. UI Commands

### 17.1 BTR Desktop

| UI Action | Command / API |
| --------- | ------------- |
| Save (new barcode) | `RegisterBrgBarcodeCommand` |
| Save (existing barcode) | `CorrectBrgBarcodeCommand` |
| Activate | `ActivateBrgBarcodeCommand` |
| Deactivate | `DeactivateBrgBarcodeCommand` |
| Scan / enter barcode value (Enter) | `GetBrgBarcodeByValueQuery` |
| Search registry | `ListBrgBarcodeQuery` |
| Open mapping from browser | `GetBrgBarcodeQuery` |
| Load Item barcodes (Barcode tab) | `ListBrgBarcodeByBrgQuery` |
| Select Item | existing `IBrowser<BrgBrowserView>` |
| Open barcode browser | existing `IBrowser<BrgBarcodeBrowserView>` |

### 17.2 BGud

| UI Action | Command / API |
| --------- | ------------- |
| Login | `POST api/Auth/login` (I-07) |
| Login-time master data sync | `GET /api/barcodes/sync`, `GET /api/Brg/{serverId}` (I-05, I-06) |
| Scan barcode | local Room query on `barcode_entity.barcodeValueKey` — **no API** |
| Manual barcode entry | local Room query — **no API** |
| Save registration | local insert into `barcode_registration_request_entity` (status `PENDING`); later `POST /api/barcode-registration` (I-04) |
| Save correction | local update; later `POST /api/barcode-registration` (correction carries the barcode identity) |
| Search Barcode Registry screen | local Room query — no API |
| Sync Now | `POST /api/barcode-registration` (queued requests), then `GET /api/barcodes/sync`, then `GET /api/Brg/{serverId}`, then refresh own request statuses |
| View request outcome | `GET api/BarcodeRegistration/status?clientRequestId=…` (read-only projection of `BTRADE_BarcodeRegistrationRequest` for the caller's own requests) |

### 17.3 Synchronization Client (`j07-btrade-sync`)

| UI Action | Command / API |
| --------- | ------------- |
| Sync Barcode (publish) | `ListBrgBarcodeChangedQuery` → `POST api/Barcode/sync` (I-01) |
| Process registration requests | `GET api/BarcodeRegistration/pending` (I-02) → `ProcessBarcodeRegistrationRequestCommand` → `POST api/BarcodeRegistration/ack` (I-03) |
| Sync User (credential projection) | `POST api/User` (I-08) |

---

## 18. Validation Ownership

### 18.1 Main Office (Authoritative — always applied)

* Barcode uniqueness within the Product Master authority (BR-001, BR-011,
  ADR-004, INV-02).
* Item existence (`BrgId` must exist) (BR-004, INV-04).
* Item Active state on registration requests (BQ-7).
* Unit validity — `(BrgId, Satuan)` must exist in `BTR_BrgSatuan` (INV-05).
* Lifecycle legality (DOMAIN §8, INV-08).
* Acceptance or rejection of every relayed registration request (BR-012,
  BR-013).
* Audit stamping (`CreatedBy`, `CreatedDate`, `ModifiedBy`, `ModifiedDate`).
* Non-mutation of Product Master (BR-008).

**Nothing downstream may override or skip these.** The unique index is the
final enforcement point.

### 18.2 Desktop Client (usability only)

* Required fields present (barcode value, Item).
* Barcode value normalization before send (BQ-5).
* Unit selection restricted to the selected Item's own units (IR-01).
* Enable/disable per §16.
* Duplicate detection before save for immediate feedback; the Main Office
  decision is authoritative.

### 18.3 Cloud API (boundary only)

* JWT presence and validity (ADR-002).
* Rejection of any client-supplied `ServerId` (ADR-007, P-06).
* Request-shape validation: `BarcodeValue` non-empty after normalization,
  `BrgId` non-empty, `ClientRequestId` present.
* Tenant scoping of every read and write.
* **No barcode business validation.** The Cloud never decides uniqueness,
  existence, or lifecycle.

### 18.4 BGud Client (usability + offline guardrails)

* Required fields present (barcode, Item).
* Barcode normalization before local storage and before lookup (BQ-5).
* Item must exist in the **local Barang cache** and be Active (BQ-7).
* Unit restricted to the selected Item's cached units.
* Local duplicate detection against the local cache (best-effort; the Main
  Office remains authoritative).
* No scan may require a network call (P-07).

---

## 19. View Models

### 19.1 Desktop DTOs (`btr.distrib/InventoryContext/BrgBarcodeAgg/`)

#### `BrgBarcodeFormBarcodeDto`

* **Purpose:** Row model for the maintenance grid and the Item Barcode tab.
* **Key Fields:** `BrgBarcodeId`, `BarcodeValue`, `BrgId`, `BrgCode`,
  `BrgName`, `Satuan`, `IsAktif`, `ModifiedBy`, `ModifiedDate`
* **Source Query:** `ListBrgBarcodeQuery` / `ListBrgBarcodeByBrgQuery`

#### `BrgBarcodeBrowserView`

* **Purpose:** Read-only selection row for the barcode browser.
* **Key Fields:** `BrgBarcodeId`, `BarcodeValue`, `BrgCode`, `BrgName`,
  `Satuan`, `IsAktif`
* **Source Query:** `ListBrgBarcodeQuery`

### 19.2 Cloud Contracts

#### `BarcodeSyncResponse`

* **Purpose:** Payload of `GET /api/barcodes/sync`.
* **Key Fields:** `ListBarcode: BarcodeType[]`, `ServerId`, `SyncedAt`
* **Source Query:** `BarcodeSyncListQuery`

#### `BarcodeRegistrationSubmitRequest` / `Response`

* **Purpose:** Payload of `POST /api/barcode-registration`.
* **Request Fields:** `ClientRequestId`, `BarcodeValue`, `BrgId`, `Satuan`
  — **no `ServerId`** (ADR-007)
* **Response Fields:** `BarcodeRegistrationId`, `Status`, `ReceivedAt`
* **Source Command:** `BarcodeRegistrationSubmitCommand`

#### `BarcodeRegistrationStatusResponse`

* **Purpose:** Lets a device learn its own request outcomes (§8.3).
* **Key Fields:** `ClientRequestId`, `Status`, `ProcessedNote`
* **Source Query:** projection over `BTRADE_BarcodeRegistrationRequest`
  restricted to the caller's own `RequestedBy`.

### 19.3 BGud View Models (`viewmodel/`)

| ViewModel | Purpose | Key Fields | Source |
| --------- | ------- | ---------- | ------ |
| `LoginViewModel` | Authenticate and bind operational context | `username`, `password`, `warehouse`, `isLoading`, `error` | `POST api/Auth/login` |
| `HomeViewModel` | Context and sync summary | `user`, `warehouse`, `office`, `isOnline`, `lastSyncAt`, `pendingCount` | DataStore + Room |
| `ScanViewModel` | Cache-first scan resolution | `lastScannedValue`, `scanState` (`Scanning`/`Resolving`/`Found`/`NotFound`), `resolvedItem` | Room `barcode_entity` |
| `RegisterBarcodeViewModel` | Capture a registration request | `barcodeValue`, `itemQuery`, `itemResults`, `selectedItem`, `selectedUnit`, `saveState` | Room `barang_entity` + `barcode_entity` |
| `BarcodeRegistryViewModel` | Search and list cached mappings | `query`, `results`, `isEmpty` | Room `barcode_entity` |
| `EditBarcodeViewModel` | Correct a cached mapping | `barcodeValue` (read-only), `selectedItem`, `selectedUnit`, `saveState` | Room `barcode_entity` |
| `SynchronizationViewModel` | Sync state and manual sync | `lastBarangSyncAt`, `lastBarcodeSyncAt`, `pendingCount`, `successCount`, `rejectedCount`, `isSyncing`, `error` | Room + Cloud I-04/I-05/I-06 |

> BGud follows the existing `BTrade3` convention of a ViewModel plus a
> `…ViewModelFactory` per screen (`viewmodel/`, `ui/Navigation.kt`).

---

## 20. Frontend Performance Architecture

### Desktop — Barcode Registry Maintenance

```text
Worklist default load:   200 rows (server-side top-N)
Virtualization:          Not applicable (WinForms grid, existing convention)
Auto refresh:            None
Search trigger:          Enter or Search button (existing BrgForm convention)
Item selection:          Browser dialog, not inline scan of the full Item list
```

### BGud — Barcode Registry List

```text
Default load:            50 rows
Paging:                  Incremental (load more on scroll end)
Minimum characters:      3
Auto search:             Yes, debounced 300 ms
Enter to search:         Yes
Item search source:      Local Room cache only (no network)
```

### BGud — Scan

```text
Scanner:                 CameraX preview + ML Kit Barcode Scanning
Resolution mode:         Single-scan — resolve one barcode then re-arm
Lookup:                  Local Room unique-index hit on barcodeValueKey
Network per scan:        None (ADR-006)
Manual entry fallback:   Same resolution path as a scan
```

### BGud — Synchronization

```text
Download granularity:    Full Active barcode set for the bound Office
Frequency:               Login + manual only (UX Blueprint §13)
Background scheduling:   Not scheduled in MVP
WorkManager usage:       Executes the sync worker reliably under constraints;
                         no periodic work is enqueued
Queue submission:        Batched per sync run; one request per HTTP call
```

---

# PART C — TRACEABILITY

---

## 21. Traceability Matrix

### 21.1 Business Rules

| Domain Rule | Workflow | Gap / Decision | Architecture Element |
| ----------- | -------- | -------------- | -------------------- |
| BR-001 Barcode values unique | UC-002 | GAP-010, ADR-004 | §5.1 INV-02; §6.1 `UX_BTR_BrgBarcode_BarcodeValueKey` |
| BR-002 One Item, many Barcodes | UC-004 | GAP-009 | §5.1 (no child entities; mapping attribute); §6.1 `IX_BTR_BrgBarcode_BrgId` |
| BR-003 One Barcode, one Item | UC-002 | — | §5.1 aggregate root; single `BrgId` column |
| BR-004 Registration requires existing Item | UC-002 | BQ-7 | §5.1 INV-04; §18.1; §6.1 `FK_BTR_BrgBarcode_BTR_Brg` |
| BR-005 Packaging Level optional | UC-002 | GAP-009, IR-01 | §5.1 VO-02; nullable-by-convention `Satuan` default `''` |
| BR-006 Inactive retained for history | UC-003 | — | §6.1 soft-delete = none; `IsAktif`; no delete path |
| BR-007 Only Active returned on lookup | UC-001 | TQ-3, TQ-6 | §7.1 `GetBrgBarcodeByValueQuery` (Active only); §6.2 active-only table; §6.4 active-only cache |
| BR-008 Cannot modify Product Master | all | — | §4.1 "Explicitly Does Not"; §5.1 INV-09; §18.1 |
| BR-009 Registration from Mobile and Desktop, equal authority | UC-002 | GAP-005, GAP-007 | §7.1 `RegisterBrgBarcodeCommand` shared by Desktop and relay |
| BR-010 Mappings may be corrected | UC-003 | BQ-6 | §7.1 `CorrectBrgBarcodeCommand`; §5.1 INV-07 |
| BR-011 Uniqueness validated by Main Office | UC-002 | ADR-004, GAP-007 | §18.1; §4.2 "Explicitly Does Not"; IR-08 |
| BR-012 Offline registrations are requests | UC-002 | GAP-011 | §5.3; §8.2; §6.4 request queue; §14.3 |
| BR-013 Conflict → authority wins, request rejected | UC-002 | GAP-011 | §8.2 rejection reasons; §5.3 INV-13; §14.6 |
| BR-014 Requests are integration artifacts, not master data | Barcode Sync WF | GAP-016 | §5.3 INV-10, INV-14; §8.2 separate endpoints; P-09 |

### 21.2 Workflows

| Workflow | Screens | Commands / APIs | Integration |
| -------- | ------- | --------------- | ----------- |
| UC-001 Barcode Lookup | SCR-DESK-001, SCR-DESK-003, SCR-MOB-003 | `GetBrgBarcodeByValueQuery`; local Room query | none (cache-first) |
| UC-002 Register Barcode | SCR-DESK-001, SCR-MOB-004 | `RegisterBrgBarcodeCommand`; `POST /api/barcode-registration` | I-04, I-02, I-03 |
| UC-003 Maintain Barcode | SCR-DESK-001, SCR-DESK-002, SCR-MOB-006 | `CorrectBrgBarcodeCommand`, `ActivateBrgBarcodeCommand`, `DeactivateBrgBarcodeCommand` | I-04 |
| UC-004 View Item Barcodes | SCR-DESK-002, SCR-DESK-003, SCR-MOB-005 | `ListBrgBarcodeByBrgQuery`; local Room query | — |
| Barcode Synchronization Workflow | SCR-MOB-007 | `ListBrgBarcodeChangedQuery`, `ProcessBarcodeRegistrationRequestCommand` | I-01, I-02, I-03, I-05 |

### 21.3 Screens → Workflow → Domain

| Screen | Workflow Capability | Domain Capability |
| ------ | ------------------- | ----------------- |
| SCR-DESK-001 | UC-002, UC-003 | §3.1 Registration, §3.2 Maintenance, §3.4 Activation/Deactivation |
| SCR-DESK-002 | UC-004, UC-003 | §3.5 Barcode-to-Item Mapping |
| SCR-DESK-003 | UC-001, UC-004 | §3.3 Lookup |
| SCR-MOB-001 | all (prerequisite) | §4 Actors & Roles; ADR-007 |
| SCR-MOB-002 | navigation | — |
| SCR-MOB-003 | UC-001 | §3.3 Lookup |
| SCR-MOB-004 | UC-002 | §3.1 Registration |
| SCR-MOB-005 | UC-004 | §3.5 Barcode-to-Item Mapping |
| SCR-MOB-006 | UC-003 | §3.2 Maintenance |
| SCR-MOB-007 | Barcode Sync WF | §3.7 Synchronization to Cloud Lookup |
| SCR-MOB-008 | navigation | — |

---

## 22. Architecture Risks

### R-01 — Cloud Becomes a De Facto Authority

* **Risk:** Staged registration requests are mistaken for authoritative data,
  or a lookup is served from the staging table.
* **Impact:** High — violates the authority model (P-01, P-02) and BR-011.
* **Mitigation:** The staging entity is a separate table with a separate DAL;
  no lookup query references it (§5.3 INV-10); the Cloud performs no barcode
  business validation (§18.3); the read model is written only by the publish
  command (§8.1).
* **Residual Risk:** Low — requires a deliberate review violation.

### R-02 — Uniqueness Divergence Between Stores

* **Risk:** The Main Office and the Cloud enforce different uniqueness scopes
  and a tenant collision or a duplicate slips through.
* **Impact:** High — duplicate mappings (BR-001, BR-003).
* **Mitigation:** Single authoritative validation point (§18.1) plus the
  unique index (§6.1); Cloud uniqueness is explicitly `(ServerId,
  BarcodeValue)` per ADR-004 (§6.2).
* **Residual Risk:** Low.

### R-03 — Transport Security Still Unresolved

* **Risk:** Mobile and sync clients continue to use cleartext HTTP; JWT bearer
  tokens and credential material are exposed in transit.
* **Impact:** Critical — credential and token interception.
* **Mitigation:** Out of scope for this artifact. ADR-003 records that "the
  mobile token endpoint and its transport must not rely on cleartext HTTP";
  feasibility §4.5 records transport security as still unresolved.
* **Residual Risk:** **High until resolved by a separate platform decision.**
  Listed as a blocking issue for release, not for planning (§24).

### R-04 — Coordinated Client Updates Required by ADR-002

* **Risk:** Enforcing JWT on cloud write endpoints breaks every existing client
  that sends no `Authorization` header (`BTrade3`, `j07-btr-gudang`,
  `j07-btrade-sync`).
* **Impact:** High — existing operational flows fail.
* **Mitigation:** ADR-002 explicitly accepts this consequence. This
  architecture requires the sync client and BGud to be JWT-capable before the
  barcode write path goes live (§9.1).
* **Residual Risk:** Medium — requires coordinated release planning, which is
  outside this artifact.

### R-05 — FK Declaration Deviates From the Existing No-FK Convention

* **Risk:** `BTR_BrgBarcode` declares a foreign key to `BTR_Brg`, while all
  existing BTR tables enforce integrity only at the application level.
* **Impact:** Medium — SSDT publish ordering and existing orphan data could
  block deployment.
* **Mitigation:** GAP-002 requires the FK; application-level existence
  validation remains mandatory and primary (INV-04, §18.1); the upgrade script
  must be idempotent and must verify data before creating the constraint.
* **Residual Risk:** Medium.

### R-06 — Incremental Change Detection and Watermark Loss

* **Risk:** The `RowVer` watermark is lost, reset, or advanced past an
  unprocessed change, permanently desynchronizing the Cloud read model.
* **Impact:** Medium-High — stale or missing lookups; the Cloud would silently
  hold an incomplete Active set.
* **Mitigation:** `RowVer` is monotonic and database-generated (IR-04); the
  watermark advances only after an HTTP 200 (§8.1); upsert/remove is
  idempotent; a full rebuild is possible by resetting the watermark (§10.3).
* **Residual Risk:** Medium. Recommend an operational "full republish"
  capability as a later addition; not specified here because it introduces new
  synchronization behaviour.

### R-07 — Mobile Activation/Deactivation Scope

* **Risk:** `BARCODE-REGISTRY-UX-BLUEPRINT.md` §3 assigns "Activate barcode"
  and "Deactivate barcode" to the Office Admin role, but §4 (Navigation
  Structure) and §9/§10 define no mobile screen for them.
* **Impact:** Medium — either a missing capability in BGud or an unplanned
  screen.
* **Mitigation:** This architecture realizes activation/deactivation on the
  BTR Desktop maintenance screen only, following GAP-005 and the UX
  blueprint's explicit navigation inventory (IR-06). The Cloud enforces the
  role gate regardless (§9.3).
* **Residual Risk:** **Medium — requires confirmation.** See §24.

### R-08 — Credential Replication to the Cloud

* **Risk:** Replicating a credential projection (`BTRADE_User`) to the Cloud
  creates a second credential store with its own exposure surface (IR-05).
* **Impact:** High if breached.
* **Mitigation:** Only a verification projection is replicated; no plaintext
  password is stored or transmitted beyond the login call; the projection is
  written only by the synchronization client over an authenticated endpoint
  (I-08); the Main Office remains the store of record (§9.4).
* **Residual Risk:** Medium — inherent to any cloud-side token issuance and
  compounded by R-03.

### R-09 — Domain Artifact Out of Sync With Closed Decisions

* **Risk:** `BARCODE-REGISTRY-DOMAIN.md` still contains `PackagingLevel`,
  "globally unique" (BR-001), and the pre-GAP-009/010 aggregate boundary.
* **Impact:** Medium — implementers following the domain artifact would build
  the wrong model.
* **Mitigation:** This architecture consumes the closed decisions and the ADRs,
  which the feasibility assessment explicitly records as superseding the domain
  artifact (GAP-009, GAP-010 follow-ups).
* **Residual Risk:** Medium until the domain artifact is updated by a
  Knowledge Curator pass (§24).

### R-10 — Duplicate Synchronization Codebase Temptation

* **Risk:** Barcode synchronization is added to `j05-btr-distrib\btr.sync`
  because it contains matching class names.
* **Impact:** Medium — duplicated, divergent synchronization logic.
* **Mitigation:** GAP-015 excludes `btr.sync` from the target architecture;
  §4.3 and §23.4 restate it as a prohibited shortcut.
* **Residual Risk:** Low.

---

## 23. Implementation Guidance

### 23.1 Backend Constraints

* Place the barcode aggregate in `BrgContext/BrgBarcodeAgg` and mirror the
  `Brg` layering exactly: `BrgBarcodeModel` + `IBrgBarcodeKey` (domain);
  `BrgBarcodeBuilder`, `BrgBarcodeWriter`, `BrgBarcodeValidator`,
  `IBrgBarcodeDal`, queries (application); `BrgBarcodeDal` with Dapper
  (infrastructure).
* Every authoritative write is a single `TransHelper.NewScope()` transaction.
* Normalize barcode values in the domain before persistence and before every
  comparison (BQ-5). Never compare raw values.
* No barcode endpoint may read `ServerId` from the request.
* The Main Office has no tenancy concept. Do not add one.
* Keep the change feed (`ListChanged`) separate from every other query.
* Cloud handlers must not validate barcode business rules.
* Register every new schema object in its `.sqlproj` and ship an idempotent
  upgrade script (GAP-012).

### 23.2 Frontend Constraints

* Desktop: add the menu seed + role grants; open the form as an MDI child
  through the DI container; reuse `IBrowser<T>`; no scanner SDK (ADR-001).
* BGud: reuse the `BTrade3` structure — `dao` / `database` / `model` /
  `model/api` / `network` / `repository` / `ui/screen` / `ui/component` /
  `viewmodel` + `…ViewModelFactory` / `datastore` / `ui/Navigation.kt`.
* No scan may trigger a network call (P-07).
* All writes from BGud are local-first; submission happens only during a sync
  run.
* The device must not store or send `ServerId` as a command input (IR-09).
* Camera acquisition is isolated behind a single component so the domain stays
  scanner-agnostic (P-10).

### 23.3 Critical Invariants

```text
INV-01  BarcodeValue mandatory after normalization
INV-02  BarcodeValue unique within the Product Master authority, all states
INV-03  One barcode → exactly one Item
INV-04  BrgId must exist
INV-05  (BrgId, Satuan) must exist when Satuan is present
INV-06  Registration creates the barcode Active
INV-07  Correction never changes activation state
INV-08  Deactivate/Activate only; Inactive is never deleted
INV-09  Product Master is never modified
INV-10  A registration request is never a lookup source
INV-11  Request status transitions PENDING → ACCEPTED | REJECTED, terminal
INV-12  (ServerId, ClientRequestId) is unique
INV-13  The Main Office decides acceptance, never the Cloud
INV-14  The request queue never participates in snapshot replication
```

### 23.4 Prohibited Shortcuts

* Do not implement barcode synchronization in `j05-btr-distrib\btr.sync`
  (GAP-015).
* Do not add a barcode column to `BTR_Brg` or `BTRADE_Brg` (GAP-002, GAP-003).
* Do not create a new Item identity model — use `BrgId` / `BrgCode`
  (GAP-009).
* Do not create a second unit-classification model — reuse `BTR_BrgSatuan`
  (GAP-009, IR-01).
* Do not build `GET /api/barcodes/{barcode}` for MVP (TQ-5).
* Do not let the Cloud validate uniqueness, existence, or lifecycle (BR-011).
* Do not serve lookups from the staging table (GAP-007).
* Do not apply snapshot/replace semantics to the registration queue (BR-014,
  GAP-016).
* Do not accept `ServerId` on any Barcode Registry command or endpoint
  (ADR-007).
* Do not add barcode scope to `BTrade3` (BQ-1).
* Do not introduce a migration framework (GAP-012).
* Do not automatically re-home queued registrations to a different operational
  location (ADR-007, IR-09).
* Do not implement automatic background synchronization scheduling in BGud MVP
  (UX Blueprint §13).

---

## 24. Architecture Readiness

### Ready For Planning

```text
YES
```

All sixteen gaps (GAP-001 … GAP-016) are CLOSED and all business and technical
questions (BQ-1 … BQ-7, TQ-1 … TQ-7, OQ-1 … OQ-4) are RESOLVED. No blocking
business ambiguity remains that would prevent a planner from slicing the work
described in §4 through §20.

### Blocking Issues

None for planning.

### Must Be Confirmed Before Implementation

Recorded as risks, not blockers:

| # | Item | Why | Reference |
| - | ---- | --- | --------- |
| C-1 | Confirm that activation/deactivation is a Desktop-only capability for this release and that no BGud screen is required. | The UX blueprint assigns the capability to the Office Admin role (§3) but defines no mobile screen (§4, §9, §10). Architecture followed the explicit screen inventory. | IR-06, R-07 |
| C-2 | Confirm the credential projection approach for cloud token issuance. | GAP-008 requires "existing BTR user accounts"; the Cloud has no user store. Architecture selected a replicated verification projection. | IR-05, R-08 |
| C-3 | Resolve transport security for mobile and sync clients. | Explicitly unresolved by the feasibility assessment; ADR-003 forbids cleartext for the token endpoint. Blocks release, not planning. | R-03 |
| C-4 | Update `BARCODE-REGISTRY-DOMAIN.md` to reflect GAP-009 (Packaging Level removed), GAP-010 (uniqueness scope), and BR-012/BR-013/BR-014. | Code and knowledge must remain consistent. | R-09 |
| C-5 | Produce `BARCODE-REGISTRY-WORKFLOW.md` if the artifact chain requires a standalone workflow artifact. | The architecture consumed UC-001 … UC-004 from DOMAIN §10; no separate workflow artifact exists. | §1 Inputs |
| C-6 | Confirm the Desktop menu identifier and role grants for the new maintenance form. | `BTR_Menu.MenuId` is a 4-character key and `BTR_RoleMenu` drives visibility; the concrete value is a data decision. | §9.3, §11.1 |

### Planner Guidance

**Expected implementation areas**

1. Main Office domain + application + infrastructure for the `BrgBarcode`
   aggregate (§5.1, §7.1).
2. Main Office schema object `BTR_BrgBarcode` plus sqlproj registration and an
   idempotent upgrade script (§6.1, §10.1).
3. Desktop maintenance form, Item Barcode tab, and barcode browser, with menu
   seed and role grants (§11.1, §12.1, §12.2).
4. Cloud read model, staging queue, location mapping, and credential
   projection, with their DALs, use cases, and controllers (§5.2, §5.3, §6.2,
   §6.3, §7.2).
5. Cloud authentication: token issuance and JWT enforcement on Barcode
   Registry endpoints (§9.1, §9.2).
6. Synchronization client: incremental publish service, registration relay
   service with explicit acknowledgement, credential projection uploader, and
   `SyncForm` wiring (§4.3, §8.1, §8.2, §8.3).
7. New BGud Android application: login, home, scan, register, registry list,
   edit, synchronization, settings; Room schema; API client; sync worker
   (§11.2, §12, §13.2, §14, §19.3).

**Major dependencies**

* Item identity (`BrgId`) and the existing `Brg` aggregate and `BTR_BrgSatuan`
  model.
* The authoritative synchronization client `j07-btrade-sync` and its registry
  state (`ServerTargetID`, watermark).
* Cloud API JWT configuration and endpoint enforcement (ADR-002).
* Main Office credential source of record (`BTR_User`) and its SHA-256
  verification semantics.
* .NET Framework 4.8 alignment across `j07-btrade-sync`, `btr.application`,
  and `btr.infrastructure` (IR-08).

**Sequencing considerations** (not phases, not slices)

* The Main Office registry and its schema must exist before any
  synchronization or request-relay work.
* Authentication and tenant resolution precede any write path (feasibility
  §11: "authentication and tenant resolution precede any write path").
* The BGud application precedes mobile lookup and registration.
* Registration relay must run before barcode publish within a synchronization
  run (§8.2).
* Request processing and master-data replication must be built and reasoned
  about separately (P-09, BR-014).

**Review concerns** (from feasibility §11, restated as objective checks)

* Prove the Cloud never serves lookup from unvalidated registration data
  (INV-10).
* Prove uniqueness is enforced in the Main Office and nowhere else (BR-011).
* Prove only Active barcodes are published and cached (TQ-3, TQ-6).
* Prove no Product Master data is modified (BR-008).
* Prove no command accepts a client-supplied `ServerId` (ADR-007).
