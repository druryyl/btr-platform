# Implementation Plan: M23 — Alert Center

## Document Status

| Field | Value |
| --- | --- |
| Initiative | M23 Alert Center — **What requires attention right now across the entire business?** |
| Internal purpose | **Management attention aggregator** — consumes M17–M22 snapshot attention rows; does not define signals |
| Authoritative requirements | `docs/work/btr-portal/M23-Alert-Center-Analysis.md` — **Section 17 (Final Product Decisions)** |
| Alert catalog | `docs/features/btr-portal/ALERT-REGISTRY.md` — PO-governed SignalKey metadata |
| Related features | `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/btr-portal/btr-portal-architecture.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md` |
| Reference pattern | M16 `DashboardExecutiveComposer` (read-time composition); M17–M22 domain attention lists (producer snapshots) |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |
| Status | **Ready for implementation** |
| Product decisions | **Confirmed** — analysis Section 17, 2026-06-09 |
| Open questions | **None** — all PO decisions in Section 17; architect-resolved rules in Section 2.4–2.6 |

---

## 1. Goal

Deliver **Alert Center** at `/alerts` — a company-wide management attention view that **aggregates, prioritizes, and surfaces** attention signals already materialized by M17–M22 domain dashboards and selected M16/platform flags.

**Primary outcomes:**

- New route `/alerts`; page title **Alert Center**. **`/dashboard` (M16) remains landing page** — unchanged title and layout.
- **Read-time aggregator** — no new snapshot tables, refresh workers, or `SignalKey` definitions in M23.
- **Seven categories:** Sales · Customer · Collection · Inventory · Purchasing · Location · Platform.
- **Top 20 entity alerts per category** (producer priority sort within category).
- **M19 inventory risk = summary only** — KPI counts + link; no SKU rows in Alert Center.
- **Separate sections:** Platform (pinned) · Category summary · Alerts (exceptions) · Inventory risk summary · Concentrations (informational) · Domain navigation.
- **Deduplication** per analysis Section 12 / Section 17.3 (M20 wins customer overdue; LegacyDebt suppresses Dormant; M20 wins salesman workload).
- **Drill-down:** Alert row → owning domain dashboard → report (`?q=` pre-filter where producer provides entity name).
- M16 gains **Open Alert Center** affordance; sidebar adds **Alert Center** entry.
- Maintain **`ALERT-REGISTRY.md`** in sync with code metadata; PO approves new alert types.

**Explicitly out of scope (PO confirmed):**

- New `SignalKey` types or transactional SQL for alert qualification (Q30).
- Alert acknowledgment, history, snooze, assignment (Q25–Q26).
- Real-time / event-driven refresh after posting (Q27).
- Cross-domain priority scoring formulas (Q12).
- Generic Critical/Warning/Info severity engine (Q13) — except existing Sales Achievement bands.
- M16 Executive Dashboard layout or composer changes (Q16) — navigation link only on frontend.
- M16 Top 5 exposure lists in Alert Center (Q7).
- M19 item-level attention rows in feed (Q6).
- Desktop menu deep links (Q21).
- Role-based alert views (Q3).
- Operational vs Managerial tabs (Q18).
- Faktur Kembali aggregate, Retur, Effective Call, DSO, Tagihan pipeline, Sales Omzet Health, Stok Balance Health (Q29).

---

## 2. Authoritative Product Decisions

Source: analysis Section 17. Do not re-decide these rules during implementation.

### 2.1 Positioning and audience

| Decision | Value |
| --- | --- |
| Route | `/alerts` |
| Page title | **Alert Center** |
| Landing page | M16 `/dashboard` unchanged |
| Audience | All authenticated users — no RBAC in M23 |
| Cadence | Daily morning review — snapshot-based, not real-time |
| M16 relationship | Complementary — executive summary vs exception workspace |

### 2.2 Feed scope and volume

| Decision | Value |
| --- | --- |
| Entity alert cap | **Top 20 per category** |
| M19 in feed | **Summary KPI panel only** + link to `/dashboard/inventory-risk` |
| M16 Top 5 lists | **Excluded** — not alerts |
| Sections | **Alerts** (exceptions) vs **Concentrations** (informational) — separate |
| Phase 2 KPIs | **Allowed on M23** before M16 promotes them (Q15) |
| New SignalKeys | **Forbidden in M23** (Q30) |

### 2.3 Deduplication policy

| Situation | Rule |
| --- | --- |
| Customer overdue overlap (M17 vs M20) | **M20 wins** — one canonical customer row from M20 priority chain |
| M17 `Dormant` vs M20 `LegacyDebt` | **Suppress Dormant** — show LegacyDebt only |
| M17 `PlafondBreach` vs M20 `PlafondBreachOverdue` | **M20 wins** when overdue present; else M17 |
| M18 `HighOverdueExposure` vs M20 `HighOverdueWorkload` | **M20 wins** — suppress M18 row for same salesman |
| Different entity grains | **Not duplicates** — customer ≠ salesman ≠ warehouse ≠ principal |

