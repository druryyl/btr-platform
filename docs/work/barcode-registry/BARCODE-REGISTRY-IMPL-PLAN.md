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
| C-2 | **CONFIRMED (S3.6):** the Cloud verifies credentials against the replicated `BTRADE_User` verification projection (IR-05) and resolves `ServerId` from `BTRADE_Location`; no Cloud user store is introduced. The projection is populated by `UserSyncCommand`/I-08 (S4.4); the Cloud never validates business rules, only the SHA-256 credential parity with `LoginForm.cs`. | S3.6 (token issuance) |
| C-3 | Resolve transport security (release-blocking, not planning-blocking) | None (release gate) |
| C-4 | Update `BARCODE-REGISTRY-DOMAIN.md` (Knowledge Curator pass) | Knowledge sync (parallel) |
| C-5 | Produce `BARCODE-REGISTRY-WORKFLOW.md` if required by artifact chain | Knowledge sync (parallel) |
| C-6 | **CONFIRMED (S2.4):** `BTR_Menu` identifier `IM5` (GroupOrder 32, FormType `MASTR`, MenuName `IM5BrgBarcodeMenu`, Caption `IM5-Barcode`), matching the S2.1 ribbon button. `BTR_RoleMenu` grant seeded for `SYSAD` (System Administrator). The Office Admin grant is deferred to live role configuration (`XX4-RoleMenu`) because no `Office Admin` role exists in `BTR_Role` seeds. | S2.4 (Desktop menu) |
| C-7 | **CONFIRMED (AUTH-GAP-001 remediation):** sync-client authentication shall use dedicated, non-human **service accounts** (for example `sync_jogja`, `sync_mgl`), used exclusively by `j07-btrade-sync`, each mapped to a single Office / `ServerId`, with credentials managed by system administrators and not tied to individual employees. A human Office Admin account is rejected (password changes may break synchronization; poor auditability; operational dependency on an individual user). This is an externally approved architecture decision translated into slice S4.6; it is not originated by this plan. | S4.6 (sync client authentication) |

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

#### S3.8 — Unified JWT validation stack (AUTH-VERIFY-001 remediation)

- **Objective:** Remove the IdentityModel generation split that makes `JwtBearer`
  reject every valid token (AUTH-VERIFY-001, ADR-008). Make
  `Microsoft.IdentityModel.JsonWebTokens` the single handler library for both
  issuance and validation by wiring a `JsonWebTokenHandler`-backed
  `ISecurityTokenValidator` into `JwtBearerOptions.SecurityTokenValidators`. No
  IdentityModel version change and no target-framework change.
- **Dependencies:** S3.6, S3.7; AUTH-VERIFY-001 (§8.2); ADR-008.
- **Complexity:** 3
- **Acceptance Criteria:**
  - Issuance and validation both execute on `Microsoft.IdentityModel.JsonWebTokens`
    8.0.1; `System.IdentityModel.Tokens.Jwt` `JwtSecurityTokenHandler` is not on
    either request path.
  - An `[Authorize]` Barcode Registry endpoint returns `200 OK` when called with a
    valid issued token, and `401` with no token or an invalid token — the
    AUTH-VERIFY-001 flow: Issue Token → Call Authorized Endpoint → `200 OK`.
  - Claims `sub`, `role`, `locationId`, `serverId` remain resolvable from the
    authenticated principal.
  - No `MissingMethodException` / `TypeLoadException` occurs on issuance or
    validation.
  - Regression protection is committed: a repeatable automated integration test
    (`btrade.webapi.Test`) asserts anonymous → `401` and valid token → `200`
    against an authenticated endpoint using the real `AddPresentation` JWT
    configuration.
  - Login response shape, claim names, tenant resolution, and all Barcode Registry
    behavior are unchanged.
- **Review Focus:** Security Compliance; single-generation stack (ADR-008);
  AUTH-VERIFY-001 closure evidence.
