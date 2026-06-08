# Implementation Plan: M16 Executive Dashboard (Management Attention Center)

## Document Status

| Field | Value |
| --- | --- |
| Initiative | M16 Executive Dashboard — **Management Attention Center** |
| Authoritative requirements | `docs/work/btr-portal/portal-analysis-m16-executive-dashboard.md` — **Section 10 (Final Product Decisions)** |
| Related features | `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md` |
| Reference pattern | Dashboard Overview (M11) + domain dashboards (M13–M15) + Health endpoint |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |
| Status | **Ready for implementation** |
| Product decisions | **Confirmed** — analysis Section 10, 2026-06-08 |

---

## 1. Goal

Replace the operational dashboard home (`/dashboard`) with a **Management Attention Center** that answers *What requires management attention today?* by **composing existing snapshot metrics** across Sales, Piutang, Inventory, and Purchasing.

**Primary outcomes:**

- `/dashboard` becomes the Management Attention Center for **all authenticated users** (no role-based routing).
- Page title and framing: **Management Attention Center** — not generic "Dashboard" or "summary".
- Surfaces **approved promoted KPIs** (Section 2) with **Achievement % bands** on Sales only; all other domains use **Attention Indicator** presentation.
- **Top 5 exposure lists** grouped by domain (Customers, Categories, Suppliers, Principals) — no mixed-domain table.
- **Consolidated `Last Refreshed`** timestamp and **"⚠ Dashboard Data Not Fresh"** banner when stale.
- Navigation path: **Executive → Domain Dashboard → Report** — executive sections link to domain dashboards only.
- Domain detail dashboards and reports remain unchanged as the analytical depth layer.

**Explicitly out of scope (PO confirmed):**

- New snapshot tables, new aggregators, or new SQL against transactional tables.
- Generic severity engine (Info / Warning / Critical) — **rejected**; only Sales Achievement % bands apply.
- Weekly trend on executive page (domain dashboards only).
- Slow-moving/dead stock, Faktur Kembali backlog, Sales Omzet Health, pipeline omzet, Retur Beli, collection effectiveness.
- Historical piutang/aging trends, prior-month/quarter comparison.
- Mixed-domain Top Risks table, role-based routing, chart drill-down.
- Operational volume totals: Total Faktur, Total Customer, Total Item, Total Invoice.
- Changes to snapshot refresh cadence, workers, BTR Desktop, or report DALs.

---

## 2. Authoritative Product Decisions

Source: analysis Section 10. Do not re-decide these rules during implementation.

### 2.1 Positioning and audience

| Decision | Value |
| --- | --- |
| Route | **Replace `/dashboard`** — Management Attention Center is the default landing page |
| Audience | All authenticated users — no management-only route or RBAC in M16 |
| Cadence | Daily morning review — *What requires management attention today?* |
| Page title | **Management Attention Center** |
| Sidebar | Dashboard → **Executive** (default, `/dashboard`); Sales, Piutang, Inventory, Purchasing unchanged |

### 2.2 Attention rules

| Rule | Value |
| --- | --- |
| Sales Achievement % bands | ≥ 100% → **Healthy** · 80–99% → **Warning** · < 80% → **Critical** · null → **Unknown** (no targets). No day-of-month logic. |
| Overdue customers | Any count **> 0** is attention-worthy — show count with Attention Indicator |
| > 90 Days aging | Show **amount** and **% of Total Piutang** — no red/yellow threshold; management decides visually |
| Pending posting | Show **count** and **value (BELUM)** — no threshold |
| Concentration ratios | **In scope:** Top Customer % · Top Category % · Top Supplier % · Top Principal % |
| Staleness | Banner **"⚠ Dashboard Data Not Fresh"** when any domain snapshot exceeds its refresh interval |
| Other severity | **Attention Indicator** presentation only — not generic Warning/Critical badges |

### 2.3 Promoted metrics (executive view)

