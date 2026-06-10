# Implementation Plan: M18.5 — Sales Route & Visit Monitoring (Field Activity)

## Document Status

| Field | Value |
| --- | --- |
| Initiative | M18.5 Field Activity — operational visit execution, route monitoring, GPS visibility, map-centric demonstration |
| Authoritative requirements | `docs/work/btr-portal/M18-5-Sales-Visit-Analysis.md` — **Section 12 (Approved Product Decisions)** |
| Related features | `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-architecture.md`, `docs/features/visit-plan/feature.md`, `docs/features/sales-order/feature.md` |
| Reference pattern | Desktop RO1 (check-in distance), RO3 (effective call), SM7 (effective visit plan); M18 salesman drill-down API (parameterized read) |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |
| Status | **Ready for implementation** |
| Product decisions | **Confirmed** — analysis Section 12, 2026-06-11 |

---

## 1. Goal

Deliver **Field Activity Control Tower** at `/dashboard/field-activity` — a map-dominant dashboard that answers:

- *Which customers were planned, visited, missed, and which visits produced sales?* (Objective A — operational visibility)
- *Can BTR Portal showcase route planning, GPS check-in, and field coverage in a visually impressive way?* (Objective B — demonstration value)

**Primary outcomes (Release 1):**

| Outcome | Description |
| --- | --- |
| Control Tower UX | Three-panel layout — KPI strip + missed list (left), **dominant map** (center), visit timeline + replay (right) |
| Coverage map | Visited / missed / unplanned pin colors on first load after salesman-day selection |
| Planned vs actual routes | Distinct polyline styles on same map — flagship visual |
| Route replay | Animated daily playback with adjustable speed; timeline sync |
| Visit execution KPIs | Planned, Actual, Effective, Missed, Unplanned, Visit Execution %, Effective Call Rate |
| GPS validation | RO1 distance bands on list rows and map pins |
| Salesman-day filters | Required salesman selector; Today · Yesterday · Custom Date |
| Demo readiness | Development seed data when operational data is sparse |

**Business question:** *Did the field team execute the planned route, and where did they go?*

**Explicitly out of scope (PO confirmed — do not implement in Release 1):**

- Visit execution KPIs on M18 `/dashboard/salesmen` (separate nav item; future summary card only — Q19)
- Route compliance composite score, productivity index, sequence tolerance scoring → **M25**
- Wilayah heatmap, 7/30-day trends, team-level map, Alert Center signals → **Release 2**
- Continuous GPS tracking, road-network routing, mobile app changes, SM7 visit-plan editing
- Collection route effectiveness → **M20**
- `BTRPD_*` snapshot domain or `btr.portal.worker` refresh job for Release 1

---

## 2. Authoritative Product Decisions

Source: `M18-5-Sales-Visit-Analysis.md` Section 12. Do not re-decide during implementation.

### 2.1 Milestone positioning

| Decision | Rule |
| --- | --- |
| M18 scope | Outcome KPIs — achievement, exposure, rankings |
| M18.5 scope | Execution lens — planned vs actual, GPS, map, replay |
| Visual impact | **Explicit success criterion** — demo value may justify Release 1 even when management KPI value is Medium |
| Visibility flywheel | Release visit KPIs despite imperfect discipline — dashboard drives enforcement |

### 2.2 Dashboard presentation

| # | Decision |
| - | -------- |
| Q1 | Route: **`/dashboard/field-activity`** |
| Q2 | Layout: **Concept C — Control Tower**; map occupies **most of viewport** |
| Q3 | Default date: **Today**; quick selectors: Today · Yesterday · Custom Date |
| Q4 | Default salesman: **No automatic load** — user must select salesman first |
| Q5 | M18 relationship: **Separate navigation item** (not tab on `/dashboard/salesmen`) |

### 2.3 KPI and visit rules

| # | Decision |
| - | -------- |
| Q6 | Multiple check-ins same customer same day: **count as one actual visit**; use **earliest `CheckInTime`** for route sequence |
| Q7 | Visit Execution % when Planned = 0: display **N/A** |
| Q8 | Release 1 aggregation: **daily only** |
| Q9 | Effective Call: RO3 definition — check-in with ≥1 `BTR_Order` same date, customer, `UserEmail` |
| Q10 | Unplanned Visit: **separate KPI** and **separate map pin color** |

**Release 1 KPI set:** Planned Visit · Actual Visit · Effective Call · Missed Visit · Unplanned Visit · Visit Execution % · Effective Call Rate

### 2.4 GPS and map

