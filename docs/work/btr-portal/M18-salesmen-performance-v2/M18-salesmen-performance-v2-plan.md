# Implementation Plan: M18 — Salesmen Performance V2

## Document Status

| Field | Value |
| --- | --- |
| Initiative | M18 Salesmen Performance V2 — align Release 1 with PO decisions and deliver approved post–Release 1 enhancements |
| Authoritative requirements | `docs/work/btr-portal/M18-Salesmen-Performance-Analysis.md` — **Section 9 (Final Product Decisions)** |
| Prior delivery | M18 V1 implemented — `docs/work/btr-portal/M18 Salesman Performance - Implementation.md` |
| Related features | `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/sales-person-principal-target/feature.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md`, M17 Customer Analytics, M20 Collection Dashboard |
| Reference pattern | M14 Piutang V2 (incremental snapshot extension), M17 attention-first layout |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |
| Status | **Ready for implementation** |
| Product decisions | **Confirmed** — analysis Section 9, 2026-06-11 |

---

## 1. Goal

Evolve `/dashboard/salesmen` from M18 V1 (baseline salesman snapshot dashboard) into **Salesmen Performance V2** that:

1. **Closes Release 1 gaps** against authoritative PO decisions (attention signal rename, exposure thresholds, active-default view).
2. **Delivers approved within-M18 enhancements** — Principal Achievement drill-down and achievement trend via snapshot retention.
3. **Preserves V1 boundaries** — no visit execution KPIs (M18.5), no collection performance (M20), no executive changes.

**Primary outcomes:**

| Phase | Outcome |
| --- | --- |
| **V2.1 — Release alignment** | Rename `NoTarget` → `MissingTargetSetup`; High Piutang/Overdue Exposure use **Top 20%** configurable threshold; default UI shows **active salesmen only** with **Show Inactive Salesmen** toggle |
| **V2.2 — Principal drill-down** | Per-salesman Principal Achievement breakdown (target, omzet, achievement %) inside M18 — not a separate Principal Performance dashboard |
| **V2.3 — Achievement trend** | Month-over-month achievement trajectory per salesman via **snapshot retention** — not live multi-month Faktur queries |
| **V2.4 — Extended rankings** *(optional, lower priority)* | Top 10 by Invoice Count, Customer Count; New Customer KPI |

**Business question (unchanged):** *Is my sales force performing effectively?* — outcome-oriented, attention-first.

**Explicitly out of scope (PO confirmed — do not implement in V2):**

- Visit execution %, planned vs actual, visit-based coverage % → **M18.5**
- Collection amount, collection achievement, collection ranking → **M20**
- Field effectiveness, route compliance score → **M25**
- Separate Principal Performance dashboard, GPS/route replay, check-in investigation UI
- Changes to executive dashboard, Sales Dashboard Top 10, or other snapshot domains (except shared materialized-dashboard history pattern documentation)

---

## 2. Authoritative Product Decisions

Source: `M18-Salesmen-Performance-Analysis.md` Section 9. Do not re-decide during implementation.

### 2.1 Scope and presentation

| # | Decision | V2 action |
| - | -------- | --------- |
| Q1 | Principal Achievement is **salesman drill-down** within M18 | V2.2 |
| Q7 | M18 exposes **piutang exposure only** — no collection performance | No change (verify no regression) |
| Q9 | **Default:** active salesmen only; optional **Show Inactive Salesmen** | V2.1 |
| Q10 | Visit execution KPIs → M18.5 | Out of scope |

### 2.2 Target and achievement rules

| # | Decision | V2 action |
| - | -------- | --------- |
| Q5 | **SM6 wins:** `Monthly Target = SUM(Principal Targets)` when Principal rows exist; else legacy `BTR_SalesOmzetTarget` | **Already implemented** in `SalesOmzetTargetDal` — verify tests document rule; no code change unless regression found |
| Q4 | Achievement trend uses **snapshot retention** — not live multi-month queries | V2.3 |
| Q12 | Attention signal **Missing Target Setup** — planning gap distinct from Below Target | V2.1 (rename + relabel) |

### 2.3 Attention signal thresholds

| # | Decision | V2 action |
| - | -------- | --------- |
| Q8 | **High Piutang Exposure** = Top 20% of salesmen by open balance; **High Overdue Exposure** = Top 20% by overdue balance — **configurable**, not hardcoded nominal | V2.1 |

### 2.4 Customer metrics (deferred within M18)