| Domain | Promote | Exclude from executive |
| --- | --- | --- |
| **Sales** | Achievement % (+ band), Total Achievement | Total Faktur, Total Customer, weekly trend |
| **Piutang** | Total Piutang, Overdue Customer, > 90 Day Amount (+ % of total), Top Customer % | — |
| **Inventory** | Total Inventory Value, Top Category %, Top Supplier % | Total Item |
| **Purchasing** | Pending Posting Count, Pending Posting Value (BELUM), Top Principal % | Total Invoice, Grand Total Purchase (not on attention card) |
| **System** | Snapshot health status, consolidated **Last Refreshed** | Per-domain timestamps on executive page |

**Exposure lists:** Top **5** per list (truncate from snapshot Top 10).

**Layout (Proposal A — approved):** Fixed section order:

1. Attention Cards  
2. Critical Exposure Lists (grouped by domain — separate sections)  
3. Domain Summaries  
4. Navigation to Details (links to domain dashboards)

### 2.4 Semantics and navigation

| Topic | Decision |
| --- | --- |
| Piutang scope | **All-time open balance** — same as Piutang Dashboard (`KurangBayar > 1`) |
| Piutang report link | When user reaches report via domain dashboard, standard **current month** default applies — no special executive pre-filter |
| Executive card links | **Domain dashboards only** — not reports |
| User path | Executive → Domain Dashboard → Report |

---

## 3. Architecture Overview

### 3.1 Target topology

```text
Existing snapshot workers (unchanged)
    ↓
BTR_PortalDashboard* tables (unchanged)
    ↓
Browser → GET /api/dashboard/executive          (NEW — composition read)
    ↓ MediatR
GetDashboardExecutiveHandler
    ↓ IDashboardExecutiveDal
DashboardExecutiveDal
    ↓ injects existing IDashboard*SnapshotDal (4 domains)
    ↓ DashboardExecutiveComposer (application service)
DashboardExecutiveResponse

Browser → GET /api/dashboard/overview           (EXISTING — unchanged; not used by home)

Domain detail pages → GET /api/dashboard/{sales|piutang|inventory|purchasing}  (unchanged)
Reports → GET /api/reports/*                    (unchanged)
```

**No new worker, no new snapshot write path, no database schema change.**

### 3.2 Design principles

| Principle | Application |
| --- | --- |
| Compose, don't re-aggregate | Read existing snapshot tables via existing snapshot DALs only |
| Preserve domain dashboards | Full charts, weekly trends, Top 10 remain on detail pages |
| Preserve business rules | All aggregation rules stay in existing workers/aggregators |
| Presentation-only derivations | Concentration ratios, > 90 Days %, Achievement band, consolidated freshness, staleness flag |
| PO-approved bands only | `SalesAchievementBand` is the sole automated Healthy/Warning/Critical classification |
| Fail gracefully | Partial snapshot availability → `HasUnavailableDomain` + unavailable sections |

### 3.3 Architecture decisions

| Decision | Choice | Rationale |
| --- | --- | --- |
| API endpoint | **`GET /api/dashboard/executive`** | Rich executive DTO without breaking overview contract |
| Data access | Inject four existing `IDashboard*SnapshotDal` + refresh log reader | Reuse proven snapshot read SQL |
| Composition | `DashboardExecutiveComposer` in Application layer | Unit-test bands, ratios, freshness without SQL |
| Achievement band | `ExecutiveSalesAchievementBandResolver` (or method on composer) | PO thresholds: 100 / 80 — Sales only |
| Staleness | Any domain `UtcNow − GeneratedAt > IntervalMinutes` → `IsDataFresh = false` | Aligns with Q10 banner; use configured intervals from `DashboardSnapshotOptions` |
| Consolidated Last Refreshed | **`Min(GeneratedAt)`** across available domain snapshots | Conservative — reflects oldest domain data on screen |
| Overview API | Leave unchanged | Home switches to executive endpoint only |
| Frontend | Replace `DashboardHomeView.vue`; update `MainLayout.vue` sidebar label Overview → **Executive** | Q1, Q4 |
| Exposure layout | **Stacked sections** with headings (not mixed table; tabs optional) | Q27 — grouped by domain |
| Report links on executive | **None** | Q25 — reports reached via domain dashboards |

---

## 4. Impact Analysis

### 4.1 Affected modules

