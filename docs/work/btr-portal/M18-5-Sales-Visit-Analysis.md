# M18.5 Sales Route & Visit Monitoring — Feasibility Analysis

**Status:** Analysis complete — Product Owner decisions recorded (Section 12). Ready for Architect.  
**Scope:** Business and feasibility analysis only. No implementation plans, database designs, API designs, or architecture.  
**Date:** 2026-06-11 (analysis) · Product Owner decisions recorded 2026-06-11  
**Role:** Analyst (`docs/agents/analyst-agent.md`)  
**System:** BTR Portal (`/dashboard/field-activity`) consuming BTR.Distrib operational data

**Business questions:**

- *Objective A — Operational Visibility:* Which customers were planned, visited, missed, and which visits produced sales?
- *Objective B — Visual Demonstration Value:* Can BTR Portal showcase route planning, GPS check-in, and field coverage in a visually impressive way for demos and owner presentations?

**Relationship to other milestones:**

| Milestone | Focus | Relationship to M18.5 |
| --------- | ----- | --------------------- |
| **M18** Salesman Performance | Outcome KPIs — achievement, piutang exposure, rankings | M18.5 complements M18 with **execution** lens; M18 may later consume a summarized Visit Execution score |
| **M18.5** (this analysis) | Operational visibility, visit execution, route monitoring, maps, replay | Primary scope |
| **M20** Collection Dashboard | Collection performance | Out of scope |
| **M25** Sales Force Effectiveness | Productivity composites, route compliance score, coaching signals | M18.5 provides foundation data; advanced scoring and coaching belong to M25 |

**Reference documents:** `docs/foundation/PRODUCT.md`, `docs/foundation/DOMAIN.md`, `docs/foundation/LANDSCAPE.md`, `docs/foundation/WORKFLOW.md`, `docs/features/visit-plan/feature.md`, `docs/features/sales-order/feature.md`, `docs/features/btr-portal/btr-portal-domain.md`, `docs/work/btr-portal/M18-Salesmen-Performance-Analysis.md`, `docs/investigations/sales-route-materialization-analysis.md`

---

## 1. Executive Summary

### Overall feasibility: **Medium–High for core visit KPIs · High for map and replay visuals · Medium for route compliance scoring**

BTR already captures the operational data needed to answer planned-vs-actual visit questions, reconstruct daily visit sequences, and render map-based field activity visualizations. The **Territory Execution Plan** (`BTR_VisitPlan` + `BTR_VisitPlanException`) now provides a stable dated planned-visit denominator — the critical foundation that was missing before materialization shipped.

**What works reliably today**

- **Planned visits (dated):** Materialized `BTR_VisitPlan` with exception overlay and `EffectiveVisitPlanResolver` — includes `NoUrut` sequence per salesman per date
- **Actual visits:** `BTR_CheckIn` with date, time, salesman email, customer, GPS coordinates, and accuracy
- **Effective calls:** Established join pattern in Desktop RO3 — `BTR_CheckIn` LEFT JOIN `BTR_Order` on same `CustomerId` + `UserEmail` + `CheckInDate`
- **Customer locations:** `BTR_Customer.Latitude/Longitude` plus coordinate snapshot on each check-in; location history in `BTR_CustomerLocHist`
- **GPS distance validation:** Haversine distance already computed in Desktop RO1 with operational color bands (≤50 m, 50–100 m, >100 m)
- **Route template:** `BTR_SalesRute` / `BTR_SalesRuteItem` / `BTR_HariRute` — recurring plan with visit sequence (`NoUrut`)
- **Identity bridge:** `BTR_SalesPerson.Email` ↔ `BTR_CheckIn.UserEmail` ↔ `BTR_Order.UserEmail` — approved as mandatory for field-enabled reps (M18 PO Q11)

**What is partially available**

- **Historical planned-visit depth:** Limited to visit-plan go-live date; no retroactive backfill of past plans (by design — template state before go-live is not recoverable)
- **Check-in completeness:** Depends on BTrade3 adoption discipline; not every customer interaction may produce a check-in
- **Coordinate coverage:** Customer master coordinates exist but coverage percentage is deployment-specific (many rows may still be `0,0`)
- **Portal map capability:** BTR Portal has Chart.js charts but **no map library** today — map features are data-feasible but represent new UI capability

**What is not available (limits expectations)**

- **Continuous GPS tracking:** Only point-in-time check-in coordinates — no breadcrumb trail between customers; route lines are inferred visit-to-visit segments, not driven paths
- **Direct order-to-check-in link:** No `CheckInId` on `BTR_Order` — effective call is inferred by date + customer + email match
- **Substitute salesman coverage:** Not modeled in visit plan v1
- **Route compliance score (composite):** Deferred to M25 — M18.5 can show raw compliance signals but not unified productivity scoring

### Alignment with dual objectives

| Objective | Assessment |
| --------- | ---------- |
| **A — Operational Visibility** | **Achievable** for supervisors and area managers once visit-plan rollout and check-in discipline are in place. Core KPIs (planned, actual, missed, effective call, execution %) are join-feasible with known risks. |
| **B — Visual Demonstration Value** | **Strong opportunity — explicit success criterion.** BTR differentiates from traditional distributor ERP reporting by combining route planning, dated visit schedules, GPS check-in, and map replay. This is the primary reason M18.5 exists as a separate milestone from outcome-focused M18. |