| # | Decision |
| - | -------- |
| Q11 | Distance bands: RO1 — Valid ≤50 m · Warning 50–100 m · Suspicious >100 m |
| Q12 | Map provider: **MapLibre GL JS + OpenStreetMap** |
| Q13 | Zero-coordinate customers: **hide from map**; show in list with **"No Coordinates"** badge |
| Q14 | Replay speed: **adjustable slider** |

### 2.5 Data and integrations

| # | Decision |
| - | -------- |
| Q15 | Visit-plan history: **no backfill** before materialization go-live |
| Q16 | Demo readiness: **seed realistic demonstration data** in development |
| Q17 | Snapshot vs live query: **Architect decision** — see Section 4.2 |
| Q18 | Alert Center integration: **Release 2** |
| Q19 | M18 summary card: **allowed in future** — one Visit Execution summary max |
| Q20 | M25 inputs: Visit Execution % · Effective Call Rate · Unplanned Visit Rate · GPS Suspicious % |

### 2.6 Flagship visuals (PO emphasized)

| Feature | Priority |
| ------- | -------- |
| Animated daily route replay | First-class Release 1 |
| Coverage map (visited / missed / unplanned) | Primary view |
| Planned vs actual route comparison | Flagship visual — hero interaction, not secondary toggle |

---

## 3. Current State vs Target

| Area | Current state | M18.5 target |
| ---- | ------------- | ------------ |
| Portal route | No `/dashboard/field-activity` | New route + nav item **Field Activity** |
| Map capability | Chart.js only; no map library | MapLibre GL JS + OSM tiles |
| Visit plan data | `BTR_VisitPlan` + `EffectiveVisitPlanResolver` in Desktop/worker | Portal consumes via `IEffectiveVisitPlanDal` |
| Check-in data | `BTR_CheckIn` via RO1; synced from BTrade3 | Portal live read per salesman-day |
| Effective call | `EffectiveCallDal` in Desktop RO3 | Same join pattern in Portal composer |
| GPS distance | `CheckInView.Distance` Haversine in RO1 form | Extract to shared classifier in application layer |
| M22 Locations dashboard | Warehouse/Wilayah **inventory/sales** performance | **Unrelated** — do not extend; M18.5 is field visit execution |
| Demo data | None in portal | Dev-only SQL seed script |

---

## 4. Architecture Overview

### 4.1 Target topology

```text
Browser — Field Activity Control Tower
  Select salesman + date (no load until both set)
        ↓
GET /api/dashboard/field-activity?salesPersonId=&visitDate=
GET /api/dashboard/field-activity/salesmen          [selector list]
        ↓ MediatR
GetFieldActivityQueryHandler
        ↓
FieldActivityComposer
  ├─ ISalesPersonDal              (identity + Email bridge)
  ├─ IEffectiveVisitPlanDal       (planned stops — reuse Desktop resolver)
  ├─ IFieldActivityCheckInDal     [NEW — per salesman-day, deduped earliest]
  ├─ IFieldActivityOrderDal       [NEW — effective-call existence per customer-day]
  └─ ICustomerCoordinateDal       [NEW — lat/lng for planned stops]
        ↓
FieldActivityResponse
  KPIs · plannedStops · actualStops · missedVisits · routeGeometry · gpsSummary · meta

No btr.portal.worker domain · No BTRPD_* tables (Release 1)
```

### 4.2 Architecture decisions

| Decision | Choice | Rationale |
| -------- | ------ | --------- |
| Data access model | **Live query on API request** | PO Q17; parameterized `(salesPersonId, visitDate)`; user-triggered load; intraday freshness for Today; visit-level coordinates poor fit for `CURRENT` snapshot replace |
| Snapshot deferral | Team trends, rankings, Alert Center → **Release 2** | Analysis Section 10.2; avoids salesman × date × horizon explosion in `BTRPD_*` |
| Business logic location | `FieldActivityComposer` in `btr.application` | Testable; mirrors `DashboardExecutiveComposer` read-time composition pattern |
| Planned visit source | `IEffectiveVisitPlanDal.ListEffectivePlan` | Approved denominator; do not reimplement exception merge in Portal |
| Effective call | `LEFT JOIN BTR_Order` on `CustomerId + UserEmail + OrderDate = CheckInDate` | RO3 parity; count distinct customer per day for KPI |
| GPS distance | Haversine on check-in coords vs **customer snapshot coords on check-in row** (`CustomerLatitude/Longitude`) | RO1 uses same fields via `CheckInView`; preserves point-in-time accuracy |
| GPS classification | `GpsValidationClassifier` with RO1 bands | Valid ≤50 m · Warning 50–100 m · Suspicious >100 m; zero coords → `Invalid` (hidden from map) |
| Customer coords for planned stops | `BTR_Customer.Latitude/Longitude` | Planned pins use master coords; missed planned stops may lack coords |
| Coordinate coverage KPI | `Planned stops with non-zero coords ÷ planned stops × 100` for selected day | Data-health signal per PO Section 10.1 #14 |
| Visit plan pre-go-live dates | Return empty planned set + `PlanDataAvailable = false` in meta | PO Q15 — no backfill |
| Demo data | `Seed_FieldActivity_Demo.sql` (dev/staging only) | PO Q16; not loaded in production |
| Map library | `maplibre-gl` npm package | PO Q12; OSM raster/vector tiles via public tile URL |
| API auth | `[Authorize]` — same as other dashboard endpoints | No RBAC in Release 1 |