### 2.4 Architect-resolved rules (delegated from analysis)

Analysis delegates cross-domain merge mechanics, API shape, and concentration sourcing to the architect. This plan resolves them.

#### 2.4.1 Producer → category mapping

Map each included row by `SignalKey` using `AlertCenterRegistry` (Section 6.3). Default mapping:

| Category | Signal sources |
| --- | --- |
| **Sales** | Synthetic `SalesAchievementWarning` / `SalesAchievementCritical` (from M11 sales snapshot + band resolver); M18 `BelowTarget`, `NoTarget`, `DormantCustomerPortfolio` |
| **Customer** | M17 `Overdue`, `Dormant`, `PlafondBreach`, `SuspendedWithSales` (after dedup) |
| **Collection** | M20 all attention rows (`ChronicOverdue`, `PlafondBreachOverdue`, `LegacyDebt`, `Overdue`, `HighOverdueWorkload`, `LowRecoveryVsBilling`, `WilayahHotspot`) |
| **Inventory** | M21 `PrincipalAtRiskExposure` only (entity alerts); M19 item signals **excluded** from entity feed |
| **Purchasing** | M21 `QualifiedBacklog`, `CompoundDependency`, `PrincipalInventoryNoPurchase`, `PurchasingInactivity`, `UnknownPrincipal` |
| **Location** | M22 `WarehouseInactiveWithStock`, `WarehouseNoSalesWithInventory` (exception signals only) |
| **Platform** | Synthetic `SnapshotStale`, `SnapshotDegraded`, `DomainUnavailable` |

**Concentrations section** (informational — not capped at 20):

| Source | Items |
| --- | --- |
| M18 attention rows | `CustomerConcentration`, `HighPiutangExposure` |
| M21 attention rows | `PrincipalSpendConcentration`, `PrincipalInventoryConcentration` |
| M22 attention rows | `WarehouseAtRiskConcentration`, `WarehouseInventoryConcentration`, `WarehouseSalesConcentration`, `WarehousePurchasingConcentration` |
| M17 KPI snapshot | `TopOmzetCustomerPercent`, `TopPiutangCustomerPercent` |
| M20 KPI snapshot | `RecoveryVsBillingPercent`, `OverdueConcentrationPercent` |
| M19 KPI snapshot | `AtRiskInventoryPercent` |
| M22 KPI snapshot | `Top1WarehouseInventoryPercent`, `Top1WarehouseAtRiskPercent`, `Top1WarehouseSalesPercent`, `Top1WilayahSalesPercent` |
| M16 executive compose | Piutang `TopCustomerPercent` when computable (from piutang snapshot via executive path — informational only) |

**Excluded from Concentrations:** M16 Top 5 exposure tables (Q7).

#### 2.4.2 Deduplication algorithm

Execute in `DashboardAlertCenterComposer` after loading producer rows, **before** category assignment and cap.

**Step 1 — Index M20 customer canonical rows**

```
For each M20 attention row where EntityType = "Customer":
  key = NormalizeEntityKey(EntityId, EntityCode)
  canonicalCustomer[key] = row   // M20 aggregator already applied signal priority — one row per customer max
```

**Step 2 — Filter M17 customer rows**

| M17 SignalKey | Suppress when |
| --- | --- |
| `Overdue` | `canonicalCustomer` contains same key (any M20 customer signal) |
| `PlafondBreach` | M20 row exists with `PlafondBreachOverdue` for same key |
| `Dormant` | M20 row exists with `LegacyDebt` for same key |
| `SuspendedWithSales` | Never suppressed by M20 overlap |

**Step 3 — Index M20 salesman workload rows**

```
For each M20 row where EntityType = "Salesman" AND SignalKey = "HighOverdueWorkload":
  workloadSalesman[key] = true
```

**Step 4 — Filter M18 salesman rows**

| M18 SignalKey | Suppress when |
| --- | --- |
| `HighOverdueExposure` | `workloadSalesman` contains same salesman key |
| All other M18 signals | Never suppressed by M20 |

**Entity key normalization:**

| Entity | Key |
| --- | --- |
| Customer | Trimmed `EntityId`; fallback `EntityCode` |
| Salesman | Trimmed `EntityId`; fallback `EntityCode` |
| Wilayah | Trimmed `EntityId`; fallback `EntityName` |
| Principal | Trimmed `EntityId`; fallback `EntityName` |
| Warehouse | Trimmed `EntityCode`; fallback `EntityName` |
| Company | Constant `"COMPANY"` |

#### 2.4.3 Sorting and cap