| Layer | Module | Change type |
| --- | --- | --- |
| Application | `ReportingContext/DashboardExecutiveAgg/` (new) | Query, handler, contracts, composer, band resolver, DTOs |
| Infrastructure | `ReportingContext/DashboardExecutiveAgg/DashboardExecutiveDal.cs` (new) | Orchestrates snapshot reads + composer |
| API | `Controllers/Dashboard/ExecutiveDashboardController.cs` (new) | Thin MediatR controller |
| API | DI registration | Register `IDashboardExecutiveDal` |
| Frontend | `DashboardHomeView.vue` | Replace with Management Attention Center |
| Frontend | `MainLayout.vue` | Sidebar: Overview → **Executive** |
| Frontend | `models/dashboard.ts`, `api/dashboardApi.ts`, `stores/dashboardStore.ts` | Executive types and loader |
| Frontend | New components (Section 7) | Attention cards, exposure sections, domain summaries, staleness banner |
| Tests | `btr.test` | Composer unit tests including Achievement bands |
| Docs | Post-delivery feature docs | Operational, domain, architecture updates |

### 4.2 Unaffected modules

| Module | Reason |
| --- | --- |
| Snapshot workers, aggregators, all `BTR_PortalDashboard*` tables | Read-only composition |
| Domain dashboard APIs and views | Detail layer unchanged |
| Report APIs, DALs, views | Unchanged |
| `DashboardOverviewDal`, `GET /api/dashboard/overview` | Retained; home no longer calls it |
| `HealthController` | Unchanged |
| BTR Desktop, auth/JWT | No changes |

### 4.3 Metric traceability

| Executive field | Source | Rule |
| --- | --- | --- |
| Achievement % | Sales KPI snapshot | From `SalesOmzetChartAchievementPolicy` via aggregator |
| Achievement band | Derived | ≥100 Healthy · 80–99 Warning · <80 Critical · null Unknown |
| Total Achievement | Sales KPI `TotalAchievement` | Current-month invoiced omzet |
| Total Piutang | Piutang KPI | All-time open balance |
| Overdue Customer | Piutang KPI | Distinct customers past due |
| > 90 Days amount | Piutang aging bucket | Same bucket key as `DashboardPiutangAggregator` |
| > 90 Days % | Derived | `Over90Amount / TotalPiutang` when total > 0 |
| Top Customer % | Derived | Top-1 customer balance / Total Piutang |
| Total Inventory Value | Inventory KPI | Excludes In-Transit |
| Top Category % | Derived | Top-1 category value / Total Inventory Value |
| Top Supplier % | Derived | Top-1 supplier value / Total Inventory Value |
| Pending Posting Count | Purchasing KPI | BELUM invoice count |
| Pending Posting Value | Purchasing posting status BELUM row | Sum GrandTotal for unposted |
| Top Principal % | Derived | Top-1 principal purchase / sum of all principal purchases in Top 10 snapshot **or** use `GrandTotalPurchase` from KPI as denominator |
| Top 5 lists | Respective Top 10 tables | Ranks 1–5 slice |
| Last Refreshed | Derived | `Min(GeneratedAt)` across four domain KPI snapshots |
| IsDataFresh | Derived | No domain exceeds its configured interval |

**Top Principal % denominator:** Use `GrandTotalPurchase` from purchasing KPI snapshot (same month total as domain dashboard) — not displayed on executive card but required for ratio calculation.

---

## 5. Database Design

**No database changes required for M16.**

---

## 6. Backend Implementation

### 6.1 New folder structure

```text
btr.application/ReportingContext/
└── DashboardExecutiveAgg/
    ├── Contracts/
    │   └── IDashboardExecutiveDal.cs
    ├── Queries/
    │   └── GetDashboardExecutiveQuery.cs
    └── Services/
        ├── DashboardExecutiveComposer.cs
        └── ExecutiveSalesAchievementBandResolver.cs

btr.infrastructure/ReportingContext/
└── DashboardExecutiveAgg/
    └── DashboardExecutiveDal.cs

btr.portal.api/Controllers/Dashboard/
└── ExecutiveDashboardController.cs
```

Add all new `.cs` files to respective `.csproj` Compile includes.

