# IMPLEMENTATION PLAN

# Barcode Registry

| Field | Value |
| ----- | ----- |
| Deliverable | `BARCODE-REGISTRY-IMPL-PLAN.md` |
| Location | `docs/work/barcode-registry/BARCODE-REGISTRY-IMPL-PLAN.md` |
| Method | `docs/skills/planning-skill.md` |
| Planning Authority | `docs/work/barcode-registry/BARCODE-REGISTRY-ARCHITECTURE.md` |
| Author role | Planner (Planning only) |
| Status | **DRAFT** |

> This document is a planning artifact. It translates the approved architecture
> into implementation phases and slices. It does not implement, review, or
> redesign. It introduces no new business or architecture decisions.

---

## 1. Planning Authority

```text
ARCHITECTURE
```

Reference: `docs/work/barcode-registry/BARCODE-REGISTRY-ARCHITECTURE.md`

The architecture is authoritative. This plan does not reinterpret any
architecture decision. Where the architecture records open items, they are
carried as pre-implementation confirmations (§4), not resolved here.

---

## 2. Scope Summary

Realize the Barcode Registry business capability across four systems:

1. **Main Office** (`j05-btr-distrib`) — the authoritative `BrgBarcode`
   aggregate (§5.1), its persistence (§6.1), application commands/queries
   (§7.1), and the Desktop maintenance surface (§11.1).
2. **Cloud** (`j06-pkl-btrade-api`) — the active-only read model (§5.2, §6.2),
   the registration staging queue (§5.3, §6.3), location mapping and credential
   projection (§6.3), application use cases (§7.2), token issuance and JWT
   enforcement (§9), and the Barcode Registry HTTP contract (§8.5).
3. **Synchronization** (`j07-btrade-sync`) — incremental Main Office → Cloud
   publish (§8.1) and Cloud → Main Office registration relay with explicit
   acknowledgement (§8.2, §8.3), plus the credential projection uploader
   (IR-05).
4. **BGud** (new Android application) — cache-first offline lookup and offline
   registration request capture (§4.4, §11.2), Room local store (§6.4), and
   synchronization worker (§20).

Out of scope (by architecture decision, not repeated here): no migration
framework, no point-lookup endpoint, no barcode scope in `BTrade3`, no mobile
activate/deactivate screen, no transport-security resolution (R-03, C-3).

---

## 3. Impact Inventory

### Backend

| Component | System | Reference |
| --------- | ------ | --------- |
| `BrgBarcodeModel` + `IBrgBarcodeKey` | `btr.domain` | §5.1 |
| `BrgBarcodeBuilder` | `btr.application` | §7.1 |
| `BrgBarcodeValidator` | `btr.application` | §7.1 |
| `BrgBarcodeWriter` | `btr.application` | §7.1 |
| `IBrgBarcodeDal` contract | `btr.application` | §5.1 |
| `BrgBarcodeDal` (Dapper) | `btr.infrastructure` | §5.1 |
| Commands: `RegisterBrgBarcodeCommand`, `CorrectBrgBarcodeCommand`, `ActivateBrgBarcodeCommand`, `DeactivateBrgBarcodeCommand`, `ProcessBarcodeRegistrationRequestCommand` | `btr.application` | §7.1 |
| Queries: `GetBrgBarcodeQuery`, `GetBrgBarcodeByValueQuery`, `ListBrgBarcodeByBrgQuery`, `ListBrgBarcodeQuery`, `ListBrgBarcodeChangedQuery` | `btr.application` | §7.1 |
| `BarcodeType` + `IBarcodeKey` (record) | `btrade.domain` | §5.2 |
| `BarcodeRegistrationRequestType` | `btrade.domain` | §5.3 |
| `IBarcodeDal`, `IBarcodeRegistrationDal` contracts | `btrade.application` | §7.2 |
| Cloud DAL implementations | `btrade.infrastructure` | §7.2 |
| `BarcodeSyncCommand`, `BarcodeSyncListQuery` | `btrade.application` | §7.2 |
| `BarcodeRegistrationSubmitCommand`, `BarcodeRegistrationPendingQuery`, `BarcodeRegistrationAckCommand` | `btrade.application` | §7.2 |
| `UserSyncCommand`, `IssueTokenCommand` | `btrade.application` | §7.2 |
| `BrgBarcodeType`, `BrgBarcodeDal` (Main Office read) | `j07-btrade-sync` | §4.3 |
| `BarcodeSyncService`, `BarcodeRegistrationRelayService` | `j07-btrade-sync` | §4.3 |

### Database

| Object | System | Reference |
| ------ | ------ | --------- |
| `BTR_BrgBarcode` (+ `UX_BTR_BrgBarcode_BarcodeValueKey`, `IX_BTR_BrgBarcode_BrgId`, `IX_BTR_BrgBarcode_RowVer`, `FK_BTR_BrgBarcode_BTR_Brg`) | `btr.sql` | §6.1 |
| `BTRADE_BrgBarcode` | `btrade.sqldb` | §6.2 |
| `BTRADE_BarcodeRegistrationRequest` | `btrade.sqldb` | §6.3 |
| `BTRADE_Location` (+ seed data) | `btrade.sqldb` | §6.3 |
| `BTRADE_User` | `btrade.sqldb` | §6.3 |
| Idempotent upgrade scripts (`Upgrade_*.sql`) | `btr.sql`, `btrade.sqldb` | §10.1 |
| `.sqlproj` registrations | `btr.sql`, `btrade.sqldb` | §10.1 |
| Room entities: `barcode_entity`, `barang_entity`, `barcode_registration_request_entity`, `session_preferences` DataStore | BGud | §6.4 |