| Scope | Sort order |
| --- | --- |
| Within category (Alerts) | 1) Producer `SortOrder` asc (or registry priority when null) · 2) `ValueAmount` desc (nulls last) · 3) `EntityName` asc |
| Platform alerts | Always first section — order: `SnapshotDegraded` → `SnapshotStale` → `DomainUnavailable` |
| Per category | Take **first 20** after sort; expose `TotalCount` (pre-cap) and `DisplayedCount` (≤ 20) in category summary |
| Concentrations | Registry-defined display order (Section 6.3); **no cap** |
| Cross-domain | **No global merge** — categories rendered in fixed order: Sales → Customer → Collection → Inventory → Purchasing → Location |

#### 2.4.4 Synthetic platform and sales headline alerts

**Platform** (compose from same inputs as `DashboardExecutiveComposer`):

| SignalKey | Emit when | DashboardRoute |
| --- | --- | --- |
| `SnapshotDegraded` | `OverallHealthStatus == "degraded"` | `/dashboard` |
| `SnapshotStale` | `IsDataFresh == false` (any core domain stale per executive freshness rules) | `/dashboard` |
| `DomainUnavailable` | `HasUnavailableDomain == true` | `/dashboard` |

**Sales company achievement** (not a snapshot attention row):

| SignalKey | Emit when | Value |
| --- | --- | --- |
| `SalesAchievementCritical` | Sales snapshot available AND band = `Critical` | `AchievementPercent` + band |
| `SalesAchievementWarning` | Sales snapshot available AND band = `Warning` | `AchievementPercent` + band |

Use `ExecutiveSalesAchievementBandResolver` — **do not** duplicate band thresholds.

Re-use `DashboardExecutiveComposer` for platform/headline derivation **or** extract shared `DashboardSnapshotHealthComposer` helper called by both executive and alert center composers. **Prefer shared helper** to avoid drift; **must not** change executive API response shape.

#### 2.4.5 Inventory risk summary panel (M19)

Read `IDashboardInventoryRiskSnapshotDal.GetCurrent()` KPI fields only:

| Display field | Source |
| --- | --- |
| Dead Stock count + value | `DeadStockItemCount`, `DeadStockValue` |
| Slow Moving count + value | `SlowMovingItemCount`, `SlowMovingValue` |
| Never Sold count + value | `NeverSoldItemCount`, `NeverSoldValue` |
| At-Risk % | `AtRiskInventoryPercent` |
| Link | `/dashboard/inventory-risk` |

Do **not** read `AttentionList` from M19 snapshot for M23 feed.

#### 2.4.6 Navigation metadata per alert row

| Field | Source |
| --- | --- |
| `DashboardRoute` | `AlertCenterRegistry` per `SignalKey` — **authoritative** for M23 drill-down |
| `ReportRoute` | Producer row when present; null allowed (e.g. `WilayahHotspot`) |
| `EntityFilterQuery` | Entity display name for `navigateToReport` — CustomerName, SalesPersonName, WarehouseName, PrincipalName, WilayahName as applicable |

**CompoundDependency:** `DashboardRoute = "/dashboard/purchasing"` only (Q22) — no secondary dashboard link on row.

#### 2.4.7 Layout (fixed section order — analysis Section 18.1)

1. Page header + Last Refreshed + platform status chip  
2. **Platform Alerts** (pinned)  
3. **Alert Summary by Category** (count badges)  
4. **Alerts** — grouped by category, 20-row cap each  
5. **Inventory Risk Summary** (M19 KPI panel)  
6. **Concentrations** — informational KPIs + concentration attention rows  
7. **Navigation to Domain Dashboards**  

### 2.5 Data architecture

| Decision | Value |
| --- | --- |
| Snapshot tables | **None new** — read existing `BTR_PortalDashboard*Attention` and KPI tables |
| Refresh workers | **None new** — M23 is read-time only |
| Composition | `DashboardAlertCenterComposer` at API read — mirrors executive pattern |
| Read API | `GET /api/dashboard/alerts` |
| Caching | None in V1 — single composed response per request |
| Staleness | Reuse executive freshness + health semantics; show consolidated `LastRefreshed` = **min** `GeneratedAt` across snapshots read |

---

## 3. Architecture Overview

### 3.1 Target topology

```text
Existing snapshot tables (unchanged producers)
  BTR_PortalDashboardCustomerAttention      (M17)
  BTR_PortalDashboardSalesmanAttention      (M18)
  BTR_PortalDashboardInventoryRisk*         (M19 — KPI only for M23)
  BTR_PortalDashboardCollectionAttention    (M20)
  BTR_PortalDashboardPurchasingManagementAttention (M21)
  BTR_PortalDashboardLocationAttention      (M22)
  BTR_PortalDashboardSalesKpi / PiutangKpi / … (M11–M16 headlines)
  BTR_PortalDashboardRefreshLog             (platform health)
    ↓
Existing snapshot DALs (read only — no writes)
    ↓
DashboardAlertCenterDal
    ↓ DashboardAlertCenterComposer
      ↳ AlertCenterRegistry (SignalKey metadata — mirrors ALERT-REGISTRY.md)
      ↳ DashboardSnapshotHealthHelper (shared with executive — platform flags)
      ↳ Deduplication + category map + Top 20 cap
    ↓
GET /api/dashboard/alerts
    ↓ MediatR GetDashboardAlertCenterQuery
AlertCenterView.vue (/alerts)

Parallel unchanged paths:
  M17–M22 workers, aggregators, domain dashboard APIs
  M16 executive composer and /api/dashboard/executive
```

