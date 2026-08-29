# BTrade3 Product Capability Analysis

## 1. Executive Summary

**BTrade3** is a mobile Sales Force Automation (SFA) application for distributor field sales. It enables salespeople to take customer orders and prove customer visits while working offline, then synchronize results to the distributor backend (Cloud API / BTR ecosystem).

**Primary users:** Field sales personnel (salesman / sales representative) operating from a depot branch (currently Jogja or Magelang).

**Operational areas supported:**
- Field sales order capture
- Customer visit attendance (GPS check-in / check-out)
- Customer geo-location registration
- Master data consumption (customers, products, salespersons)
- Local sales productivity visibility
- Synchronization of orders, visits, and customer coordinates to the backend

**Not in scope of this app:** Payment/collection, delivery execution, returns, promotions engine, warehouse operations, customer creation, photo capture, printing, or messaging.

---

## 2. User Roles

BTrade3 does not implement role-based access control. Identity is a Google account email. The concepts below are the operational personas reflected in the product.

### Field Salesperson (primary user)

| Aspect | Detail |
|--------|--------|
| **Role name** | Field Salesperson / Salesman |
| **How identified** | Google Sign-In email stored as `userEmail` |
| **Responsibilities** | Visit customers, record attendance, create sales orders, register customer GPS (first time), sync masters and transactions |
| **Main workflows** | Login → Sync master data → Check-in → Create/finish order → Check-out → Sync transactions → Review local summary |

### Salesperson Master (order attribution, not login)

| Aspect | Detail |
|--------|--------|
| **Role name** | Sales Person (master data) |
| **How identified** | Synced list: ID, code, name |
| **Responsibilities** | Attribution field on each sales order (who “owns” the order commercially) |
| **Main workflows** | Selected during order entry; last selection remembered |

### Depot / Branch Operator (implicit)

| Aspect | Detail |
|--------|--------|
| **Role name** | Branch-scoped user |
| **How identified** | Server selection at login: **JOG** (Jogja) or **MGL** (Magelang) |
| **Responsibilities** | Work against the correct depot’s master data and upload transactions to that branch context |
| **Main workflows** | Choose server before Google Sign-In; all downloads/uploads carry `serverId` |

### Office / Admin (indirect beneficiary)

| Aspect | Detail |
|--------|--------|
| **Role name** | Sales Admin / Back-office (not an app user) |
| **Responsibilities** | Receive synced orders (with optional “Note For Admin”), visit records, and customer GPS updates via backend |
| **Main workflows** | Outside BTrade3; consumes synced data in BTR Desktop / Cloud API |

---

## 3. Business Capability Map

### Sales Execution

| Capability | Description | User |
|------------|-------------|------|
| Offline sales order creation | Create and edit sales orders without network | Field Salesperson |
| Dual-unit quantity entry | Order in large and small units with conversion | Field Salesperson |
| Bonus quantity | Record free/bonus units on a line | Field Salesperson |
| Multi-tier line discount | Apply up to four sequential percentage discounts | Field Salesperson |
| Order note to admin | Attach a note for back-office processing | Field Salesperson |
| Finish / reopen order | Mark order Ready to sync, or reopen for editing | Field Salesperson |
| Branch-scoped ordering | Orders tied to JOG or MGL depot | Field Salesperson |

### Customer Management

| Capability | Description | User |
|------------|-------------|------|
| Browse / search customers | Offline customer list with wilayah and address | Field Salesperson |
| View customer on map | Open registered coordinates in Google Maps | Field Salesperson |
| Register customer GPS (first-time) | Capture and save store coordinates when missing | Field Salesperson |
| Sync customer location | Upload new GPS pins to backend | Field Salesperson |

### Visit Management

| Capability | Description | User |
|------------|-------------|------|
| GPS check-in | Check in only to customers within 100 m | Field Salesperson |
| Active visit indicator | See open visit and elapsed time on home screen | Field Salesperson |
| Manual check-out | Close visit from history (time only) | Field Salesperson |
| Auto check-out | Previous open visit closes when checking in elsewhere | Field Salesperson |
| Visit history | Review visits by date; sync status; open in Maps | Field Salesperson |

### Order Management

| Capability | Description | User |
|------------|-------------|------|
| Order list & search | Manage in-progress, ready, and sent orders | Field Salesperson |
| Order line management | Add, edit, delete product lines | Field Salesperson |
| Order sync queue | Upload Ready orders individually or in batch | Field Salesperson |
| Delete local orders | Remove orders locally (bulk select supported) | Field Salesperson |

### Collection Management

| Capability | Description | User |
|------------|-------------|------|
| *(Not implemented)* | No payment, AR, or collection recording | — |