### Frontend

| Component | System | Reference |
| --------- | ------ | --------- |
| `BrgBarcodeForm` (SCR-DESK-001) + DTOs | `btr.distrib` | §11.1, §19.1 |
| `BrgBarcodeBrowser` (SCR-DESK-003) | `btr.distrib` | §11.1 |
| Item Barcode tab on `BrgForm` (SCR-DESK-002) | `btr.distrib` | §11.1 |
| `BTR_Menu` / `BTR_RoleMenu` seed + role grants | `btr.sql` / `btr.distrib` | §9.3 |
| BGud screens SCR-MOB-001..008 + ViewModels | BGud | §11.2, §19.3 |
| BGud navigation (`ui/Navigation.kt`) | BGud | §13.2 |

### Integration

| # | Endpoint | System | Reference |
| - | -------- | ------ | --------- |
| I-01 | `POST api/Barcode/sync` | `j07-btrade-sync` → Cloud | §8.1 |
| I-02 | `GET api/BarcodeRegistration/pending` | `j07-btrade-sync` → Cloud | §8.2 |
| I-03 | `POST api/BarcodeRegistration/ack` | `j07-btrade-sync` → Cloud | §8.3 |
| I-04 | `POST /api/barcode-registration` | BGud → Cloud | §8.5 |
| I-05 | `GET /api/barcodes/sync` | Cloud → BGud | §8.4 |
| I-06 | `GET /api/Brg/{serverId}` (existing) | Cloud → BGud | §8.4 |
| I-07 | `POST api/Auth/login` | BGud → Cloud | §9.1 |
| I-08 | `POST api/User` | `j07-btrade-sync` → Cloud | IR-05 |
| I-09 | `GET api/BarcodeRegistration/status` | BGud → Cloud | §8.3 |

---

## 4. Pre-Implementation Confirmations

These are architecture-recorded risks/confirmations (§24), not slices. Each
must be answered before the slice that depends on it begins. They are listed
here so the planner does not invent decisions.

| # | Item | Blocks slice |
| - | ---- | ------------ |
| C-1 | Confirm activation/deactivation is Desktop-only (no BGud screen) | S5 (BGud inventory) |
| C-2 | Confirm credential projection approach for token issuance | S4.5 (token issuance) |
| C-3 | Resolve transport security (release-blocking, not planning-blocking) | None (release gate) |
| C-4 | Update `BARCODE-REGISTRY-DOMAIN.md` (Knowledge Curator pass) | Knowledge sync (parallel) |
| C-5 | Produce `BARCODE-REGISTRY-WORKFLOW.md` if required by artifact chain | Knowledge sync (parallel) |
| C-6 | Confirm `BTR_Menu.MenuId` value and role grants | S2.4 (Desktop menu) |

---

## 5. Phases

| Phase | Name | System(s) | Business Value |
| ----- | ---- | --------- | -------------- |
| P1 | Main Office registry (domain, persistence, application) | `j05-btr-distrib` | Authoritative barcode-to-Item mapping exists and can be validated/queried |
| P2 | Desktop maintenance surface | `j05-btr-distrib` | Office Admin can register/correct/activate/deactivate and view Item barcodes |
| P3 | Cloud read model, staging queue, and authentication | `j06-pkl-btrade-api` | Cloud can receive published data, stage requests, and issue JWTs |
| P4 | Synchronization client | `j07-btrade-sync` | Main Office and Cloud stay in sync; offline requests are relayed and acknowledged |
| P5 | BGud Android application | BGud | Warehouse staff resolve barcodes offline and capture registration requests |

---

## 6. Slices

Complexity scale: **1 = Easy, 5 = Most Complex**. Complexity drives the AI
model and reasoning level selected for implementation.

### Phase P1 — Main Office Registry

#### S1.1 — `BTR_BrgBarcode` schema + sqlproj registration + upgrade script

- **Objective:** Create the authoritative barcode table, indexes, unique
  constraint, and FK exactly as specified in §6.1, register it in
  `btr.sql.sqlproj`, and ship an idempotent upgrade script.
- **Dependencies:** None.
- **Complexity:** 1
- **Acceptance Criteria:**
  - `btr.sql/Tables/BrgContext/BTR_BrgBarcode.sql` exists with the exact DDL from §6.1 (persisted computed `BarcodeValueKey`, `UX_BTR_BrgBarcode_BarcodeValueKey`, `IX_BTR_BrgBarcode_BrgId`, `IX_BTR_BrgBarcode_RowVer`, `FK_BTR_BrgBarcode_BTR_Brg`, `RowVer ROWVERSION`).
  - The table is registered in `btr.sql.sqlproj`.
  - An idempotent upgrade script (`IF OBJECT_ID(...) IS NULL ...` / `IF COL_LENGTH(...) IS NULL ...`) exists in `btr.sql/Scripts/`.
  - No existing table, column, or index is modified or dropped (§10.1).
  - The script is re-runnable without error.