**Why read-time composition (no M23 snapshot domain):**

- PO Q30 — M23 must not redefine signals; all qualification already persisted by producers.
- Avoids duplicate refresh cadence and consistency problems between alert feed and domain dashboards.
- Alert feed always reflects latest producer snapshots at read time.

**Why `AlertCenterRegistry` in code:**

- `ALERT-REGISTRY.md` is the PO-governed document; application code needs typed metadata for category, section, dashboard route, and dedup hints.
- Registry entries are **alert types**, not instances — matches analysis Section 16.

### 3.2 Design principles

| Principle | Application |
| --- | --- |
| Consumer only | M23 reads producer snapshots — never queries `BTR_Faktur`, `BTR_Piutang`, etc. |
| Preserve ownership | Deduplication respects M20 > M17/M18 boundaries |
| Reuse UI patterns | Attention list DataTable, staleness banner, `navigateToReport`, `ExecutiveAttentionCard` for category summary |
| Simplicity | Single API endpoint; single composer; no alert history store |
| M16 unchanged | Backend executive path untouched; frontend adds navigation affordance only |
| Registry sync | Code registry + `ALERT-REGISTRY.md` updated together when PO approves changes |

### 3.3 Architecture decisions

| Decision | Choice | Rationale |
| --- | --- | --- |
| API route | `GET /api/dashboard/alerts` | Consistent with `api/dashboard/executive`, `api/dashboard/collection` |
| Application module | `DashboardAlertCenterAgg/` | Parallel to `DashboardExecutiveAgg` |
| Composer location | `DashboardAlertCenterComposer` in application | Testable pure logic |
| Registry | `AlertCenterRegistry.cs` static metadata | Enforces PO catalog at runtime |
| Health helper | Extract from executive composer if needed | Avoid duplicate freshness/degraded logic |
| Database DDL | **None** | Aggregator only |
| Worker changes | **None** | No new domain |
| Frontend route | `alerts` → `AlertCenterView.vue` | PO Q1 |
| Sidebar position | **Alert Center** after **Executive**, before **Sales** | High visibility for exception workspace |
| Store | Extend `dashboardStore` with `loadAlerts()` | Same pattern as domain dashboards |
| Category display | Accordion or stacked sections per category | Proposal A — no tabs (Q18) |

---

## 4. Impact Analysis

### 4.1 Affected modules

| Layer | Module | Change type |
| --- | --- | --- |
| Application | `DashboardAlertCenterAgg/Queries/GetDashboardAlertCenterQuery.cs` | **New** |
| Application | `DashboardAlertCenterAgg/Contracts/IDashboardAlertCenterDal.cs` | **New** |
| Application | `DashboardAlertCenterAgg/Services/DashboardAlertCenterComposer.cs` | **New** |
| Application | `DashboardAlertCenterAgg/Services/AlertCenterRegistry.cs` | **New** |
| Application | `DashboardAlertCenterAgg/Services/DashboardSnapshotHealthHelper.cs` | **New** (optional extract from executive) |
| Application | `DashboardExecutiveAgg/Services/DashboardExecutiveComposer.cs` | **Optional refactor** — delegate health to shared helper |
| Infrastructure | `DashboardAlertCenterAgg/DashboardAlertCenterDal.cs` | **New** |
| API | `AlertCenterDashboardController.cs` | **New** |
| API | `InfrastructurePortalExtensions.cs` | Register DAL, composer |
| Frontend | `views/alerts/AlertCenterView.vue` | **New** |
| Frontend | `components/alerts/AlertCenter*.vue` | **New** section components |
| Frontend | `router/index.ts`, `MainLayout.vue` | Route + sidebar |
| Frontend | `views/dashboard/DashboardHomeView.vue` | **Open Alert Center** button |
| Frontend | `models/dashboard.ts`, `api/dashboardApi.ts`, `stores/dashboardStore.ts` | Alert center types + load |
| Tests | `btr.test/ReportingContext/DashboardAlertCenterComposerTest.cs` | **New** |
| Tests | `btr.test/ReportingContext/AlertCenterRegistryTest.cs` | Registry completeness vs known SignalKeys |
| Docs | `docs/features/btr-portal/ALERT-REGISTRY.md` | Keep in sync with code registry |

### 4.2 Unaffected modules

