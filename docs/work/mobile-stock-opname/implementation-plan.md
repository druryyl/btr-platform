# Implementation Plan — Mobile-Assisted Stok Opname

## Document Status

| Field | Value |
| ----- | ----- |
| Task | Mobile-Assisted Stok Opname |
| Authoritative requirements | [../../features/stock-opname/feature.md](../../features/stock-opname/feature.md) |
| Evidence source | Current repository and `desktop-stock-opname-codebase-excavation.md` |
| Author role | Architect |
| Status | **Feasible; plan pending business approval** |
| Implementation authorization | **Not granted** |

This plan intentionally stops before implementation. Product Owner and Inventory owner approval of the feature rules and open decisions is required first.

---

## 1. Feasibility Verdict

The feature is technically feasible against the current codebase.

Feasibility is supported by existing components:

| Capability | Existing evidence | Assessment |
| ---------- | ----------------- | ---------- |
| Android client | `src/BTrade3`: Kotlin, Jetpack Compose, Room, Retrofit | Reusable mobile foundation |
| Offline local storage | Room database and draft/sent patterns for Orders and Check-In | Reusable pattern, new opname entities required |
| Office-to-cloud master upload | `j07-btrade-sync` uploads Items and related master data | Reusable transport shell |
| Mobile master download | BTrade3 downloads Items by `ServerId` | Reusable client/network shell |
| Mobile-to-cloud submission | Check-In and Order upload paths | Reusable retry pattern, stronger idempotency required |
| Cloud relay | `j06-pkl-btrade-api` with MediatR, Dapper, SQL project | Suitable extension point |
| Cloud-to-office download | `j07-btrade-sync` downloads Orders and Check-Ins | Suitable extension point, acknowledgement must be improved |
| Inventory posting | BTR application workers add/remove stock and regenerate balances | Reusable low-level inventory operations |
| Desktop inventory UI | Existing `StokOpForm` and reporting menu | Existing entry point, but current behavior is unsuitable for mobile staging |

### Overall assessment

| Dimension | Rating | Reason |
| --------- | ------ | ------ |
| Business fit | High, after workflow corrections | Removes paper/rekeying and preserves supervisor control |
| Technical feasibility | High | All four required runtime tiers already exist |
| Reuse potential | Medium–High | Transport and inventory primitives exist |
| Implementation size | High | Android, Cloud API, office sync, desktop, SQL, security, and tests all change |
| Data-integrity risk | High if current posting path is reused | Current form posts per line against live stock |
| Recommended delivery | Phased | Establish session/posting integrity before mobile rollout |

This is not a small extension to `BTR_StokOp`. It introduces an authoritative session, snapshot, synchronization, approval, and idempotent posting boundary.

---

## 2. Current-System Findings

### 2.1 Current desktop behavior

The active Inventory menu opens `StokOpForm`:

- `src/j05-btr-distrib/btr.distrib/SharedForm/MainForm.cs`
- `src/j05-btr-distrib/btr.distrib/InventoryContext/OpnameAgg/StokOpForm.cs`

The current form:

1. selects date, Warehouse, and Kategori;
2. loads Items and live `BTR_StokBalanceWarehouse` quantities;
3. initializes the displayed physical quantity from live quantity;
4. invokes `SaveStokOpWorker` on each `QtyOpnameInputStr` cell change;
5. immediately posts stock through `GenStokStokOpWorker`.

`GenStokStokOpWorker` calculates:

```text
adjustment = entered physical quantity - live balance at line-save time
```

This behavior has no session header, immutable snapshot, submission, supervisor verification, or final posting command.

`BTR_StokOp` contains one row per Item adjustment and has no status, session identity, approval fields, attempt history, version, or idempotency key.

### 2.2 Current mobile and sync behavior

`src/BTrade3` already provides:

- Compose navigation and screens;
- Room local persistence;
- Retrofit API access;
- Item download;
- draft/retry upload behavior;
- user email and office `ServerId` selection.

`src/j07-btrade-sync` already provides:

- outbound office-to-cloud Item upload;
- cloud-to-office Order and Check-In download;
- configurable five-minute polling;
- direct BTR database persistence.

`src/j06-pkl-btrade-api` already provides:

- per-`ServerId` cloud storage patterns;
- MediatR commands/queries;
- SQL-backed persistence;
- upload and incremental download endpoints.