- **Notes:** Alternatives rejected — referencing `System.IdentityModel.Tokens.Jwt`
  8.0.1 leaves the legacy handler on the validation path (two handler types);
  moving `j06-pkl-btrade-api` to net8.0 for `JwtBearer`'s native `TokenHandlers` is
  a larger deployment change and is not required for the outcome.

> **Release gate:** No authenticated Barcode Registry endpoint is production-ready
> until S3.8 is GO.

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

#### S4.6 — Sync Client Authentication

- **Objective:** Enable `j07-btrade-sync` to authenticate against the Cloud API
  and automatically attach bearer tokens to all protected Barcode Registry
  synchronization requests, so I-01 / I-02 / I-03 / I-08 succeed end-to-end
  (AUTH-GAP-001; Architecture §9.1, R-04).
- **Approved Authentication Model:** dedicated **service accounts** (for example
  `sync_jogja`, `sync_mgl`). Characteristics:
  - Non-human accounts.
  - Used exclusively by `j07-btrade-sync`.
  - Mapped to a single Office / `ServerId`.
  - Credentials managed by system administrators.
  - Not tied to individual employees.

  Rejected alternative: a human **Office Admin** account — password changes may
  break synchronization, auditability is poor, and it creates an operational
  dependency on an individual user. See C-7.
- **Scope:**
  - **In:** service-account authentication; token acquisition
    (`POST api/Auth/login`, I-07); token caching; bearer attachment; token
    refresh / re-login behavior; end-to-end verification.
  - **Out:** Barcode Registry business logic; synchronization business rules;
    BGud; Desktop UI.
- **Dependencies:** S3.6 (I-07 token issuance), S3.7 (I-01 / I-02 / I-03 / I-08
  protection), S3.8 (usable validation stack), S4.2, S4.3, S4.4 (the callers
  that must present the token), S4.5 (run wiring). Relates to Architecture §9.1
  and R-04.
- **Complexity:** 4
- **Acceptance Criteria:**
  - **Authentication:** a dedicated service account authenticates via
    `POST api/Auth/login` (I-07) and receives a JWT — the flow
    `Service Account → Login → JWT Received`.
  - **Authorized calls:** authenticated calls from `j07-btrade-sync` to I-01,
    I-02, I-03, and I-08 return `200 OK`.
  - **Bearer attachment:** every protected Barcode Registry synchronization
    request carries the `Authorization: Bearer <token>` header; no protected
    call is issued without it.
  - **Token caching:** the token is cached and reused across requests; the
    service does not re-authenticate on every request.
  - **Failure handling:** on an expired or rejected token the client
    re-authenticates and retries the failed request —
    `Expired Token → Re-authenticate → Retry`.
  - **Tenant binding:** the token is issued for the service account's mapped
    Office / `ServerId`; no `ServerId` is added to command payloads (ADR-007).
  - **Verification:** end-to-end proof —
    `j07-btrade-sync → Authenticate → Receive JWT → Call Protected API → 200 OK`.
  - **Closure linkage:** satisfying these criteria closes AUTH-GAP-001 (§8.2.1).
- **Review Focus:** Security Compliance; service-account model (non-human,
  single Office / `ServerId`); token lifecycle (acquisition, caching, refresh,
  retry); AUTH-GAP-001 closure evidence.

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