| Module | Reason |
| --- | --- |
| All M17–M22 snapshot workers and aggregators | M23 consumes output only (Q30) |
| All `BTR_PortalDashboard*` table DDL | No schema changes |
| M16 executive API response | No layout/composer changes (Q16) — optional internal health extract only |
| Domain dashboard views and APIs | Unchanged |
| BTR Desktop | No portal Desktop links (Q21) |
| Health endpoint | Unchanged — M23 reads same refresh log |
| Reports | Unchanged — drill-down reuses `?q=` convention |

### 4.3 Traceability

| Alert Center field | Source | Validating reference |
| --- | --- | --- |
| Customer alert row | M17 snapshot attention (post-dedup) | Customer Dashboard attention list |
| Collection alert row | M20 snapshot attention | Collection Dashboard attention list |
| Sales rep alert row | M18 snapshot attention (post-dedup) | Salesman Dashboard attention list |
| Purchasing alert row | M21 snapshot attention | Purchasing Dashboard attention list |
| Location alert row | M22 snapshot attention (exception signals) | Location Dashboard attention list |
| Inventory summary | M19 KPI snapshot | Inventory Risk Dashboard cards |
| Concentration KPI | Domain KPI snapshots | Respective domain dashboard |
| Platform alert | Executive-equivalent health compose | M16 staleness banners |
| Category counts | Composer dedup + cap | Manual count on domain dashboards |

**Reconciliation rule:** For any entity alert row, the same entity × signal (when not deduped away) must exist on the owning domain dashboard attention list.

---

## 5. Backend Design

**No database changes.** This section defines API contracts and composer behavior only.

### 5.1 `GET /api/dashboard/alerts` response shape

```csharp
public class DashboardAlertCenterResponse
{
    public bool IsAvailable { get; set; }
    public bool IsDataFresh { get; set; }
    public string OverallHealthStatus { get; set; }
    public bool HasUnavailableDomain { get; set; }
    public DateTime? LastRefreshed { get; set; }

    public IList<DashboardAlertCenterPlatformAlert> PlatformAlerts { get; set; }
    public IList<DashboardAlertCenterCategorySummary> CategorySummaries { get; set; }
    public IList<DashboardAlertCenterCategoryGroup> AlertGroups { get; set; }
    public DashboardAlertCenterInventoryRiskSummary InventoryRiskSummary { get; set; }
    public IList<DashboardAlertCenterConcentrationItem> Concentrations { get; set; }
    public DashboardAlertCenterNavigationLinks Navigation { get; set; }
}

public class DashboardAlertCenterPlatformAlert
{
    public string SignalKey { get; set; }
    public string SignalLabel { get; set; }
    public string ValueText { get; set; }
    public string DashboardRoute { get; set; }
}

public class DashboardAlertCenterCategorySummary
{
    public string Category { get; set; }
    public int TotalCount { get; set; }
    public int DisplayedCount { get; set; }
    public bool HasMore { get; set; }  // TotalCount > 20
}

public class DashboardAlertCenterCategoryGroup
{
    public string Category { get; set; }
    public IList<DashboardAlertCenterAlertRow> Alerts { get; set; }
}

public class DashboardAlertCenterAlertRow
{
    public string Category { get; set; }
    public string EntityType { get; set; }
    public string EntityCode { get; set; }
    public string EntityName { get; set; }
    public string SignalKey { get; set; }
    public string SignalLabel { get; set; }
    public decimal? ValueAmount { get; set; }
    public string ValueText { get; set; }
    public string AchievementBand { get; set; }  // Sales achievement only; else null
    public string DashboardRoute { get; set; }
    public string ReportRoute { get; set; }
    public string EntityFilterQuery { get; set; }
    public int SortOrder { get; set; }
}

public class DashboardAlertCenterInventoryRiskSummary
{
    public bool IsAvailable { get; set; }
    public int DeadStockItemCount { get; set; }
    public decimal DeadStockValue { get; set; }
    public int SlowMovingItemCount { get; set; }
    public decimal SlowMovingValue { get; set; }
    public int NeverSoldItemCount { get; set; }
    public decimal NeverSoldValue { get; set; }
    public decimal? AtRiskInventoryPercent { get; set; }
    public string DashboardRoute { get; set; }  // "/dashboard/inventory-risk"
}

public class DashboardAlertCenterConcentrationItem
{
    public string Label { get; set; }
    public string ValueText { get; set; }
    public decimal? ValuePercent { get; set; }
    public string DashboardRoute { get; set; }
    public string SourceDomain { get; set; }  // e.g. "M20", "M22" — display optional
    public int SortOrder { get; set; }
}

public class DashboardAlertCenterNavigationLinks
{
    public string ExecutiveDashboardRoute { get; set; }
    public string SalesDashboardRoute { get; set; }
    public string PiutangDashboardRoute { get; set; }
    public string CustomerDashboardRoute { get; set; }
    public string SalesmanDashboardRoute { get; set; }
    public string CollectionDashboardRoute { get; set; }
    public string InventoryDashboardRoute { get; set; }
    public string InventoryRiskDashboardRoute { get; set; }
    public string PurchasingDashboardRoute { get; set; }
    public string LocationDashboardRoute { get; set; }
}
```