### Product Owner direction — visual impact as success criterion

Unlike most BTR Portal dashboards, M18.5 is expected to serve **owner presentations, product demonstrations, prospect showcases, and internal presentations**. Visual differentiation is an **explicit product objective**, not a nice-to-have.

**Approved evaluation rule:** A feature with Medium management value and Medium operational value may still be **Release 1** if demonstration value is Very High. Do not optimize solely for management KPI value.

**Approved operational flywheel:** Historically route planning was underutilized because management had no visibility. Exposing visit KPIs in Release 1 — even where current discipline is imperfect — is intentional:

```text
Dashboard Visibility → Management Attention → Operational Enforcement → Better Data Quality
```

Visit execution KPIs must **not** be postponed solely because current check-in or route-plan discipline is imperfect.

### Recommended posture (confirmed by Product Owner)

M18.5 is a **Control Tower dashboard with map as visual centerpiece** — not a traditional dashboard with a small map widget. The user should understand planned route, actual route, missed customers, and effective visits within seconds. KPI cards support the story; the map carries it. Deep route compliance scoring and coaching composites belong to M25.

---

## 2. Existing Data Inventory

### 2.1 Route planning (recurring template)

| Entity | Table | Key fields | Retention | Quality notes |
| ------ | ----- | ---------- | --------- | ------------- |
| Route day slot | `BTR_HariRute` | `HariRuteId`, `HariRuteName` (H11–H26, Minggu-1/2 × Mon–Sat) | Master — stable | 14-day cycle; Sunday excluded |
| Salesman route header | `BTR_SalesRute` | `SalesRuteId`, `SalesPersonId`, `HariRuteId` | **Current state only** — overwritten on SM4 save | No version history |
| Route customers | `BTR_SalesRuteItem` | `SalesRuteId`, `NoUrut`, `CustomerId` | **Current state only** | Sequence order preserved via `NoUrut` |
| Maintenance UI | SM4-Rute | — | — | Template editor; triggers future visit-plan regeneration |

**Analytics relevance:** Defines recurring customer universe and intended visit sequence per cycle day. Cannot answer historical "who was on route last month" without visit-plan materialization.

### 2.2 Materialized visit plan (dated operational schedule)

| Entity | Table | Key fields | Retention | Quality notes |
| ------ | ----- | ---------- | --------- | ------------- |
| Materialized plan | `BTR_VisitPlan` | `SalesPersonId`, `VisitDate`, `CustomerId`, `NoUrut`, `HariRuteId`, `PlanSource`, `MaterializedAt` | **Indefinite** for past dates (immutable); rolling horizon forward (default 90 days) | Unique per `(SalesPersonId, VisitDate, CustomerId)` |
| Plan exceptions | `BTR_VisitPlanException` | `ExceptionType` (Add/Remove/Replace), `CustomerId`, `ReplacementCustomerId` | Per exception row | Future dates only in SM7 |
| Effective plan | `EffectiveVisitPlanResolver` (computed) | Merged list with `NoUrut`, `Origin` | Derived at read time | Approved denominator for coverage % (M18 PO Q2) |
| Worker | `btr.visitplan.worker` | — | Daily horizon maintenance | Params: `ROUTE_CYCLE_ANCHOR_DATE`, `VISIT_PLAN_HORIZON_DAYS` |
| Maintenance UI | SM7-Jadwal Kunjungan | — | — | Supervisor exceptions (LEADR, SFKTR, SYSAD) |

**Schedule coverage:** Active for dates within `[go-live, today + horizon]`. Historical depth starts at materialization go-live — not retroactive.

**Route maintenance quality signals (observable):**

- Salesmen without `BTR_SalesRute` rows → zero planned visits
- Customers on route but suspended (`BTR_Customer.IsSuspend`) → still planned unless manually excepted
- Exception volume per salesman/date → schedule volatility indicator

### 2.3 Salesman check-in (actual visit)

| Entity | Table / source | Key fields | Retention | Quality notes |
| ------ | -------------- | ---------- | --------- | ------------- |
| Check-in record | `BTR_CheckIn` | `CheckInId`, `CheckInDate`, `CheckInTime`, `UserEmail`, `CustomerId`, `CheckInLatitude/Longitude`, `Accuracy`, `CustomerLatitude/Longitude` (snapshot), `StatusSync` | **Indefinite** — no purge policy discovered | Synced from BTrade3 via `j07-btrade-sync` |
| Mobile capture | BTrade3 `checkin_table` | Same shape | Local until sync | GPS captured at check-in moment |
| Desktop inquiry | RO1-CheckIn List | Distance column, color bands | Max 3-month period in UI | Joins `BTR_SalesPerson` via `Email` for `SalesName` |
| Indexes | `BTR_CheckIn` | PK on `CheckInId` only | — | Date/email/customer indexes exist but commented out — query performance risk at scale |

**Data completeness factors:**

