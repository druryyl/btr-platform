# BTR Portal Navigation Refactoring — Risk Analysis

## Document Status

| Field | Value |
| ----- | ----- |
| Task | Navigation IA refactoring |
| Authoritative requirements | [portal-navigation-ux-analysis.md](../../features/btr-portal/portal-navigation-ux-analysis.md) |
| Author role | Architect |
| Status | **Approved for implementation** |

---

## 1. Implementation Risks

| Risk | Severity | Description | Mitigation |
| ---- | -------- | ----------- | ---------- |
| **Registry drift (frontend vs backend)** | High | Menu codes defined in two places (TypeScript registry + C# `PortalMenuRegistry`) could diverge | Both registries mirror §4.7 authoritative menu table; unit tests assert 24 items, codes, routes; code review checklist requires paired updates |
| **Incomplete Alert Center links** | Medium | Domain Dashboards panel may still omit new milestones | `PortalMenuRegistry.GetDomainDashboardLinks()` returns all dashboard routes except Alert Center; test asserts 18 links |
| **Sidebar overflow on small screens** | Low | 8 group headings + 24 items increases vertical scroll | Accept per spec (no collapsible groups in scope); existing responsive sidebar scroll preserved |
| **Investigation step label regression** | Medium | Changing step labels from semantic text to `CODE · Label` may confuse users expecting old wording | Labels derived from approved menu table only; compound-dependency steps map route → code |
| **API contract change (Alert Center Navigation)** | Low | Adding `DomainDashboards` array to response | Portal not in production; existing route properties retained for category routing |

---

## 2. Regression Risks

| Area | Risk | Mitigation |
| ---- | ---- | ---------- |
| **Vue Router** | Accidental route path/name change breaks bookmarks | Zero changes to `router/index.ts`; navigation config references existing `routeName` values only |
| **Authentication redirect** | Login redirect depends on `dashboard` route name | Route names untouched; regression test in `router/index.spec.ts` |
| **Presentation Mode** | Menu codes hidden during demos | Spec requires codes remain visible; no conditional hiding added |
| **Cross-dashboard drill-down** | Investigation metadata routes unchanged | Routes preserved; only display labels enriched |
| **Alert Center category routing** | `getCategoryDashboardRoute()` depends on existing navigation properties | Individual route properties on `DashboardAlertCenterNavigationLinks` preserved |
| **Backend tests** | InvestigationRegistry tests check step count, not label text | Update compound-dependency step label assertions if added |

---

## 3. UI Risks

| Risk | Impact | Mitigation |
| ---- | ------ | ---------- |
| **Code readability** | Codes too small or low contrast in sidebar | `PortalMenuLabel` component uses monospace code span with muted color; middle dot separator |
| **Active state confusion** | Long labels wrap awkwardly with codes | Flex layout: code fixed-width, label wraps; test visually on 240px sidebar |
| **NavigationSection inconsistency** | Some pages show old labels | All 7 `*NavigationSection.vue` components updated to use `formatMenuLabelByRoute()` |
| **Duplicate "Dashboard" suffix** | Cross-nav links say "Sales Dashboard" while sidebar says "SA01 · Sales" | Cross-nav labels use registry canonical labels (without "Dashboard" suffix where spec label omits it) |

---

## 4. Routing Risks

| Risk | Severity | Mitigation |
| ---- | -------- | ---------- |
| **Route/path mismatch in registry** | High — broken sidebar links | Registry unit test validates every `routeName` resolves via Vue Router route table |
| **Report routes under wrong group** | Medium — IA confusion | Reports assigned to domain groups per §4.7; test asserts group membership |
| **Orphan route names** | Low | Test asserts bijection: 24 registry items ↔ 24 authenticated child routes (excluding login) |

---

## 5. Operational Risks

| Risk | Mitigation |
| ---- | ---------- |
| **Training materials reference old "Dashboard → Reports" paths** | Update `btr-portal-operational.md` §Navigation Structure with domain groups and code reference table |
| **Support confusion with Desktop codes** | Document in operational guide: Portal codes (`SA01`) ≠ Desktop codes (`RO2`) |
| **Future milestone code assignment errors** | Document extension procedure in `portal-navigation-ux-analysis.md` §5.4; registry append-only |

---

## 6. Rollback Strategy

Navigation changes are frontend-metadata and label-only. Rollback:

1. Revert `MainLayout.vue` to hardcoded `navSections`
2. Remove `src/navigation/` directory
3. Revert NavigationSection and Alert Center components
4. Revert backend `PortalMenuRegistry.cs` and InvestigationRegistry label changes

No database migration or route changes required for rollback.

---

## 7. Risk Acceptance

| Accepted risk | Rationale |
| ------------- | --------- |
| Dual registry maintenance | Required for server-rendered investigation labels and client sidebar; simplest approach without API round-trip for codes |
| No collapsible domain groups | Explicitly out of scope per UX spec §2.4 |
| No RBAC menu filtering | Deferred per domain doc §12.5 |