`IsAvailable`: `true` when at least one producer snapshot loaded successfully (same pragmatic rule as domain dashboards).

### 5.2 `DashboardAlertCenterDal`

Inject read-only snapshot DALs (same set as full portal dashboard ecosystem):

| DAL | M23 use |
| --- | --- |
| `IDashboardSalesSnapshotDal` | Sales achievement headline |
| `IDashboardPiutangSnapshotDal` | Executive top customer % (concentrations) |
| `IDashboardInventorySnapshotDal` | Availability check only |
| `IDashboardPurchasingSnapshotDal` | Availability check only |
| `IDashboardPurchasingManagementSnapshotDal` | Purchasing attention + KPI |
| `IDashboardCustomerSnapshotDal` | Customer attention + concentration KPIs |
| `IDashboardSalesmanSnapshotDal` | Salesman attention |
| `IDashboardCollectionSnapshotDal` | Collection attention + concentration KPIs |
| `IDashboardInventoryRiskSnapshotDal` | Inventory summary + at-risk % |
| `IDashboardLocationSnapshotDal` | Location exception attention + concentration KPIs |
| `IDashboardSnapshotRefreshLogDal` | Platform health |

Pass loaded snapshots + refresh statuses to `DashboardAlertCenterComposer.Compose(...)`.

### 5.3 `AlertCenterRegistry`

Static class defining for each known `SignalKey`:

```csharp
public sealed class AlertCenterRegistryEntry
{
    public string SignalKey { get; }
    public string DefaultLabel { get; }
    public string Category { get; }           // Sales | Customer | …
    public AlertCenterSection Section { get; } // Alerts | Concentrations | InventorySummaryExcluded | Platform
    public string DashboardRoute { get; }
    public int Priority { get; }              // Tie-break within category
}
```

**Implementer requirements:**

1. Seed all entries from `docs/features/btr-portal/ALERT-REGISTRY.md`.
2. Add unit test `AlertCenterRegistry_ContainsAllProducerSignalKeys` — cross-check constants from `Dashboard*Aggregator` public `Signal*` constants.
3. Unknown `SignalKey` at runtime → **log warning, skip row** (do not invent category).

### 5.4 `DashboardAlertCenterComposer` pipeline

```text
1. Resolve platform flags (health helper)
2. Build synthetic platform alert rows
3. Build synthetic sales achievement rows (Warning/Critical only)
4. Collect producer attention rows:
   - M17 Customer.AttentionList
   - M18 Salesman.AttentionList
   - M20 Collection.AttentionList
   - M21 PurchasingManagement.AttentionList
   - M22 Location.AttentionList (exception signals filtered by registry Section=Alerts)
5. Apply deduplication (Section 2.4.2)
6. Map rows → DashboardAlertCenterAlertRow (registry metadata + entity filter query)
7. Group by category; sort; cap at 20; compute category summaries
8. Build inventory risk summary from M19 KPI
9. Build concentrations from registry Section=Concentrations rows + KPI snapshot fields (Section 2.4.1)
10. Set LastRefreshed, IsDataFresh, OverallHealthStatus
```

### 5.5 DI and registration checklist

| Registration | Project |
| --- | --- |
| `DashboardAlertCenterComposer` | application |
| `AlertCenterRegistry` | application (static — no DI) |
| `IDashboardAlertCenterDal` → `DashboardAlertCenterDal` | infrastructure |
| MediatR handler for `GetDashboardAlertCenterQuery` | application |
| `AlertCenterDashboardController` | portal.api |

**No worker registration.**

### 5.6 Controller

```csharp
[Authorize]
[RoutePrefix("api/dashboard/alerts")]
public class AlertCenterDashboardController : ApiController
{
    [HttpGet, Route("")]
    public async Task<IHttpActionResult> Get()
    {
        var result = await _mediator.Send(new GetDashboardAlertCenterQuery());
        return Ok(ApiResponse<DashboardAlertCenterResponse>.Success(result));
    }
}
```

---

## 6. Frontend Implementation

### 6.1 Route and navigation

| Item | Value |
| --- | --- |
| Route | `alerts` → `AlertCenterView.vue` |
| Sidebar | Add **Alert Center** (`pi pi-bell`) as second item under Dashboard group — after Executive, before Sales |
| M16 entry | Prominent **Open Alert Center** button in `DashboardHomeView.vue` header → `router.push('/alerts')` |
| Store action | `dashboardStore.loadAlerts()` |
| API | `GET /api/dashboard/alerts` |

### 6.2 New components