- **Review Focus:** Persistence Compliance; SSDT/idempotency; FK deviation (R-05) mitigation.

#### S1.2 — `BrgBarcodeModel`, `IBrgBarcodeKey`, and barcode normalization

- **Objective:** Create the domain model and key interface (§5.1), including
  the VO-01 normalization rules and VO-02 optional `Satuan` semantics.
- **Dependencies:** S1.1.
- **Complexity:** 2
- **Acceptance Criteria:**
  - `btr.domain/BrgContext/BrgBarcodeAgg/BrgBarcodeModel.cs` and `IBrgBarcodeKey.cs` exist, mirroring `BrgModel`/`IBrgKey` shape.
  - A normalization routine removes CR/LF/TAB, trims whitespace, preserves leading zeros, and produces an `UPPER` comparison key (BQ-5).
  - `Satuan` defaults to `''` when absent (BR-005).
- **Review Focus:** Architecture Compliance; domain layering (§5.1); VO-01 normalization.

#### S1.3 — `IBrgBarcodeDal` contract and `BrgBarcodeDal` (Dapper)

- **Objective:** Implement the application DAL contract and Dapper infrastructure
  implementation, including the specialized members `GetByValue`,
  `ListByBrg`, `ListChanged`, and `ExistsByValue` (§5.1).
- **Dependencies:** S1.1, S1.2.
- **Complexity:** 3
- **Acceptance Criteria:**
  - `IBrgBarcodeDal` composes `IInsert`, `IUpdate`, `IGetData`, `IListData` and declares the four specialized members.
  - `BrgBarcodeDal` (Dapper) implements all members.
  - `ListChanged(byte[] watermark)` returns rows with `RowVer > watermark` ordered by `RowVer` (IR-04).
  - `GetByValue`/`ExistsByValue` operate on `BarcodeValueKey` (uppercase normalized key), never raw value.
- **Review Focus:** Persistence Compliance; Dapper conventions; change-feed correctness (IR-04).

#### S1.4 — `BrgBarcodeBuilder`

- **Objective:** Implement the builder that resolves/denormalizes `BrgCode` and
  `BrgName` from Product Master and exposes `Create`, `Load`, `Attach`,
  `BarcodeValue`, `Brg`, `Satuan`, `Activate`, `Deactivate` (§7.1).
- **Dependencies:** S1.2, S1.3.
- **Complexity:** 3
- **Acceptance Criteria:**
  - `BrgBarcodeBuilder` mirrors `BrgBuilder` methods.
  - `BrgCode`/`BrgName` are resolved from `BTR_Brg` (read-only; never mutated, INV-09).
  - `Satuan` accepts only a `BTR_BrgSatuan.Satuan` value belonging to the mapped Item (IR-01).
- **Review Focus:** Architecture Compliance; INV-09 (Product Master never modified).

#### S1.5 — `BrgBarcodeValidator`

- **Objective:** Implement FluentValidation rules for INV-01, INV-04, INV-05.
- **Dependencies:** S1.2.
- **Complexity:** 2
- **Acceptance Criteria:**
  - `BrgBarcodeValidator : AbstractValidator<BrgBarcodeModel>` enforces non-empty normalized `BarcodeValue` (INV-01), existing `BrgId` (INV-04), and existing `(BrgId, Satuan)` when `Satuan` present (INV-05).
- **Review Focus:** Validation Ownership (§18.1); invariant coverage.

#### S1.6 — Commands: Register, Correct, Activate, Deactivate + `BrgBarcodeWriter`

- **Objective:** Implement the four lifecycle commands and the single-transaction
  writer (P-03), including uniqueness pre-check (INV-02), audit stamping, and
  lifecycle legality (INV-06/07/08).
- **Dependencies:** S1.3, S1.4, S1.5.
- **Complexity:** 4
- **Acceptance Criteria:**
  - `RegisterBrgBarcodeCommand` normalizes the value, validates uniqueness/Item/unit, creates the mapping Active (INV-06), stamps audit fields, and commits in one `TransHelper.NewScope()`.
  - `CorrectBrgBarcodeCommand` changes only `BrgId`/`Satuan`, preserves activation state (INV-07), stamps `ModifiedBy`/`ModifiedDate`.
  - `ActivateBrgBarcodeCommand` / `DeactivateBrgBarcodeCommand` enforce INV-08; no delete path exists.
  - Duplicate `BarcodeValue` is rejected (INV-02); the unique index is the final enforcement point.
- **Review Focus:** Persistence Compliance (P-03 single transaction); invariant enforcement (INV-02/06/07/08); audit stamping.

#### S1.7 — Queries (Get, GetByValue, ListByBrg, List, ListChanged)

- **Objective:** Implement the five queries in §7.1.
- **Dependencies:** S1.3.
- **Complexity:** 2
- **Acceptance Criteria:**
  - `GetBrgBarcodeByValueQuery` returns only Active mappings (BR-007).
  - `ListBrgBarcodeByBrgQuery` returns all states for one Item (UC-004).
  - `ListBrgBarcodeChangedQuery` exposes the change feed (IR-04) separate from all other queries.
- **Review Focus:** Query/read-model correctness; Active-only lookup (BR-007).

#### S1.8 — `ProcessBarcodeRegistrationRequestCommand`

