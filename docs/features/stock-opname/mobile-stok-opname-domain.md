# Mobile-Assisted Stok Opname

## Document Status

| Field | Value |
| ----- | ----- |
| Business area | Inventory |
| Primary owner | BTR Desktop |
| Participating systems | BTR Desktop, BTR.Sync, Cloud API, Android |
| Status | **Proposed — requires Product Owner and Inventory owner approval** |

---

## Purpose

Mobile-Assisted Stok Opname lets warehouse staff perform physical inventory counting with an Android device instead of a printed item list.

The feature removes the current paper-and-rekey process while preserving the existing business authority:

- warehouse staff record physical quantities;
- a supervisor verifies the result;
- BTR Desktop remains the source of truth;
- inventory changes occur only after office verification.

The cloud is a durable synchronization relay. It does not own inventory and must never post inventory adjustments by itself.

---

## Business Problem

The current operational process is:

1. Inventory staff prepare and print an item list in the main office.
2. The counter carries the paper to the physically separate warehouse.
3. The counter writes physical quantities on the paper.
4. The counter returns to the office.
5. Staff re-enter the quantities in BTR Desktop.
6. The desktop feature immediately changes inventory per entered line.

This process creates avoidable work and risk:

- the same count is recorded twice;
- handwriting can be unclear;
- rekeying can introduce errors;
- counting progress is not visible;
- the paper has no durable system identity;
- entered lines change inventory before supervisor verification.

---

## Target Users

### Inventory Administrator

- Creates a Stok Opname session in BTR Desktop.
- Selects the Warehouse and item scope.
- Publishes the session for mobile counting.
- Monitors synchronization and completeness.

### Warehouse Counter

- Downloads an assigned session to an Android device.
- Counts each Item at the Warehouse.
- Records an explicit physical quantity, including zero.
- Reviews and submits the completed session.

### Inventory Supervisor

- Reviews submitted counts and variances in BTR Desktop.
- Requests a recount when necessary.
- Verifies and posts an accepted result.

---

## Core Concepts

### Stok Opname Session

A Stok Opname Session is one controlled physical-count exercise.

It identifies:

- one BTR office/server;
- one Warehouse;
- one item scope;
- one count date;
- one expected-stock snapshot;
- one assigned counter;
- one current count attempt;
- one verification and posting outcome.

A session groups all count lines under one durable identity. A mobile result cannot be imported as unrelated item adjustments.

### Count Line

A Count Line represents one Item within a session.

It records:

- Item identity and description snapshot;
- unit and conversion snapshot;
- expected quantity at session cutoff;
- physical quantity, which is null until counted;
- counted time and counter identity;
- optional counter note;
- verified variance;
- posting outcome.

`Not counted` and `counted as zero` are different states.

### Expected-Stock Snapshot

The expected-stock snapshot is the BTR Desktop quantity for each selected Item and Warehouse when the session is published.

It is immutable for the current count attempt. A normal Item master refresh must not change an open session's lines, units, or expected quantities.

### Variance

```text
Variance = Physical Quantity - Snapshot Expected Quantity
```

The approved variance is the inventory adjustment. It is not recalculated as physical quantity minus the live balance on the later posting date.

Example:

```text
Snapshot expected quantity = 100
Physical count             = 98
Approved variance          = -2

Later live quantity        = 90
Quantity after posting     = 88
```

Setting the later live quantity back to 98 would reverse legitimate movements that occurred after counting and is therefore incorrect.

### Cloud Relay

The Cloud Relay stores published sessions and submitted results until the office has durably received them.

It:

- routes data to the correct BTR office/server;
- supports retry and duplicate delivery;
- retains synchronization evidence;
- does not determine expected stock;
- does not verify results;
- does not update inventory.

---

## Target Workflow

```text
Inventory Administrator creates session in BTR Desktop
    ↓
BTR captures Warehouse/item/unit/expected-quantity snapshot
    ↓
Session is published through BTR.Sync to Cloud
    ↓
Assigned counter downloads session to Android
    ↓
Counter performs physical count and saves locally
    ↓
Counter reviews completeness and submits
    ↓
Android uploads result to Cloud (retry-safe)
    ↓
BTR.Sync downloads and durably stages the result in BTR
    ↓
Supervisor reviews variances
    ├── Recount required → new controlled count attempt
    └── Accepted → Verify and Post
                    ↓
              BTR Desktop applies approved variances
                    ↓
              Session becomes Posted
```

The employee's physical return to the main office is not a synchronization requirement. The office may receive the result as soon as the Android device and office sync process both have internet access.

No inbound internet connection to the office database is required. The office-side synchronization process initiates outbound communication to the Cloud API.

---

## Session Lifecycle

| Status | Meaning | Editable by |
| ------ | ------- | ----------- |
| Draft | Office is preparing scope and assignment | Inventory Administrator |
| Published | Immutable count package is available to the assigned counter | Nobody changes scope |
| Counting | The assigned counter has started recording quantities | Assigned Counter |
| Submitted | Counter declared the attempt complete; mobile quantities are locked | Nobody |
| Received | Submitted result is durably stored in BTR and awaiting review | Supervisor review only |
| Recount Required | Supervisor rejected the attempt and supplied a reason | New count attempt |
| Posted | Supervisor accepted the result and BTR applied the variances | Read-only |
| Cancelled | Session ended without inventory posting and with a reason | Read-only |

The business record must retain who and when for creation, publication, counting, submission, receipt, verification, posting, recount, and cancellation.

---

## Business Rules

### Session Preparation

