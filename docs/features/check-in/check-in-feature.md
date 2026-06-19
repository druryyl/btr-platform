# Check-In Feature

## Purpose

Check-In records a salesperson's **physical presence at a customer location** during field operations.

The feature captures GPS coordinates, timestamp, and customer identity at the moment of visit. Management uses this data to verify field execution, compare planned versus actual visits, and measure visit productivity (effective calls).

Check-In is an **operational accountability** capability. It does not create sales transactions, inventory movements, or receivable changes.

---

# Business Context

BTR serves distribution companies whose sales personnel visit customers daily for order collection, invoice delivery, and payment collection.

Management needs visibility into whether salespeople actually visit assigned customers — not only whether orders or payments were recorded later. Check-In closes that gap by producing a timestamped, location-attested visit record at the customer.

Check-In sits alongside — but is separate from — other field activities:

| Activity | What it proves |
| -------- | -------------- |
| **Check-In** | Salesperson was at (or near) the customer location |
| **Sales Order** | Customer placed a purchase request |
| **Payment** | Customer paid outstanding receivables |

A visit may produce a check-in without an order (unproductive visit). An order may be created without a prior check-in (phone order, drive-by). These are independent events linked only by heuristic matching for reporting.

---

# Target Users

## Sales Personnel

- Capture check-ins during customer visits via BTrade3 mobile app
- Review personal check-in history
- Sync draft check-ins to the office when connectivity is available

## Supervisors and Area Managers

- Review check-in lists and GPS distance from customer coordinates (RO1)
- Monitor effective calls — visits that produced orders (RO3)
- Use BTR Portal Field Activity dashboard for planned vs actual visit analysis

## Finance / Operations Administration

- Indirect consumers through visit execution and productivity KPIs
- Do not maintain check-in records directly

---

# Core Concepts

## Check-In

A point-in-time record that a salesperson visited a specific customer.

Each check-in contains:

| Field group | Business meaning |
| ----------- | ---------------- |
| **Identity** | `CheckInId` (ULID), `CheckInDate`, `CheckInTime` |
| **Salesperson** | `UserEmail` — links to `BTR_SalesPerson.Email` |
| **Visit location** | `CheckInLatitude`, `CheckInLongitude`, `Accuracy` — GPS at capture moment |
| **Customer** | `CustomerId`, `CustomerCode`, `CustomerName`, `CustomerAddress` |
| **Customer location snapshot** | `CustomerLatitude`, `CustomerLongitude` — master coordinates at check-in time |
| **Sync state** | `StatusSync` — lifecycle position in mobile-to-office pipeline |
| **Check-out (optional)** | `CheckOutTime`, `CheckOutLatitude`, `CheckOutLongitude`, `CheckOutAccuracy` — departure record when salesperson leaves |

Customer coordinates are **snapshotted** at check-in time. If master customer coordinates change later, the check-in record preserves the coordinates that were known when the visit occurred. This supports historical GPS validation.

---

## Check-Out

Check-out records **visit duration** and supports operational discipline monitoring. It is **not** a workflow gate — the app never blocks or penalizes salespeople because of check-out status.

| Field | Business meaning |
| ----- | ---------------- |
| `CheckOutTime` | HH:mm:ss when the visit ended |
| `CheckOutLatitude`, `CheckOutLongitude`, `CheckOutAccuracy` | GPS at checkout moment (populated for `AUTO` closes only; manual check-out records time only) |
| `CheckOutMode` | How the visit was closed: `MANUAL` or `AUTO` |

### Check-Out Mode

| Value | Meaning |
| ----- | ------- |
| `MANUAL` | Salesperson explicitly checked out before leaving |
| `AUTO` | Visit was silently closed when the salesperson checked in elsewhere |
| *(empty)* | Visit still open, or legacy record before mode was introduced |

`AUTO` is normal business behavior — it indicates the salesperson forgot to check out manually. Management and HR may use this for discipline assessment; the application never treats it as an error.

### Active Visit Rule

Only **one active visit** may exist per salesperson at a time. On BTrade3, a visit is **active** only when both of the following are true:

| Condition | Meaning |
| --------- | ------- |
| `CheckOutTime` is empty | Visit has not been closed |
| `isExplicitlyOpen = true` | Visit was opened by the checkout-aware app (local device flag) |

Legacy check-in records created before the Check-Out feature have empty `CheckOutTime` but `isExplicitlyOpen = false` after upgrade. They remain visible in History but are **not** treated as active visits. Empty checkout on a legacy row does not imply an ongoing visit.