### 2.3 Important limitations

1. Current cloud controllers have no `[Authorize]` enforcement.
2. Android and BTR.Sync currently use a cleartext HTTP endpoint.
3. Android enables cleartext traffic and BODY-level HTTP logging.
4. `ServerId` is client-selected and is not bound to an authenticated tenant claim.
5. Current Item sync stores total Item stock across Warehouses, not a Warehouse-specific opname snapshot.
6. Current Item sync is a replace-all master refresh and cannot represent an immutable session package.
7. Current sync is a WinForms timer process that must remain running.
8. Existing download patterns may acknowledge cloud records before durable office processing; the opname path must not copy that behavior.
9. Current Stock Opname correction rolls back an earlier line before its replacement transaction, creating a recovery gap.
10. No automated Stock Opname tests exist in the desktop test project.

Security and delivery acknowledgement are production release gates, not optional hardening.

---

## 3. Workflow Validation

The user's proposed six steps are directionally correct:

```text
Upload inventory → mobile count → office download → verify → update inventory
```

They require the following corrections:

| Proposed step | Required correction |
| ------------- | ------------------- |
| Upload inventory Items | Create and upload a specific session containing immutable Warehouse-scoped snapshot lines, not the mutable general Item master |
| Input quantities directly to Cloud | Save locally first and upload retry-safe; warehouse connectivity must not determine whether counting can continue |
| Employee returns to office | Not a technical dependency; office receipt may occur as soon as both sides have outbound internet |
| Server downloads result | Download into local staging/session tables only; do not post inventory |
| Supervisor verifies | Preserve submitted attempt, show all variances, and support reasoned recount |
| System updates stock | Apply the approved snapshot variance as an idempotent, atomic BTR-owned posting |

The corrected workflow is appropriate for an inventory system when the movement-freeze rule, snapshot basis, explicit-zero handling, audit trail, and posting idempotency are enforced.

---

## 4. Architecture

### 4.1 Target topology

```text
BTR Desktop
  Create session + Warehouse snapshot
        ↓ local database
BTR.Sync
  Upload session package over authenticated HTTPS
        ↓
Cloud API
  Durable tenant-scoped relay
        ↓
Android
  Download → Room → count offline → submit → retry upload
        ↓
Cloud API
  Store immutable attempt versions
        ↓
BTR.Sync
  Download → commit local inbox/session → explicit cloud acknowledgement
        ↓
BTR Desktop
  Review → recount or verify/post
        ↓
BTR inventory workers
  Apply approved variance → stock lots, mutations, balance
```

### 4.2 Ownership boundaries

| System | Owns | Must not do |
| ------ | ---- | ----------- |
| BTR Desktop | Session authority, snapshot, verification, posting, inventory audit | Accept unauthenticated remote writes |
| BTR.Sync | Outbound transport, retry, durable handoff, acknowledgement | Decide variance or post inventory |
| Cloud API | Tenant-scoped relay and synchronization history | Become inventory source of truth |
| Android | Local counting workflow and submission intent | Calculate authoritative expected stock or post inventory |

### 4.3 Reuse versus replacement

Reuse:

- BTrade3 application shell, Room, Retrofit, and sync UI patterns;
- Cloud API MediatR/Dapper layering;
- BTR.Sync registry-based office routing and outbound connection model;
- Item, Warehouse, Kategori, unit conversion, stock balance, add-stock, remove-FIFO, mutation, and balance-generation components in BTR.

Do not reuse unchanged:

- `BTRADE_Brg.Stok` as the opname expected quantity;
- the general Item endpoint as the mobile count package;
- `BTR_StokOp` as the session header;
- `SaveStokOpWorker` for synchronized results;
- `GenStokStokOpWorker` for delayed posting;
- GET-side acknowledgement patterns;
- current unauthenticated/HTTP transport.

**Android packaging decision:** implement Stock Opname as a role-gated module in the existing BTrade3 Android project for the initial release. This reuses the proven Android stack and avoids maintaining a second application. Warehouse users must see only capabilities authorized for their role. A separate APK remains an option if device administration, branding, or release independence later requires it.

---

## 5. Proposed Data Model

Names are implementation recommendations; the Implementer may align suffixes with repository conventions without changing semantics.