### Delivery Support

| Capability | Description | User |
|------------|-------------|------|
| *(Not implemented)* | No delivery / shipping workflow | — |

### Inventory Visibility

| Capability | Description | User |
|------------|-------------|------|
| Product stock display | Show `stok` badge when selecting products | Field Salesperson |
| Product master browse | Search products by name/code with category and price | Field Salesperson |

Note: Stock is display-only; ordering does not reserve or adjust inventory.

### Promotion Management

| Capability | Description | User |
|------------|-------------|------|
| Manual line discounts | Disc1–Disc4 percentage cascade (not a promo engine) | Field Salesperson |

### Reporting

| Capability | Description | User |
|------------|-------------|------|
| Local order summary | Counts, gross sales, and item totals by user/date | Field Salesperson |

### Administration

| Capability | Description | User |
|------------|-------------|------|
| Google authentication | Sign in / logout with Google account | Field Salesperson |
| Depot server selection | Choose JOG or MGL | Field Salesperson |
| Master data sync | Download barang, customer, salesperson | Field Salesperson |
| Transaction sync | Upload orders and draft check-ins | Field Salesperson |

---

## 4. Detailed Feature Catalog

### Google Sign-In & Depot Selection

Purpose:
Authenticate the field user and bind the session to a depot server.

Business Value:
Ensures orders and visits are attributable to a person and a branch.

Workflow:
1. User selects JOG or MGL.
2. User signs in with Google.
3. Email is stored; app opens Sales Orders home.
4. Logout clears the session email.

Data Captured:
- User email
- Selected server ID (JOG / MGL)

Offline Support:
Partial (login requires Google/network; subsequent work is offline)

Evidence:
- Screen: `LoginScreen`
- Route: `login`
- Service/API: Google Sign-In SDK
- Relevant locations: `LoginScreen.kt`, `Navigation.kt` prefs helpers, `ServerHelper`

---

### Master Data Synchronization

Purpose:
Download product, customer, and salesperson masters for offline use.

Business Value:
Salesperson can work in the field without continuous connectivity using current depot data.

Workflow:
1. Open Sync Master Data.
2. Sync Barang, Customer, and/or Sales Person.
3. Local masters are replaced with server download.

Data Captured:
- Products (code, name, category, units, conversion, price, stock)
- Customers (code, name, address, wilayah, GPS if present)
- Salespersons (id, code, name)

Offline Support:
Yes (after download); sync itself requires network

Evidence:
- Screen: `SyncScreen`
- Use case: Keep field device current before route day
- Service/API: `GET Brg/{serverId}`, `GET Customer/{serverId}`, `GET SalesPerson/{serverId}`
- Repository: `SyncRepository`, `NetworkRepository`
- Relevant locations: `ApiService.kt`, `SyncScreen.kt`

---

### Sales Order Entry

Purpose:
Capture a customer purchase request (sales order) in the field.

Business Value:
Converts customer demand into structured order data for back-office fulfillment (BTR Desktop / warehouse path).

Workflow:
1. From home, create new order (status In Progress).
2. Select customer and salesperson.
3. Add product lines (qty besar/kecil, bonus, disc1–4).
4. Optionally enter “Note For Admin”.
5. Finish Order → Ready; or Reopen for Editing → In Progress.
6. Sync Ready orders to backend → Sent.

Data Captured:
- Order header: customer, address, GPS snapshot, date, salesperson, total, user email, note, sync status
- Order lines: product, dual qty, bonus, conversion, unit price, four discounts, line total

Offline Support:
Yes (create/edit fully offline; upload requires network)

Evidence:
- Screens: `OrderListScreen`, `OrderEntryScreen`, `ItemListScreen`, `AddBarangScreen`, `BarangSelectionScreen`, `SalesSelectionScreen`, `CustomerSelectionScreen`
- Use case: Field order taking
- Service/API: `POST Order`
- Repository: `OrderRepository`, `OrderSyncRepository`
- Relevant locations: `Order.kt`, `OrderItem.kt`, `OrderSyncStatus.kt`, `OrderSyncRequest.kt`

---

### Product Line Pricing & Discounts

Purpose:
Calculate line totals using dual UOM, unit price, and cascading percentage discounts.

Business Value:
Matches distributor pricing practices (besar/kecil units + multi-discount negotiation) without requiring online price engines.

Workflow:
1. Select product (price and conversion prefilled from master).
2. Enter qty besar and/or qty kecil; optional bonus qty.
3. Enter disc1–disc4 percentages.
4. System computes line total; order total aggregates lines.

Data Captured:
- Qty besar / kecil, bonus qty, unit price, disc1–4, line total