S3.6, S3.7 ─ S3.8
S3.7 ─ S4.2, S4.3, S4.4
S4.1 ─ S4.2, S4.3
S1.8 ─ S4.3
S4.2, S4.3, S4.4 ─ S4.5
S3.6, S3.7, S3.8 ─ S4.6
S4.2, S4.3, S4.4, S4.5 ─ S4.6

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
| S2.1 | GO | 4 | `btr.distrib` |
| S2.2 | GO | 2 | `btr.distrib` |
| S2.3 | GO | 3 | `btr.distrib` |
| S2.4 | GO | 1 | `btr.sql` / `btr.distrib` |
| S3.1 | GO | 2 | `btrade.sqldb` |
| S3.2 | GO | 2 | `btrade.domain` / `btrade.application` |
| S3.3 | GO | 2 | `btrade.infrastructure` |
| S3.4 | GO | 3 | `btrade.application` |
| S3.5 | GO | 3 | `btrade.application` |
| S3.6 | GO | 5 | `btrade.application` / `btrade.webapi` |
| S3.7 | GO | 4 | `btrade.webapi` |
| S3.8 | GO | 3 | `btrade.webapi` / `btrade.webapi.Test` |
| S4.1 | GO | 2 | `j07-btrade-sync` |
| S4.2 | GO | 4 | `j07-btrade-sync` |
| S4.3 | GO | 5 | `j07-btrade-sync` |
| S4.4 | GO | 3 | `j07-btrade-sync` |
| S4.5 | GO | 3 | `j07-btrade-sync` |
| S4.6 | PLANNED | 4 | `j07-btrade-sync` |
| S5.1 | GO | 5 | BGud |
| S5.2 | GO | 3 | BGud |
| S5.3 | GO | 5 | BGud |
| S5.4 | GO | 3 | BGud |
| S5.5 | PLANNED | 2 | BGud |
| S5.6 | PLANNED | 4 | BGud |
| S5.7 | PLANNED | 3 | BGud |
| S5.8 | PLANNED | 2 | BGud |
| S5.9 | PLANNED | 2 | BGud |
| S5.10 | PLANNED | 2 | BGud |
| S5.11 | PLANNED | 3 | BGud |

Lifecycle: `PLANNED → IN IMPLEMENTATION → IMPLEMENTED → IN REVIEW → GO` (or
`NO-GO → REMEDIATION → IN REVIEW → GO`).

### 8.1 Review History