### 5.1 BTR Desktop database

#### `BTR_StokOpSession`

Session header:

- `SessionId` — globally unique stable identifier;
- `WarehouseId`;
- `KategoriId` or approved scope descriptor;
- `CountDate`;
- `SnapshotAt`;
- `MovementFreezeFrom`, `MovementFreezeReleasedAt`;
- `AssignedCounterId` and/or authenticated email;
- `Status`;
- `CurrentAttemptNo`;
- created/published/received/verified/posted/cancelled timestamps and users;
- `RowVersion` or equivalent optimistic concurrency value;
- notes and reason fields;
- unique posting reference.

#### `BTR_StokOpSessionItem`

Immutable published line plus final result:

- `SessionId`, `LineNo`, `BrgId`;
- Item code/name snapshot;
- Kategori snapshot;
- large/small unit and conversion snapshot;
- `ExpectedQtyPcs`;
- final accepted physical quantity and variance;
- posting reference/status.

Constraints:

- primary key `(SessionId, LineNo)`;
- unique `(SessionId, BrgId)`;
- non-negative expected quantity where the current stock model guarantees it;
- immutable snapshot fields after publication.

#### `BTR_StokOpAttempt`

- `SessionId`, `AttemptNo`;
- counter identity/device;
- started/submitted/received timestamps;
- status;
- supervisor decision, reason, and timestamp;
- content/version hash for duplicate/conflict detection.

#### `BTR_StokOpAttemptItem`

- `SessionId`, `AttemptNo`, `LineNo`;
- nullable counted quantity until explicit count;
- counted timestamp;
- counter identity;
- note;
- explicit-count flag;
- submitted version.

Prior attempts are never deleted or overwritten.

#### `BTR_StokOpSyncInbox`

Optional but recommended durable inbox:

- remote message/version identity;
- payload hash;
- received timestamp;
- processing status/error;
- unique constraint preventing duplicate processing.

This allows BTR.Sync to acknowledge the Cloud only after the payload and business rows commit locally.

### 5.2 Cloud database

Use separate tenant-scoped session and attempt tables rather than extending `BTRADE_Brg`.

Required characteristics:

- every key includes or validates `ServerId`;
- published session versions are immutable;
- mobile submission is an upsert only for the same pre-submission attempt/version;
- submitted attempts cannot be silently overwritten;
- upload/download acknowledgement is explicit and timestamped;
- retention supports replay and audit;
- unique identities prevent duplicate delivery effects.

Suggested tables:

- `BTRADE_StokOpSession`;
- `BTRADE_StokOpSessionItem`;
- `BTRADE_StokOpAttempt`;
- `BTRADE_StokOpAttemptItem`;
- `BTRADE_StokOpDelivery`.

### 5.3 Android Room database

Add:

- `stok_opname_session`;
- `stok_opname_session_item`;
- `stok_opname_attempt`;
- `stok_opname_attempt_item`.

Local fields include:

- server/tenant identity;
- download version;
- local workflow status;
- nullable physical quantity;
- dirty/upload status;
- retry/error metadata;
- content hash or server ETag/version.

Room migration must preserve all existing Order, Item, Customer, Sales Person, and Check-In data.

---

## 6. Synchronization Contract

### 6.1 Office → Cloud

Operations:

1. publish immutable session/version;
2. query publication status;
3. cancel an unsubmitted session when permitted.

The publish command must be idempotent by `(ServerId, SessionId, PublicationVersion)`.

### 6.2 Cloud → Android

Operations:

1. list sessions assigned to the authenticated counter;
2. download one immutable session package;
3. report optional start/progress metadata;
4. obtain Cloud receipt status for a submitted attempt.

The API derives tenant and user scope from authentication. It must not trust an arbitrary `ServerId` or counter email supplied in the request body.

The Android session response must be purpose-built and data-minimized:

- include only the assigned session, Item identity, count units, scope, and required workflow metadata;
- omit HPP, sales price, unrelated Item stock, and unrelated office master data;
- omit expected quantities when blind count is enabled.

### 6.3 Android → Cloud

Operations:

1. upload draft/progress version, if progress synchronization is included;
2. submit a complete attempt;
3. retry the same attempt/version safely.

Submission validation:

- authenticated user matches assignment;
- session is open for the current attempt;
- publication version matches;
- every required line is explicitly counted;
- no physical quantity is negative;
- line identities exactly match the published scope;
- submitted content hash is stable.

A repeated identical request returns success. A different payload for an already submitted version returns a conflict and requires a new attempt.

### 6.4 Cloud → Office

Operations:

1. list submitted attempts using a stable cursor;
2. download the complete attempt package;
3. acknowledge a specific version only after the BTR transaction commits;
4. replay by session/attempt for recovery.

Cursor and acknowledgement rules:

- do not acknowledge during GET;
- do not advance the durable local cursor after partial local failure;
- local upsert/inbox is idempotent;
- response pagination is drained or backlog is visible;
- Cloud retains unacknowledged data;
- manual replay does not duplicate business rows.

### 6.5 Delivery semantics

Use at-least-once transport with idempotent consumers.

Exactly-once network delivery is not required. Exactly-once inventory effect is required and is enforced by the local unique posting identity plus transaction.

---

## 7. Inventory Posting Design

### 7.1 Posting formula

For every accepted line:

```text
ApprovedVariance = PhysicalQty - SnapshotExpectedQty
ResultingLiveQty = CurrentLiveQtyAtPosting + ApprovedVariance
```

Do not call the current logic that calculates:

```text
PhysicalQty - CurrentLiveQtyAtPosting
```

for a delayed synchronized count.

### 7.2 New application boundary

Add a session-owned use case such as:

```text
VerifyAndPostStokOpSessionWorker
```

Responsibilities:

1. load the submitted session and current attempt;
2. authorize the supervisor;
3. verify status and completeness;
4. acquire concurrency protection for the session;
5. reject an already posted posting identity;
6. calculate immutable snapshot variances;
7. validate all negative adjustments against currently removable stock;
8. apply positive/negative deltas using existing stock primitives;
9. write stock mutations referencing session and line;
10. persist verification/posting audit;
11. transition the session to Posted;
12. commit everything in one transaction.

If any line fails, no session line or status may remain partially posted.

### 7.3 Concurrency

The implementation must:

- serialize posting per session;
- prevent two active sessions with overlapping initial-release scope;
- prevent a second post by database uniqueness, not UI state alone;
- lock or otherwise protect stock validation and mutation from a stale check/write race;
- surface a recoverable supervisor error when later stock-out transactions make a negative variance impossible to apply.

### 7.4 Existing feature transition

The safest migration is:

1. add the session-based workflow alongside the current `IT1-Opname`;
2. route mobile sessions only through the new review/post path;
3. preserve current desktop behavior during controlled rollout;
4. after acceptance, decide whether all new desktop opname work must also use sessions;
5. retain legacy rows and reports as historical data.

Do not reinterpret existing `BTR_StokOp` rows as session attempts.

---

## 8. User Interface Changes

### 8.1 BTR Desktop

Add a session-oriented Inventory screen with:

- session list and synchronization status;
- Create Session wizard;
- Warehouse, Kategori, date, counter, and count-mode selection;
- immutable publication summary;
- completeness and upload/download indicators;
- supervisor review grid with expected, physical, variance, notes, counter, and timestamps;
- Recount Required action with mandatory reason;
- Verify and Post action with confirmation;
- posting error/recovery details;
- audit/history view.

The review grid must include zero physical quantities and zero variances.

### 8.2 Android

Add a role-gated Stock Opname area:

- assigned session list;
- download and local availability state;
- item search/filter;
- count entry in large/small units with normalized base quantity;
- explicit zero action;
- counted/not-counted progress;
- optional blind-count presentation;
- local auto-save;
- completeness review;
- submit confirmation;
- upload retry and office-received status;
- recount reason and new attempt handling.

If barcode scanning is approved, it must resolve a documented Item barcode identity. `BrgCode` must not be assumed to be a barcode without business confirmation.

### 8.3 Identity and authorization

Current Google sign-in only yields an email to the app and does not secure the API.

Before rollout:

- establish API-issued or federated tokens validated by Cloud API;
- bind authenticated identity to one or more BTR `ServerId` tenants;
- define Counter and Supervisor roles;
- enforce roles server-side;
- prevent arbitrary tenant switching;
- keep desktop posting authorization inside BTR as well as in the UI.

---

## 9. Affected Modules

### `src/j05-btr-distrib`