| # | Decision | V2 phase |
| - | -------- | -------- |
| Q6 | **New Customer** = first transaction in company history; reactivated dormant excluded | V2.4 (optional) |

### 2.5 Approved attention list signals (updated labels)

| Signal key | Display label | Inclusion rule (V2) |
| ---------- | ------------- | ------------------- |
| `BelowTarget` | Below Target | Target exists (`> 0`) AND achievement % in Warning or Critical band (≥80% thresholds unchanged) |
| `MissingTargetSetup` | Missing Target Setup | Month activity (`OmzetAmount > 0` OR `CustomerCount > 0`) AND no configured target (null or `≤ 0`) |
| `HighOverdueExposure` | High Overdue Exposure | Rep in **Top N%** by overdue balance among reps with overdue balance `> 0` |
| `HighPiutangExposure` | High Piutang Exposure | Rep in **Top N%** by open balance among reps with open balance `> 0` |
| `CustomerConcentration` | Customer Concentration | `OmzetAmount > 0` AND top-customer % computable — informational (no % threshold) |
| `DormantCustomerPortfolio` | Dormant Customer Portfolio | `DormantCustomerCount > 0` via last-invoicing attribution, 90-day M17 rule |

**Migration note:** Replace signal key `NoTarget` with `MissingTargetSetup` in aggregator constants, snapshot rows, API, frontend filters, and tests. One-time data migration not required — snapshot is rebuilt on next refresh.

### 2.6 Active vs inactive visibility (Q9)

| Term | Definition |
| ---- | ---------- |
| **Active salesman** | ≥1 current-calendar-month Faktur (`SalesPersonId`) |
| **Inactive salesman** | No current-month Faktur |
| **Default view** | Show active salesmen only in attention list, rankings, and segmentation counts used for management focus |
| **Show Inactive Salesmen** | UI toggle reveals inactive reps in list/rankings; inactive with **zero omzet, zero open balance, and no attention signals** remain hidden unless toggle is on |
| **Snapshot storage** | Continue materializing **full rep universe** at refresh — filtering is read-side / presentation |

---

## 3. Current State vs V2 Delta

M18 V1 is **implemented** (`BTRPD_Salesman*` domain, worker, API, Vue page). Gap analysis from analysis Section 4:

| Capability | V1 state | V2 change |
| ---------- | -------- | --------- |
| SM6 target resolution | Implemented in `SalesOmzetTargetDal.ListTargetsForMonth` | Verify + document only |
| `MissingTargetSetup` signal | Uses `NoTarget` key/label | Rename (V2.1) |
| Exposure thresholds | Any rep with balance > 0 / any overdue customer | Top 20% configurable (V2.1) |
| Active-default view | All reps visible | Client filter + `IsActive` on rows (V2.1) |
| Principal Achievement | Not exposed | Drill-down panel + snapshot table (V2.2) |
| Achievement trend | Not available | Monthly rep history table (V2.3) |
| Top Invoice/Customer/Principal rankings | Not materialized | Optional V2.4 |
| New Customer KPI | Not computed | Optional V2.4 |
| Visit execution KPIs | Not in Portal | **M18.5** — do not implement |
| Collection performance | Exposure only | **M20** — do not implement |

---

## 4. Architecture Overview

### 4.1 Target topology (V2 additions highlighted)

```text
Source DALs (refresh time — unchanged base)
  IFakturViewDal
  IPiutangOpenBalanceWithSalesmanDal
  ICustomerLastFakturDal.ListLastFakturWithSalesmanByCustomer()
  ISalesPersonDal
  ISalesOmzetTargetDal.ListTargetsForMonth()          [SM6 sum — existing]
  ISalesPersonPrincipalTargetDal                      [NEW input — V2.2]
  IFakturPrincipalOmzetDal                            [NEW — V2.2 line-level omzet]
    ↓
RefreshDashboardSalesmanSnapshotWorker
    ↓ DashboardSalesmanAggregator                     [EXTEND — thresholds, signals, history, principal]
BTRPD_Salesman* (6 existing tables)                   [column rename in KPI only]
BTRPD_SalesmanPrincipalAchievement                    [NEW — V2.2]
BTRPD_SalesmanRepHistory                              [NEW — V2.3]
    ↓
GET /api/dashboard/salesmen                           [EXTEND — IsActive, filter hints]
GET /api/dashboard/salesmen/{salesPersonId}/principals   [NEW — V2.2]
    ↓
SalesmanDashboardView.vue                             [EXTEND — toggle, drill-down, trend]
```