### 4.3 Join path (implementation reference)

```text
SalesPersonId
  → BTR_SalesPerson.Email
    → BTR_CheckIn (UserEmail, CheckInDate, CustomerId)     [Actual]
    → BTR_Order (UserEmail, OrderDate, CustomerId)         [Effective Call]

Effective Visit Plan (SalesPersonId, VisitDate)
  = BTR_VisitPlan + exceptions via EffectiveVisitPlanResolver

Missed = planned CustomerId NOT IN actual CustomerId (same date, same salesman)
Unplanned = actual CustomerId NOT IN planned CustomerId

Planned sequence = effective plan ORDER BY NoUrut
Actual sequence = earliest check-in per customer ORDER BY CheckInTime
```

### 4.4 KPI formulas (Release 1 — daily grain)

| KPI | Formula |
| --- | --- |
| Planned Visits | `COUNT(DISTINCT CustomerId)` on effective visit plan |
| Actual Visits | `COUNT(DISTINCT CustomerId)` on deduped check-ins (earliest per customer) |
| Effective Calls | `COUNT(DISTINCT CustomerId)` where ≥1 matching order same date |
| Missed Visits | `Planned − Actual` (set difference count) |
| Unplanned Visits | Check-in customers not on effective plan |
| Visit Execution % | `Actual ÷ Planned × 100`; **null display as N/A** when Planned = 0 |
| Effective Call Rate | `Effective ÷ Actual × 100`; N/A when Actual = 0 |
| Coordinate Coverage % | Planned stops with `(lat ≠ 0 OR lng ≠ 0)` ÷ Planned × 100 |

Team-level aggregation (sum numerators / sum denominators) is **Release 2** per Q8.

---

## 5. Impact Analysis

### 5.1 Affected modules

| Layer | Module | Change |
| ----- | ------ | ------ |
| Application | `ReportingContext/DashboardFieldActivityAgg/` | **New** — composer, queries, contracts, DTOs, `GpsValidationClassifier` |
| Infrastructure | `ReportingContext/DashboardFieldActivityAgg/FieldActivityDal.cs` | **New** — SQL for check-ins, orders, customer coords |
| Infrastructure | Register visit-plan + check-in DALs in `InfrastructurePortalExtensions` | `IEffectiveVisitPlanDal`, `IVisitPlanDal`, `IVisitPlanExceptionDal`, `IEffectiveVisitPlanResolver` |
| API | `Controllers/Dashboard/FieldActivityDashboardController.cs` | **New** |
| API | `ApplicationPortalExtensions.cs` | Register MediatR handlers |
| Frontend | `router/index.ts`, `MainLayout.vue` | Route + nav **Field Activity** |
| Frontend | `views/dashboard/FieldActivityDashboardView.vue` | **New** — Control Tower shell |
| Frontend | `components/field-activity/*` | Map, KPI strip, timeline, replay, missed list |
| Frontend | `api/fieldActivityApi.ts`, `models/fieldActivity.ts` | **New** |
| Frontend | `package.json` | Add `maplibre-gl` |
| SQL | `Scripts/Seed_FieldActivity_Demo.sql` | **New** — dev/staging only |
| SQL | `Scripts/Upgrade_M18_5_FieldActivity_Index.sql` | **Optional** — `BTR_CheckIn` index for salesman-day queries |
| Tests | `FieldActivityComposerTest.cs` | KPI, dedupe, missed/unplanned, N/A, GPS class |
| Tests | `GpsValidationClassifierTest.cs` | Band boundaries |
| Docs | `docs/features/btr-portal/btr-portal-domain.md` | Post-delivery Field Activity section |

### 5.2 Unaffected modules

| Module | Reason |
| ------ | ------ |
| `btr.portal.worker`, `BTRPD_*` refresh pipeline | Live-query Release 1 |
| M18 `DashboardSalesmanAggregator`, `BTRPD_Salesman*` | Separate milestone; no visit KPIs |
| M22 Location dashboard | Warehouse/Wilayah inventory — different domain |
| Visit plan worker, SM4/SM7 Desktop forms | Portal read-only |
| BTrade3 sync (`j07-btrade-sync`) | No mobile changes |
| Alert Center (`DashboardAlertCenterComposer`) | Release 2 integration |