| Slice | Date | Result | Findings | Remediation |
| ----- | ---- | ------ | -------- | ----------- |
| S3.2 | 2026-09-16 | GO | None blocking. **INFO-001:** Architecture §5.3 names no key interface for `BarcodeRegistrationRequestType`, but the §7.2-mandated `IDelete<>` / `IGetDataMayBe<>` composition requires one; `IBarcodeRegistrationRequestKey` (`BarcodeRegistrationId`) was realized by convention, mirroring existing `IOrderKey` / `ICheckInKey`. No business or architecture decision introduced. **INFO-002:** no automated test project targets `btrade.domain` / `btrade.application`; verification was by build (`dotnet build btrade.application.csproj` → 0 errors). | None required. |
| S3.6 | 2026-09-16 | GO | None blocking. **INFO-001:** the slice required supporting components not named in the §3 impact inventory — `UserType` / `LocationType` (`btrade.domain`) and read-only `IUserDal` / `ILocationDal` + Dapper DALs (`btrade.infrastructure`) — as direct prerequisites for `BTRADE_User` verification and `BTRADE_Location` resolution; no unauthorized scope expansion. **INFO-002:** JWT issuance uses `JsonWebTokenHandler` (`Microsoft.IdentityModel.JsonWebTokens` 8.0.1, already in the graph via MediatR 13) because the legacy `JwtSecurityTokenHandler` (`System.IdentityModel.Tokens.Jwt` 6.35.0) throws `TypeLoadException` against `Microsoft.IdentityModel.Tokens` 8.0.1 under the net6.0 runtime; output remains standard HS256 with the configured issuer/audience/key. **INFO-003:** token validation should follow the same IdentityModel generation — `JwtBearer` 6.0.36 default validators use the legacy handler; verify end-to-end with a real token on the authenticated endpoint path (S3.7). | None required for S3.6. INFO-003 remains a verification watch item for the authenticated path. |
| S3.7 | 2026-09-16 | GO | None blocking. **INFO-001:** `BarcodeSyncRequest` carries `BarcodeType[]`, whose shape includes a `ServerId` member; item-level values are inert because the handler overrides them with the JWT-resolved tenant, and `BarcodeType[]` is mandated by Architecture §8.1. **INFO-002:** the §19.2 response envelopes (`BarcodeSyncResponse`, `BarcodeRegistrationSubmitResponse`) are not produced; the source command/query contracts are S3.4/S3.5 (GO) and S3.7 acceptance criteria do not specify response shape. **INFO-003:** I-09 does not accept the optional `clientRequestId` filter from Architecture §17.2; it returns all of the caller's own requests. **INFO-004:** no automated test project targets `btrade.webapi`; verification was by build plus live unauthenticated route probing (7 routes → 401). | None required. Scope: `UserSyncCommand` + `IUserDal` write members were direct prerequisites for I-08; I-06/I-07 were pre-existing and untouched. Findings retained here; the standalone review report was deleted. |
| S4.4 | 2026-09-16 | GO | None blocking. **INFO-001:** the sync client attaches no JWT on I-08, though Architecture §9.1 requires the operator account to present one. Not introduced by S4.4 and identical to accepted S4.2 (I-01) / S4.3 (I-02/I-03); client token acquisition is not owned by any planned slice. **INFO-002:** `UserType` / `UserDal` are supporting components not named in the §3 impact inventory, required to read `BTR_User` for the projection. | N/A for S4.4. INFO-001 is a cross-cutting architecture item to be tracked separately. |
| S4.5 | 2026-09-16 | GO | None blocking. **INFO-001:** the run is triggered by a new operator-initiated `Sync Barcode` button in `SyncForm`; Architecture §8.1 only mandates that publish is part of the operator-initiated master-data synchronization run, so the concrete UI trigger is an implementation detail, not a new decision. **INFO-002:** the three steps run sequentially with `await` (`user projection → registration relay → barcode publish`); relay precedes publish per §8.2. A failed step is logged (red) and does not abort the remaining steps; watermark/ack advance only inside the step that committed (S4.2/S4.3). **INFO-003:** no automated test project targets `j07-btrade-sync` (net48 WinForms); verification was by `MSBuild /t:Rebuild` → exit 0 (only pre-existing CS0436/CS0108 warnings, none in `SyncForm.cs` / `SyncForm.Designer.cs`). **INFO-004:** S4.4's `UserType` / `UserDal` / `UserSyncService` remain uncommitted in the working tree; S4.5 depends on them. | None required. |
| S3.8 | 2026-09-16 | GO | None blocking. **INFO-001:** `Program.cs` gained `public partial class Program { }` solely so the new test project can use `WebApplicationFactory<Program>` (documented ASP.NET Core top-level-statements pattern); no runtime behavior change. **INFO-002:** the integration test replaces `IBarcodeDal` with a test-only `FakeBarcodeDal` (via `ConfigureTestServices`) so the real `AddPresentation` authentication pipeline is exercised without a database; test-only, no production coupling. **INFO-003:** the test project aligns `Microsoft.NET.Test.Sdk` 17.13.0 / `xunit` 2.9.3 / `xunit.runner.visualstudio` 3.0.0 with the versions already transitively referenced by `btrade.application` / `btrade.infrastructure` (pre-existing), avoiding NU1605 downgrades. **INFO-004:** `Microsoft.IdentityModel.JsonWebTokens` / `Microsoft.IdentityModel.Tokens` resolve to 8.0.1 and `System.IdentityModel.Tokens.Jwt` 6.35.0 remains in the output as a `JwtBearer` dependency but is no longer registered as a validator. | None required. |
| S3.6 | 2026-09-16 | GO | **Revalidation (post-S3.8).** INFO-002 (issuance/validation IdentityModel generation mismatch) and INFO-003 (IdentityModel validation watch item) are **CLOSED**: validation now runs on `Microsoft.IdentityModel.JsonWebTokens` 8.0.1 via `JsonWebTokenSecurityTokenValidator`, and the end-to-end flow is verified (AUTH-VERIFY-001-R). INFO-001 (supporting `UserType` / `LocationType` / `IUserDal` / `ILocationDal`) unchanged. | None required. |
| S3.7 | 2026-09-16 | GO | **Revalidation (post-S3.8).** INFO-004 (no automated test project targeting `btrade.webapi`) is **CLOSED**: `btrade.webapi.Test` now exercises the real `AddPresentation` JWT pipeline over `GET /api/barcodes/sync` (anonymous → `401`, invalid → `401`, issued token → `200`). INFO-001 reaffirmed safe — `BarcodeSyncCommand` re-stamps every item with the JWT-resolved `ServerId` (`BarcodeSyncCommand.cs:29`) and keys `ListRemove` deletes by it (`BarcodeSyncCommand.cs:39`). INFO-002 / INFO-003 unchanged (non-auth). | None required. |
| S4.2 | 2026-09-16 | GO | **Revalidation (post-S3.8).** No slice-level regression. Assumption invalidated: the endpoint's `401` is no longer masked by the validator defect — the sync client presents **no** JWT (`BarcodeSyncService.cs:48-56`), so I-01 cannot succeed in production. Carried as AUTH-GAP-001 (release-blocking; not introduced by this slice). | Cross-cutting: AUTH-GAP-001. |
| S4.3 | 2026-09-16 | GO | **Revalidation (post-S3.8).** No slice-level regression. Same invalidated assumption as S4.2 for I-02 / I-03 (`BarcodeRegistrationRelayService.cs:69-72,97-105`); relayed calls will `401` without a client token. Carried as AUTH-GAP-001. | Cross-cutting: AUTH-GAP-001. |
| S4.4 | 2026-09-16 | GO | **Revalidation (post-S3.8).** INFO-001 (no JWT on I-08) **remains and is reclassified** from INFO to MAJOR / release-blocking, because with the validator fixed it is the only remaining barrier to functional authenticated Barcode Registry integration (`UserSyncService.cs:38-42`). Not introduced by S4.4; recorded as AUTH-GAP-001 rather than reopening this slice. INFO-002 unchanged. | Cross-cutting: AUTH-GAP-001. |
| S4.5 | 2026-09-16 | GO | **Revalidation (post-S3.8).** Run ordering (`user projection → registration relay → barcode publish`) remains correct and failures still surface without advancing watermark/ack; however the run cannot complete end-to-end while AUTH-GAP-001 is open. INFO-003 (no automated test project for `j07-btrade-sync`) unchanged. | Cross-cutting: AUTH-GAP-001. |
| S5.1 | 2026-09-16 | GO | None blocking. **INFO-001:** `barang_entity` carries `brgCode` / `brgName` / `isAktif` / `satKecil` / `satBesar` although Architecture §6.4 names only its key (`brgId`); the fields mirror the shared `Brg` reference shape (ADR-005, `BTrade3` `Barang`) as a direct prerequisite for cached-Active validation (BQ-7) and Item Code/Name/Unit display, with full sync mapping deferred to S5.3. No business or architecture decision introduced. **INFO-002:** no automated test project targets `BGud`; verification was by `assembleDebug` → `app-debug.apk` (mandated TQ-7 stack resolves and compiles; Room KSP + DataStore included). `local.properties` (SDK path) is gitignored and untracked. | None required. Scope: Retrofit/OkHttp/Gson + CameraX/ML Kit/WorkManager are declared as dependencies only; the API client (S5.2), sync worker (S5.3), and screens (S5.4..S5.11) are untouched — `ui/Navigation.kt` is a documented placeholder. |
| S5.2 | 2026-09-16 | GO | None blocking. **INFO-001:** `serverId` appears as a receive-only field on `LoginResult` / `BarcodeDto` / `BrgDto`; permitted use per §8.4 (display + legacy I-06 read route only, ADR-007 §8) — no request body, query, or header carries `ServerId` as a command input, and neither DataStore nor Room stores it. **INFO-002:** no automated test project targets `BGud` (same as S5.1 INFO-002); verification was by `:app:assembleDebug` → `BUILD SUCCESSFUL` with the new `network/` + `model/api/` classes present in the debug output. Envelope shape (`status`/`code`/`data`) verified by reflection against the referenced `Nuna.Lib` assembly; payload fields verified against the S3.7 controllers and use-case records. | None required. Scope: 5 new files only (`model/api/ApiModels.kt`, `network/BtradeApiService.kt`, `network/AuthInterceptor.kt`, `network/ApiClient.kt`, `datastore/SessionBinding.kt`); no existing file modified except the tracker; sync worker/repository (S5.3) and screens (S5.4..S5.11) untouched. |
| S5.3 | 2026-09-16 | GO | None blocking. **INFO-001:** I-06 carries no `IsAktif` field, so every `BrgDto` maps to `BarangEntity(isAktif = true)` and removal-by-replace enforces cached-Active validation (BQ-7); direct prerequisite mapping, no business decision introduced. **INFO-002:** no automated test project targets `BGud` (same as S5.1 INFO-002 / S5.2 INFO-002); verification was by `:app:assembleDebug` → `BUILD SUCCESSFUL` (one intermediate compile error — `SyncRunResult`/`SubmitOutcome` type mismatch in `sync()` — fixed before the passing build; no existing file touched). **INFO-003:** a completed run always returns `Result.success` carrying counts + `errors` output data for the S5.10 retry surface; `Result.retry` is reserved for transport `IOException` escaping the repository and `Result.failure` for missing `baseUrl`/token (IR-M8). | None required. Scope: 2 new files only (`repository/BarcodeSyncRepository.kt`, `sync/BarcodeSyncWorker.kt`); no existing source file modified except the tracker; screens (S5.4..S5.11) untouched. |
| S5.4 | 2026-09-16 | GO | None blocking. **INFO-001:** no automated test project targets `BGud` (same as S5.1 INFO-002 / S5.2 INFO-002 / S5.3 INFO-002); verification was by `:app:compileDebugKotlin --rerun-tasks` → `BUILD SUCCESSFUL` plus `:app:assembleDebug` → `BUILD SUCCESSFUL`. **INFO-002:** `CLOUD_BASE_URL` in `ui/Navigation.kt` is intentionally blank (transport unresolved, C-3/R-03; no URL hardcoded per S5.2) — login with unconfigured server reports via the error region instead of issuing a call. **INFO-003:** a sync-phase failure keeps the user on login with the error region (strict §13.2: `home` requires login sync completion); the session is already saved so retry is cheap and idempotent. | None required. Scope: 3 new files (`viewmodel/LoginViewModel.kt`, `viewmodel/LoginViewModelFactory.kt`, `ui/screen/LoginScreen.kt`) + `ui/Navigation.kt` start-destination gate + tracker; home placeholder and all S5.5..S5.11 screens untouched. |