**Design principles (unchanged from V1):**

- Read **source DALs** at refresh — do not compose from Sales/Piutang/Customer snapshot tables.
- `SnapshotKey = 'CURRENT'` delete-and-replace for current-month dashboard sections.
- **History table** uses period-keyed upsert — separate retention model from `CURRENT` replace.
- Attention Indicator presentation (M16/M17) — no per-signal severity engine.

### 4.2 Architecture decisions

| Decision | Choice | Rationale |
| -------- | ------ | --------- |
| Exposure threshold | **Percentile rank** among reps with metric > 0; default **Top 20%** | PO Q8; avoids hardcoded Rp thresholds; scales with team size |
| Threshold configuration | `DashboardSnapshotOptions.SalesmanExposureTopPercent` (default `20`) | Configurable without DB seed for v1; optional future `BTR_ParamSistem` if ops requests runtime change |
| Signal rename | Breaking change on `SignalKey` string only | Snapshot rebuilds every 30 min; SPA ships with API |
| Active filter | **Persist `IsActive` on snapshot child rows** + **default client filter** | Keeps API useful for Alert Center; consistent row metadata |
| Principal drill-down data | Materialize `BTRPD_SalesmanPrincipalAchievement` on same worker | Avoids live `FakturItem` scan on drill-down click |
| Principal omzet source | `FakturItem.Total` grouped by `Faktur.SalesPersonId` + `Brg.SupplierId` | Matches SM6 feature achievement rule; retur excluded in v1 |
| Achievement trend | `BTRPD_SalesmanRepHistory` keyed `(PeriodYear, PeriodMonth, SalesPersonId)` | PO Q4 — snapshot retention; one row per rep per month updated each refresh |
| Trend depth | **12 months** rolling display; retain history rows indefinitely | Aligns with M14 V2 deferred trend posture; storage cost low at rep count |
| Extended rankings | Defer to V2.4 unless implementer capacity allows same release | Lower management priority per analysis Section 7.2 |

---

## 5. Impact Analysis

### 5.1 Affected modules

| Layer | Module | Change |
| ----- | ------ | ------ |
| SQL | `BTRPD_SalesmanKpi.sql` | Rename `NoTargetCount` → `MissingTargetSetupCount` (upgrade script) |
| SQL | `BTRPD_SalesmanAttention.sql` | No schema change — `SignalKey` values change at refresh |
| SQL | `BTRPD_SalesmanTop*.sql`, `BTRPD_SalesmanSegmentation.sql` | Add `IsActive BIT` column (V2.1) |
| SQL | `BTRPD_SalesmanPrincipalAchievement.sql` | **New** (V2.2) |
| SQL | `BTRPD_SalesmanRepHistory.sql` | **New** (V2.3) |
| SQL | Upgrade script `Upgrade_M18_SalesmenPerformance_V2.sql` | Alter + create |
| Application | `DashboardSalesmanAggregator.cs` | Threshold logic, signal rename, principal + history builders |
| Application | `DashboardSalesmanAggregateResult.cs` | New collections; renamed count property |
| Application | `DashboardSnapshotOptions.cs` | `SalesmanExposureTopPercent` |
| Application | `IFakturPrincipalOmzetDal` + DTO | **New** (V2.2) |
| Application | `GetDashboardSalesmanQuery.cs` | `MissingTargetSetupCount`, `IsActive` on rows |
| Application | `GetSalesmanPrincipalAchievementQuery.cs` | **New** (V2.2) |
| Infrastructure | `DashboardSalesmanSnapshotDal.cs` | Read/write new tables + `IsActive` |
| Infrastructure | `FakturPrincipalOmzetDal.cs` | **New** (V2.2) |
| Infrastructure | `DashboardSalesmanDal.cs` | Map new fields; principal query |
| API | `SalesmanDashboardController.cs` | Principal sub-resource endpoint |
| Frontend | `salesmanAttentionSignals.ts` | Key/label rename |
| Frontend | `SalesmanDashboardView.vue` | Inactive toggle, principal panel, trend section |
| Frontend | New components | `SalesmanPrincipalBreakdown.vue`, `SalesmanAchievementTrend.vue` |
| Tests | `DashboardSalesmanAggregatorTest.cs` | Threshold, rename, principal, history cases |
| Docs | `btr-portal-domain.md`, `materialized-dashboard-domain.md` | Post-delivery knowledge sync |