### 5.3 Downstream consumers (future)

| Consumer | Release 1 impact | Future |
| -------- | ---------------- | ------ |
| M25 Sales Force Effectiveness | None | Consumes Visit Execution %, Effective Call Rate, Unplanned Rate, GPS Suspicious % (Q20) |
| M18 salesman dashboard | None | Optional single summary card (Q19) |
| M23 Alert Center | None | Unplanned visit attention signal (Q18) |

### 5.4 Performance note

Analysis flags `BTR_CheckIn` indexes as commented out in DDL. Release 1 queries filter by `CheckInDate` + `UserEmail` (single day, single salesman). Expected row volume is low per request. Include optional index script; monitor API latency in staging before mandating production index deploy.

---

## 6. Database Design

### 6.1 Release 1 — no new `BTRPD_*` tables

Field Activity uses operational tables only:

| Table | Role |
| ----- | ---- |
| `BTR_VisitPlan` | Materialized planned visits |
| `BTR_VisitPlanException` | Exception overlay |
| `BTR_CheckIn` | Actual visits + GPS |
| `BTR_Order` | Effective call detection |
| `BTR_Customer` | Master coordinates for planned pins |
| `BTR_SalesPerson` | `Email` identity bridge |

### 6.2 Optional performance index

```sql
-- Upgrade_M18_5_FieldActivity_Index.sql (idempotent)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_BTR_CheckIn_CheckInDate_UserEmail'
      AND object_id = OBJECT_ID(N'dbo.BTR_CheckIn'))
CREATE NONCLUSTERED INDEX IX_BTR_CheckIn_CheckInDate_UserEmail
    ON BTR_CheckIn (CheckInDate, UserEmail)
    INCLUDE (CustomerId, CheckInTime, CheckInLatitude, CheckInLongitude,
             CustomerLatitude, CustomerLongitude, Accuracy);
```

### 6.3 Demo seed script (dev/staging only)

`Scripts/Seed_FieldActivity_Demo.sql`:

- Target: 1–2 `BTR_SalesPerson` rows with valid `Email`, existing `BTR_SalesRute` / visit-plan rows for a fixed demo date (e.g. yesterday).
- 8–12 customers with realistic non-zero Jakarta-area coordinates.
- `BTR_VisitPlan` rows for demo date (or rely on visit-plan worker horizon).
- `BTR_CheckIn` rows: mix of on-plan, missed (planned customer with no check-in), unplanned (check-in off-plan), varying GPS distances (valid/warning/suspicious).
- `BTR_Order` rows for subset of check-ins (effective calls).
- Script must be **idempotent** and guarded by `@EnableFieldActivityDemoSeed = 1` variable.
- Document in script header: **never run in production**.

---

## 7. Backend Implementation

### 7.1 New DAL contracts

```csharp
// IFieldActivityCheckInDal.cs
public interface IFieldActivityCheckInDal
{
    IReadOnlyList<FieldActivityCheckInRow> ListBySalesPersonDate(
        string salesPersonEmail, DateTime visitDate);
}

public class FieldActivityCheckInRow
{
    public string CheckInId { get; set; }
    public string CustomerId { get; set; }
    public string CustomerCode { get; set; }
    public string CustomerName { get; set; }
    public string CheckInTime { get; set; }       // HH:mm:ss — earliest per customer
    public double CheckInLatitude { get; set; }
    public double CheckInLongitude { get; set; }
    public double CustomerLatitude { get; set; }    // snapshot on check-in row
    public double CustomerLongitude { get; set; }
    public float Accuracy { get; set; }
}
```

**SQL dedupe rule (earliest check-in per customer):**

```sql
WITH Ranked AS (
    SELECT
        aa.CheckInId, aa.CustomerId, aa.CustomerCode, aa.CustomerName,
        aa.CheckInTime, aa.CheckInLatitude, aa.CheckInLongitude,
        aa.CustomerLatitude, aa.CustomerLongitude, aa.Accuracy,
        ROW_NUMBER() OVER (
            PARTITION BY aa.CustomerId
            ORDER BY aa.CheckInTime ASC, aa.CheckInId ASC
        ) AS rn
    FROM BTR_CheckIn aa
    WHERE aa.CheckInDate = @VisitDate
      AND aa.UserEmail = @UserEmail
)
SELECT ... FROM Ranked WHERE rn = 1
ORDER BY CheckInTime ASC, CustomerId ASC
```