- **Objective:** Implement accept/reject of a relayed request (BR-012/BR-013):
  delegate to `RegisterBrgBarcodeCommand` on accept; reject without mutation on
  conflict; return `ACCEPTED`/`REJECTED` with a reason (§7.1, §8.2).
- **Dependencies:** S1.6.
- **Complexity:** 4
- **Acceptance Criteria:**
  - Produces `ACCEPTED` (delegates to register) or `REJECTED` (no mutation) with one of `DUPLICATE_BARCODE`, `ITEM_NOT_FOUND`, `ITEM_INACTIVE`, `INVALID_UNIT`.
  - Re-validates Item active state (BQ-7) and unit validity (INV-05).
  - No partial state: accept is a single Main Office transaction (P-03).
- **Review Focus:** Authority model (BR-011/013); idempotent re-relay safety (§8.2).

### Phase P2 — Desktop Maintenance Surface

#### S2.1 — `BrgBarcodeForm` + DTOs (SCR-DESK-001)

- **Objective:** Implement the maintenance form (register/correct/activate/
  deactivate/search) per §12.1, §14.1, §16 (IR-D1..D6), wiring UI actions to
  commands/queries in §17.1.
- **Dependencies:** S1.6, S1.7.
- **Complexity:** 4
- **Acceptance Criteria:**
  - `BrgBarcodeForm` opens as an MDI child via the DI container using `BringMdiChildToFrontIfLoaded<T>()`.
  - Save (new) → `RegisterBrgBarcodeCommand`; Save (existing) → `CorrectBrgBarcodeCommand`; Activate/Deactivate → respective commands; Enter on barcode input → `GetBrgBarcodeByValueQuery`.
  - Duplicate detection surfaces "View Existing Mapping" and never mutates state (IR-D6, §14.1).
  - Enable/disable rules match §16 IR-D1..D6 (role gate for Activate/Deactivate per BQ-6).
  - `BrgBarcodeFormBarcodeDto` matches §19.1.
- **Review Focus:** UI State Compliance (§14.1); Workflow Compliance (UC-002/003); role gating (BQ-6).

#### S2.2 — `BrgBarcodeBrowser` (SCR-DESK-003)

- **Objective:** Implement the read-only `IBrowser<BrgBarcodeBrowserView>`
  browser (§11.1, §19.1).
- **Dependencies:** S1.7.
- **Complexity:** 2
- **Acceptance Criteria:**
  - `BrgBarcodeBrowser` reuses the existing `IBrowser<T>` infrastructure.
  - `BrgBarcodeBrowserView` matches §19.1; source query is `ListBrgBarcodeQuery`.
  - Double-click returns the selected mapping to the caller.
- **Review Focus:** Reuse of existing browser infrastructure; read-only guarantee.

#### S2.3 — Item Barcode tab on `BrgForm` (SCR-DESK-002)

- **Objective:** Add the Barcode tab to `BrgForm` with grid + toolbar, using the
  existing `BindingList<Dto>` + grid pattern (§11.1, §12.2).
- **Dependencies:** S1.7, S2.2 (optional for editing).
- **Complexity:** 3
- **Acceptance Criteria:**
  - A "Barcode" tab appears alongside Satuan/Harga/Stok tabs.
  - Grid shows Barcode, Unit, Status, Modified; loads via `ListBrgBarcodeByBrgQuery` (UC-004).
  - Add/Edit/Activate/Deactivate wire to the §7.1 commands; participates in the form's save transaction via `BrgBarcodeWriter`.
- **Review Focus:** UI State Compliance; persistence transaction integration; UC-004.

#### S2.4 — Menu seed + role grants

- **Objective:** Add `BTR_Menu` row and `BTR_RoleMenu` grants so the form is
  reachable through the role-gated ribbon (§9.3, §11.1).
- **Dependencies:** S2.1; C-6 (menu id value).
- **Complexity:** 1
- **Acceptance Criteria:**
  - A new `BTR_Menu` row exists for the barcode maintenance form.
  - `BTR_RoleMenu` grants map to BQ-6 roles (Office Admin, System Administrator).
  - The form is reachable through `MainForm.SetupUserMenu` for granted roles only.
- **Review Focus:** Security Compliance (role gating); menu/ribbon convention.

### Phase P3 — Cloud Read Model, Staging Queue, Authentication

#### S3.1 — Cloud schema objects + sqlproj registration + upgrade script

- **Objective:** Create `BTRADE_BrgBarcode`, `BTRADE_BarcodeRegistrationRequest`,
  `BTRADE_Location` (+ seed data), and `BTRADE_User` per §6.2/§6.3, register in
  `btrade.sqldb.sqlproj`, ship idempotent upgrade scripts.
- **Dependencies:** None.
- **Complexity:** 2
- **Acceptance Criteria:**
  - Four tables exist with the exact DDL from §6.2/§6.3, including `UX_BTRADE_BrgBarcode_ServerId_BarcodeValue` and `UX_BTRADE_BRR_ServerId_ClientRequestId`.
  - `BTRADE_Location` seed data matches §6.3 (GAMPING/CONCAT → JOGJA, MAGELANG → MGL).
  - All objects registered in `btrade.sqldb.sqlproj`.
  - Idempotent upgrade scripts provided; re-runnable.
