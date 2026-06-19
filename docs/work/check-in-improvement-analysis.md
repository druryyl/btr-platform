# Check-In Improvement — Impact Analysis and Implementation Plan

## Summary

Three enhancements to the Check-In feature:

1. **Check-Out** — record when a salesperson leaves a customer (optional, attached to existing check-in)
2. **Immediate cloud upload** — upload after capture without waiting for Sync screen
3. **GPS accuracy** — aggressive high-accuracy acquisition instead of cached `lastLocation`

Approach: extend the existing `CheckIn` entity and `POST /api/CheckIn` pipeline. No new tables or endpoints.

---

## 1. Impact Analysis

### Affected Modules

| Module | Change |
| ------ | ------ |
| **BTrade3 (Android)** | Room migration, `LocationHelper`, check-in/check-out ViewModels and screens, immediate upload in repository |
| **BTRADE Cloud API** | Extend upload command, domain model, DAL |
| **j07-btrade-sync** | Extend model/DAL; upsert on download (was insert-only) |
| **btr.sync (j05)** | Same as j07 |
| **BTR Desktop** | Extend `BTR_CheckIn` DDL, domain model, DAL |
| **BTR Portal** | No change required (field activity KPIs use check-in time only; checkout available for future use) |
| **RO1 / RO3** | RO1 adds Check-Out Time and Check-Out Mode columns; RO3 unchanged |

### Database Changes

Add four nullable-equivalent columns to `BTR_CheckIn` and `BTRADE_CheckIn`:

| Column | Type | Default | Meaning |
| ------ | ---- | ------- | ------- |
| `CheckOutTime` | VARCHAR(8) | `''` | HH:mm:ss; empty = visit still open |
| `CheckOutLatitude` | FLOAT | 0 | GPS at checkout |
| `CheckOutLongitude` | FLOAT | 0 | GPS at checkout |
| `CheckOutAccuracy` | FLOAT | 0 | GPS accuracy at checkout |
| `CheckOutMode` | VARCHAR(10) | `''` | `MANUAL`, `AUTO`, or empty |

**Room (`checkin_table`):** migrations 24 → 25 (checkout GPS/time), 25 → 26 (`checkOutMode`).

**Backward compatibility:** existing rows and older app versions remain valid (empty checkout = open visit).

### API Changes

Extend `CheckInUploadCommand` and `POST /api/CheckIn` body with optional checkout fields. Upload remains idempotent (delete-then-insert by `CheckInId`). Re-upload after checkout sets cloud status back to `TERKIRIM`, enabling incremental re-download.

No new endpoints.

### Synchronization Changes

| Stage | Behavior |
| ----- | -------- |
| **Mobile immediate upload** | After local save, call existing sync API for that record. Success → `SENT`; failure → remain `DRAFT` |
| **Mobile Sync screen** | Unchanged; uploads all `DRAFT` records (including checkout updates on previously `SENT` records) |
| **Cloud** | Delete-insert by `CheckInId`; checkout fields stored with check-in |
| **Office download** | **Change:** upsert instead of skip-if-exists, so checkout updates propagate after re-upload |

### Reporting Impact

- **Actual visit / effective call KPIs:** unchanged (still keyed on check-in date + customer)
- **Visit duration:** available as `CheckOutTime − CheckInTime`; surfaced in RO1 via Check-Out Time column
- **GPS validation:** unchanged (uses check-in coordinates only)

### Portal Impact

None for current M18.5 dashboard. Checkout data is persisted for future productivity analysis.

---

## 2. GPS Root Cause

Current `LocationHelper.getCurrentLocation()`:

1. Calls `fusedLocationClient.lastLocation` first — returns **cached network/Wi‑Fi fix** (often 50–100 m)
2. Returns immediately when cache exists — **never requests fresh GPS**
3. `requestNewLocation` callback is never removed; no accuracy threshold or timeout

Opening Google Maps forces a fresh high-accuracy fix, which explains user workaround.