On first launch after upgrading to the checkout-aware app, a Room migration sets `isExplicitlyOpen = false` on all existing local check-in rows.

When a new check-in occurs while a previous **explicitly open** visit is still active:

1. The previous visit is **automatically closed** (silent — no dialog, no blocking)
2. `CheckOutTime` is set to the **new check-in time**
3. Check-out GPS uses the **new check-in coordinates**
4. `CheckOutMode = AUTO`
5. The new check-in proceeds immediately

Manual check-out is available from the **Home screen** (Sales Orders) and **Check-In History**. It records end time only — no GPS capture. A confirmation dialog prevents accidental check-out. Check-in GPS remains the visit validation coordinate (`CheckOutMode = MANUAL`).

Visit duration (when checkout exists): `CheckOutTime − CheckInTime` on the same date.

---

## Actual Visit

A customer for whom at least one check-in exists on a given date for a given salesperson.

For KPI counting, the system deduplicates by distinct `CustomerId` per salesman per day — multiple check-ins to the same customer on the same day count as one actual visit.

---

## Effective Call

A visit that produced at least one sales order on the same day.

Effective call is **inferred**, not directly linked:

```text
BTR_CheckIn.CustomerId + UserEmail + CheckInDate
    ↔
BTR_Order.CustomerId + UserEmail + OrderDate
```

There is no `CheckInId` on `BTR_Order`. A check-in and an order on the same day for the same customer and salesperson are treated as a productive visit.

---

## GPS Validation

A classification of whether the check-in location is consistent with the customer's registered coordinates.

Distance is computed using the Haversine formula between check-in coordinates and customer coordinates (from the check-in snapshot).

| Classification | Criteria (Portal `GpsValidationClassifier`) | Desktop RO1 color band |
| -------------- | ----------------------------------------- | ---------------------- |
| **Valid** | Distance ≤ 50 m AND accuracy ≤ 30 m | White / green |
| **Warning** | Distance 50–100 m OR accuracy 30–50 m | Yellow |
| **Suspicious** | Distance > 100 m OR accuracy > 50 m, or missing coordinates | Red |
| **Invalid** | Both check-in and customer coordinates are zero | — |

GPS validation is an **authenticity signal**, not a hard business gate. The mobile app does not block check-in when distance exceeds thresholds.

---

# Business Flow

## End-to-End Pipeline

```text
Field Visit (Sales Personnel)
    ↓
BTrade3: GPS capture + customer selection + local save (DRAFT)
    ↓
BTrade3: Immediate upload attempt (→ SENT on success; remain DRAFT on failure)
    ↓
BTrade3: Sync screen uploads remaining DRAFT records (offline fallback)
    ↓
BTRADE Cloud API: stores as TERKIRIM
    ↓
j07-btrade-sync: incremental download (→ DOWNLOADED on cloud; upsert on desktop)
    ↓
BTR Desktop: BTR_CheckIn table
    ↓
Reporting: RO1 inquiry, RO3 effective call, Portal field activity
```

**Check-out paths:**

```text
Manual:  Home or History → confirm → record CheckOutTime → CheckOutMode=MANUAL → upload
Auto:    New Check-In → silently close prior open visit → CheckOutMode=AUTO → upload both
```

Both paths use the same upload pipeline (idempotent by `CheckInId`).

---

## 1. Mobile Capture (BTrade3)

**Entry points:**

- **Home screen** (Sales Orders) — shows active visit card when an open check-in exists; supports check-out with confirmation
- Order List screen → Check-In action (FAB)
- Check-In History screen for review and deletion of local drafts

**Capture sequence:**

1. App requests GPS permission and **acquires a fresh high-accuracy GPS fix** (continuous updates until ≤30 m accuracy or 30 s timeout; avoids stale cached coordinates)
2. Reverse geocoding resolves a human-readable address (display only)
3. App lists **nearby customers within 100 meters** of current GPS position
4. Salesperson selects a customer from the nearby list
5. App records check-in with:
   - Current GPS coordinates and accuracy
   - Selected customer identity and coordinate snapshot
   - Logged-in user's email
   - Current date and time
   - `StatusSync = DRAFT`
6. If the salesperson has an **open visit**, it is silently auto-closed first (see Check-Out section)
7. Record is stored locally in Room database (`checkin_table`)
8. App **immediately attempts cloud upload** for any auto-closed visit and the new check-in; on success local status becomes `SENT`