| Project | Change |
| ------- | ------ |
| `btr.sql` | Add session, line, attempt, attempt-line, inbox, constraints, indexes |
| `btr.domain` | Add session/attempt/status models and keys |
| `btr.application` | Add create, publish, receive, recount, verify/post use cases |
| `btr.infrastructure` | Add DALs, transactional inbox, queries, posting uniqueness |
| `btr.distrib` | Add session preparation/review/audit UI and menu wiring |
| `btr.test` | Add lifecycle, variance, posting, idempotency, concurrency tests |

### `src/j07-btrade-sync`

- session publication uploader;
- submitted-attempt downloader;
- explicit acknowledgement;
- durable cursor/replay;
- configuration flags and status logging;
- manual retry/reconciliation commands;
- authenticated HTTPS client.

### `src/j06-pkl-btrade-api`

- tenant/auth enforcement;
- session and attempt domain/application/infrastructure types;
- publish, assignment download, submit, office download, acknowledgement, replay endpoints;
- SQL project tables and indexes;
- integration tests for tenancy, immutability, idempotency, and conflict handling.

### `src/BTrade3`

- Room entities, DAOs, repositories, and migration;
- Retrofit contracts and authenticated client;
- Stock Opname navigation, list, entry, review, and sync screens;
- unit normalization/validation;
- offline and retry behavior;
- tests for migrations, state transitions, completeness, explicit zero, and duplicate retry.

---

## 10. Implementation Sequence

### Phase 0 — Business and security decisions

1. Approve the feature artifact.
2. Confirm movement-freeze policy.
3. Confirm blind-count behavior.
4. Confirm role matrix and counter identity.
5. Confirm Warehouse versus Depo/rack scope.
6. Confirm barcode scope.
7. Approve authentication and HTTPS approach.

Exit criterion: no unresolved decision changes session cardinality, posting basis, identity, or API tenancy.

### Phase 1 — BTR session and posting core

1. Add BTR session/attempt schema and constraints.
2. Implement session creation and immutable snapshot capture.
3. Implement received-attempt staging without stock impact.
4. Implement recount history.
5. Implement approved-variance posting with atomicity and uniqueness.
6. Add unit/integration tests.
7. Add a minimal desktop test harness or session UI sufficient to exercise the lifecycle.

Exit criterion: a locally staged attempt can be reviewed and posted exactly once with correct movement results, without mobile or Cloud.

### Phase 2 — Secure Cloud relay

1. Enforce authenticated HTTPS access.
2. Bind identity to tenant and role.
3. Add Cloud session/attempt schema.
4. Implement idempotent publish/download/submit/ack/replay contracts.
5. Add API integration and tenant-isolation tests.

Exit criterion: duplicate and conflicting requests behave deterministically; one tenant cannot access another.

### Phase 3 — Office synchronization

1. Add session publisher to BTR.Sync.
2. Add submitted-attempt downloader and local transaction.
3. Add explicit Cloud acknowledgement after local commit.
4. Add cursor, pagination, retry, replay, and operational logging.
5. Add manual reconciliation controls.

Exit criterion: failure injection before/after Cloud and local commits produces no loss and no duplicate local attempts.

### Phase 4 — Android counting

1. Add Room migration and opname data layer.
2. Add assigned-session download and immutable local snapshot.
3. Add count-entry UI, explicit zero, progress, review, and submit.
4. Add authenticated upload, retry, and receipt state.
5. Add recount workflow.
6. Add Android unit, migration, repository, and UI tests.

Exit criterion: a device can count completely offline after download, survive restart, retry submission, and never duplicate an attempt.

### Phase 5 — Desktop production UI and reporting

1. Complete session preparation, status, review, recount, post, and audit screens.
2. Add reports that include zero counts and all attempts.
3. Add role-based menu and service authorization.
4. Add synchronization health/replay visibility.

Exit criterion: supervisors can operate and recover the end-to-end workflow without direct database manipulation.

### Phase 6 — Pilot and transition

1. Pilot one Warehouse and one Kategori with controlled movement freeze.
2. Run paper and mobile counts in parallel for validation only.
3. Compare expected, physical, variance, posted mutation, and final balance.
4. Exercise offline, duplicate-submit, server-down, recount, and insufficient-stock scenarios.
5. Obtain Inventory owner acceptance.
6. Decide whether to retire immediate-post desktop entry for future sessions.