| Component | Purpose |
| --- | --- |
| `AlertCenterPlatformSection.vue` | Pinned platform alerts — reuse `Message` severity pattern from M16 |
| `AlertCenterCategorySummary.vue` | Badge row: `Sales (3) · Customer (12) · …` |
| `AlertCenterAlertTable.vue` | Unified attention DataTable — Category, Entity, Signal, Value, Dashboard link |
| `AlertCenterInventoryRiskSummary.vue` | M19 KPI summary panel + **View Inventory Risk →** |
| `AlertCenterConcentrationsSection.vue` | Informational concentration grid/list |
| `AlertCenterNavigationSection.vue` | Links to all domain dashboards |

**Reuse:** `formatCurrency`, `formatPercent`, `formatDateTime`, `navigateToReport`, staleness banner styling from `DashboardHomeView`.

### 6.3 `AlertCenterView.vue` layout

Fixed section order per Section 2.4.7.

**Header copy:**

- Title: **Alert Center**
- Subtitle: *What requires attention right now across the business?*
- Last Refreshed + platform status chip (`OK` | `Stale` | `Degraded`)

### 6.4 Row click behavior

```typescript
// Primary action — dashboard first (Q20)
function openDashboard(row: DashboardAlertCenterAlertRow): void {
  router.push(row.DashboardRoute)
}

// Secondary action — report evidence when ReportRoute present
function openReport(row: DashboardAlertCenterAlertRow): void {
  if (row.ReportRoute && row.EntityFilterQuery) {
    navigateToReport(router, row.ReportRoute, row.EntityFilterQuery)
  }
}
```

Display dashboard link as primary column; optional report icon when `ReportRoute` set (same pattern as domain attention lists).

### 6.5 TypeScript models

Add `DashboardAlertCenterResponse` and nested types to `models/dashboard.ts`; `fetchDashboardAlerts()` in `api/dashboardApi.ts`.

---

## 7. Testing

### 7.1 Unit tests — `DashboardAlertCenterComposerTest`

| Case | Assertion |
| --- | --- |
| M20 customer ChronicOverdue present | M17 Overdue for same customer **suppressed** |
| M20 LegacyDebt present | M17 Dormant for same customer **suppressed** |
| M20 PlafondBreachOverdue present | M17 PlafondBreach for same customer **suppressed** |
| M20 HighOverdueWorkload present | M18 HighOverdueExposure for same rep **suppressed** |
| M17 SuspendedWithSales | **Not** suppressed by M20 customer rows |
| Different entity grains | Customer + salesman rows both appear |
| Category cap | 25 collection rows → `TotalCount=25`, `DisplayedCount=20`, `HasMore=true` |
| M19 item rows | **Never** appear in alert groups |
| M19 summary | Inventory risk summary populated from KPI |
| Sales Critical band | Synthetic `SalesAchievementCritical` in Sales category |
| Platform degraded | `SnapshotDegraded` pinned in platform section |
| Concentration rows | `CustomerConcentration` in Concentrations, **not** in Alerts |
| M22 warehouse concentration signals | In Concentrations only |
| M22 exception signals | `WarehouseInactiveWithStock` in Location Alerts |
| Producer sort preserved | Within category, lower SortOrder first |
| CompoundDependency route | `/dashboard/purchasing` only |
| Unknown SignalKey | Row skipped, compose succeeds |

### 7.2 Unit tests — `AlertCenterRegistryTest`

| Case | Assertion |
| --- | --- |
| All M17–M22 aggregator `Signal*` constants | Have registry entry |
| Registry categories | Match ALERT-REGISTRY.md |
| M19 item SignalKeys | Section ≠ Alerts |

### 7.3 Integration / manual test checklist

1. `GET /api/dashboard/alerts` returns full response for authenticated user.
2. Page at `/alerts` — title **Alert Center**; sections in fixed order.
3. Login still lands on `/dashboard` (M16).
4. **Open Alert Center** on M16 navigates to `/alerts`.
5. Sidebar **Alert Center** entry works.
6. Platform stale/degraded banners match M16 when snapshots unhealthy.
7. Customer appearing on M17 Overdue and M20 ChronicOverdue → **one row** (M20) in Alert Center.
8. Salesman with M18 HighOverdueExposure and M20 HighOverdueWorkload → **one row** (M20).
9. Collection category shows max 20 rows; summary badge shows true total.
10. Inventory section shows summary counts — **no SKU rows**.
11. Concentrations section separate — concentration signals not mixed in Alerts.
12. Click alert row dashboard link → correct domain dashboard.
13. Click report icon → correct report with `?q=` when applicable.
14. **View Inventory Risk →** opens `/dashboard/inventory-risk`.
15. Refresh button reloads API without error.
16. No new snapshot tables or worker domains introduced.
17. Domain dashboard attention lists unchanged after M23 deploy.

---

## 8. Risk Analysis