- **Review Focus:** Persistence Compliance; uniqueness constraints (ADR-004, INV-12).

#### S3.2 — Cloud domain models + DAL contracts

- **Objective:** Implement `BarcodeType`/`IBarcodeKey` and
  `BarcodeRegistrationRequestType` (§5.2, §5.3), plus `IBarcodeDal` and
  `IBarcodeRegistrationDal` contracts (§7.2).
- **Dependencies:** S3.1.
- **Complexity:** 2
- **Acceptance Criteria:**
  - `BarcodeType` record matches §5.2 shape (`IServerId` composition).
  - `BarcodeRegistrationRequestType` matches §5.3 shape with `Status` field.
  - DAL contracts follow `IBrgDal` composition (`IInsert`, `IUpdate`, `IDelete`, `IGetDataMayBe`, `IListDataMayBe`).
- **Review Focus:** Architecture Compliance; contract shape fidelity.

#### S3.3 — Cloud DAL implementations

- **Objective:** Implement `IBarcodeDal` and `IBarcodeRegistrationDal` (Dapper).
- **Dependencies:** S3.1, S3.2.
- **Complexity:** 2
- **Acceptance Criteria:**
  - DALs implement all contract members.
  - Read/write operations are always scoped by `ServerId` (tenant boundary, ADR-007).
  - No lookup method reads the registration staging table (INV-10).
- **Review Focus:** Tenant scoping (P-06); INV-10 (staging never a lookup source).

#### S3.4 — `BarcodeSyncCommand` + `BarcodeSyncListQuery`

- **Objective:** Implement incremental apply (upsert Active / remove deactivated,
  scoped to JWT `ServerId`) and bulk Active download (§7.2, §8.1).
- **Dependencies:** S3.3.
- **Complexity:** 3
- **Acceptance Criteria:**
  - `BarcodeSyncCommand` upserts `ListUpsert` and deletes `ListRemove`, all scoped to the resolved `ServerId`; never replaces the tenant snapshot (P-09).
  - `BarcodeSyncListQuery` returns all Active barcodes for the `ServerId`.
  - No `ServerId` field is accepted on the command payload (ADR-007).
- **Review Focus:** Architecture Compliance (P-09); idempotency (§10.6).

#### S3.5 — Registration submit/pending/ack commands + queries

- **Objective:** Implement `BarcodeRegistrationSubmitCommand`,
  `BarcodeRegistrationPendingQuery`, `BarcodeRegistrationAckCommand` (§7.2),
  plus a status projection for I-09.
- **Dependencies:** S3.3.
- **Complexity:** 3
- **Acceptance Criteria:**
  - Submit stages one request `Status = PENDING`; enforces `(ServerId, ClientRequestId)` idempotency (INV-12).
  - Pending returns `PENDING` requests for the `ServerId`.
  - Ack enforces terminal transition `PENDING → ACCEPTED|REJECTED` only (INV-11); sets `ProcessedAt`, `ProcessedNote`.
  - Status projection returns only the caller's own requests (`RequestedBy`).
- **Review Focus:** Idempotency (INV-12); terminal status (INV-11); no business validation (BR-011).

#### S3.6 — Token issuance (`IssueTokenCommand`) + `POST api/Auth/login`

- **Objective:** Implement SHA-256 password verification against `BTRADE_User`,
  location → `ServerId` resolution, and JWT issuance (IR-05, GAP-008, ADR-007).
- **Dependencies:** S3.1 (seed/projection), S3.3; C-2.
- **Complexity:** 5
- **Acceptance Criteria:**
  - `POST api/Auth/login` verifies `SHA-256(password)` against `BTRADE_User.Password` exactly as `LoginForm.cs`.
  - Resolves `ServerId` from `BTRADE_Location(locationId)`.
  - JWT carries `sub = userId`, `role`, `locationId`, `serverId` (§9.1).
  - Rejects unknown users and invalid locations.
- **Review Focus:** Security Compliance; credential verification parity (IR-05); R-08.

#### S3.7 — Barcode Registry controllers + JWT enforcement

- **Objective:** Implement controllers for I-01..I-09 with JWT enforcement and
  server-side `ServerId` resolution (no client-supplied `ServerId`).
- **Dependencies:** S3.4, S3.5, S3.6.
- **Complexity:** 4
- **Acceptance Criteria:**
  - `POST api/Barcode/sync` (I-01), `GET api/BarcodeRegistration/pending` (I-02), `POST api/BarcodeRegistration/ack` (I-03), `POST /api/barcode-registration` (I-04), `GET /api/barcodes/sync` (I-05), `POST api/User` (I-08), `GET api/BarcodeRegistration/status` (I-09) are all authenticated.
  - Mandated routes (`GET /api/barcodes/sync`, `POST /api/barcode-registration`) are implemented verbatim as attribute routes (IR-07); remaining routes use PascalCase controller convention.
  - No endpoint reads `ServerId` from route/query/header/body for Barcode Registry routes (P-06).
  - Unauthenticated calls are rejected (ADR-002).
- **Review Focus:** Security Compliance (JWT, tenant binding); routing conventions (IR-07).

### Phase P4 — Synchronization Client

#### S4.1 — Main Office read model + change feed DAL