```csharp
// IFieldActivityOrderDal.cs — effective-call existence
public interface IFieldActivityOrderDal
{
    IReadOnlySet<string> ListCustomerIdsWithOrder(
        string salesPersonEmail, DateTime visitDate);
}
```

```sql
SELECT DISTINCT CustomerId
FROM BTR_Order
WHERE OrderDate = @VisitDate AND UserEmail = @UserEmail
```

```csharp
// ICustomerCoordinateDal.cs
public interface ICustomerCoordinateDal
{
    IReadOnlyDictionary<string, CustomerCoordinateRow> ListByCustomerIds(
        IEnumerable<string> customerIds);
}
```

### 7.2 `GpsValidationClassifier`

Extract RO1 Haversine from `CheckInView` into application-layer utility:

```csharp
public static class GpsValidationClassifier
{
    public static GpsValidationClass Classify(
        double checkInLat, double checkInLng,
        double customerLat, double customerLng,
        float accuracy)
    {
        if (IsZeroCoord(checkInLat, checkInLng) && IsZeroCoord(customerLat, customerLng))
            return GpsValidationClass.Invalid;
        if (IsZeroCoord(checkInLat, checkInLng) || IsZeroCoord(customerLat, customerLng))
            return GpsValidationClass.Suspicious;

        var distanceMeters = HaversineMeters(checkInLat, checkInLng, customerLat, customerLng);

        if (distanceMeters > 100 || accuracy > 50)
            return GpsValidationClass.Suspicious;
        if (distanceMeters > 50 || accuracy > 30)
            return GpsValidationClass.Warning;
        return GpsValidationClass.Valid;
    }
}
```

Enum: `Valid`, `Warning`, `Suspicious`, `Invalid`.

### 7.3 `FieldActivityComposer`

Orchestration steps:

1. Resolve `BTR_SalesPerson` by `salesPersonId`; require non-empty `Email` — return `400` if missing (field-enabled rep gate).
2. Load effective visit plan via `IEffectiveVisitPlanDal.ListEffectivePlan(salesPersonId, visitDate)`.
3. Load customer coordinates for all planned customer IDs.
4. Load deduped check-ins by `Email + visitDate`.
5. Load order customer IDs for effective-call set.
6. Classify each stop:
   - **PlannedVisited** — on plan and checked in
   - **PlannedMissed** — on plan, no check-in
   - **Unplanned** — check-in not on plan
7. Build KPI block from counts.
8. Build `plannedStops` with `NoUrut`, coords, `HasCoordinates`.
9. Build `actualStops` with time-order sequence number, `IsEffectiveCall`, `GpsValidationClass`, `DistanceMeters`.
10. Build `missedVisits` list (include `NoCoordinates` flag when master coords zero).
11. Build `routeGeometry`:
    - `plannedLine`: ordered coords from planned stops with coordinates (GeoJSON LineString)
    - `actualLine`: ordered coords from actual stops with coordinates
12. Set `meta.planDataAvailable` when `visitDate >= visitPlanGoLive` (read from `BTR_VisitPlan` MIN materialized date or config constant `VISIT_PLAN_GO_LIVE_DATE` in `appsettings`).

### 7.4 API contracts

**`GET /api/dashboard/field-activity`**

| Query param | Required | Description |
| ----------- | -------- | ----------- |
| `salesPersonId` | Yes | Selected salesman |
| `visitDate` | Yes | `yyyy-MM-dd` |

**`GET /api/dashboard/field-activity/salesmen`**

Returns salesman selector list:

```json
{
  "Items": [
    { "SalesPersonId": "...", "SalesPersonName": "...", "Email": "...", "HasEmail": true }
  ]
}
```

Filter: include all salesmen; UI may visually de-emphasize reps without `Email` (cannot load field data). Do not hide — supervisor may need to see gap.

**`FieldActivityResponse` (core fields):**