| Risk | Likelihood | Impact | Mitigation |
| --- | --- | --- | --- |
| Executive vs alert center health logic drift | Medium | Medium | Extract `DashboardSnapshotHealthHelper`; shared unit tests |
| Dedup key mismatch (customer id vs code) | Medium | High | Normalize keys per Section 2.4.2; fixture tests for overlap cases |
| Registry out of sync with ALERT-REGISTRY.md | Medium | Medium | `AlertCenterRegistryTest` + PR checklist |
| Alert volume still high in Collection category | Medium | Low | Top 20 cap + category summary `HasMore`; drill to domain dashboard |
| Read-time compose latency (10 snapshot reads) | Low | Medium | Single DB round-trips per DAL (existing pattern); acceptable for daily review |
| Implementer adds new SignalKey in M23 | Low | High | Code review against Q30; registry governance |
| M16 accidental backend change | Low | Medium | PR scope check — frontend navigation only on executive view |
| Unknown future producer SignalKey | Medium | Low | Skip + log warning; PO updates registry before inclusion |

---

## 9. Documentation Updates (post-delivery)

| Document | Update |
| --- | --- |
| `docs/features/btr-portal/btr-portal-operational.md` | Alert Center route, sections, dedup, drill-down, M16 entry point |
| `docs/features/btr-portal/btr-portal-domain.md` | M23 aggregator role in attention ecosystem |
| `docs/features/btr-portal/btr-portal-architecture.md` | Alert Center read-time topology, registry |
| `docs/features/btr-portal/ALERT-REGISTRY.md` | Confirm entries match shipped code |
| `docs/features/materialized-dashboard/materialized-dashboard-domain.md` | Note M23 as consumer — no new domain |
| `docs/work/btr-portal/M23-Alert-Center-Analysis.md` | Link to this plan |

---

## 10. Implementation Steps

Execute in order. Each phase should compile before proceeding.

### Phase 1 — Backend core (registry + composer)

1. Add `DashboardAlertCenterAgg` project folder structure (queries, contracts, services).
2. Implement `AlertCenterRegistry.cs` from `ALERT-REGISTRY.md`.
3. Add `AlertCenterRegistryTest` — verify all producer SignalKey constants covered.
4. Implement `DashboardSnapshotHealthHelper` (extract from executive composer if practical).
5. Implement `DashboardAlertCenterComposer` with dedup, cap, and section pipeline.
6. Add `DashboardAlertCenterComposerTest` covering Section 7.1 cases.

### Phase 2 — Backend integration

7. Implement `DashboardAlertCenterDal` — inject all snapshot DALs.
8. Add `GetDashboardAlertCenterQuery` + MediatR handler.
9. Add `AlertCenterDashboardController`.
10. Register DI in `InfrastructurePortalExtensions.cs`.
11. Run unit tests.

### Phase 3 — Frontend

12. Add TypeScript models and `fetchDashboardAlerts()`.
13. Add `dashboardStore.loadAlerts()`.
14. Build alert center section components (Section 6.2).
15. Create `AlertCenterView.vue` with fixed layout (Section 2.4.7).
16. Add route in `router/index.ts`.
17. Add sidebar **Alert Center** in `MainLayout.vue`.
18. Add **Open Alert Center** button to `DashboardHomeView.vue`.

### Phase 4 — Verification

19. Run full unit test suite.
20. Execute manual checklist (Section 7.3).
21. Spot-check dedup cases against live Customer + Collection dashboards.
22. Update feature documentation (Section 9).

---

## Appendix A — Category summary display order

| Order | Category |
| --- | --- |
| 1 | Platform |
| 2 | Sales |
| 3 | Customer |
| 4 | Collection |
| 5 | Inventory |
| 6 | Purchasing |
| 7 | Location |

Platform section always rendered first regardless of summary row position.

---

## Appendix B — Concentration display order (recommended)

| Order | Label source |
| --- | --- |
| 1 | M20 Recovery vs Billing % |
| 2 | M20 Overdue Concentration % |
| 3 | M17 Top Customer Omzet % |
| 4 | M17 Top Customer Piutang % |
| 5 | M19 At-Risk Inventory % |
| 6 | M22 Top Warehouse Inventory % |
| 7 | M22 Top Warehouse At-Risk % |
| 8 | M22 Top Warehouse Sales % |
| 9 | M22 Top Wilayah Sales % |
| 10+ | Informational attention rows (M18, M21, M22) sorted by registry priority |

---

## Appendix C — Milestone boundary summary

| Question | Owner |
| --- | --- |
| Which customers are overdue / chronic / legacy? | M20 Collection (canonical in M23 feed) |
| Which customers are dormant / plafond breach (no M20 row)? | M17 Customer |
| Which salesmen are below target? | M18 Salesman |
| Which principals have backlog / dependency? | M21 Purchasing |
| Which warehouses are inactive / idle? | M22 Location |
| Which SKUs are dead / slow / never sold? | M19 Inventory Risk (detail); M23 summary only |
| What needs attention company-wide right now? | **M23 Alert Center** (aggregator) |
| What is happening at executive level? | M16 Executive Dashboard (unchanged) |

---

*End of implementation plan — M23 Alert Center*