- **Objective:** Implement `BrgBarcodeType` transport model and `BrgBarcodeDal`
  (Main Office read) exposing the `ListChanged` change feed (§4.3).
- **Dependencies:** S1.1, S1.3.
- **Complexity:** 2
- **Acceptance Criteria:**
  - `BrgBarcodeDal` reads `BTR_BrgBarcode` and maps to `BrgBarcodeType` with `IsAktif`.
  - `ListChanged(watermark)` returns rows `RowVer > watermark` ordered by `RowVer`.
  - No duplicate of authoritative business logic (reads only, IR-08).
- **Review Focus:** Persistence Compliance; change-feed correctness (IR-04).

#### S4.2 — `BarcodeSyncService` (incremental publish)

- **Objective:** Implement Main Office → Cloud publish: map `ListChanged` into
  upsert/remove lists, `POST api/Barcode/sync`, advance watermark only on
  HTTP 200 (§8.1).
- **Dependencies:** S4.1, S3.7 (I-01).
- **Complexity:** 4
- **Acceptance Criteria:**
  - `IsAktif = 1 → upsert`, `IsAktif = 0 → remove` (TQ-3).
  - Payload carries no `ServerId`; JWT-scoped on the Cloud.
  - Watermark advances only after HTTP 200; failure leaves watermark unchanged and reports failure (§8.1).
  - Watermark persisted using the existing `RegistryHelper` pattern (IR-04).
- **Review Focus:** Incremental publish (TQ-3); watermark handling (R-06); idempotent replay (§10.6).

#### S4.3 — `BarcodeRegistrationRelayService` (relay + ack)

- **Objective:** Implement Cloud → Main Office relay: pull pending, invoke
  `ProcessBarcodeRegistrationRequestCommand` in-process, acknowledge explicitly
  (§8.2, §8.3).
- **Dependencies:** S1.8, S4.1, S3.7 (I-02/I-03).
- **Complexity:** 5
- **Acceptance Criteria:**
  - Pulls `GET api/BarcodeRegistration/pending`, processes each via the in-process Main Office command (IR-08), then `POST api/BarcodeRegistration/ack`.
  - Re-relay is safe: already-accepted requests yield `DUPLICATE_BARCODE` and are acknowledged `ACCEPTED` only if the authoritative mapping matches; otherwise `REJECTED` (§8.2).
  - Rejection reasons are transported verbatim (§8.2 table).
  - Relay runs before publish in the same run (§8.2).
- **Review Focus:** Idempotency (§10.6); single-authority validation (IR-08, BR-011); explicit ack (GAP-007).

#### S4.4 — Credential projection uploader (`UserSyncCommand` wiring)

- **Objective:** Replicate `BTR_User` → `BTRADE_User` projection via the existing
  master-data uploader pattern (IR-05).
- **Dependencies:** S3.1, S3.7 (I-08).
- **Complexity:** 3
- **Acceptance Criteria:**
  - Publishes `BTRADE_User` records with the same SHA-256 hash as `BTR_User.Password`.
  - Follows the existing `BrgSyncService` uploader pattern.
  - Runs over an authenticated endpoint (I-08).
- **Review Focus:** Security Compliance (R-08); uploader-pattern reuse.

#### S4.5 — `SyncForm` wiring + run ordering

- **Objective:** Wire the new services into the existing sync run UI, ordering
  relay before publish, and include the user projection upload (§4.3, §8.1, §8.2).
- **Dependencies:** S4.2, S4.3, S4.4.
- **Complexity:** 3
- **Acceptance Criteria:**
  - The synchronization run executes: user projection → registration relay → barcode publish, in that order.
  - Failures are surfaced in the sync UI; watermark/ack state reflects only successful commits.
- **Review Focus:** Workflow Compliance (§8); run ordering (P-09).

### Phase P5 — BGud Android Application

> C-1 must be confirmed before finalizing the screen inventory (activation/
> deactivation is Desktop-only per IR-06).

#### S5.1 — Project scaffold + Room schema

- **Objective:** Create the BGud Android project (Kotlin, Jetpack Compose,
  Navigation Compose, Room, Retrofit, OkHttp, Gson, DataStore) and the Room
  database/entities/DAOs per §6.4, §23.2.
- **Dependencies:** None.
- **Complexity:** 5
- **Acceptance Criteria:**
  - Project compiles with the mandated stack (§4.4).
  - `AppDatabase` with `barcode_entity`, `barang_entity`, `barcode_registration_request_entity` matches §6.4 (keys, fields, `barcodeValueKey`).
  - Indexes: `barcode_entity(barcodeValueKey)` unique, `barcode_entity(brgId)`, `barcode_registration_request_entity(status)`.
  - `session_preferences` DataStore for token/user/warehouseCode/officeCode/last sync timestamps.
- **Review Focus:** Architecture Compliance (stack, schema); offline-cache-only Active (TQ-6).

#### S5.2 — Network layer + API client + DataStore session

- **Objective:** Implement Retrofit/OkHttp/Gson API client with JWT interceptor
  and the session/warehouse binding model (§23.2, IR-09).
- **Dependencies:** S5.1.
- **Complexity:** 3
- **Acceptance Criteria:**
  - API client defines the I-04, I-05, I-06, I-07, I-09 endpoints.
  - Every request attaches the JWT; no `ServerId` is sent as a command input (IR-09).
  - Session stores `warehouseCode` (not `ServerId`) on queued requests.