**Active visit on Home screen:**

When an open visit exists, the Home screen (Sales Orders) displays an active visit card showing customer name, check-in time, upload status (`Uploaded` / `Waiting for Upload`), and a Check Out button. The card is hidden when no visit is open.

**Nearby customer selection rules:**

- Customers are filtered from the locally synced customer master
- Default radius: **100 meters**
- After a customer is selected, nearby list excludes that customer and optionally filters by the same `wilayah` (territory area)
- There is **no validation** that the selected customer is on today's visit plan
- There is **no minimum accuracy requirement** to complete check-in

---

## 2. Mobile Sync (BTrade3 → Cloud)

Check-in sync runs alongside order sync on the **Order Sync** screen as an **offline fallback**.

**Immediate upload (primary path):**

After each successful local check-in or check-out save, the app attempts direct upload to `POST /api/CheckIn`. Network failure leaves the record as `DRAFT` for later retry. Offline-first behavior is preserved — check-in never requires connectivity.

**Sync screen (fallback):**

1. App reads all local records where `statusSync = 'DRAFT'`
2. Each record is posted individually to `POST /api/CheckIn`
3. On success, local status updates to `SENT`
4. Failed records remain `DRAFT` for retry

Check-out updates on previously synced visits reset local status to `DRAFT` and re-upload the full record (cloud delete-then-insert by `CheckInId`).

Sync is **best-effort per record** — partial success is possible. There is no batch atomicity.

---

## 3. Cloud Storage (BTRADE API)

The cloud API (`BTRADE_CheckIn`) acts as a **multi-tenant relay** between mobile devices and on-premise BTR installations.

**Upload behavior:**

- Delete-then-insert by `CheckInId` (idempotent re-upload)
- Status set to `TERKIRIM` on insert
- `ServerId` identifies the target BTR installation

**Download behavior:**

- Incremental query by date range and `ServerId`
- Returns records with `StatusSync = TERKIRIM`
- Marks returned records as `DOWNLOADED` on cloud after fetch

---

## 4. Office Download (j07-btrade-sync → BTR Desktop)

The `j07-btrade-sync` Windows service downloads check-ins from the cloud API on a configurable schedule.

**Download sequence:**

1. Service queries incremental endpoint for a rolling date window (default: last 3 months, max 122 days in RO1 UI)
2. For each returned check-in, **insert if new or update if CheckInId already exists** in `BTR_CheckIn`
3. Join `BTR_SalesPerson` via `UserEmail = Email` for display name

Download is optional — controlled by `DownloadCheckIn` registry flag in sync configuration.

---

## 5. Office Inquiry and Reporting

### RO1 — Check-In List (Desktop)

Supervisors query check-ins by date range and optionally filter by salesman email or customer keyword.

Displays:

- Check-in and customer details
- Haversine distance between check-in GPS and customer coordinates
- Color-coded distance bands (≤50 m, 50–100 m, >100 m)
- **Check-Out Time** and **Check-Out Mode** (for visit duration and discipline review)
- Excel export (includes check-out columns)

Maximum query period: **122 days** (~4 months).

### RO3 — Effective Call (Desktop)

Lists check-ins in a date range with a count of matching orders (`LEFT JOIN BTR_Order` on customer + email + date).

Rows with `OrderCount > 0` indicate productive visits.

### BTR Portal — Field Activity Dashboard (M18.5)

Composes a daily visit narrative per salesperson:

| KPI | Definition |
| --- | ---------- |
| **Planned Visits** | Customers on effective visit plan for the date |
| **Actual Visits** | Distinct customers with check-ins |
| **Missed Visits** | Planned − Actual |
| **Unplanned Visits** | Actual − Planned |
| **Visit Execution %** | Actual ÷ Planned |
| **Effective Calls** | Check-in customers with same-day orders |
| **Effective Call Rate** | Effective ÷ Actual |
| **GPS Valid / Warning / Suspicious** | Count by validation class |

Requires `BTR_SalesPerson.Email` to match `BTR_CheckIn.UserEmail`. Salespersons without email cannot appear in field activity reporting.

---

# Relationship to Visit Plan

Check-In represents **actual execution**. The Territory Execution Plan (`BTR_VisitPlan` + exceptions) represents **planned execution**.

```text
Visit Plan (planned)          Check-In (actual)
        │                              │
        └──────── compare ─────────────┘
                    │
            Visit Execution KPIs
            (planned, actual, missed, unplanned)
```