### 5.2 Unaffected modules

| Module | Reason |
| ------ | ------ |
| `DashboardExecutiveComposer`, executive API | PO — no executive changes |
| `BTRPD_SalesTopSalesman`, Sales dashboard worker | Additive M18 domain only |
| Visit plan worker, `BTR_VisitPlan*` | M18.5 scope |
| Collection dashboard (`BTRPD_Collection*`) | M20 scope |
| BTR Desktop forms (SM4–SM7, RO2, FF2) | Portal read-only consumption |

### 5.3 Downstream consumers

| Consumer | Impact |
| -------- | ------ |
| Alert Center (`DashboardAlertCenterComposer`) | Update signal key references `NoTarget` → `MissingTargetSetup`; verify dedup rules |
| M24 investigation metadata | Update signal label map if hardcoded |
| Health / refresh admin | No new domain — same `Salesman` worker |

---

## 6. Database Design

Deploy via `Upgrade_M18_SalesmenPerformance_V2.sql` plus table scripts in `btr.sql/Tables/ReportingContext/`.

### 6.1 KPI column rename (V2.1)

```sql
-- BTRPD_SalesmanKpi: rename column (preserve data on upgrade)
EXEC sp_rename 'BTRPD_SalesmanKpi.NoTargetCount', 'MissingTargetSetupCount', 'COLUMN';
```

### 6.2 `IsActive` on child tables (V2.1)

Add to `BTRPD_SalesmanTopOmzet`, `BTRPD_SalesmanTopAchievement`, `BTRPD_SalesmanTopPiutang`, `BTRPD_SalesmanAttention`, `BTRPD_SalesmanSegmentation` (where row is rep-scoped):

| Column | Type | Description |
| ------ | ---- | ----------- |
| IsActive | BIT NOT NULL DEFAULT(0) | `1` when rep has ≥1 current-month Faktur |

Segmentation summary rows (`SegmentType = Activity`) already encode active/inactive counts — no `IsActive` on those aggregate rows.

### 6.3 `BTRPD_SalesmanPrincipalAchievement` (V2.2)

Grain: one row per `(SnapshotKey, SalesPersonId, SupplierId)` for current period.

| Column | Type | Description |
| ------ | ---- | ----------- |
| SalesmanPrincipalAchievementId | VARCHAR(26) PK | Generated ID |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| SalesPersonId | VARCHAR(13) | |
| SalesPersonCode | VARCHAR(20) | |
| SalesPersonName | VARCHAR(50) | |
| SupplierId | VARCHAR(13) | Principal key |
| SupplierName | VARCHAR(50) | Display |
| TargetAmount | DECIMAL(18,2) NULL | SM6 target for Principal; null if no row |
| CompletedOmzet | DECIMAL(18,2) | `SUM(FakturItem.Total)` for month |
| AchievementPercent | DECIMAL(9,4) NULL | `SalesOmzetChartAchievementPolicy.ComputePercent` |
| SortOrder | INT | Stable ordering (achievement % desc, then omzet desc) |

Unique: `(SnapshotKey, SalesPersonId, SupplierId)`

**Row set rule:** Include Principals from **union** of (a) SM6 targets for rep/month, (b) Principals with month omzet > 0. Omit Principals with zero target and zero omzet.

### 6.4 `BTRPD_SalesmanRepHistory` (V2.3)

Grain: one row per `(PeriodYear, PeriodMonth, SalesPersonId)` — updated each refresh during that calendar month.

| Column | Type | Description |
| ------ | ---- | ----------- |
| SalesmanRepHistoryId | VARCHAR(26) PK | Generated ID |
| PeriodYear | INT | |
| PeriodMonth | INT | |
| SalesPersonId | VARCHAR(13) | |
| SalesPersonCode | VARCHAR(20) | |
| SalesPersonName | VARCHAR(50) | |
| TargetAmount | DECIMAL(18,2) NULL | Resolved monthly target |
| CompletedOmzet | DECIMAL(18,2) | Month Faktur omzet |
| AchievementPercent | DECIMAL(9,4) NULL | |
| AchievementBand | VARCHAR(20) NULL | Healthy / Warning / Critical / Unknown |
| OpenBalance | DECIMAL(18,2) | All-time open at refresh time (context) |
| IsActive | BIT | Month activity flag |
| LastRefreshLogId | VARCHAR(26) | |
| UpdatedAt | DATETIME | Last upsert |

