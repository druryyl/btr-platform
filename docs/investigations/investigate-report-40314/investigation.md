# Investigation Summary

## Expected Behavior

When a user clicks **Investigate** on a dashboard, Alert Center row, or ranking/attention table, BTR Portal should perform client-side navigation to the matching report route (for example `/reports/piutang`) with structured query parameters (`q`, entity IDs, `periodMode`, `posting`, breadcrumb fields). The report view should mount, hydrate filters from the query string, load report data, and display filtered evidence with an investigation breadcrumb.

## Actual Behavior

Clicking **Investigate** shows an IIS error page instead of the report:

```text
HTTP Error 403.14 - Forbidden
The Web server is configured to not list the contents of this directory.
```

The report UI never renders.

## Affected Modules

| Layer | Module / artifact | Role |
| ----- | ----------------- | ---- |
| Frontend navigation | `btr.portal.web/src/services/navigateToInvestigation.ts` | Central handler for all Investigate actions |
| Frontend routing | `btr.portal.web/src/router/index.ts` | Report child routes under `reports/{sales,piutang,inventory,purchasing}` |
| Frontend build / hosting | `btr.portal.web/vite.config.ts` (`base: '/portal/'`) | Public URL prefix for built assets and router history |
| Frontend hosting | `btr.portal.web/public/web.config` | IIS URL Rewrite SPA fallback |
| Backend routing metadata | `InvestigationRegistry.cs`, `InvestigationMetadataBuilder.cs` | Authoritative `ReportRoute` values (`/reports/...`) |
| Dashboard producers | M16 executive, M17–M22 dashboards, M23 Alert Center DAL/composers | Emit `Investigation` metadata on attention/ranking rows |
| Report views | `SalesReportView.vue`, `PiutangReportView.vue`, `InventoryReportView.vue`, `PurchasingReportView.vue` | Investigation hydration + data load on mount |

### Investigate entry points (all call `navigateToInvestigation`)

| Surface | File |
| ------- | ---- |
| Alert Center | `components/alerts/AlertCenterAlertTable.vue` |
| Collection attention | `components/dashboard/CollectionAttentionList.vue` |
| Customer attention | `components/dashboard/CustomerAttentionList.vue` |
| Salesman attention | `components/dashboard/SalesmanAttentionList.vue` |
| Purchasing attention | `components/dashboard/PurchasingAttentionList.vue` |
| Location attention | `components/dashboard/LocationAttentionList.vue` |
| Inventory risk attention | `components/dashboard/InventoryRiskAttentionList.vue` |
| Executive Top 5 | `components/dashboard/ExecutiveExposureSection.vue` |
| Customer rankings | `views/dashboard/CustomerDashboardView.vue` |
| Salesman rankings | `views/dashboard/SalesmanDashboardView.vue` |
| Collection rankings | `views/dashboard/CollectionDashboardView.vue` |
| Purchasing rankings | `views/dashboard/PurchasingDashboardView.vue` |
| Location rankings | `views/dashboard/LocationDashboardView.vue` |
| Legacy sales / piutang / inventory rankings | `SalesDashboardView.vue`, `PiutangDashboardView.vue`, `InventoryDashboardView.vue` |
| Inventory risk item rows | `InventoryRiskDashboardView.vue` |

Deprecated but still present: `navigateToReport()` wrapper used by `PurchasingPrincipalExposureTable.vue` (row click, not labeled Investigate).

### Navigation flow

```text
Dashboard / Alert Center UI event
  → navigateToInvestigation(router, investigation, sourceLabel)
    → buildInvestigationQuery(investigation, sourceLabel)
    → router.push({
         path: investigation.ReportRoute,   // e.g. /reports/piutang
         query,                           // q, customerId, periodMode, ...
         state: { dashboardRoute, desktopNextStep, investigationSteps }
       })
  → Matched report view onMounted
    → useReportInvestigationHydration().hydrateFromRoute(route)
    → store.loadReport(...)
```

### Target routes generated

Backend registry and all dashboard producers use these frontend paths (not API paths):

| Report | `ReportRoute` |
| ------ | ------------- |
| Sales | `/reports/sales` |
| Piutang | `/reports/piutang` |
| Inventory | `/reports/inventory` |
| Purchasing | `/reports/purchasing` |

With `vite` base `/portal/`, Vue Router resolves investigation navigation to browser URLs such as:

`/portal/reports/piutang?q=...&customerId=...&periodMode=allOpenBalances&...`

Router registration matches this design: child routes `reports/sales`, `reports/piutang`, etc. under layout path `/`.

API report endpoints remain separate at `/api/reports/{name}` and are not used for page navigation.

## Root Cause Candidates

### Candidate A — IIS SPA fallback misconfiguration for `/reports/*` deep links

Likelihood: **High**

Evidence:

- `HTTP 403.14` is an IIS static-file/directory-browsing error. It is not emitted by Vue, Vite, or `btr.portal.api`.
- `public/web.config` rewrites unmatched requests to `url="/"` (site root), not to `index.html` within the portal application:

```xml
<action type="Rewrite" url="/" />
```

- For a direct HTTP GET to `/portal/reports/piutang` (or `/reports/piutang` if base prefix is wrong), IIS must serve `index.html` so the SPA can boot. If the request maps to a directory or misses rewrite, IIS returns **403.14**.
- Built `dist/` contains only `index.html`, `assets/`, and static icons — no physical `reports/` folder — so a failed rewrite commonly surfaces as directory-listing forbidden.
- Deployment docs specify IIS static site `/btr-portal`, while the build hardcodes `base: '/portal/'` in `vite.config.ts`. A prefix mismatch increases the chance that report URLs hit the wrong IIS application or an unhandled path.