### 6.2 API contract — `GET /api/dashboard/executive`

**Auth:** JWT required.

**Response:** `ApiResponse<DashboardExecutiveResponse>`

```csharp
public class DashboardExecutiveResponse
{
    public bool HasUnavailableDomain { get; set; }
    public bool IsDataFresh { get; set; }
    public DateTime? LastRefreshed { get; set; }           // Min GeneratedAt across domains
    public string OverallHealthStatus { get; set; }        // ok | refreshing | degraded | unknown
    public DashboardExecutiveSalesAttention Sales { get; set; }
    public DashboardExecutivePiutangAttention Piutang { get; set; }
    public DashboardExecutivePurchasingAttention Purchasing { get; set; }
    public DashboardExecutiveInventoryAttention Inventory { get; set; }
    public DashboardExecutiveCriticalExposures CriticalExposures { get; set; }
    public IList<DashboardExecutiveDomainSummary> DomainSummaries { get; set; }
}

public class DashboardExecutiveSalesAttention
{
    public decimal? AchievementPercent { get; set; }
    public decimal TotalAchievement { get; set; }
    public string AchievementBand { get; set; }            // Healthy | Warning | Critical | Unknown
    public bool RequiresAttention { get; set; }            // band != Healthy when percent present
    public bool IsAvailable { get; set; }
}

public class DashboardExecutivePiutangAttention
{
    public decimal TotalPiutang { get; set; }
    public int OverdueCustomer { get; set; }
    public decimal AgingOver90Amount { get; set; }
    public decimal? AgingOver90Percent { get; set; }
    public decimal? TopCustomerPercent { get; set; }
    public bool RequiresAttention { get; set; }            // OverdueCustomer > 0 OR AgingOver90Amount > 0
    public bool IsAvailable { get; set; }
}

public class DashboardExecutivePurchasingAttention
{
    public int PendingPostingInvoiceCount { get; set; }
    public decimal PendingPostingValue { get; set; }       // BELUM amount
    public decimal? TopPrincipalPercent { get; set; }
    public bool RequiresAttention { get; set; }            // count > 0 OR value > 0
    public bool IsAvailable { get; set; }
}

public class DashboardExecutiveInventoryAttention
{
    public decimal TotalInventoryValue { get; set; }
    public decimal? TopCategoryPercent { get; set; }
    public decimal? TopSupplierPercent { get; set; }
    public bool RequiresAttention { get; set; }            // informational — true when either ratio present
    public bool IsAvailable { get; set; }
}

public class DashboardExecutiveCriticalExposures
{
    public IList<DashboardExecutiveRiskItem> TopCustomers { get; set; }      // max 5
    public IList<DashboardExecutiveRiskItem> TopCategories { get; set; }
    public IList<DashboardExecutiveRiskItem> TopSuppliers { get; set; }
    public IList<DashboardExecutiveRiskItem> TopPrincipals { get; set; }
}

public class DashboardExecutiveRiskItem
{
    public int Rank { get; set; }
    public string Name { get; set; }
    public decimal Amount { get; set; }
}

public class DashboardExecutiveDomainSummary
{
    public string Domain { get; set; }                     // Sales | Piutang | Inventory | Purchasing
    public string SummaryText { get; set; }
    public string DetailDashboardRoute { get; set; }
    public bool IsAvailable { get; set; }
}
```

**Removed from prior draft (PO rejected or out of scope):** per-domain `GeneratedAt` on executive response, `WeeklyTrendDirection`, `ReportRoute`, `SystemAlerts` array (replace with `IsDataFresh` + `OverallHealthStatus` + UI banner copy), `GrandTotalPurchase` on purchasing attention card.

### 6.3 `ExecutiveSalesAchievementBandResolver`

```csharp
public static string Resolve(decimal? achievementPercent)
{
    if (!achievementPercent.HasValue) return "Unknown";
    if (achievementPercent.Value >= 100m) return "Healthy";
    if (achievementPercent.Value >= 80m) return "Warning";
    return "Critical";
}
```

Unit-test boundary values: null, 79.9, 80, 99.9, 100, 150.

### 6.4 `DashboardExecutiveComposer`