### 8.2 Verification Tracker

This section tracks the **current disposition** of verification items. It is a
tracker, not a chronological log: each item appears once, in its current state.
Superseded outcomes are retained as historical evidence in §8.2.3 and are not
active items.

- **Release-blocking:** AUTH-GAP-001 (OPEN; remediation owned by S4.6 — Sync Client Authentication). No other item in this section is release-blocking.
- **Open:** AUTH-GAP-001, DOC-001, TRACK-001, SEC-INFO-001 — §8.2.1.
- **Closed:** AUTH-VERIFY-001, AUTH-VERIFY-001-R, AUTH-REVALIDATE-001, ARCH-DEFECT-001 — §8.2.2.
- **Archived (superseded):** AUTH-VERIFY-001 original `FAIL` — §8.2.3.

#### 8.2.1 Open Items

| ID | Severity | Status | Verification | Evidence | Next Action |
| -- | -------- | ------ | ------------ | -------- | ----------- |
| AUTH-GAP-001 | MAJOR / release-blocking | **OPEN** (owner: **S4.6**) | `j07-btrade-sync` does not present a JWT on I-01 / I-02 / I-03 / I-08 although Architecture §9.1 requires it and all four endpoints are `[Authorize]` | `BarcodeSyncService.cs:48-56`, `BarcodeRegistrationRelayService.cs:69-72,97-105`, `UserSyncService.cs:38-42` send no `Authorization` header; `BarcodeController` / `BarcodeRegistrationController` / `UserController` carry `[Authorize]`; no planned slice owns client token acquisition (S4.4 review INFO-001). | Remediated by S4.6 — Sync Client Authentication (§6 S4.6), using the approved service-account model (C-7). **Closure criteria = S4.6 acceptance criteria:** service account authenticates (`Service Account → Login → JWT Received`); authenticated calls to I-01 / I-02 / I-03 / I-08 return `200 OK`; bearer attached to every protected sync request; token cached; expired token → re-authenticate → retry; end-to-end `j07-btrade-sync → Authenticate → Receive JWT → Call Protected API → 200 OK`. Relates to Architecture §9.1 and R-04. |
| DOC-001 | MINOR | **OPEN** | ADR-008 consequence statement is stale after S3.8 | `adrs/ADR-008-jwt-implementation-library.md` (`Consequences → Negative`) still states validation defaults to the legacy handler and "must be aligned", contradicting `PresentationService.cs:52-53`. | Knowledge Curator pass on ADR-008. |
| TRACK-001 | INFO | **OPEN** | `ARCH-DEFECT-001` not traceable to any other repository artifact | Repo-wide search finds no occurrence outside this record. | The ARCH-DEFECT-001 disposition row (§8.2.2) serves as the register entry. |
| SEC-INFO-001 | INFO (pre-existing) | **OPEN** | JWT signing key committed in cleartext in `appsettings.json`; now load-bearing | `btrade.webapi/appsettings.json:10`; `Program.cs:5-7` supports an optional machine-specific override. | Platform-owned; track under R-03 / R-08. Not introduced by S3.8. |