```json
{
  "SalesPersonId": "...",
  "SalesPersonName": "...",
  "VisitDate": "2026-06-11",
  "Kpis": {
    "PlannedVisits": 12,
    "ActualVisits": 9,
    "EffectiveCalls": 6,
    "MissedVisits": 3,
    "UnplannedVisits": 1,
    "VisitExecutionPercent": 75.0,
    "EffectiveCallRate": 66.7,
    "CoordinateCoveragePercent": 91.7,
    "GpsValidCount": 7,
    "GpsWarningCount": 1,
    "GpsSuspiciousCount": 1
  },
  "PlannedStops": [
    {
      "CustomerId": "...",
      "CustomerCode": "...",
      "CustomerName": "...",
      "NoUrut": 1,
      "Latitude": -6.2,
      "Longitude": 106.8,
      "HasCoordinates": true,
      "VisitStatus": "Visited"
    }
  ],
  "ActualStops": [
    {
      "CustomerId": "...",
      "Sequence": 1,
      "CheckInTime": "08:15:00",
      "Latitude": -6.2,
      "Longitude": 106.8,
      "HasCoordinates": true,
      "VisitStatus": "Visited",
      "IsEffectiveCall": true,
      "GpsValidation": "Valid",
      "DistanceMeters": 12.4
    }
  ],
  "MissedVisits": [
    {
      "CustomerId": "...",
      "CustomerName": "...",
      "NoUrut": 4,
      "HasCoordinates": false
    }
  ],
  "RouteGeometry": {
    "Planned": { "type": "LineString", "coordinates": [[lng, lat], ...] },
    "Actual": { "type": "LineString", "coordinates": [[lng, lat], ...] }
  },
  "Meta": {
    "PlanDataAvailable": true,
    "VisitPlanGoLiveDate": "2026-03-01",
    "QueriedAt": "2026-06-11T10:30:00"
  }
}
```

Use `null` for percent fields when denominator is zero; frontend displays **N/A**.

### 7.5 Portal DI registration

Add to `InfrastructurePortalExtensions.AddInfrastructurePortal`:

```csharp
services.AddScoped<IVisitPlanDal, VisitPlanDal>();
services.AddScoped<IVisitPlanExceptionDal, VisitPlanExceptionDal>();
services.AddScoped<IEffectiveVisitPlanResolver, EffectiveVisitPlanResolver>();
services.AddScoped<IEffectiveVisitPlanDal, EffectiveVisitPlanDal>();
services.AddScoped<ISalesPersonDal, SalesPersonDal>();
services.AddScoped<IFieldActivityCheckInDal, FieldActivityCheckInDal>();
services.AddScoped<IFieldActivityOrderDal, FieldActivityOrderDal>();
services.AddScoped<ICustomerCoordinateDal, CustomerCoordinateDal>();
services.AddScoped<IFieldActivityDal, FieldActivityDal>();
services.AddScoped<FieldActivityComposer>();
```

Add to `ApplicationPortalExtensions`:

```csharp
services.AddMediatR(typeof(GetFieldActivityQuery).Assembly);
```

### 7.6 Controller

```csharp
[Authorize]
[RoutePrefix("api/dashboard/field-activity")]
public class FieldActivityDashboardController : ApiController
{
    [HttpGet, Route("")]
    public async Task<IHttpActionResult> Get(string salesPersonId, DateTime visitDate) { ... }

    [HttpGet, Route("salesmen")]
    public async Task<IHttpActionResult> ListSalesmen() { ... }
}
```

Validate `salesPersonId` and `visitDate` required — return `400` with clear message if missing.

---

## 8. Frontend Implementation

### 8.1 Dependencies

```bash
npm install maplibre-gl
```

Add MapLibre CSS import in `FieldActivityMap.vue` or global `main.ts`.

**Tile source:** OpenStreetMap — use MapLibre demo style or self-hosted OSM raster tiles. Default:

```javascript
style: {
  version: 8,
  sources: {
    osm: {
      type: 'raster',
      tiles: ['https://tile.openstreetmap.org/{z}/{x}/{y}.png'],
      tileSize: 256,
      attribution: '© OpenStreetMap contributors'
    }
  },
  layers: [{ id: 'osm', type: 'raster', source: 'osm' }]
}
```

### 8.2 Routing and navigation

| Item | Value |
| ---- | ----- |
| Path | `/dashboard/field-activity` |
| Route name | `field-activity-dashboard` |
| Nav label | **Field Activity** |
| Nav position | After **Salesmen**, before **Collection** |
| Icon | `pi pi-map` |

### 8.3 Control Tower layout

```text
┌─────────────────────────────────────────────────────────────────────────┐
│ Toolbar: [Salesman ▼]  [Today] [Yesterday] [Custom Date]  [Load]       │
│          Data health: Coordinate Coverage 92%  ·  Plan data from Mar 2026 │
├──────────────┬──────────────────────────────────────┬───────────────────┤
│ LEFT (~20%)  │ CENTER MAP (~55–60%)                 │ RIGHT (~20–25%)   │
│ KPI strip    │  Legend: ■ Planned ■ Actual ■ Missed │ Visit timeline    │
│ (compact)    │  Layer toggles: Planned / Actual     │ ordered by time   │
│              │  Planned route: dashed cool (teal)   │                   │
│ Missed list  │  Actual route: solid warm (orange)   │ Replay controls   │
│ (scrollable) │  Pins: numbered planned / actual     │ [▶] speed slider  │
│              │  Missed: red outline                 │                   │
│ Link → M18   │  Unplanned: blue                     │                   │
└──────────────┴──────────────────────────────────────┴───────────────────┘
```