Unique: `(PeriodYear, PeriodMonth, SalesPersonId)`

**Retention:** Upsert on each Salesman refresh for current `(PeriodYear, PeriodMonth)`. Do **not** delete prior months. Trend UI reads last 12 distinct periods.

---

## 7. Backend Implementation

### 7.1 Exposure threshold algorithm (V2.1)

Add to `DashboardSalesmanAggregator`:

```csharp
private static HashSet<string> ResolveTopPercentByMetric(
    IEnumerable<RepState> reps,
    Func<RepState, decimal> metricSelector,
    decimal topPercent)
{
    var eligible = reps
        .Select(r => new { r.SalesPersonId, Value = metricSelector(r) })
        .Where(x => x.Value > 0)
        .OrderByDescending(x => x.Value)
        .ThenBy(x => x.SalesPersonId, StringComparer.OrdinalIgnoreCase)
        .ToList();

    if (eligible.Count == 0)
        return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    var take = Math.Max(1, (int)Math.Ceiling(eligible.Count * topPercent / 100m));
    return eligible.Take(take).Select(x => x.SalesPersonId)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);
}
```

Apply separately for `OpenBalance` (High Piutang) and `OverdueBalance` (High Overdue). Inject `topPercent` from `DashboardSnapshotOptions.SalesmanExposureTopPercent` via worker.

**Count cards:** `HighPiutangExposureCount` and `HighOverdueExposureCount` = size of respective top-percent sets (not all reps with balance > 0).

### 7.2 Signal constant rename (V2.1)

| Old | New |
| --- | --- |
| `SignalNoTarget = "NoTarget"` | `SignalMissingTargetSetup = "MissingTargetSetup"` |
| `NoTargetCount` | `MissingTargetSetupCount` |
| Label `"No Target"` | `"Missing Target Setup"` |

Update `DashboardSalesmanDal` attention card `PerformanceRequiresAttention` derivation:

`BelowTargetCount > 0 OR MissingTargetSetupCount > 0`

### 7.3 `IFakturPrincipalOmzetDal` (V2.2)

```csharp
public interface IFakturPrincipalOmzetDal
{
    IReadOnlyList<FakturPrincipalOmzetDto> ListOmzetBySalesPersonPrincipal(Periode periode);
}

public class FakturPrincipalOmzetDto
{
    public string SalesPersonId { get; set; }
    public string SupplierId { get; set; }
    public string SupplierName { get; set; }
    public decimal CompletedOmzet { get; set; }
}
```

**SQL pattern** (Infrastructure):

```sql
SELECT
    ISNULL(sp.SalesPersonId, '') AS SalesPersonId,
    ISNULL(sup.SupplierId, '') AS SupplierId,
    ISNULL(sup.SupplierName, '') AS SupplierName,
    SUM(ISNULL(fi.Total, 0)) AS CompletedOmzet
FROM BTR_Faktur f
INNER JOIN BTR_FakturItem fi ON f.FakturId = fi.FakturId
INNER JOIN BTR_Brg b ON fi.BrgId = b.BrgId
LEFT JOIN BTR_Supplier sup ON b.SupplierId = sup.SupplierId
LEFT JOIN BTR_SalesPerson sp ON f.SalesPersonId = sp.SalesPersonId
WHERE f.FakturDate BETWEEN @Tgl1 AND @Tgl2
  AND f.VoidDate = '3000-01-01'
GROUP BY sp.SalesPersonId, sup.SupplierId, sup.SupplierName
HAVING SUM(ISNULL(fi.Total, 0)) > 0
```

Load principal targets via `ISalesPersonPrincipalTargetDal.ListBySalesPersonPeriod` or batch list for month — prefer existing `SumByPeriod` + per-rep principal list method; add `ListByPeriod(int year, int month)` if not present.

### 7.4 Principal achievement builder (V2.2)

In aggregator, after rep-level metrics:

1. Build `principalTargets` map: `(SalesPersonId, SupplierId) → TargetAmount`
2. Build `principalOmzet` map from `IFakturPrincipalOmzetDal`
3. Union keys; compute achievement % per row
4. Emit `DashboardSalesmanPrincipalAchievementRow` list sorted per table spec
5. Persist via `DashboardSalesmanSnapshotDal.ReplacePrincipalAchievement`

### 7.5 Rep history upsert (V2.3)