Offline Support:
Yes

Evidence:
- Screen: `AddBarangScreen`
- ViewModel pricing logic: `AddBarangViewModel`
- Model: `OrderItem`

---

### Customer Browse & Map Access

Purpose:
Find customers offline and navigate to registered locations.

Business Value:
Supports route execution and location verification.

Workflow:
1. Open Manage Customers or pick customer during order entry.
2. Search/browse list.
3. If GPS exists, open in Google Maps.
4. If GPS missing, launch location registration.

Data Captured:
- Uses existing customer master; no new customer creation

Offline Support:
Yes (browse); Maps requires Maps app/network

Evidence:
- Screen: `CustomerSelectionScreen`
- Utils: Map open helpers in customer/check-in/location screens

---

### Customer Location Registration

Purpose:
Capture GPS coordinates for customers that do not yet have a pin.

Business Value:
Builds a geo-master for visit enforcement, routing, and supervision.

Workflow:
1. From customer list, open Set Customer Location.
2. Acquire high-accuracy GPS (with reverse geocode awareness).
3. Save only if customer has no coordinates yet (write-once from app).
4. Mark customer updated and upload via PATCH.

Data Captured:
- Latitude, longitude, accuracy, timestamp, capturing user email

Offline Support:
Partial (capture can be local; upload needs network; flagged `isUpdated`)

Evidence:
- Screen: `LocationCaptureScreen`
- Service/API: `PATCH Customer`
- Repository: `CustomerSyncRepository`
- Models: `Customer`, `CustomerSyncRequest`
- Relevant locations: `LocationCaptureViewModel.kt` (`canRegisterLocation`)

---

### GPS Check-In / Visit Attendance

Purpose:
Prove the salesperson was physically at the customer location.

Business Value:
Visit compliance and route discipline for sales supervision.

Workflow:
1. Open Check-In from home FAB.
2. Acquire GPS; list customers within 100 meters.
3. Select nearby customer and Check In.
4. If another visit is open, it is auto-closed with current GPS.
5. Visit saved as Draft and upload attempted immediately.

Data Captured:
- Check-in date/time, GPS, accuracy
- Customer snapshot and customer GPS
- User email, sync status, open-visit flag

Offline Support:
Yes (local Draft); upload immediate if online, else later via Sync Transaction

Evidence:
- Screen: `CheckInScreen`
- ViewModel: `CheckInViewModel` (100 m radius)
- Service/API: `POST CheckIn`
- Repository: `CheckInRepository`, `CheckInSyncRepository`
- Model: `CheckIn`, `CheckInRequest`

---

### Check-Out & Visit History

Purpose:
Close visits and review attendance history.

Business Value:
Completes visit lifecycle; provides local audit trail of field activity.

Workflow:
1. Manual: History → Check Out (records time only, mode MANUAL).
2. Auto: Next check-in closes previous open visit with GPS (mode AUTO).
3. History filterable by date; can open location in Maps; can delete local records; can sync drafts.

Data Captured:
- Check-out time; optional check-out GPS/accuracy (auto only); check-out mode

Offline Support:
Yes

Evidence:
- Screen: `CheckInHistoryScreen`
- Repository: `CheckInRepository.manualCheckOut`, `autoCloseOpenVisit`
- Model: `CheckOutMode`

---

### Transaction Synchronization

Purpose:
Upload Ready sales orders and Draft check-ins to the backend.

Business Value:
Moves field activity into the distributor’s central operational system.

Workflow:
1. Open Sync Transaction.
2. Select Ready orders and/or sync draft check-ins.
3. On success, orders become Sent; check-ins leave Draft.

Data Captured:
- Full order + lines; full check-in/out payload; serverId

Offline Support:
No (requires network); queueing of Ready/Draft is offline

Evidence:
- Screen: `OrderSyncScreen`
- Also: single-order sync from order list
- Service/API: `POST Order`, `POST CheckIn`
- Repositories: `OrderSyncRepository`, `CheckInSyncRepository`

---

### Local Order Summary

Purpose:
Show salesperson productivity metrics from local orders.

Business Value:
Self-monitoring of daily order count, gross sales, and items—without needing a BI portal.

Workflow:
1. Open Order Summary from menu.
2. View aggregates by user email and order date.

Data Captured:
- Derived: orderCount, grossSales, totalItems (from local DB)

Offline Support:
Yes

Evidence:
- Screen: `OrderSummaryScreen`
- Model: `OrderSummary`
- Repository/DAO aggregate query via `OrderRepository` / `OrderSummaryViewModel`

---

## 5. Mobile Field Activities