- Check-in requires BTrade3 login → `SalesPerson.Email` mandatory (approved)
- Salesman may visit without checking in (phone order, drive-by)
- Multiple check-ins same customer same day possible — dedupe rule needed for KPI counts (recommend: count distinct `CustomerId` per day per salesman)

**Historical retention:** Full history retained in `BTR_CheckIn` — no archival policy found. Suitable for trend and replay features.

### 2.4 Customer coordinates

| Entity | Table | Key fields | Coverage | Quality notes |
| ------ | ----- | ---------- | -------- | ------------- |
| Customer master | `BTR_Customer` | `Latitude`, `Longitude`, `Accuracy`, `CoordinateTimestamp` | All customers have fields (default `0`) | Real coverage % is deployment-specific — requires data-quality audit |
| Location history | `BTR_CustomerLocHist` | `CustomerId`, `ChangeDate`, `Latitude`, `Longitude` | Audit trail of coordinate changes | Supports "coordinate was updated after visit" investigations |
| Check-in snapshot | `BTR_CheckIn.CustomerLatitude/Longitude` | Point-in-time customer coords at visit | Per check-in | Preserves coords even if master later changes — valuable for GPS validation |

**Coordinate coverage assessment:** Technically queryable (`Latitude != 0 OR Longitude != 0`) but no canonical coverage KPI exists today. Map visualizations require filtering zero-coordinates; dashboard should surface **Coordinate Coverage %** as a data-health signal.

### 2.5 Sales orders (effective call linkage)

| Entity | Table | Key fields | Link to visit | Quality notes |
| ------ | ----- | ---------- | ------------- | ------------- |
| Mobile order | `BTR_Order` | `OrderId`, `OrderDate`, `CustomerId`, `UserEmail`, `TotalAmount`, `StatusSync` | **Inferred** — same date + customer + email | Table exists in production via sync; not in `btr.sql` DDL project |
| Order–Faktur map | `BTR_OrderMap` | `OrderId` → `FakturId` | Downstream invoicing | Not needed for effective-call KPI |
| Effective call logic | `EffectiveCallDal` (RO3) | `COUNT(OrderId)` per check-in | `LEFT JOIN` on `CustomerId`, `UserEmail`, `CheckInDate` | No `CheckInId` on order — heuristic match |
| Mobile order | BTrade3 `order_table` | Same fields + customer coords | Created independently of check-in | Order without prior check-in possible |

**Effective call definition (approved — M18 PO Q3):** Check-in resulting in ≥1 Sales Order on the **same calendar date**, same customer, same `UserEmail`.

**Risks:**

- Order captured without check-in → not an effective call but still a sale
- Multiple orders same day same customer → still one effective call (count check-in once)
- Cross-midnight edge cases negligible (date is `yyyy-MM-dd` string)

### 2.6 Salesman identity and territory

| Entity | Table | Key fields | Bridge role |
| ------ | ----- | ---------- | ----------- |
| Sales person | `BTR_SalesPerson` | `SalesPersonId`, `SalesPersonName`, `Email`, `WilayahId`, `SegmentId` | `Email` bridges plan (`SalesPersonId`) to check-in/order (`UserEmail`) |
| Territory | `BTR_Wilayah` (via `WilayahId`) | Wilayah name | Segmentation for territory map and coverage heatmap |

### 2.7 Existing Desktop analytics (not yet in Portal)

| Code | Screen | Capability | M18.5 reuse potential |
| ---- | ------ | ---------- | --------------------- |
| RO1 | Check-In List | Period filter, distance, color bands, Excel export | GPS validation rules, distance formula |
| RO3 | Effective Call | Check-in × order join, effective flag | Effective call KPI logic |
| SM7 | Jadwal Kunjungan | Effective plan grid | Planned visit source validation |

### 2.8 Portal platform context

| Aspect | Current state | M18.5 implication |
| ------ | ------------- | ----------------- |
| UI stack | Vue 3 + PrimeVue + Chart.js | Charts feasible; maps need new library (Leaflet, MapLibre, or Google Maps) |
| Data pattern | `BTRPD_*` snapshot tables + `btr.portal.worker` | Visit KPIs may be snapshot or live-query depending on performance |
| Map components | **None** | Greenfield visual capability |
| Related dashboard | M18 `/dashboard/salesmen` — outcome only | M18.5 is separate route; cross-link navigation encouraged |

---

## 3. KPI Feasibility Matrix — Group A: Visit Execution

**Approved definitions (authoritative — M18 PO Q2, Q3, Q10, Q11):**

| Term | Definition |
| ---- | ---------- |
| Planned Visit | Row on Effective Visit Plan for `(SalesPersonId, VisitDate, CustomerId)` |
| Actual Visit | Check-in for same salesman (via email) + customer + date |
| Effective Call | Check-in with ≥1 matching `BTR_Order` same date |
| Missed Visit | Planned customer with no matching check-in that date |
| Visit Execution % | Actual Visits ÷ Planned Visits (deduped per customer per day) |
| Coverage % | Visited Customers ÷ Planned Customers (Effective Visit Plan denominator) |