At end of `Aggregate()`:

For each `RepState`, emit history row for `(PeriodYear, PeriodMonth, SalesPersonId)`.

`ReplaceCurrent` transaction:

1. Delete-and-replace six `CURRENT` tables (existing)
2. Delete-and-replace `BTRPD_SalesmanPrincipalAchievement` where `SnapshotKey = 'CURRENT'`
3. **Upsert** `BTRPD_SalesmanRepHistory` for current period rows (`MERGE` or delete-insert by period+rep set)

Do not truncate prior months.

### 7.6 API contracts

#### `GET /api/dashboard/salesmen` (extend)

```csharp
public class DashboardSalesmanAttentionCards
{
    public int MissingTargetSetupCount { get; set; }  // renamed from NoTargetCount
    // ... unchanged fields
}

public class DashboardSalesmanAttentionItem
{
    public bool IsActive { get; set; }  // NEW
    // ...
}

public class DashboardSalesmanRankingRow
{
    public bool IsActive { get; set; }  // NEW
    // ...
}

public class DashboardSalesmanResponse
{
    public DashboardSalesmanFilterDefaults Filters { get; set; }  // NEW
}

public class DashboardSalesmanFilterDefaults
{
    public bool DefaultActiveOnly { get; set; } = true;
    public decimal ExposureTopPercent { get; set; }  // echo config for UI subtitle
}
```

#### `GET /api/dashboard/salesmen/{salesPersonId}/principals` (V2.2)

Returns `ApiResponse<SalesmanPrincipalAchievementResponse>`:

```csharp
public class SalesmanPrincipalAchievementResponse
{
    public string SalesPersonId { get; set; }
    public string SalesPersonName { get; set; }
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }
    public IList<SalesmanPrincipalAchievementRow> Principals { get; set; }
}

public class SalesmanPrincipalAchievementRow
{
    public string SupplierId { get; set; }
    public string SupplierName { get; set; }
    public decimal? TargetAmount { get; set; }
    public decimal CompletedOmzet { get; set; }
    public decimal? AchievementPercent { get; set; }
    public string AchievementBand { get; set; }
}
```

Read from `BTRPD_SalesmanPrincipalAchievement` snapshot — no live query.

#### `GET /api/dashboard/salesmen/{salesPersonId}/trend` (V2.3)

Query params: `months=12` (default, max 12).

Returns monthly `AchievementPercent`, `CompletedOmzet`, `TargetAmount` from `BTRPD_SalesmanRepHistory`.

---

## 8. Frontend Implementation

### 8.1 V2.1 — Active filter toggle

| Item | Change |
| ---- | ------ |
| `SalesmanDashboardView.vue` | Add `showInactiveSalesmen` ref default `false` |
| Filter helper | `filterActiveSalesmen<T extends { IsActive: boolean }>(rows, showInactive)` |
| Apply to | Attention list, all three ranking tables |
| UI control | PrimeVue `InputSwitch` or checkbox: **Show Inactive Salesmen** in page toolbar |
| Subtitle | When default: *Showing active salesmen only (current-month Faktur)* |

### 8.2 V2.1 — Signal rename

Update `salesmanAttentionSignals.ts`:

- `NoTarget` → `MissingTargetSetup`
- Label → `Missing Target Setup`
- Update `SalesmanAttentionCardGroup.vue` card label
- Update `salesmanAttentionSignals.spec.ts`

### 8.3 V2.2 — Principal drill-down

| Interaction | Behavior |
| ----------- | -------- |
| Trigger | Click salesman name in attention list or ranking row |
| Presentation | Slide-over panel or expandable row (match M17 customer detail pattern if exists) |
| Content | Table: Principal · Target · Omzet · Achievement % · band badge |
| Empty state | *No principal targets or sales this month* |
| Investigation | Optional link to Sales Report `?q={SalesPersonName}` — no new report route |

Component: `SalesmanPrincipalBreakdown.vue` — loads `fetchSalesmanPrincipals(salesPersonId)` on open.

### 8.4 V2.3 — Achievement trend

| Item | Detail |
| ---- | ------ |
| Placement | Below Performance Rankings or inside principal panel tab **Trend** |
| Chart | Simple line chart — Achievement % over last 12 months (PrimeVue Chart or existing dashboard chart component) |
| Empty | Fewer than 2 history months → show *Trend available after month-end snapshots accumulate* |
| Scope | Per salesman when opened from drill-down; optional team median deferred |