Check-In does **not** enforce route compliance at capture time. A salesperson can check in to any nearby customer regardless of today's plan. Route compliance is measured **after the fact** in reporting (M18.5 now; composite scoring deferred to M25).

---

# Sync Status Lifecycle

| Stage | Location | Status value | Meaning |
| ----- | -------- | ------------ | ------- |
| Captured | BTrade3 local | `DRAFT` | Recorded on device, not yet uploaded |
| Uploaded | BTrade3 local | `SENT` | Successfully posted to cloud API |
| Received | BTRADE cloud | `TERKIRIM` | Stored on cloud, awaiting office download |
| Downloaded | BTRADE cloud | `DOWNLOADED` | Fetched by j07-btrade-sync |
| Stored | BTR Desktop | (from cloud) | Available for inquiry and dashboards |

Note: BTR Desktop stores whatever `StatusSync` value arrives from sync. The operational statuses above span two systems with different naming conventions (`SENT` on mobile vs `TERKIRIM` on cloud).

---

# Business Rules

## Capture Rules

1. Check-in requires a valid GPS location and a selected customer
2. Customer must appear in the nearby list (within 100 m radius) — manual entry of arbitrary customers is not supported
3. Multiple check-ins to the same customer on the same day are allowed
4. Check-in is not required before creating a sales order
5. Check-in does not modify customer master data, inventory, or receivables
6. Check-out is optional; manual check-out sets `CheckOutMode = MANUAL` and records end time only (no checkout GPS)
7. Forgetting to check out manually triggers silent `AUTO` close on the next check-in — never blocks the user
8. Only one active (explicitly open) visit per salesperson at a time

## Identity Rules

1. `UserEmail` on check-in must match the logged-in BTrade3 user's email
2. `UserEmail` must match `BTR_SalesPerson.Email` for office reporting and KPI joins
3. Salespersons without configured email are invisible to field activity dashboards

## Reporting Rules

1. **Actual visit count** = distinct `CustomerId` per salesman per day
2. **Effective call** = check-in customer with ≥1 order same day, same email (heuristic match)
3. **Missed visit** = planned customer with no check-in on that date
4. **Unplanned visit** = check-in to customer not on effective plan for that date
5. Visit plan comparison is only available from visit-plan go-live date forward (no retroactive plans)

## GPS Rules

1. Distance validation uses coordinates stored on the check-in record (customer snapshot), not live master lookup
2. Zero coordinates (`0, 0`) are treated as missing/invalid for validation
3. Mobile app displays accuracy quality (green/yellow/red) but does not block submission
4. GPS acquisition uses **continuous high-accuracy updates** (`PRIORITY_HIGH_ACCURACY`, wait-for-accurate-location) until ≤30 m accuracy or 30 s timeout; stale cached fixes are not accepted
5. UI updates continuously as accuracy improves during acquisition; updates stop after capture completes

---

# Systems and Ownership

| System | Role | Storage |
| ------ | ---- | ------- |
| **BTrade3** | Mobile capture, local history, upload | `checkin_table` (Room) |
| **BTRADE Cloud API** | Relay between mobile and on-premise | `BTRADE_CheckIn` |
| **j07-btrade-sync** | Scheduled download to office | — |
| **BTR Desktop** | Persistent store, RO1/RO3 inquiry | `BTR_CheckIn` |
| **BTR Portal** | Field activity dashboard, map replay | Reads `BTR_CheckIn` |

**Business area:** Sales (field execution)

**Collaborates with:**

- Master Data (customer coordinates)
- Visit Plan (planned visit denominator)
- Sales Order (effective call numerator)

---

# Business Concerns and Risks

## 1. Adoption Discipline

Check-in is voluntary at capture time. A salesperson can visit a customer, take an order, and never check in. Management visibility depends on consistent mobile usage.

**Impact:** Actual visit counts understate real field activity. Visit execution % and effective call rate are only as reliable as check-in discipline.

**Mitigation posture:** BTR Portal field activity dashboard is designed to create a visibility → enforcement → data quality flywheel. KPIs are published even when current discipline is imperfect.

---

## 2. GPS Authenticity

Check-in proves a GPS coordinate was captured, not that the salesperson was physically inside the customer's premises.

**Risk factors:**

- GPS inaccuracy in urban canyons or indoor locations
- Customer master coordinates may be wrong or default `0, 0`
- No continuous breadcrumb trail — only a single point-in-time coordinate
- No anti-spoofing controls on mobile device