#### 8.2.2 Closed Items

| ID | Status | Verification | Closed By | Evidence |
| -- | ------ | ------------ | --------- | -------- |
| AUTH-VERIFY-001 | **CLOSED** | Issue a JWT with `JsonWebTokenHandler` (real `JwtTokenService`), call an authenticated (`[Authorize]`) endpoint, verify `JwtBearer` accepts the token | S3.8 remediation (§6 S3.8); closure re-verified by AUTH-VERIFY-001-R | Original `FAIL` archived in §8.2.3. Closure evidence: AUTH-VERIFY-001-R. Release gate for authenticated Barcode Registry endpoints satisfied. |
| AUTH-VERIFY-001-R | **CLOSED** | S3.8 remediation: re-run the AUTH-VERIFY-001 flow (Issue Token → Call Authorized Endpoint → `200 OK`) against the real `AddPresentation` config with the unified `JsonWebTokenHandler` validation stack | S3.8 remediation | `dotnet test btrade.webapi.Test` → 4/4 passed: anonymous → `401`; invalid token → `401`; token issued by the real `JwtTokenService` → `200 OK` on `GET /api/barcodes/sync`; principal exposes `sub`, `role`, `locationId`, `serverId`. Negative control: reverting the `SecurityTokenValidators` wiring reproduces the original defect (issued token → `401`). Resolved assemblies: `Microsoft.IdentityModel.JsonWebTokens` / `Microsoft.IdentityModel.Tokens` 8.0.1. No `MissingMethodException` / `TypeLoadException`. AUTH-VERIFY-001 closed. |
| AUTH-REVALIDATE-001 | **CLOSED** | Targeted revalidation of auth-dependent findings in S3.6 / S3.7 / S4.2 / S4.3 / S4.4 / S4.5 after S3.8 GO | Independent re-run against S3.8 GO | Independently reproduced `dotnet test btrade.webapi.Test` → 4/4 passed; resolved graph confirmed (`Microsoft.IdentityModel.JsonWebTokens` / `Microsoft.IdentityModel.Tokens` 8.0.1; `System.IdentityModel.Tokens.Jwt` 6.35.0 present but not registered as a validator); `PresentationService.cs:52-53` clears the default validators; tenant override verified (`BarcodeSyncCommand.cs:29,39`). Findings closed: S3.6 INFO-002 / INFO-003, S3.7 INFO-004. Finding escalated: S4.4 INFO-001 → AUTH-GAP-001. |
| ARCH-DEFECT-001 | **CLOSED** | Architecture-level defect underlying AUTH-VERIFY-001 (IdentityModel generation split; ADR-008 negative consequence / S3.6 INFO-003) | S3.8 (unified `JsonWebTokenHandler` validation stack) | S3.8 aligns issuance and validation on `Microsoft.IdentityModel.JsonWebTokens` 8.0.1 via `JsonWebTokenSecurityTokenValidator`; `JwtSecurityTokenHandler` is not on either request path; AUTH-VERIFY-001-R PASS. Note: the identifier `ARCH-DEFECT-001` is not recorded in any other repository artifact; this row records its disposition for traceability (TRACK-001). Follow-up: documentation sync — update ADR-008 `Consequences → Negative`, which still describes validation as unaligned (tracked as DOC-001). If the identifier instead denotes the sync-client authentication gap, it is OPEN — see AUTH-GAP-001. |