Component: `SalesmanAchievementTrend.vue`

### 8.5 Layout (updated section order)

1. Attention Cards  
2. **Toolbar:** Show Inactive toggle + period subtitle  
3. Attention List  
4. Performance Rankings  
5. Exposure Rankings  
6. **Salesman detail drawer** (Principal + Trend tabs) — V2.2/V2.3  
7. Segmentation Summary  
8. Navigation  

---

## 9. Testing

### 9.1 Unit tests — aggregator (extend `DashboardSalesmanAggregatorTest`)

| Case | Assertion |
| ---- | --------- |
| MissingTargetSetup rename | Signal key `MissingTargetSetup`; not `NoTarget` |
| HighPiutang top 20% | 10 reps with balance → exactly 2 in set (ceil 20%) |
| HighPiutang single rep | 1 rep with balance → that rep in set |
| HighPiutang zero balances | Empty set; count = 0 |
| HighOverdue top 20% | Same pattern on overdue balance |
| BelowTarget unchanged | 79% still Critical |
| Principal rows | SM6 target + omzet → correct achievement % |
| Principal omzet only | No target row → achievement null; row still listed |
| Rep history upsert | Two refreshes same month → one row updated |
| IsActive flag | Rep with month Faktur → `IsActive = true` on ranking row |

### 9.2 API / integration

1. Refresh Salesman domain; verify `MissingTargetSetupCount` in KPI.
2. Exposure counts drop vs V1 when many reps have small balances (threshold effect).
3. `GET .../principals` returns rows matching SM6 + FakturItem spot-check.
4. History table gains rows; trend endpoint returns ≤12 points.
5. Alert Center still resolves salesman signals after key rename.

### 9.3 Manual checklist

1. Default view hides inactive reps with zero activity/balance.
2. Toggle shows inactive reps in list and rankings.
3. Attention card **Missing Target Setup** filters correctly.
4. High exposure signals only for top 20% — spot-check with 5+ reps.
5. Principal panel opens from row click; achievement matches Desktop RO2 for sample rep/principal.
6. Trend chart shows current month after refresh; prior months after simulated history seed.
7. Executive and Sales Top 10 dashboards unchanged.

---

## 10. Risk Analysis

| Risk | Likelihood | Impact | Mitigation |
| ---- | ---------- | ------ | ---------- |
| Top 20% threshold surprises users accustomed to V1 broad exposure list | Medium | Medium | Document in operational guide; expose `ExposureTopPercent` in API for UI subtitle |
| Signal key rename breaks Alert Center / M24 mappings | Medium | Medium | Grep all `NoTarget` references; ship API + SPA + composer together |
| `FakturItem` scan increases refresh duration | Medium | Medium | Single grouped SQL; measure refresh log duration in staging; index `BTR_Faktur(FakturDate)` exists |
| Principal rows without SM6 targets inflate drill-down | Low | Low | Show omzet with null target; band = Unknown |
| History table growth | Low | Low | ~N reps × 12 months visible; indefinite retention acceptable per analysis |
| Active filter hides rep with piutang but no month sales | Medium | Medium | PO Q9 default is active-only; inactive with balance appears when **Show Inactive** enabled — document clearly |

---

## 11. Implementation Phases

Execute in order. Each phase should compile and pass tests before the next.

### Phase 1 — V2.1 Release alignment

1. Add `SalesmanExposureTopPercent` to `DashboardSnapshotOptions` + `appsettings.json`.
2. Implement top-percent threshold logic in aggregator.
3. Rename signal constant, labels, KPI column (`MissingTargetSetupCount`).
4. Add `IsActive` column + populate on snapshot child rows.
5. Extend API response fields; update `DashboardSalesmanDal` mapping.
6. Frontend: signal rename, inactive toggle, filter helpers.
7. Update Alert Center composer signal key if referenced.
8. Unit tests + manual V2.1 checklist.
9. SQL upgrade script for KPI column rename + `IsActive` columns.

### Phase 2 — V2.2 Principal Achievement drill-down

1. Add `IFakturPrincipalOmzetDal` + infrastructure SQL.
2. Add `BTRPD_SalesmanPrincipalAchievement` table + DAL read/write.
3. Extend aggregator + worker inputs.
4. Add `GET /api/dashboard/salesmen/{id}/principals`.
5. Frontend principal panel component + row click wiring.
6. Unit tests for principal achievement rows.

### Phase 3 — V2.3 Achievement trend