**Fix:** continuous high-accuracy updates until accuracy ≤ 30 m or 30 s timeout; skip stale cache older than 5 s or worse than target accuracy; stop updates after capture.

---

## 3. Implementation Plan

### Phase 1 — Schema and API

1. Add checkout columns to `BTR_CheckIn.sql`, `BTRADE_CheckIn.sql`, upgrade script
2. Extend cloud domain model, upload command, DAL
3. Extend desktop and sync models/DALs
4. Change sync `ProcessCheckIn` to upsert

### Phase 2 — Mobile Core

1. Room migration 24 → 25
2. Rewrite `LocationHelper.acquireHighAccuracyLocation()`
3. Refactor `CheckInSyncRepository.uploadCheckIn()` for single-record upload
4. Update `CheckInViewModel` — improved GPS + immediate upload after check-in

### Phase 3 — Check-Out UI

1. `CheckOutViewModel` + `CheckOutScreen` (GPS + confirm)
2. Check-out button on history cards for open visits
3. Immediate upload after checkout; reset `SENT` → `DRAFT` before upload

### Phase 4 — Documentation and Tests

1. Update `docs/features/check-in/check-in-feature.md`
2. Unit tests for location acceptance logic and sync request mapping

---

## 4. Sync Status Lifecycle (Updated)

```text
Capture (Check-In or Check-Out update)
    ↓
Local save (DRAFT)
    ↓
Immediate upload attempt
    ├─ Success → SENT (device)
    └─ Failure → DRAFT (retry via Sync screen)
    ↓
Cloud: TERKIRIM
    ↓
Office download (upsert) → BTR_CheckIn
```

Check-out on a previously synced visit sets local status to `DRAFT`, re-triggers upload, cloud replace, and office upsert.

---

## 5. Risks and Mitigations

| Risk | Mitigation |
| ---- | ---------- |
| Duplicate cloud records | Existing delete-then-insert by `CheckInId` |
| Desktop misses checkout update | Upsert on download |
| Battery drain from GPS | Stop updates after capture or timeout |
| Old app versions | Empty checkout columns; server accepts missing fields as defaults |

---

## 6. Acceptance Criteria Mapping

| Criterion | Implementation |
| --------- | -------------- |
| User can check out after check-in | CheckOutScreen from history |
| Check-out persisted and synced | Columns + upsert + immediate upload |
| Existing check-ins valid | Default empty checkout |
| Immediate cloud visibility | `uploadCheckIn()` after save |
| Offline check-in preserved | Local-first; upload best-effort |
| Sync screen fallback | `getDraftCheckIns()` unchanged |
| No duplicates | Idempotent cloud upload |
| GPS improves before capture | `acquireHighAccuracyLocation()` |
| UI reflects improving accuracy | Continuous `onLocationUpdate` callback |

---

## 7. Revision — Check-Out Behavior (Visit Duration)

### Business Philosophy

Check-out measures **visit duration** and operational discipline. The app **never blocks** salespeople because of open visits.

### Auto Check-Out

Only **one active visit** per salesperson. When a new check-in occurs while a prior visit is open:

1. Silently close the prior visit (no dialog)
2. Set `CheckOutTime` to the new check-in time
3. Set `CheckOutMode = AUTO`
4. Upload auto-closed visit, then proceed with new check-in

Manual check-out from history sets `CheckOutMode = MANUAL`.

### CheckOutMode Column

| Column | Type | Values |
| ------ | ---- | ------ |
| `CheckOutMode` | VARCHAR(10) | `MANUAL`, `AUTO`, or empty (open/legacy) |

Constants: `CheckOutMode` object (Kotlin), `CheckOutMode` static class (C#).

Room migration 25 → 26.

### RO1 Reporting

Added **Check-Out Time** and **Check-Out Mode** columns to grid and Excel export.

### Future Analytics (not implemented)

Visit duration, average duration, manual vs auto rate, checkout discipline — data model supports these.