**Input:** four optional snapshot aggregates + refresh statuses + `DashboardSnapshotOptions` + `DateTime.UtcNow`

| Derivation | Logic |
| --- | --- |
| Achievement band | `ExecutiveSalesAchievementBandResolver.Resolve(sales.AchievementPercent)` |
| Sales RequiresAttention | `AchievementBand` is Warning or Critical |
| > 90 Days amount | Piutang aging bucket matching aggregator constant for `> 90 Days` |
| > 90 Days % | `Over90 / TotalPiutang` when total > 0 |
| Top Customer % | TopCustomers[0].Balance / TotalPiutang |
| Top Category / Supplier % | TopCategories[0] / TotalInventoryValue; TopSuppliers[0] / TotalInventoryValue |
| Top Principal % | TopPrincipals[0] / GrandTotalPurchase from purchasing KPI |
| Pending posting value | Posting breakdown row where `StatusKey == "BELUM"` |
| Purchasing RequiresAttention | `PendingPostingInvoiceCount > 0` OR `PendingPostingValue > 0` |
| Piutang RequiresAttention | `OverdueCustomer > 0` OR `AgingOver90Amount > 0` |
| Critical exposures | Slice each Top 10 list to rank ≤ 5 |
| Last Refreshed | `Min(GeneratedAt)` over non-null domain snapshots |
| IsDataFresh | For each available domain: `(UtcNow - GeneratedAt).TotalMinutes <= IntervalMinutes` |
| Overall health | `DashboardSnapshotHealthStatusResolver.ResolveOverallStatus(refresh statuses)` |
| Domain summaries | One-line text per domain from attention fields; e.g. Purchasing: `Pending Posting · 7 invoices · Rp xxx` |

**Constants:**

```csharp
public const int ExecutiveRiskListCount = 5;
```

**Do not implement:** weekly trend direction, generic severity engine, mixed-domain risk ranking.

### 6.5 Controller and DI

Mirror `OverviewDashboardController` pattern. Register handler, DAL, and composer in existing Scrutor/MediatR setup.

---

## 7. Frontend Implementation

### 7.1 Route and sidebar

| Item | Change |
| --- | --- |
| Route `/dashboard` | Management Attention Center content |
| `MainLayout.vue` | Rename menu item **Overview** → **Executive** (still routes to `/dashboard`) |
| Login redirect | Unchanged (`/dashboard`) |
| Domain sub-routes | Unchanged |

### 7.2 Store and API

- `fetchDashboardExecutive()` in `dashboardApi.ts`
- `executive` ref + `loadExecutive()` in `dashboardStore.ts`
- Home calls `loadExecutive()` instead of `loadDashboard()`

### 7.3 Page layout — Proposal A (approved)

```text
┌─────────────────────────────────────────────────────────────┐
│ Management Attention Center          Last Refreshed: …  [↻] │
│ What requires management attention today                      │
├─────────────────────────────────────────────────────────────┤
│ [⚠ Dashboard Data Not Fresh]  (when !IsDataFresh)            │
│ [Health degraded message]     (when OverallHealthStatus != ok)│
├─────────────────────────────────────────────────────────────┤
│ ATTENTION CARDS (4)                                           │
│ Sales (band badge) | Piutang | Purchasing | Inventory         │
│ each links → /dashboard/{domain}                              │
├─────────────────────────────────────────────────────────────┤
│ CRITICAL EXPOSURES (stacked sections)                         │
│ Top 5 Customers | Top 5 Categories | Top 5 Suppliers | …  │
├─────────────────────────────────────────────────────────────┤
│ DOMAIN SUMMARIES + [Details →] links to domain dashboards     │
└─────────────────────────────────────────────────────────────┘
```

**No report links on this page.**

### 7.4 Components

| Component | Purpose |
| --- | --- |
| `KpiCard.vue` | Attention cards (reuse) |
| `Top10RankingTable.vue` | Top 5 rows per exposure section (reuse) |
| `Message` (PrimeVue) | Staleness banner, errors, degraded health |
| `ExecutiveAttentionCard.vue` | **New** — domain card with Attention Indicator or Achievement band badge |
| `ExecutiveExposureSection.vue` | **New** — heading + Top 5 table per list |
| `ExecutiveDomainSummaryRow.vue` | **New** — summary line + link to domain dashboard |