Activities supported by code:

- Sign in with Google and select depot (JOG / MGL)
- Download master data (products, customers, salespersons)
- Browse and search customers offline
- Register customer GPS (first-time only)
- Open customer location in Google Maps
- Perform GPS check-in to nearby customer (≤ 100 m)
- View active open visit and elapsed time
- Manually check out from visit history
- Review check-in history by date
- Create sales order offline
- Assign salesperson to order
- Select products and enter dual-unit quantities
- Enter bonus quantity and up to four line discounts
- Add note for admin
- Finish order (Ready) or reopen for editing
- Search, select, and delete local orders
- Sync ready orders and draft visits to backend
- View local order summary (counts / gross / items)
- Log out

Activities **not** supported (confirmed absent):

- Record payment / collection
- View outstanding invoices / AR
- Create or edit customer master (beyond GPS)
- Capture photos or signatures
- Print receipts
- Share via WhatsApp
- Bluetooth printing
- Process returns
- Reserve or adjust stock
- Plan routes / visit schedules ahead of time

---

## 6. Customer Visit Workflow

Actual lifecycle reconstructed from implementation:

```text
Login + Depot Selection
    ↓
Sync Master Data (customers, products, salespersons)
    ↓
[Optional] Register missing customer GPS (write-once)
    ↓
Travel to customer
    ↓
GPS Check-In (must be within 100 m of customer pin)
    ↓
Active visit shown on home (elapsed time)
    ↓
Customer Interaction
    ├── Browse customer / open Maps
    └── Create Sales Order
            ├── Select customer & salesperson
            ├── Add lines (qty, bonus, discounts)
            ├── Note For Admin (optional)
            └── Finish Order → Ready
    ↓
Check-Out
    ├── Manual (from history; time only), or
    └── Auto (next check-in elsewhere; with GPS)
    ↓
Synchronization
    ├── Upload Ready orders (POST Order)
    ├── Upload Draft check-ins (POST CheckIn)
    └── Upload new customer GPS (PATCH Customer)
    ↓
Local Order Summary (optional self-review)
```

**Important nuances:**
- There is no formal “visit plan” or scheduled route object—visits start at check-in time.
- Check-in is geofenced to 100 m; customers without GPS cannot be checked into via the nearby list.
- Order entry is not hard-gated to an open check-in (orders can be created independently), but the product is designed for visit + order together.
- Only one explicitly open visit per user; starting a new check-in auto-closes the previous.

---

## 7. Data Collection Inventory

| Data | Where used |
|------|------------|
| User email (Google) | Login identity; order ownership; check-in attribution; location capture user |
| Depot server ID (JOG/MGL) | Scopes all master downloads and uploads |
| Customer master | Order header; check-in target; location registration |
| Customer GPS (lat/lng/accuracy/timestamp) | Visit geofence; Maps; synced to backend for geo-master |
| Salesperson master | Order commercial attribution |
| Product master (incl. stock, price, UOM) | Product selection and line pricing |
| Sales order header | Field demand capture; sync to backend for fulfillment |
| Order line details (dual qty, bonus, disc1–4) | Pricing and fulfillment detail |
| Order note | Message to sales admin |
| Order sync status | Offline queue control (In Progress / Ready / Sent) |
| Check-in GPS + time | Visit proof / attendance |
| Check-out time (+ GPS if auto) | Visit duration / closure |
| Check-out mode (MANUAL/AUTO) | How visit was closed |
| Check-in sync status (DRAFT/SENT) | Visit upload queue |
| Local order summary aggregates | Salesperson self-reporting |

**Not collected:** photos, signatures, payments, survey answers, voice, Bluetooth device data, printed documents.

---

## 8. Offline Capability Analysis

### Features that work offline

- Browse customers, products, salespersons (after master sync)
- Create/edit/delete sales orders and lines
- Finish / reopen orders
- GPS check-in and check-out (saved locally as Draft)
- Capture customer GPS locally (`isUpdated` flag)
- View order list, visit history, local order summary

### Features that require network

- Google Sign-In
- Master data download
- Upload orders, check-ins, customer GPS
- Google Maps navigation (external)

### Synchronization mechanisms

- **User-initiated only** (no background WorkManager / silent sync found)
- Master sync: full replace (delete all + insert download)
- Order queue: In Progress → Ready → Sent
- Check-in queue: Draft → Sent (immediate upload attempted at check-in/out; residual drafts via Sync Transaction)
- Customer GPS: flagged `isUpdated` until PATCH succeeds

### Local storage

- Room database (`sales_order_database`) for orders, items, customers, barang, salespersons, check-ins
- SharedPreferences for user email and related prefs
- DataStore via `ServerHelper` for selected server