- **Review Focus:** Security Compliance (JWT); tenant-bound session (ADR-007, IR-09).

#### S5.3 — Sync worker + repository (submission, download, status)

- **Objective:** Implement the WorkManager sync worker and repository that, in
  order: submit pending requests → download barcodes → download Barang → refresh
  request statuses (§15.2, §20).
- **Dependencies:** S5.1, S5.2.
- **Complexity:** 5
- **Acceptance Criteria:**
  - Submission batches queued requests, one HTTP call each (I-04).
  - Downloads `GET /api/barcodes/sync` (I-05) and `GET /api/Brg/{serverId}` (I-06), replacing local Active caches.
  - Requests its own outcomes via `GET api/BarcodeRegistration/status` (I-09) and updates local queue statuses (`PENDING → SYNCED|REJECTED`).
  - One sync run at a time (IR-M6); no periodic scheduling in MVP (UX §13).
- **Review Focus:** Sync ordering (§15.2); idempotency; no periodic scheduling.

#### S5.4 — Login screen (SCR-MOB-001)

- **Objective:** Implement login + office resolution + login-time master data
  sync → home (§13.2, §14, UX §5).
- **Dependencies:** S5.2, S5.3.
- **Complexity:** 3
- **Acceptance Criteria:**
  - Login calls `POST api/Auth/login` (I-07) with username/password/warehouse.
  - Warehouse selector shows Gudang Gamping / Gudang Concat / Gudang Magelang.
  - On success: master data sync runs, then navigate to home; on failure: error region.
  - No valid JWT → all operational commands blocked (IR-M8).
- **Review Focus:** Security Compliance (auth); navigation (start destination = login when no session).

#### S5.5 — Home screen (SCR-MOB-002)

- **Objective:** Implement the home hub with context header, quick actions, sync
  status card, and navigation (§12.4, UX §6).
- **Dependencies:** S5.4.
- **Complexity:** 2
- **Acceptance Criteria:**
  - Shows user/warehouse/office, sync status (online/offline, last sync, pending count).
  - Quick actions and navigation match §12.4.
- **Review Focus:** UI State Compliance; navigation wiring.

#### S5.6 — Scan Barcode screen (SCR-MOB-003)

- **Objective:** Implement CameraX + ML Kit scanning, cache-first lookup via Room
  unique index on `barcodeValueKey`, and Not Found → Register flow (§12.5, §14.2).
- **Dependencies:** S5.1, S5.3.
- **Complexity:** 4
- **Acceptance Criteria:**
  - Lookup is local Room only; no network call per scan (P-07, ADR-006).
  - Found shows Barcode, Item Code, Item Name, Unit (Scenario A).
  - Not Found offers Register Barcode / Cancel (Scenario B).
  - Manual entry fallback uses the same resolution path.
  - Single-scan resolution mode, then re-arm (§20).
- **Review Focus:** Cache-first lookup (P-07); camera isolation (P-10); UX-002 scan-first.

#### S5.7 — Register Barcode screen (SCR-MOB-004)

- **Objective:** Implement registration request capture with local Barang search,
  optional Unit, and local-only save (§12.6, §14.3).
- **Dependencies:** S5.1, S5.6 (barcode argument).
- **Complexity:** 3
- **Acceptance Criteria:**
  - Barcode is read-only (captured); Item search is local Room cache.
  - Save requires an Active cached Item (BQ-7, IR-M3); Unit optional (BR-005).
  - Save writes locally to the request queue (`PENDING`); no network (UX §8).
  - Local duplicate detection surfaces "Barcode sudah terdaftar." (IR-M4).
  - Inactive Item → "Item sudah tidak aktif." (IR-M3).
- **Review Focus:** UI State Compliance (§14.3); offline write = queued intent (P-08).

#### S5.8 — Barcode Registry list screen (SCR-MOB-005)

- **Objective:** Implement searchable local cache list with paging/debounce per
  §20 (UX §9).
- **Dependencies:** S5.1.
- **Complexity:** 2
- **Acceptance Criteria:**
  - Search by barcode/Item code/name against local Room cache (min 3 chars, 300ms debounce).
  - Row action navigates to Edit Barcode.
  - Default load 50 rows, load-more on scroll.
- **Review Focus:** Frontend performance (§20); local-only source.

#### S5.9 — Edit Barcode screen (SCR-MOB-006)

- **Objective:** Implement correction capture: change Item/Unit, local save
  (§12.8, §14.4, UX §10).
- **Dependencies:** S5.1, S5.8.
- **Complexity:** 2
- **Acceptance Criteria:**
  - Barcode value is read-only (never editable).
  - Save writes locally (`PENDING` correction carrying the barcode identity).
  - No activation/deactivation surface (IR-06, C-1).
- **Review Focus:** Correction semantics (INV-07); no activation surface (IR-06).

#### S5.10 — Synchronization screen (SCR-MOB-007)

- **Objective:** Implement sync state display + Sync Now trigger (§12.9, §14.5).
- **Dependencies:** S5.3.
- **Complexity:** 2
- **Acceptance Criteria:**
  - Shows last Barang/barcode sync timestamps, pending/success/rejected counts.
  - Sync Now triggers the worker; success/failure states with retry (§14.5).
  - Offline disables Sync Now (IR-M5).