**Do not embed:** `WeeklyTrendChart`, `AgingPieChart`, `PostingStatusPieChart`.

### 7.5 Visual treatment

| Element | Treatment |
| --- | --- |
| Sales Achievement band | **Healthy** — green badge · **Warning** — amber · **Critical** — red · **Unknown** — neutral |
| Other domains | **Attention Indicator** (e.g. accent border/icon) when `RequiresAttention` — not generic severity colors |
| > 90 Days, concentration % | Display amount and % — neutral text; no threshold coloring |
| Staleness | Fixed copy: **"⚠ Dashboard Data Not Fresh"** |
| Last Refreshed | Single line in header: `Last Refreshed: YYYY-MM-DD HH:mm` |

### 7.6 Attention card content (exact fields)

| Card | Display |
| --- | --- |
| **Sales** | Achievement % with band badge · Total Achievement (currency) |
| **Piutang** | Total Piutang · Overdue Customer (count) · > 90 Day Amount + % · Top Customer % |
| **Purchasing** | `Pending Posting · {count} invoices · {value}` · Top Principal % |
| **Inventory** | Total Inventory Value · Top Category % · Top Supplier % |

Each card: `RouterLink` to respective `/dashboard/{domain}` only.

---

## 8. Testing

### 8.1 Unit tests — `ExecutiveSalesAchievementBandResolverTest`

| Input | Expected band |
| --- | --- |
| null | Unknown |
| 79.9 | Critical |
| 80 | Warning |
| 99.9 | Warning |
| 100 | Healthy |
| 150 | Healthy |

### 8.2 Unit tests — `DashboardExecutiveComposerTest`

| Case | Assertion |
| --- | --- |
| All snapshots present | `HasUnavailableDomain = false`; all sections available |
| Missing piutang | `HasUnavailableDomain = true`; piutang unavailable |
| Achievement band Critical | 75% → Critical; `Sales.RequiresAttention = true` |
| Achievement band Healthy | 105% → Healthy; `RequiresAttention = false` |
| Overdue attention | OverdueCustomer = 3 → `Piutang.RequiresAttention = true` |
| > 90 Days percent | Total 1000, Over90 250 → 25% |
| Top Customer % | Top balance 400, Total 1000 → 40% |
| BELUM value | From posting breakdown |
| Top 5 truncation | 10 customers → 5 in response |
| Last Refreshed | Min of four GeneratedAt values |
| IsDataFresh false | One domain GeneratedAt older than its interval |
| IsDataFresh true | All domains within interval |
| Top Principal % | Uses GrandTotalPurchase as denominator |

### 8.3 Manual test checklist

1. Sign in → lands on **Management Attention Center** at `/dashboard`.
2. Sidebar shows **Executive** (not Overview) as default dashboard entry.
3. Sales card shows Achievement % with correct Healthy/Warning/Critical badge.
4. Piutang, Purchasing values match domain dashboards for same metrics.
5. Top 5 lists match first five rows of respective domain Top 10 tables.
6. **"⚠ Dashboard Data Not Fresh"** appears when any domain exceeds refresh interval.
7. Header shows single **Last Refreshed** — no per-domain timestamps on page.
8. All executive links go to **domain dashboards only** — no direct report links.
9. Weekly trends absent from executive page; present on domain dashboards.
10. Total Faktur, Total Customer, Total Item, Total Invoice absent from executive page.
11. Domain dashboards and reports unchanged.

---

## 9. Risk Analysis

| Risk | Likelihood | Impact | Mitigation |
| --- | --- | --- | --- |
| Piutang dashboard vs report total mismatch | Medium | Medium | Document: executive/dashboard = all-time open; report default = current month on Jatuh Tempo |
| Users expect old operational home | Medium | Low | PO-approved replacement; volume totals remain on domain dashboards |
| Min vs Max for Last Refreshed confusion | Low | Low | Document Min (oldest domain) in operational guide; conservative for freshness |
| Achievement band at exactly 80% boundary | Low | Low | Unit tests at 80 and 79.9 |
| Top Principal % denominator | Low | Medium | Use `GrandTotalPurchase` from KPI — same source as purchasing dashboard total |