1. BTR Desktop is the only system allowed to create the authoritative session and expected-stock snapshot.
2. A session must identify exactly one Warehouse.
3. The initial item-scope option is Warehouse plus Kategori, matching the current desktop workflow.
4. The session includes all selected Items, including Items whose expected quantity is zero when they are in scope.
5. A session must have an assigned, authorized counter before publication.
6. Published scope, item identity, units, conversions, and expected quantities are immutable.
7. Overlapping active sessions for the same Warehouse and item scope are not allowed in the initial release.

### Movement Control

1. Stock movements for the selected count scope must be operationally frozen from session publication until mobile submission.
2. The freeze may be released after submission; supervisor review and posting may happen later.
3. Movements after submission do not change the approved variance.
4. If the business cannot freeze movements during counting, the initial release must not proceed until a movement-reconciliation rule is approved.

### Mobile Counting

1. The Android device stores the downloaded session and entered quantities locally.
2. Counting continues when connectivity is unavailable.
3. A physical quantity cannot be negative.
4. Whole base-unit quantities are supported, consistent with the current inventory model.
5. Every line must be explicitly marked counted before submission.
6. Zero is a valid physical quantity and must be explicitly entered or confirmed.
7. Saving a mobile line never updates BTR inventory.
8. Submission requires a completeness review and confirmation by the counter.
9. After submission, the attempt is read-only unless the supervisor requests a recount.
10. The initial release assigns one active counter/device to one session to avoid conflicting edits.

### Count Visibility

The recommended counting mode is a blind count:

- Android displays Item identity and units;
- expected quantity and variance remain hidden until submission;
- the supervisor sees expected quantity, physical quantity, and variance during review.

Whether blind counting is mandatory is an approval decision listed under Open Business Decisions.

### Synchronization

1. Every session and attempt has a stable, globally unique identity.
2. Re-uploading or re-downloading the same version must not create duplicate sessions, attempts, lines, or inventory adjustments.
3. A successful transport response is not sufficient proof of office receipt.
4. Cloud data remains available for retry until the office acknowledges durable local storage.
5. Partial or failed uploads remain retryable on Android.
6. A master-data refresh cannot overwrite an in-progress mobile count.
7. The user must be able to see whether a session is local only, uploading, submitted to Cloud, or received by the office.

### Verification and Recount

1. Only an authorized supervisor can verify or reject a submitted result.
2. The supervisor must see all lines, not only non-zero quantities or non-zero variances.
3. Verification shows expected quantity, physical quantity, variance, counter, count time, and notes.
4. A recount requires a reason and creates a new attempt without deleting the prior attempt.
5. Prior attempts remain immutable audit evidence.
6. A supervisor cannot directly edit the counter's submitted quantity. Corrections require a recount.

### Posting

1. Cloud API, Android, and BTR.Sync cannot post inventory.
2. Only BTR Desktop's inventory application boundary may post an accepted session.
3. Posting applies each approved snapshot variance to the then-current inventory balance.
4. Posting must be atomic for the session: either every applicable line and the session status commit, or none commit.
5. The same session/attempt cannot be posted more than once.
6. Posting records the session and line reference in the inventory movement audit.
7. Insufficient current stock or another posting conflict blocks the entire post and returns the session for supervisor resolution.
8. A posted session and its audit history are immutable; corrections use a separate controlled reversal or new session.

---

## Scope

### Initial Release

- Session creation from BTR Desktop.
- Warehouse and Kategori item scope.
- Expected-stock and unit snapshot.
- One assigned counter and one Android device per session.
- Android download, local offline counting, review, and submission.
- Automatic retry of result upload.
- Office download into staging/session tables.
- Supervisor variance review, recount request, and verified posting.
- Session-level audit and synchronization status.
- Existing whole-unit and large/small-unit conversion model.

### Out of Scope

- Direct Android access to the on-premise database.
- Direct Cloud API inventory posting.
- Continuous real-time stock balance on Android.
- Multi-counter concurrent editing of one session.
- Rack/bin/Depo-level counting where it differs from Warehouse.
- Fractional base units.
- Photo evidence.
- Automatic variance approval by tolerance.
- Inventory valuation redesign.
- Replacement of unrelated sales, check-in, or warehouse-fulfillment synchronization.

---

## Acceptance Criteria

1. An Inventory Administrator can create and publish a session for one Warehouse and Kategori.
2. Published lines retain their item, unit, conversion, and expected-quantity snapshot even when master data later changes.
3. The assigned counter can download the session and count without network connectivity.
4. The mobile UI distinguishes not counted from counted as zero.
5. An incomplete session cannot be submitted.
6. Repeated upload or download attempts do not create duplicate records.
7. A submitted result appears in BTR Desktop without changing inventory.
8. A supervisor can inspect every line and request a recount without losing the earlier attempt.
9. Only a supervisor's accepted result can cause an inventory update.
10. Posting applies `physical - snapshot expected` as a delta to current stock.
11. Posting is atomic and cannot be repeated for the same accepted attempt.
12. Inventory movements identify the originating session and line.
13. The complete audit shows the counter, supervisor, timestamps, attempts, and final posting outcome.
14. The office database requires no inbound public connection.
15. All synchronization uses authenticated, authorized, encrypted communication.

---

## Open Business Decisions

The following decisions require stakeholder approval before implementation:

1. Is blind counting mandatory, optional by session, or should expected quantity always be visible?
2. Can normal inventory movements be frozen for the selected Warehouse/Kategori until mobile submission?
3. Is one counter per session sufficient for the initial release?
4. Does the current Warehouse identify the physical count area, or is Depo/rack/bin detail required?
5. Must barcode scanning be included in the initial release, and which Item field is the authoritative barcode?
6. Who may create, count, request recount, verify, post, and cancel?
7. Should any variance threshold force a second count?
8. How should physically found goods that are outside the published item scope be recorded?