- **Review Focus:** UI State Compliance (§14.5); connectivity handling.

#### S5.11 — Settings screen + navigation wiring (SCR-MOB-008)

- **Objective:** Implement Settings surface and finalize Navigation Compose wiring
  per §13.2 (§12, UX §4).
- **Dependencies:** S5.4, S5.5, S5.6, S5.7, S5.8, S5.9, S5.10.
- **Complexity:** 3
- **Acceptance Criteria:**
  - Navigation graph matches §13.2 (routes, arguments, conditions).
  - Settings surface per UX §4.
  - Start destination logic: `login` when no session, else `home`.
- **Review Focus:** Navigation compliance (§13.2); ViewModel/Factory convention (§19.3).

---

## 7. Dependency Graph (Summary)

```text
S1.1
 └─ S1.2 ─┬─ S1.3 ─┬─ S1.4 ─┬─ S1.6 ─ S1.8
          │        │        └─ S1.5 ─┘
          │        ├─ S1.7
          │        └─ S4.1
          └─ (S1.5)

S1.6, S1.7 ─ S2.1 ─ S2.4 (C-6)
S1.7 ─ S2.2, S2.3
S1.7 ─ S2.3

S3.1 ─┬─ S3.2 ─ S3.3 ─┬─ S3.4 ─┐
      │                ├─ S3.5 ─┼─ S3.7
      │                └─ S3.6 ─┘   (C-2)
      └─ (S3.6)

S3.7 ─ S4.2, S4.3, S4.4
S4.1 ─ S4.2, S4.3
S1.8 ─ S4.3
S4.2, S4.3, S4.4 ─ S4.5

S5.1 ─┬─ S5.2 ─┬─ S5.3 ─ S5.4 ─┬─ S5.5
      │        │                └─ S5.6
      │        └─ S5.4
      ├─ S5.6, S5.7, S5.8
      └─ S5.8 ─ S5.9
S5.3 ─ S5.10
S5.4..S5.10 ─ S5.11
```

---

## 8. Progress Tracker

| Slice | Status | Complexity | System |
| ----- | ------ | ---------- | ------ |
| S1.1 | IMPLEMENTED | 1 | `btr.sql` |
| S1.2 | IMPLEMENTED | 2 | `btr.domain` |
| S1.3 | GO | 3 | `btr.application` / `btr.infrastructure` |
| S1.4 | GO | 3 | `btr.application` |
| S1.5 | GO | 2 | `btr.application` |
| S1.6 | GO | 4 | `btr.application` |
| S1.7 | GO | 2 | `btr.application` |
| S1.8 | GO | 4 | `btr.application` |
| S2.1 | PLANNED | 4 | `btr.distrib` |
| S2.2 | PLANNED | 2 | `btr.distrib` |
| S2.3 | PLANNED | 3 | `btr.distrib` |
| S2.4 | PLANNED | 1 | `btr.sql` / `btr.distrib` |
| S3.1 | PLANNED | 2 | `btrade.sqldb` |
| S3.2 | PLANNED | 2 | `btrade.domain` / `btrade.application` |
| S3.3 | PLANNED | 2 | `btrade.infrastructure` |
| S3.4 | PLANNED | 3 | `btrade.application` |
| S3.5 | PLANNED | 3 | `btrade.application` |
| S3.6 | PLANNED | 5 | `btrade.application` / `btrade.webapi` |
| S3.7 | PLANNED | 4 | `btrade.webapi` |
| S4.1 | PLANNED | 2 | `j07-btrade-sync` |
| S4.2 | PLANNED | 4 | `j07-btrade-sync` |
| S4.3 | PLANNED | 5 | `j07-btrade-sync` |
| S4.4 | PLANNED | 3 | `j07-btrade-sync` |
| S4.5 | PLANNED | 3 | `j07-btrade-sync` |
| S5.1 | PLANNED | 5 | BGud |
| S5.2 | PLANNED | 3 | BGud |
| S5.3 | PLANNED | 5 | BGud |
| S5.4 | PLANNED | 3 | BGud |
| S5.5 | PLANNED | 2 | BGud |
| S5.6 | PLANNED | 4 | BGud |
| S5.7 | PLANNED | 3 | BGud |
| S5.8 | PLANNED | 2 | BGud |
| S5.9 | PLANNED | 2 | BGud |
| S5.10 | PLANNED | 2 | BGud |
| S5.11 | PLANNED | 3 | BGud |

Lifecycle: `PLANNED → IN IMPLEMENTATION → IMPLEMENTED → IN REVIEW → GO` (or
`NO-GO → REMEDIATION → IN REVIEW → GO`).

---

## 9. Plan Validation

- **Scope coverage:** All §4–§20 architecture elements are mapped to slices (§3
  Impact Inventory → §6 Slices). No architecture element is unplanned.
- **Dependencies:** Explicit per slice (§6); no slice depends on a future slice
  (§7 graph is acyclic and topologically ordered).
- **Reviewable slices:** Every slice has a single objective and objective
  acceptance criteria (§6).
- **Tracker completeness:** Every slice has a tracker entry (§8).
- **Planning authority compliance:** No new business or architecture decision is
  introduced; open items are carried as confirmations (§4), not resolved.