This explains the observed IIS error. It is the only candidate consistent with **403.14**.

### Candidate B — Report “must filter first” regression from M24 investigation filtering

Likelihood: **Low** (for 403.14)

Evidence:

- Report views still call `loadReport()` in `onMounted` (for example `PiutangReportView.vue`, `SalesReportView.vue`, `InventoryReportView.vue`, `PurchasingReportView.vue`).
- M24 added client-side investigation filters (`useReportInvestigationFilter`, `applyInvestigationQuery`) and `ReportFilterBar` UX; these filter already-loaded rows and do not block API fetch on entry.
- `ReportFilterBar` requires **Apply** only when changing server-side period bounds; initial mount still loads data with default/current period (or all-open for piutang investigations).
- A filter/id mismatch (for example `customerId` not matching `CustomerCode` in rows) can produce an empty table after navigation, which may feel like “filter required,” but would not produce an IIS HTML error page.

This hypothesis is plausible for “no visible data” confusion but not for **403.14**.

### Candidate C — Broken or missing `Investigation.ReportRoute` at runtime

Likelihood: **Low**

Evidence:

- `navigateToInvestigation` guards `if (!investigation.ReportRoute) return;` — missing route would no-op, not navigate.
- Some components show **Investigate** when `Investigation` exists but do not verify `Investigation.ReportRoute` (for example `CustomerAttentionList.vue`), while others do (Alert Center). A missing route would cause silent failure, not IIS 403.
- Backend tests (`InvestigationRegistryTest`, `DashboardExecutiveComposerTest`) assert correct `/reports/...` routes.
- API uses PascalCase JSON matching frontend TypeScript models; no camelCase transform in `httpClient`.

### Candidate D — `router.push` path does not match current router design

Likelihood: **Low**

Evidence:

- Router defines named report routes at `reports/{sales,piutang,inventory,purchasing}` under authenticated layout — same paths used by sidebar menu (`MainLayout.vue` uses `router.push('/reports/sales')`, etc.).
- `buildInvestigationQuery.spec.ts` passes; manual M24 checklist item 8 expects direct `/reports/piutang` to work.
- Local router resolution with base `/portal/` produces `href` `/portal/reports/piutang?...`, which matches registration.
- Recent commit `984fdeb` only adjusted `HistoryState` typing; no routing logic change.

## Most Likely Root Cause

**Candidate A** — IIS is serving a direct HTTP request to a `/reports/*` URL without SPA fallback.

`navigateToInvestigation` itself generates valid in-app routes (`/reports/{name}` + query). In a healthy SPA session, `router.push` should not contact IIS. The **403.14** response means the browser (or environment) is performing a **full document request** to a report URL that IIS treats as a directory, and rewrite/fallback is not returning `index.html`.

Most probable triggers in production:

1. **Incorrect `web.config` rewrite target** (`/` instead of `index.html` at the portal app root).
2. **Base path / IIS site prefix mismatch** (`/portal/` in build vs `/btr-portal` in deployment docs), causing report URLs to hit an IIS app without SPA rules.
3. **Hard navigation** to investigation URLs (refresh, open in new tab, bookmark) after M24 added query-heavy links — exposes the hosting defect that sidebar clicks may not have surfaced if users rarely deep-link reports.

The report-filtering change (Candidate B) is a red herring for this specific error code.

## Proposed Fix

Minimal safe fix (hosting + alignment, no business-logic change):

1. **Fix SPA fallback in `btr.portal.web/public/web.config`**

   Replace rewrite target:

   ```xml
   <action type="Rewrite" url="index.html" />
   ```

   (`index.html` is relative to the portal IIS application root; avoid `url="/"` which escapes the app to site root.)

2. **Align build base with the real IIS application path**

   - If production site is `/btr-portal`, set `base: '/btr-portal/'` (or inject via `VITE_BASE_PATH` at build time).
   - Rebuild and redeploy `dist/`.
   - Update `Cors:AllowedOrigins` and deployment docs to the same prefix.

3. **Verify on the target IIS server**

   - Open DevTools → Network.
   - Click **Investigate**; confirm there is **no** new `document` request to `/reports/...` (client-side navigation only).
   - Hard-open `/portal/reports/piutang?q=test` (or `/btr-portal/reports/piutang?q=test` after alignment) — should return `index.html` (200), not 403.14.
   - Confirm sidebar **Reports → Piutang Report** behaves the same way.

Optional hardening (second step, only if needed after 1–3):

- Map `ReportRoute` to Vue route **names** in `navigateToInvestigation` to reduce path-resolution fragility.
- Add a unit test asserting resolved `href` prefix matches `import.meta.env.BASE_URL`.

Do **not** change investigation query semantics or report DAL behavior for this defect.

## Regression Risks

| Change | Risk |
| ------ | ---- |
| `web.config` rewrite to `index.html` | Low — standard SPA pattern; verify static assets (`/portal/assets/*` or `/btr-portal/assets/*`) still served as files (existing `IsFile` negate condition) |
| `base` path change | Medium — all asset URLs and router links change; must rebuild frontend and update IIS site binding/docs together |
| Named-route navigation | Low–medium — requires mapping table maintenance if report routes grow |

## Confidence Level

**Medium**

- **High confidence** that **403.14** is IIS hosting/fallback, not report filtering or investigation metadata.
- **Medium confidence** on the exact production trigger (click vs hard navigation vs base mismatch) because runtime IIS topology was not available in this investigation.
- Source-level routing and `ReportRoute` values are internally consistent with the router design.

Recommended next validation step on the failing environment: capture the exact browser address bar URL and whether a **document** network request fires on Investigate click.