Exit criterion: signed operational acceptance and documented recovery procedure.

---

## 11. Test Strategy

### Business-rule tests

- null/not-counted differs from explicit zero;
- negative count rejected;
- incomplete attempt cannot submit;
- published snapshot immutable;
- submitted attempt immutable;
- recount creates a new attempt and preserves prior history;
- unauthorized counter/supervisor actions rejected.

### Variance/posting tests

- zero, positive, and negative variance;
- movements after submission produce `current + snapshot variance`;
- no movements produce final balance equal to physical count;
- insufficient removable stock blocks whole session;
- duplicate post has one inventory effect;
- failure on the last line rolls back all prior lines;
- stock mutation references session/line and carries correct business/audit dates.

### Synchronization tests

- duplicate publication;
- partial mobile upload and retry;
- identical resubmission succeeds idempotently;
- changed payload after submission conflicts;
- Cloud available while office offline;
- office commit fails before acknowledgement;
- acknowledgement response lost after local commit;
- cursor pagination/backlog;
- manual replay;
- tenant isolation.

### Android tests

- Room migration preserves existing data;
- process death/restart during counting;
- offline session download availability;
- unit conversion and normalization;
- explicit zero;
- blind-count visibility;
- submit completeness;
- retry status and conflict display.

### End-to-end reconciliation

For each pilot session prove:

```text
Sum(posted line variances)
    = Sum(physical - snapshot expected)
    = Sum(STOKOP session-referenced mutations)
```

and for each Item:

```text
final stock = stock immediately before posting + approved variance
```

---

## 12. Risks and Mitigations

| Risk | Severity | Mitigation |
| ---- | -------- | ---------- |
| Reusing current live-balance posting corrupts later movements | Critical | New snapshot-variance posting use case; regression test with post-count movement |
| Unauthenticated/cleartext transport exposes inventory and permits cross-tenant access | Critical | HTTPS, enforced auth, tenant claims, server-side roles before pilot |
| Duplicate mobile or office delivery creates duplicate adjustment | Critical | Stable IDs, unique posting key, idempotent inbox, transaction |
| Counting while stock moves produces invalid variance | High | Operational freeze until submission; do not release without alternative approved reconciliation |
| Sync process is not running | High | Auto-start/monitoring, status visibility, manual retry/replay, retained Cloud payload |
| Cloud acknowledges before local commit | High | Explicit post-commit acknowledgement |
| Multiple counters overwrite one another | High | Initial release limits one active counter/device per session |
| Item/unit master changes during count | High | Immutable per-session snapshots |
| Zero count disappears from review/report | High | Nullable count plus explicit-count flag; include all lines |
| Large session posting holds a long transaction | Medium | Scope sessions by Kategori, prevalidate, measure pilot volume, retain atomicity |
| Current FIFO stock cannot satisfy a later negative variance | Medium | Prevalidation under posting transaction; block and surface conflict |
| Android Room migration loses existing sales data | High | Non-destructive migration and instrumentation tests |
| Warehouse is not the real physical location granularity | High | Resolve Warehouse/Depo/rack decision in Phase 0 |

---

## 13. Operational Requirements

Before production:

- Cloud API has valid TLS and no cleartext fallback.
- Android release logging excludes request/response bodies and sensitive data.
- BTR.Sync is configured to start automatically and exposes last success/failure.
- Cloud retention and backup cover unacknowledged sessions and attempts.
- Office database backup includes new session/audit tables.
- A reconciliation screen or report identifies:
  - published but not downloaded sessions;
  - submitted but not office-received attempts;
  - received but not reviewed sessions;
  - posting failures;
  - Cloud/local version conflicts.
- Operations has a documented replay procedure that does not require changing inventory tables manually.
- Device time is captured for evidence, while authoritative receipt/post timestamps come from servers.

---

## 14. Approval Gates

Implementation must not begin until the Product Owner and Inventory owner approve:

1. the business feature specification;
2. movement freeze from publication through submission;
3. snapshot-variance posting;
4. blind-count policy;
5. role and identity model;
6. Warehouse/Depo/rack scope;
7. initial one-counter limitation;
8. security work as a release prerequisite.

After approval, execute through the Implementer workflow defined in `AGENTS.md`.