| KPI | Existing source | Missing source | Reliability | Feasibility | Mgmt value | Ops value | Demo value | Recommendation |
| --- | --------------- | -------------- | ----------- | ----------- | ---------- | --------- | ---------- | -------------- |
| **Planned Visits** | `BTR_VisitPlan` + `EffectiveVisitPlanResolver` | Pre-go-live history | Medium — depends on worker + SM4 maintenance | **Medium** | High | High | Medium | **Release 1** |
| **Actual Visits** | `BTR_CheckIn` | Email gaps on legacy reps | Medium — adoption dependent | **Medium** | High | High | Medium | **Release 1** |
| **Effective Calls** | `EffectiveCallDal` pattern (`BTR_CheckIn` + `BTR_Order`) | Portal exposure | Medium — heuristic join | **Medium** | High | High | Medium | **Release 1** |
| **Missed Visits** | Effective plan LEFT JOIN check-in | Dedupe rules, exception handling | Low–Medium | **Low–Medium** | High | High | Low | **Release 1** (list, not just count) |
| **Visit Execution %** | Derived | Stable denominator + numerator | Low–Medium | **Low–Medium** | High | High | Medium | **Release 1** |
| **Effective Call Rate** | Effective ÷ Actual | Same as above | Low–Medium | **Medium** | Medium | High | Medium | **Release 1** |
| **Unplanned Visits** | Check-ins NOT IN effective plan | — | Medium | **Medium** | Medium | High | Low | **Release 1** (attention signal) |
| **Zero-coordinate visits** | Check-ins where coords = 0 | — | High (detectable) | **High** | Low | Medium | Low | **Release 2** (data quality) |

**Dedupe rules (proposed for PO confirmation):**

- Count **one actual visit per customer per salesman per day** even if multiple check-ins exist (use earliest `CheckInTime` for sequence/replay)
- Planned visits: one row per customer per date on effective plan (already enforced by unique constraint)
- Visit Execution % at team level: `SUM(actual distinct) / SUM(planned distinct)` — not average of individual %

---

## 4. Route Monitoring Feasibility — Group B

| Capability | Existing source | Feasibility | Notes |
| ---------- | --------------- | ----------- | ----- |
| **Planned Route** | `BTR_VisitPlan.NoUrut` ordered customer list per date | **High** | Sequence is explicit in materialized plan |
| **Actual Route** | `BTR_CheckIn` ordered by `CheckInTime` per salesman per date | **High** | Time-based visit sequence |
| **Route Sequence (A→B→C)** | Join above — customer IDs + coordinates in order | **High** | Straight-line segments between visit points |
| **Planned vs Actual customer set** | Set comparison on customer IDs | **High** | Missed + unplanned detection |
| **Sequence compliance** | Compare `NoUrut` order vs check-in time order | **Medium** | Kendall tau / position mismatch — meaningful but noisy (salesman may reorder for traffic) |
| **Route deviation (geo)** | Distance from actual path to planned next customer | **Medium** | No continuous GPS — only visit-point deviation |
| **Route Compliance score** | Composite metric | **Low–Medium** | **Defer scoring to M25** — M18.5 shows evidence, not composite score |

**Can the system reconstruct Customer A → B → C?**

**Yes**, for actual route using `CheckInTime` ordering. For planned route using `NoUrut`. Map visualization connects consecutive visit coordinates with polylines.

**Can actual route be compared with planned route?**

**Yes** at customer-set level (visited/missed/unplanned). **Partially** at sequence level — order comparison is feasible but requires business tolerance for legitimate reordering.

**Can route deviations be detected?**

**Yes** for:

- Missed planned customers
- Unplanned extra visits
- Visit sequence inversion (planned #3 visited before #1)
- Large GPS distance from customer coordinate (RO1 pattern)

**No** for:

- Driven path deviation between customers (no intermediate GPS)
- Traffic-delay vs true deviation without time-window rules

| Feature | Mgmt value | Ops value | Demo value |
| ------- | ---------- | --------- | ---------- |
| Planned route overlay on map | Medium | High | **Very High** |
| Actual route polyline | Low | High | **Very High** |
| Side-by-side planned vs actual list | High | High | Medium |
| Sequence compliance indicator | Medium | Medium | Medium |
| Route deviation alerts | Medium | High | Low |

---

## 5. Customer Coverage Feasibility — Group C

| Metric | Formula / source | Feasibility | Management value |
| ------ | ---------------- | ----------- | ------------------ |
| **Visited Customers** | Distinct `CustomerId` in check-ins for period | **High** | High — daily/weekly pulse |
| **Unvisited Customers** | Planned NOT IN visited (per date or period) | **Medium** | High — action list for supervisors |
| **Coverage %** | Visited ÷ Planned (Effective Visit Plan) | **Medium** | High — primary compliance KPI |
| **Coverage by Salesman** | Group by `SalesPersonId` | **Medium** | High — ranking and coaching |
| **Coverage by Wilayah** | Join salesman/customer `WilayahId` | **Medium** | Medium — territory health |
| **Coverage Heatmap** | Geo-cluster visited vs planned pins | **High** (visual) | Medium — pattern spotting |
| **Period aggregation** | Week/month rollups | **Medium** | High — avoids single-day noise |

**Which coverage metrics provide management value?**

| Priority | Metric | Why |
| -------- | ------ | --- |
| 1 | Daily coverage % per salesman | Immediate supervisory action |
| 2 | Missed customer list (today + rolling 7 days) | Directly actionable — who to call back |
| 3 | Unplanned visit count | Detects route discipline vs ad-hoc selling |
| 4 | Wilayah heatmap (weekly) | Area manager territory review |
| 5 | Monthly coverage trend | Strategic — requires visit-plan history accumulation |

**Explicitly lower value for M18.5:** Coverage against "all customers in Wilayah" or "all customers with Faktur in 12 months" — those are outcome lenses (M18/M17), not execution lenses. Approved denominator is **Effective Visit Plan only** (M18 PO Q2).

---

## 6. GPS Validation Feasibility — Group D

**Existing implementation:** Desktop RO1 computes Haversine distance (meters) between `CheckInLatitude/Longitude` and `CustomerLatitude/Longitude` (snapshot at visit). Color bands:

| Distance | RO1 grid color | RO1 Excel color | Proposed classification |
| -------- | -------------- | --------------- | ----------------------- |
| ≤ 50 m | White (grid) / Light Green (Excel) | **Valid** |
| 50–100 m | Yellow | **Warning** |
| > 100 m | Red | **Suspicious** |

**Additional signals available:**

| Signal | Source | Classification impact |
| ------ | ------ | --------------------- |
| GPS accuracy > 50 m | `BTR_CheckIn.Accuracy` | Upgrade Warning → Suspicious |
| Zero coordinates | `0,0` on check-in or customer | **Invalid** — exclude from map, flag in list |
| Customer coord updated after visit | `BTR_CustomerLocHist` vs check-in date | Investigation note — not auto-invalid |

**Practical thresholds (recommended for PO approval):**

| Class | Rule |
| ----- | ---- |
| **Valid** | Distance ≤ 50 m AND accuracy ≤ 30 m AND coords non-zero |
| **Warning** | Distance 50–100 m OR accuracy 30–50 m |
| **Suspicious** | Distance > 100 m OR accuracy > 50 m OR zero coords |
| **Invalid** | Both check-in and customer coords zero |

**Feasibility:** **High** — logic exists in RO1; Portal can reuse same formula and bands.

| Feature | Mgmt value | Ops value | Demo value |
| ------- | ---------- | --------- | ---------- |
| Distance badge per visit | Medium | High | Medium |
| Valid/Warning/Suspicious breakdown KPI | Medium | High | **High** (trust story in demos) |
| Suspicious visit list | High | High | Medium |
| Map color-coded pins by validation class | Low | High | **Very High** |

---

## 7. Visualization Feasibility Matrix — Groups E & F

**Portal context:** No map library in `btr.portal.web` today. All map types require new frontend capability. Data for all types exists or is derivable from check-in + customer master + visit plan.

### 7.1 Map visualizations

| Map type | Data source | Business value | Visual appeal | Technical feasibility | Mgmt | Ops | Demo |
| -------- | ----------- | -------------- | ------------- | --------------------- | ---- | --- | ---- |
| **Visit Map** — customer markers per day | Check-in coords + customer name | High — see where field team went | **Very High** | **High** | Medium | High | **Very High** |
| **Route Map** — connected visit path | Check-ins ordered by time | Medium — verify route flow | **Very High** | **High** | Low | High | **Very High** |
| **Planned Route Overlay** — numbered pins + polyline | Visit plan `NoUrut` + customer coords | High — plan vs execution story | **Very High** | **High** | Medium | High | **Very High** |
| **Coverage Map** — green visited / red missed | Plan LEFT JOIN check-in | High — instant compliance picture | **Very High** | **High** | High | High | **Very High** |
| **Territory Map** — Wilayah boundary or aggregation | Customer/salesman `WilayahId` + coords | Medium — area manager view | High | **Medium** (boundary data may be approximate) | Medium | Medium | High |
| **Visit Density Heatmap** | Check-in coord clusters | Medium — hotspot identification | **Very High** | **High** (heatmap layer) | Low | Medium | **Very High** |
| **Effective Call Map** — marker style by order presence | Check-in + order join | Medium — productive visit geography | High | **High** | Medium | High | High |
| **GPS Validation Map** — color by Valid/Warning/Suspicious | Distance + accuracy | Medium — fraud/discipline signal | High | **High** | Medium | High | **Very High** |

### 7.2 Timeline and replay

| Feature | Data source | Feasibility | Mgmt | Ops | Demo |
| ------- | ----------- | ----------- | ---- | --- | ---- |
| **Daily visit timeline** — 08:10 A, 09:05 B… | `CheckInTime` | **High** | Medium | High | **Very High** |
| **Animated route replay** — marker moves along path | Times + coords | **High** | Low | Medium | **Very High** |
| **Split view: timeline + map sync** | Same | **High** | Medium | High | **Very High** |
| **Multi-day replay** | Date selector | **High** | Low | Medium | High |
| **Planned vs actual replay** — ghost planned route + actual animation | Plan + check-in | **High** | Medium | High | **Very High** |

**Replay limitations to set expectations:**

- Animation shows **visit-to-visit straight segments**, not road network routing
- Speed is time-compressed for demo (not real-time duration)
- No GPS positions between visits — marker jumps from customer to customer at check-in times

**Replay is the single highest demonstration-value feature** in M18.5. It should be treated as a first-class Release 1 deliverable, not a stretch goal.

### 7.3 Traditional charts (supporting visuals)

| Chart | Feasibility | Demo value | Recommendation |
| ----- | ----------- | ---------- | -------------- |
| Visit Execution % gauge | High (Chart.js) | Medium | Release 1 — KPI strip |
| Planned vs Actual vs Effective stacked bar | High | Medium | Release 1 |
| Coverage % trend (daily, 30 days) | Medium (needs history) | Medium | Release 1 or 2 |
| Top 10 salesman by execution % | High | Medium | Release 1 |
| Missed visit attention list | High | Low | Release 1 |

---

## 8. Visual Experience Assessment Summary

Features ranked by **demonstration value** (primary M18.5 differentiator):

| Rank | Feature | Mgmt | Ops | Demo |
| ---- | ------- | ---- | --- | ---- |
| 1 | **Animated daily route replay** | Low | Medium | **Very High** |
| 2 | **Coverage map** (visited/missed coloring) | High | High | **Very High** |
| 3 | **Planned + actual route overlay** | Medium | High | **Very High** |
| 4 | **GPS validation color-coded map** | Medium | High | **Very High** |
| 5 | **Visit density heatmap** | Low | Medium | **Very High** |
| 6 | **Timeline + map split view** | Medium | High | **Very High** |
| 7 | Visit execution % KPI cards | High | High | Medium |
| 8 | Missed customer list | High | High | Low |
| 9 | Sequence compliance score | Medium | Medium | Medium |
| 10 | Territory Wilayah map | Medium | Medium | High |

**Design principle:** Lead with map and replay in the default view. KPI cards support the narrative — they should not dominate the screen real estate.

---

## 9. Dashboard Concept Alternatives

### Concept A — Operations Dashboard

**Layout:** KPI cards (planned, actual, effective, missed, execution %) → missed visit table → salesman ranking by execution %.

| Aspect | Detail |
| ------ | ------ |
| **Audience** | Sales manager, supervisor — daily operational review |
| **Strengths** | Familiar Portal pattern (M16/M17/M18); fast to scan; actionable missed-visit list; lower implementation risk |
| **Weaknesses** | Low demonstration impact; indistinguishable from traditional BI; does not showcase BTR's route/GPS investment |

### Concept B — Map-Centric Dashboard

**Layout:** Full-width interactive map (70% viewport) with date + salesman selectors → planned route overlay → actual route polyline → visited/missed pin colors → optional replay button. Compact KPI strip above map.

| Aspect | Detail |
| ------ | ------ |
| **Audience** | Owner presentations, prospect demos, area managers |
| **Strengths** | **Maximum visual impact**; instantly communicates GPS + route planning value; strong product differentiation |
| **Weaknesses** | Requires map library integration; coordinate data quality exposed; less efficient for bulk missed-visit triage without supplementary list panel |

### Concept C — Control Tower Dashboard ✅ **Approved primary direction**

**Layout:** Three-panel view — left: KPI cards + missed visit list; **center: dominant map** (majority of viewport) with overlays; right: scrollable visit timeline with replay controls. Filters: date, salesman, Wilayah.

| Aspect | Detail |
| ------ | ------ |
| **Audience** | Supervisors (operations) + executives (demos) — dual-purpose |
| **Strengths** | Balances operational utility and visual impact; timeline sync with map is compelling in demos; supports investigate-then-act workflow |
| **Weaknesses** | Highest UI complexity; dense on smaller screens; more development effort than A or B alone |

**Product Owner constraint:** Map and route visualization must occupy **most of the screen**. Do not build a traditional dashboard with a small map widget. KPI cards support the narrative; the map is the visual centerpiece.

### Concept comparison

| Criterion | A — Operations | B — Map-Centric | C — Control Tower |
| --------- | -------------- | --------------- | ----------------- |
| Operational utility | **High** | Medium | **High** |
| Demonstration impact | Low | **Very High** | **Very High** |
| Implementation effort | Low | Medium | High |
| Differentiation from competitors | Low | **Very High** | **Very High** |
| Fit for M18.5 dual objectives | Partial (A only) | Partial (B only) | **Full (A + B)** |

**Approved direction:** **Concept C (Control Tower)** with map-dominant layout. Implement map + replay + planned/actual route comparison as the visual core; flank with compact KPI strip and missed-visit list.

---

## 10. Recommended M18.5 Release 1 Scope

Prioritized for **high visual impact**, **strong demonstration value**, **reasonable effort**, and **operational usefulness**.

### 10.1 Include in Release 1 (Product Owner confirmed)

| # | Capability | Rationale |
| - | ---------- | --------- |
| 1 | **Map-dominant Control Tower layout** at `/dashboard/field-activity` | Approved primary UX; map occupies most of viewport |
| 2 | **Coverage Map** — visited / missed / unplanned pin colors | **Primary view** — supervisor identifies gaps without reading tables |
| 3 | **Planned vs Actual route comparison** — distinct line styles/colors on same map | **Flagship visual** — expected to be one of the most compelling Portal features |
| 4 | **Planned route overlay** — numbered pins from `NoUrut` + planned polyline | "We plan the route" demo beat |
| 5 | **Actual route polyline** — ordered by earliest check-in time | "We verify execution" demo beat |
| 6 | **Animated daily route replay** with adjustable speed + timeline sync | **First-class Release 1** — exceptional demonstration value |
| 7 | **KPI strip** — Planned, Actual, Effective, Missed, Unplanned, Execution %, Effective Call Rate | Operational pulse; supports map story (not dominant) |
| 8 | **Missed visit list** with customer name; **"No Coordinates"** badge where applicable | Actionable for supervisors; zero-coord customers hidden from map |
| 9 | **GPS validation** — RO1 bands (≤50 m Valid, 50–100 m Warning, >100 m Suspicious) on list and pins | Consistency with Desktop RO1 |
| 10 | **Salesman selector** (required before load) + **Today / Yesterday / Custom Date** | Approved filter model |
| 11 | **MapLibre + OpenStreetMap** | Approved map provider — no Google licensing dependency |
| 12 | **Cross-link** to M18 salesman detail | Separate nav item from M18; future single summary card allowed |
| 13 | **Development demo seed data** | Realistic seeded data when operational data insufficient for presentation/testing |
| 14 | **Data health indicator** — coordinate coverage % for visible customers | Manages expectations in demos |

#### Planned vs Actual route comparison (additional Product Owner requirement)

The dashboard must provide a **visual side-by-side comparison** of planned route and actual route on the same map. This is expected to become one of the most visually compelling features in the entire Portal.

**Analyst design guidance for Architect/UI (business intent, not implementation):**

| Element | Planned route | Actual route |
| ------- | ------------- | ------------ |
| Polyline style | Dashed or dotted line | Solid line |
| Color | Cool tone (e.g., blue/teal) | Warm tone (e.g., orange/green) |
| Pin numbering | `NoUrut` sequence | Check-in time order |
| Missed customers | Planned pin, no actual visit — distinct "missed" color (e.g., red outline) | — |
| Unplanned visits | — | Distinct pin color (e.g., blue) — separate KPI and map layer |
| Legend | Always visible on map | Toggle layers: planned / actual / both |

User should grasp plan-vs-execution divergence within seconds of loading a salesman-day view.

### 10.2 Release 2 (near-term follow-up)

| Capability | Rationale |
| ---------- | --------- |
| Wilayah-level coverage heatmap | Area manager lens |
| 7-day / 30-day execution trend charts | Requires accumulated visit-plan history |
| Team-level aggregation (all salesmen map) | Higher data volume |
| Unplanned visit attention signal in Alert Center (M23) | Cross-milestone integration |
| Top 10 / Bottom 10 execution % rankings | Management comparison |
| Export / share replay snapshot for presentations | Demo workflow enhancement |

### 10.3 Defer to M25

| Capability | Rationale |
| ---------- | --------- |
| Route compliance composite score | Coaching metric — M25 scope |
| Productivity index (visits per hour, etc.) | Requires business rules beyond M18.5 |
| Sequence compliance scoring with tolerance bands | Advanced analytics |
| Principal-level visit effectiveness | Cross-dimensional — M25 |

### 10.4 Explicitly out of Release 1

| Capability | Reason |
| ---------- | ------ |
| Continuous GPS live tracking | Data does not exist |
| Road-network routing (Google Directions) | External dependency; not required for demo replay |
| Mobile app changes | M18.5 is Portal read-only |
| Visit plan editing (SM7) | Stays in Desktop |
| Collection route effectiveness | M20 scope |

---

## 11. Open Questions — Resolved

All open questions Q1–Q20 have been **resolved** by Product Owner — see [Section 12 — Approved Product Decisions](#12-approved-product-decisions-authoritative).

---

## 12. Approved Product Decisions (Authoritative)

**Status:** All open questions resolved by Product Owner — 2026-06-11.  
**Audience:** Architect — use this section as scope input. Do not re-decide business rules listed here.

### 12.1 Milestone positioning

| Decision | Rule |
| -------- | ---- |
| M18 scope | Outcome and management performance — achievement, exposure, rankings |
| M18.5 scope | Operational execution, route monitoring, visit execution, GPS visibility, visual demonstration |
| Visual impact | **Explicit success criterion** — demo/presentation value may justify Release 1 inclusion even when management KPI value is Medium |
| KPI exposure | Visit execution KPIs in Release 1 despite imperfect discipline — visibility drives enforcement |

### 12.2 Dashboard presentation

| # | Decision |
| - | -------- |
| Q1 | Portal route: **`/dashboard/field-activity`** |
| Q2 | Layout: **Concept C — Control Tower**; map is **dominant visual** (most of screen), not a small widget |
| Q3 | Default date: **Today**; quick selectors: Today · Yesterday · Custom Date |
| Q4 | Default salesman: **No automatic loading** — user must select salesman first |
| Q5 | M18 relationship: **Separate navigation item** (not a tab on `/dashboard/salesmen`) |

### 12.3 KPI and visit rules

| # | Decision |
| - | -------- |
| Q6 | Multiple check-ins same customer same day: **count as one actual visit**; use **earliest check-in** for route sequence |
| Q7 | Visit Execution % when Planned = 0: display **N/A** |
| Q8 | Release 1 aggregation: **daily only**; weekly/monthly → future enhancement |
| Q9 | Effective Call: **RO3 definition** — check-in with ≥1 `BTR_Order` same date, customer, email |
| Q10 | Unplanned Visit: **separate KPI** and **separate map pin color** |

**Release 1 KPI set (confirmed):** Planned Visit · Actual Visit · Effective Call · Missed Visit · Unplanned Visit · Visit Execution % · Effective Call Rate

### 12.4 GPS and map

| # | Decision |
| - | -------- |
| Q11 | Distance bands: **RO1 standards** — Valid ≤50 m · Warning 50–100 m · Suspicious >100 m |
| Q12 | Map provider: **MapLibre + OpenStreetMap** — avoid Google licensing dependency |
| Q13 | Zero-coordinate customers: **hide from map**; show in list with **"No Coordinates"** badge |
| Q14 | Replay speed: **adjustable** (slider) |

### 12.5 Data, demo, and integrations

| # | Decision |
| - | -------- |
| Q15 | Visit-plan history: **no backfill** required before go-live |
| Q16 | Demo readiness: **seed realistic demonstration data** in development when operational data is insufficient; demonstration quality prioritized during development |
| Q17 | Snapshot vs live query: **Architect decision** — no PO preference at analysis stage |
| Q18 | Alert Center integration: **Release 2** |
| Q19 | M18 summary card: **allowed in future** — one Visit Execution summary max; all detail stays in M18.5 |
| Q20 | M25 inputs: Visit Execution % · Effective Call Rate · Unplanned Visit Rate · GPS Suspicious % |

### 12.6 Flagship visual features (Release 1 — PO emphasized)

| Feature | Priority | Notes |
| ------- | -------- | ----- |
| **Route Replay** | First-class Release 1 | Exceptional demonstration value; primary differentiator |
| **Coverage Map** | Primary view | Visited / missed / unplanned without reading tables |
| **Planned vs Actual route comparison** | Flagship visual | Distinct line styles/colors on same map — among the most compelling Portal features |

**Planned vs Actual comparison intent:** Supervisor and demo audience must instantly see divergence between planned route sequence and actual check-in sequence. Architect and UI design must treat this as a hero interaction, not a secondary layer toggle.

---

## Appendix A — Join Path Reference (Business Level)

```text
SalesPersonId (plan)
    → BTR_SalesPerson.Email
        → BTR_CheckIn.UserEmail + CheckInDate + CustomerId     [Actual Visit]
        → BTR_Order.UserEmail + OrderDate + CustomerId         [Effective Call]

Effective Visit Plan (VisitDate, SalesPersonId)
    = BTR_VisitPlan filtered by date
    + BTR_VisitPlanException via EffectiveVisitPlanResolver

Missed Visit
    = Effective Plan CustomerId NOT IN Actual Visit CustomerId (same date, same salesman)

Route Sequence (actual)
    = BTR_CheckIn ordered by CheckInTime for (UserEmail, CheckInDate)

Route Sequence (planned)
    = Effective Visit Plan ordered by NoUrut for (SalesPersonId, VisitDate)
```

---

## Appendix B — Terminology

| Term | BTR meaning |
| ---- | ----------- |
| Actual Visit | Check-in at customer |
| Effective Call | Check-in with ≥1 sales order same day, same customer, same email |
| Effective Visit Plan | Materialized plan after exceptions |
| Planned Visit | Customer on effective visit plan for a date |
| Missed Visit | Planned but not checked in |
| Unplanned Visit | Checked in but not on effective plan |
| Visit Execution % | Actual visits ÷ planned visits (distinct customers per day) |
| Coverage % | Same as visit execution % when using plan denominator (M18 PO Q2) |
| Route Replay | Animated map playback of check-ins in time order |
| GPS Validation | Distance from check-in coordinate to customer coordinate |

---

## Appendix C — Differentiation Narrative (Demo Script Hooks)

For product demonstrations, M18.5 enables these story beats that traditional distributor systems rarely show:

1. **"We plan the route"** — Planned polyline + numbered pins (dashed/cool color)
2. **"We execute and verify"** — Actual polyline overlaid (solid/warm color) — planned vs actual divergence visible instantly
3. **"We verify field presence with GPS"** — Check-in pins with Valid/Warning/Suspicious coloring
4. **"We know who was missed"** — Coverage map: red/missed pins for planned-but-not-visited; blue for unplanned
5. **"We connect visits to orders"** — Effective call markers vs visit-only markers
6. **"We can replay the day"** — Animated timeline with adjustable speed — strongest visual moment in a presentation

**Demo data:** Development environments should include seeded salesman routes, check-ins, and orders sufficient to run the full demo script when production data is sparse (PO Q16).

---

*End of feasibility analysis. Product Owner decisions recorded in Section 12. Ready for Architect handoff.*