**Impact:** Suspicious GPS classifications may reflect data quality problems rather than fraudulent behavior. Supervisors should treat GPS validation as a signal requiring investigation, not automatic penalty.

---

## 3. Customer Coordinate Coverage

GPS distance validation requires meaningful customer coordinates. Many customer records may still have `Latitude = 0, Longitude = 0`.

**Impact:** Distance column and GPS validation KPIs are unreliable until coordinate coverage improves. Portal surfaces **Coordinate Coverage %** as a data-health indicator.

---

## 4. Weak Order–Check-In Linkage

Effective call matching uses date + customer + email heuristic. There is no foreign key between `BTR_Order` and `BTR_CheckIn`.

**Scenarios that distort effective call:**

- Order taken by phone after a drive-by (no check-in) → not counted as effective call
- Check-in at customer A, order entered later for customer B → no link
- Multiple check-ins and multiple orders same day → dedupe by customer only

**Impact:** Effective call rate measures **correlated activity**, not causal visit-to-order linkage.

---

## 5. No Route Enforcement at Capture

Mobile check-in does not validate against today's visit plan or route sequence. Unplanned visits are allowed and reported separately.

**Impact:** Route compliance is a reporting concern, not a field gate. Salespeople are not blocked from checking in off-route.

---

## 6. Sync Reliability

Check-in sync is per-record and non-transactional. Network failures leave records in `DRAFT` on the device. Immediate upload reduces but does not eliminate visibility gaps — the Sync screen remains the offline fallback.

**Impact:** Supervisors reviewing today's visits see data sooner when connectivity is available. Temporary gaps remain possible until sync completes.

---

## 7. Historical Retention

`BTR_CheckIn` has no discovered purge or archival policy. Full history is retained indefinitely.

**Impact:** Positive for trend analysis and visit replay. Query performance may degrade at scale — date/email indexes exist in DDL but are commented out.

---

## 8. Identity Dependency on Email

The entire reporting chain (`CheckIn` → `SalesPerson` → `VisitPlan` → `Order`) depends on consistent email usage.

**Impact:** Salespersons without `BTR_SalesPerson.Email` are excluded from field activity dashboards. Email changes without historical update break joins.

---

# What Check-In Does Not Do

- Does not create or modify Sales Orders, Faktur, payments, or inventory
- Does not enforce visit plan compliance at capture time
- Does not track continuous movement between customers
- Does not update customer master coordinates (separate Location Capture feature)
- Does not support substitute salesman modeling in visit plan v1
- Does not produce route compliance composite scores (deferred to M25)

---

# Key Data Entity

**Table:** `BTR_CheckIn`

**Primary key:** `CheckInId` (ULID, 26 characters)

**Identity bridge:** `UserEmail` ↔ `BTR_SalesPerson.Email` ↔ `BTR_Order.UserEmail`

**Indexes:** Primary key only in current DDL. Secondary indexes on `UserEmail`, `CheckInDate`, `CustomerId`, and `StatusSync` are defined but commented out.

---

# Related Artifacts

| Artifact | Relevance |
| -------- | --------- |
| `docs/work/check-in-improvement-analysis.md` | Check-out, immediate upload, GPS improvement analysis |
| `docs/features/visit-plan/feature.md` | Planned visit denominator for execution KPIs |
| `docs/features/sales-order/feature.md` | Order lifecycle; effective call numerator |
| `docs/features/btr-portal/btr-portal-domain.md` | Field activity KPI definitions |
| `docs/work/btr-portal/M18-5-Sales-Visit-Analysis.md` | Feasibility analysis for visit monitoring |
| `docs/foundation/PRODUCT.md` | Field sales operations context |
| `docs/foundation/WORKFLOW.md` | Collection visit workflow (adjacent, not identical) |

---

# Summary

Check-In is BTR's mechanism for **attesting customer visits** in the field. Sales personnel capture GPS-tagged visit records on BTrade3; records flow through a cloud relay into BTR Desktop and BTR Portal reporting.

The primary business value is **operational visibility**: supervisors can compare planned routes against actual visits, assess GPS authenticity, and measure whether visits produced orders (effective calls).

The primary business risks are **adoption discipline**, **GPS and coordinate data quality**, and **heuristic order linkage** — none of which block capture, by design. Check-In is a accountability and measurement layer on top of existing sales workflows, not a transactional gate.