### Conflict handling

- No multi-device conflict resolution engine
- Master re-download overwrites local masters (risk if unsynced customer GPS exists before customer re-sync)
- Orders once Sent are no longer editable
- Customer GPS from app is write-once (cannot overwrite existing coordinates from device)

### Queue processing

- Ready orders selected for upload on Sync Transaction or single sync from list
- Draft check-ins uploaded in batch on Sync Transaction
- Progress callbacks for multi-item upload UX

---

## 9. Integration Analysis

| Integration | Purpose |
|-------------|---------|
| **Cloud API / Distributor backend** (`belajar-api`) | Download masters; receive orders, check-ins, customer GPS | BTR ecosystem sync hub per LANDSCAPE |
| **ERP / BTR Desktop** (indirect) | Downstream consumer of synced sales orders for operational processing | Not called directly by the app |
| **Google Sign-In** | Authenticate field user | Login |
| **GPS / Location services** | Check-in geofence; customer pin capture; auto check-out coordinates | Android location permissions |
| **Google Maps** | Navigate to / view customer or visit coordinates | Intent / URL open |
| **Reverse geocoding** | Human-readable address context during location capture | `ReverseGeocodingHelper` |
| **Camera** | Not used | No permission / UI |
| **Bluetooth** | Not used | — |
| **Printing** | Not used | — |
| **WhatsApp** | Not used | — |

API surface:
- `GET Brg/{serverId}`
- `GET Customer/{serverId}`
- `GET SalesPerson/{serverId}`
- `POST Order`
- `PATCH Customer`
- `POST CheckIn`

---

## 10. Competitive Capability Summary

**What operational capabilities does this application provide to a distributor sales organization?**

### Sales Productivity
- Offline order taking with distributor-native dual UOM and four-tier discounts
- Fast product/customer search on-device
- Bonus quantity capture
- Admin note on order
- Local daily productivity summary

### Sales Supervision
- GPS-enforced check-in (100 m)
- Visit open/close with elapsed time
- Auto-close previous visit when checking in elsewhere
- Synced visit trail for back-office review
- User email attribution on orders and visits

### Customer Management
- Offline customer directory by wilayah
- First-time GPS registration of outlets
- Maps access to customer pins
- Upload of new coordinates to central geo-master

### Collection Control
- **Not provided** — no AR view, payment capture, or collection workflows

### Route Execution
- Field check-in/check-out cycle
- Nearby-customer discovery at current GPS
- No formal route plan / call schedule module

### Operational Visibility
- Order sync statuses (In Progress / Ready / Sent)
- Visit sync statuses (Draft / Sent)
- Local order aggregates
- Branch segregation (JOG / MGL)

---

## 11. Product Marketing Summary

### Sales Force Automation

- Offline sales order capture for field teams
- Dual-unit ordering (besar / kecil) with conversion
- Bonus quantity and multi-level percentage discounts
- Salesperson attribution on every order
- Notes for back-office processing
- Depot-aware operation (Jogja / Magelang)

### Customer Visit Management

- GPS check-in limited to customers within 100 meters
- Active visit tracking with elapsed time
- Manual and automatic check-out
- Visit history by date
- First-time customer location registration
- Google Maps navigation to outlets

### Collection Management

- Not included in current product scope

### Order & Sync Operations

- Work fully offline; sync when connected
- Ready-to-send order queue
- Batch upload of orders and visit records
- Customer GPS upload to central system

### Reporting & Visibility

- On-device order summary by day (order count, gross sales, items)
- Clear sync status for orders and visits
- Branch server indicator on the home screen

---

## Appendix: Scope Boundaries (Evidence of Absence)

The following common SFA capabilities were **searched for and not found** in BTrade3:

| Capability | Evidence of absence |
|------------|---------------------|
| Payment / collection | No models, screens, or APIs |
| Outstanding invoice / piutang | No entities or UI |
| Delivery execution | No workflows |
| Returns (retur) | No workflows |
| Camera / photo / signature | No CAMERA permission; no capture UI |
| Printing | No print integration |
| WhatsApp / share | No share intents for messaging |
| Bluetooth devices | No Bluetooth permissions/APIs |
| Create customer on device | Customer master download-only |
| Promotion engine | Only manual disc1–4 |
| Background sync | User-initiated sync only |
| Role-based security | Single Google-email identity model |

---

*Source: reverse-engineered from `src/BTrade3` application code. Business interpretation aligned with `docs/foundation/PRODUCT.md`, `DOMAIN.md`, and `LANDSCAPE.md` (BTrade3 = Mobile Sales Operations).*