---

## 10. Documentation Updates (post-delivery)

| Document | Update |
| --- | --- |
| `docs/features/btr-portal/btr-portal-operational.md` | Management Attention Center sections, Achievement bands, Last Refreshed, staleness banner, navigation path |
| `docs/features/btr-portal/btr-portal-domain.md` | Executive composition layer; promoted KPIs and bands |
| `docs/features/btr-portal/btr-portal-architecture.md` | `DashboardExecutiveAgg`, `GET /api/dashboard/executive` |
| `docs/work/btr-portal/portal-analysis-m16-executive-dashboard.md` | Link to this plan as implemented |

---

## 11. Implementation Steps

Execute in order. Each step should compile before proceeding.

### Phase 1 — Backend

1. Create `DashboardExecutiveAgg` with response DTOs in `GetDashboardExecutiveQuery.cs`.
2. Implement `ExecutiveSalesAchievementBandResolver` with unit tests.
3. Implement `DashboardExecutiveComposer` with unit tests (Section 8.2).
4. Add `IDashboardExecutiveDal` and `DashboardExecutiveDal` (four snapshot DALs + refresh log).
5. Register DI and `ExecutiveDashboardController`.
6. Verify `GET /api/dashboard/executive` JSON against populated snapshots.

### Phase 2 — Frontend

7. Add TypeScript models and `fetchDashboardExecutive`.
8. Extend `dashboardStore` with `loadExecutive`.
9. Update `MainLayout.vue`: Overview → **Executive**.
10. Create `ExecutiveAttentionCard`, `ExecutiveExposureSection`, `ExecutiveDomainSummaryRow`.
11. Rewrite `DashboardHomeView.vue` per Section 7.3 layout and field list.
12. Wire Achievement band colors and Attention Indicator styling.

### Phase 3 — Verification and docs

13. Run manual test checklist (Section 8.3).
14. Update feature documentation (Section 10).

---

## 12. Acceptance Criteria

M16 is complete when:

1. `/dashboard` displays **Management Attention Center** (Proposal A layout) for all authenticated users.
2. Sidebar shows **Dashboard → Executive** as default entry.
3. `GET /api/dashboard/executive` reads existing snapshot tables only — no new tables or aggregators.
4. Promoted metrics match domain dashboards: Achievement %, Total Achievement, Total Piutang, Overdue Customer, > 90 Days amount/%, pending posting count/value, concentration ratios, Top 5 lists.
5. Sales Achievement % displays **Healthy / Warning / Critical / Unknown** bands per PO thresholds.
6. Non-sales domains use **Attention Indicator** when `RequiresAttention` — no generic severity engine.
7. **"⚠ Dashboard Data Not Fresh"** banner when `IsDataFresh = false`.
8. Single consolidated **Last Refreshed** in header — no per-domain timestamps on executive page.
9. Executive links navigate to **domain dashboards only** — path to reports is via domain dashboards.
10. Weekly trends, operational volume totals, and out-of-scope signals are absent from executive page.
11. Domain dashboards and reports behave identically to pre-M16.
12. Composer and band resolver unit tests pass.

---

## 13. Handoff Notes for Implementer

- **Section 2 is authoritative** — do not add generic severity, weekly trends, report links on executive, or mixed risk tables.
- **Do not modify** snapshot workers, aggregators, or `BTR_PortalDashboard*` tables.
- **Do not remove** `GET /api/dashboard/overview` — home stops calling it.
- Reuse piutang aging bucket constant from `DashboardPiutangAggregator` — verify exact `BucketKey` in code before matching.
- Server-side Top 5 truncation keeps UI and API consistent.
- Purchasing attention card format: **`Pending Posting · {count} invoices · {currency value}`** (PO Q22 example).
- `GrandTotalPurchase` from purchasing KPI is used **only** for Top Principal % denominator — not displayed on executive purchasing card.
- Future milestones (M17 slow stock, Collection Dashboard, etc.) are **not** part of this delivery.