#### 8.2.3 Archived Verification History

Superseded verification outcomes are retained as historical evidence only. They
are **not** active items; the current disposition is stated in §8.2.1 / §8.2.2.

| ID | Date | Task | Result | Evidence | Follow-up | Superseded By |
| -- | ---- | ---- | ------ | -------- | --------- | ------------- |
| AUTH-VERIFY-001 | 2026-09-16 | Issue a JWT with `JsonWebTokenHandler` (real `JwtTokenService`), call an authenticated (`[Authorize]`) endpoint, verify `JwtBearer` accepts the token | **FAIL** | Token issued (3 segments). Under the real `AddPresentation` JwtBearer config: no token → `401`, garbage token → `401`, issued token → `401`; real `GET /api/barcodes/sync` (`[Authorize]`) → `401`. Diagnosis with identical `TokenValidationParameters`: `JsonWebTokenHandler.ValidateToken` → accepted (`IsValid=True`); `JwtSecurityTokenHandler.ValidateToken` (JwtBearer 6.0.36 default validator) → `MissingMethodException: Method not found: 'ICollection<BaseConfiguration> BaseConfigurationManager.GetValidLkgConfigurations()'`. Root cause: `System.IdentityModel.Tokens.Jwt` 6.35.0 executing against `Microsoft.IdentityModel.Tokens` 8.0.1 (via MediatR 13) — the validation-side form of S3.6 INFO-002. `JwtBearer` swallows the handler failure and reports `401`. | Separate remediation task required; out of scope for this verification. `JsonWebTokenHandler` is not an `ISecurityTokenValidator` in IdentityModel 8.0.1, so JwtBearer 6.0.36 cannot consume it directly; options are a validator adapter, an API target-framework move to use `TokenHandlers`, or aligning IdentityModel on 6.x. S3.7 acceptance (unauthenticated → 401) remains satisfied, so its GO is unaffected. See ADR-008. **Remediation: S3.8 (PLANNED).** Classification: not a planning blocker, not a Barcode Registry domain blocker, release blocker for authenticated API usage. | AUTH-VERIFY-001-R (PASS, S3.8 remediation) |

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
  introduced by the planner; open items are carried as confirmations (§4), not
  resolved. The sync-client service-account model recorded in C-7 / S4.6 is an
  externally approved architecture decision (AUTH-GAP-001 remediation) that this
  plan translates into a slice, not a planner-originated decision.