**Responsive:** On narrow viewports, stack panels vertically with map remaining largest block — map must not collapse to widget size.

### 8.4 Initial load behavior (Q4)

1. Page renders empty state: *Select a salesman and date, then click Load.*
2. Salesman dropdown populated on mount via `/salesmen`.
3. Date defaults to **Today**; no API call until user clicks **Load** (or re-load on filter change after first load).
4. Show loading overlay on map during fetch.

### 8.5 Map component (`FieldActivityMap.vue`)

Responsibilities:

- Initialize MapLibre map; `fitBounds` to visible stops on data load.
- Render GeoJSON sources:
  - `planned-route` — dashed line, cool color
  - `actual-route` — solid line, warm color
  - `planned-pins` — circle + `NoUrut` label
  - `actual-pins` — circle + sequence label; fill by visit status / GPS class
  - `missed-pins` — red outline, no fill
  - `unplanned-pins` — blue
- Layer toggle controls (always-visible legend per PO).
- Emit `stop-selected` for timeline sync.
- Hide stops where `HasCoordinates = false`.

**Visual spec (PO Section 10.1):**

| Element | Style |
| ------- | ----- |
| Planned polyline | Dashed/dotted, teal/blue |
| Actual polyline | Solid, orange/green |
| Planned pin label | `NoUrut` |
| Actual pin label | Check-in time order |
| Missed | Red outline pin at planned location |
| Unplanned | Blue pin |
| GPS Valid / Warning / Suspicious | Green / yellow / red pin accent |

### 8.6 Replay (`useFieldActivityReplay.ts`)

- Input: `actualStops` ordered by `CheckInTime`.
- Animation: move marker along straight segments between stops; **time-compressed** (not real-time duration).
- Speed slider: e.g. 0.5× – 4× (configurable range).
- Sync: highlight timeline row + map pin on current replay index.
- Controls: Play / Pause / Reset.
- PO Q14: adjustable speed required.

**Limitation message (tooltip/footer):** *Replay shows visit-to-visit segments, not driven road paths.*

### 8.7 KPI strip (`FieldActivityKpiStrip.vue`)

Seven cards in single row (wrap on narrow screens):

Planned · Actual · Effective · Missed · Unplanned · Execution % · Effective Call Rate

Use existing portal KPI card styling from `DashboardDetailLayout` patterns.

### 8.8 Cross-link to M18

In left panel footer:

```text
View sales performance →  [link to /dashboard/salesmen]
```

Future: pass `salesPersonId` query param if M18 supports focused rep (optional enhancement — not blocking).

### 8.9 TypeScript models

`src/models/fieldActivity.ts` — mirror API response types.

`src/api/fieldActivityApi.ts`:

```typescript
export async function getFieldActivity(salesPersonId: string, visitDate: string)
export async function listFieldActivitySalesmen()
```

---

## 9. Testing Strategy

### 9.1 Unit tests — `FieldActivityComposerTest`

| Case | Assert |
| ---- | ------ |
| All planned visited | Execution 100%; Missed 0 |
| Partial visits | Correct missed set |
| Unplanned check-in | Unplanned count + blue status |
| Duplicate check-ins same customer | Single actual; earliest time in sequence |
| Effective call | Order on same date marks effective |
| Order without check-in | Not counted as effective |
| Planned = 0 | `VisitExecutionPercent = null` |
| Actual = 0 | `EffectiveCallRate = null` |
| Zero coordinates | `HasCoordinates = false`; excluded from geometry |
| GPS bands | 40 m → Valid; 75 m → Warning; 150 m → Suspicious |

### 9.2 Unit tests — `GpsValidationClassifierTest`

Boundary values at 50 m, 100 m, accuracy 30 m, 50 m, zero coords.

### 9.3 Frontend tests

| Test | Scope |
| ---- | ----- |
| `fieldActivityApi.spec.ts` | API client param encoding |
| `useFieldActivityReplay.spec.ts` | Index progression, speed scaling |
| `router/navigation.spec.ts` | Route registered |

### 9.4 Manual test plan

1. Load demo seed salesman-day — verify map shows planned + actual overlays.
2. Verify missed customer appears in list and as red outline pin.
3. Verify unplanned visit — blue pin + KPI increment.
4. Run replay — timeline sync, speed slider.
5. Salesman without Email — selector shows; load returns meaningful error.
6. Date before visit-plan go-live — planned KPIs zero, meta flag set.
7. Cross-link to M18 navigates correctly.
8. Mobile-width viewport — map remains dominant.