1. Add `BTRPD_SalesmanRepHistory` table.
2. Implement history upsert in snapshot DAL (within refresh transaction).
3. Add `GET /api/dashboard/salesmen/{id}/trend`.
4. Frontend trend chart in detail drawer.
5. Unit tests for history upsert.

### Phase 4 — V2.4 Extended rankings *(optional — implement only if approved after V2.1–V2.3)*

1. Materialize `BTRPD_SalesmanTopInvoiceCount`, `BTRPD_SalesmanTopCustomerCount` (or extend existing tables with ranking type).
2. New Customer KPI via `MIN(FakturDate)` per customer company-wide — attribute to first Faktur `SalesPersonId`; exclude reactivated dormant per Q6.
3. UI: additional ranking tabs below mandatory Top 10 trio.

### Phase 5 — Knowledge sync

1. Update `docs/features/btr-portal/btr-portal-domain.md` — M18 section: signals, thresholds, principal drill-down, trend, active filter.
2. Update `docs/features/materialized-dashboard/materialized-dashboard-domain.md` — Salesman history retention exception.
3. Write `docs/work/btr-portal/M18-salesmen-performance-v2/implementation-summary.md` on completion.

---

## 12. Acceptance Criteria

M18 V2 is complete when:

### V2.1 (required)

1. Signal key **`MissingTargetSetup`** replaces `NoTarget` in snapshot, API, and UI.
2. **High Piutang Exposure** and **High Overdue Exposure** include only reps in configurable **Top 20%** (default) by respective balance among reps with balance > 0.
3. Default view shows **active salesmen only**; **Show Inactive Salesmen** toggle reveals inactive reps.
4. SM6 target resolution remains correct (principal sum wins over legacy).
5. Alert Center and investigation flows updated for renamed signal.

### V2.2 (required)

6. Salesman row drill-down shows **Principal Achievement** table (target, omzet, achievement % per Principal).
7. Principal data materialized in `BTRPD_SalesmanPrincipalAchievement` — no live `FakturItem` query on drill-down.

### V2.3 (required)

8. `BTRPD_SalesmanRepHistory` persists per-rep monthly metrics; updated each refresh.
9. Trend view shows achievement % for up to **12 months** from history — not live multi-month Faktur queries.

### Non-regression (required)

10. Executive dashboard and `BTRPD_SalesTopSalesman` **unchanged**.
11. Visit execution and collection performance KPIs **not** added.
12. Aggregator unit tests pass; Salesman health/refresh domain unchanged.

### V2.4 (optional)

13. If delivered: New Customer and supplemental rankings match analysis Section 7.2 rules.

---

## 13. Deferred Work (other milestones)

| Topic | Milestone | Notes |
| ----- | --------- | ----- |
| Visit Execution %, planned vs actual, visit-based Coverage % | **M18.5** | Requires Effective Visit Plan + mandatory Email |
| Collection amount, achievement, ranking | **M20** | FF2 data path exists |
| Field effectiveness, route compliance | **M25** | Beyond M18.5 summary |
| Customer Penetration (canonical denominator) | **M25** | Business rule undefined |
| Team-level achievement trend on dashboard home | Future | Per-rep trend in V2.3 only |
| `BTR_ParamSistem` for exposure % | Future ops | `appsettings` sufficient for V2 |

---

## 14. Handoff Notes for Implementer

- **Section 2 is authoritative** — do not add visit, collection, or executive scope.
- **V2.1 can ship independently** — Phases 2–3 are separable PRs if needed.
- **Rename is breaking for `SignalKey` string** — coordinate Alert Center grep (`NoTarget` → `MissingTargetSetup`).
- **Top-percent threshold** applies only to High Piutang and High Overdue signals — Customer Concentration remains informational without % cutoff (unchanged from V1 PO Q22).
- **Missing Target Setup** activity rule unchanged: `OmzetAmount > 0 OR CustomerCount > 0`.
- **Principal achievement** uses `FakturItem.Total` — retur exclusion same as SM6 v1.
- **History upsert** must not delete prior months; only current period rows are replaced/merged.
- **Read source DALs in worker** — principal omzet is a new source DAL, not composed from Sales snapshot.
- **Pre-filter drill-down** still uses `SalesPersonName` in `?q=` for reports.
- After delivery, run Knowledge Curator pass on permanent docs (Phase 5).

---

*End of implementation plan. Ready for Implementer.*