---

## 10. Implementation Phases

### Phase 1 — Backend foundation

1. Create `DashboardFieldActivityAgg` application project folder structure.
2. Implement `GpsValidationClassifier`, DALs, `FieldActivityComposer`.
3. Add unit tests for composer and classifier.
4. Register DI; add controller + MediatR queries.
5. Optional index script.

**Exit criteria:** API returns correct KPIs and geometry for seeded salesman-day via Postman.

### Phase 2 — Frontend shell + data binding

1. Add route, nav, models, API client.
2. Build `FieldActivityDashboardView` with toolbar, empty state, load flow.
3. Implement KPI strip and missed list (no map yet).

**Exit criteria:** Load salesman-day shows KPIs and missed list.

### Phase 3 — Map and route visualization

1. Integrate MapLibre; render planned/actual polylines and status-colored pins.
2. Layer toggles and legend.
3. Fit bounds and coordinate coverage indicator.

**Exit criteria:** Planned vs actual route comparison visible — flagship visual acceptance.

### Phase 4 — Timeline and replay

1. Visit timeline panel ordered by check-in time.
2. Replay composable with speed slider; map/timeline sync.
3. GPS validation badges on timeline rows.

**Exit criteria:** Full demo script (Appendix C of analysis) runnable end-to-end.

### Phase 5 — Demo seed and documentation

1. Author `Seed_FieldActivity_Demo.sql`.
2. Update `btr-portal-domain.md` with Field Activity dashboard definition.
3. Update `btr-portal-deploy.md` with demo seed note (dev only).
4. Run manual test plan; fix defects.

**Exit criteria:** Fresh dev environment can demo without production check-in data.

---

## 11. Risks and Mitigations

| Risk | Impact | Mitigation |
| ---- | ------ | ---------- |
| Sparse customer coordinates | Map looks empty | Coordinate coverage KPI; missed list with "No Coordinates" badge; demo seed |
| Salesmen without Email | Cannot correlate check-ins | Show in selector; clear error on load; aligns with M18 PO field-enablement direction |
| Visit-plan history shallow | Pre-go-live dates show zero planned | `PlanDataAvailable` meta; no backfill (PO Q15) |
| Check-in discipline gaps | Low execution % | Intentional visibility flywheel — do not hide KPIs |
| `BTR_Order` not in `btr.sql` DDL | Deploy/schema drift | Query table directly (same as `EffectiveCallDal`); document dependency on BTrade3 sync |
| MapLibre tile usage | OSM tile policy | Use courteous tile URL; document rate limits; consider self-hosted tiles for production scale |
| Live query latency at scale | Slow map load | Optional `BTR_CheckIn` index; single-day scope limits rows |
| Effective call heuristic | Order without check-in not credited | Accepted RO3 semantics; not a bug |

---

## 12. Release 2 Backlog (not in this implementation)

| Capability | Notes |
| ---------- | ----- |
| Wilayah filter on salesman list | Concept C mentioned; defer per Release 1 scope table |
| 7-day / 30-day execution trend charts | Requires history accumulation |
| Team-level map (all salesmen) | Higher data volume; consider snapshot domain |
| `BTRPD_FieldActivity*` snapshot + worker | When team trends / Alert Center need refresh cadence |
| Alert Center unplanned visit signal | M23 integration (Q18) |
| Export replay snapshot | Presentation workflow |
| M18 Visit Execution summary card | One card max (Q19) |

---

## 13. Knowledge Curator Checklist (post-implementation)

- [ ] Add **Field Activity Dashboard** section to `docs/features/btr-portal/btr-portal-domain.md` (route, KPI definitions, layout, data source = live query).
- [ ] Add architecture note to `docs/features/btr-portal/btr-portal-architecture.md` (live-query dashboard pattern).
- [ ] Record M18.5 ↔ M18 ↔ M25 relationships in domain doc.
- [ ] Remove or archive this plan after knowledge sync per AGENTS.md temporary work rules.

---

## Appendix A — Demo Script Verification Checklist

Aligns with analysis Appendix C:

1. **"We plan the route"** — dashed planned polyline + numbered pins visible.
2. **"We execute and verify"** — solid actual polyline overlaid; divergence obvious.
3. **"We verify field presence with GPS"** — Valid/Warning/Suspicious pin colors.
4. **"We know who was missed"** — red missed pins + missed list.
5. **"We connect visits to orders"** — effective call indicated on stops/timeline.
6. **"We can replay the day"** — animation with speed control.

---

*End of implementation plan. Ready for Implementer handoff.*
