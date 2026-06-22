# BTR Portal Navigation Refactoring — Task Breakdown

## Document Status

| Field | Value |
| ----- | ----- |
| Parent plan | [implementation-plan-navigation-refactoring.md](./implementation-plan-navigation-refactoring.md) |
| Author role | Architect |
| Status | **Ready for implementation** |

---

## Phase 1 — Navigation Registry (Frontend)

| ID | Task | File(s) | Est. |
| -- | ---- | ------- | ---- |
| 1.1 | Define `PortalMenuCode` union type and domain group IDs | `src/navigation/portalMenu.types.ts` | S |
| 1.2 | Define typed menu code constants (`PortalMenuCodes`) | `src/navigation/portalMenuCodes.ts` | S |
| 1.3 | Build authoritative 24-item registry with groups, icons, routes | `src/navigation/portalMenuRegistry.ts` | M |
| 1.4 | Implement lookup/format helpers | `src/navigation/portalMenuHelpers.ts` | S |
| 1.5 | Unit tests: item count, codes, routes, group order | `src/navigation/portalMenuRegistry.spec.ts` | M |

**Acceptance:** 24 items, 8 groups, all routes match §4.7 table.

---

## Phase 2 — Sidebar Rendering

| ID | Task | File(s) | Est. |
| -- | ---- | ------- | ---- |
| 2.1 | Create `PortalMenuLabel.vue` (`CODE · Label` display) | `src/components/navigation/PortalMenuLabel.vue` | S |
| 2.2 | Replace hardcoded `navSections` in MainLayout with registry import | `src/layouts/MainLayout.vue` | M |
| 2.3 | Add sidebar CSS for menu code styling | `src/layouts/MainLayout.vue` | S |

**Acceptance:** Sidebar shows 8 domain groups; each row displays `CODE · Label`; active state works.

---

## Phase 3 — Cross-Navigation Components

| ID | Task | File(s) | Est. |
| -- | ---- | ------- | ---- |
| 3.1 | Update CustomerNavigationSection link labels | `CustomerNavigationSection.vue` | S |
| 3.2 | Update SalesmanNavigationSection link labels | `SalesmanNavigationSection.vue` | S |
| 3.3 | Update CollectionNavigationSection link labels | `CollectionNavigationSection.vue` | S |
| 3.4 | Update PurchasingNavigationSection link labels | `PurchasingNavigationSection.vue` | S |
| 3.5 | Update InventoryRiskNavigationSection link labels | `InventoryRiskNavigationSection.vue` | S |
| 3.6 | Update LocationNavigationSection link labels | `LocationNavigationSection.vue` | S |
| 3.7 | Update AlertCenterNavigationSection to iterate `DomainDashboards` | `AlertCenterNavigationSection.vue` | M |

**Acceptance:** All cross-nav links show menu codes; Alert Center shows 18 domain dashboards.

---

## Phase 4 — Backend Menu Registry

| ID | Task | File(s) | Est. |
| -- | ---- | ------- | ---- |
| 4.1 | Create `PortalMenuRegistry.cs` with route→code lookup | `btr.application/.../Shared/PortalMenuRegistry.cs` | M |
| 4.2 | Add `PortalMenuLinkModel` DTO | `GetDashboardAlertCenterQuery.cs` | S |
| 4.3 | Extend `DashboardAlertCenterNavigationLinks` with `DomainDashboards` | `GetDashboardAlertCenterQuery.cs` | S |
| 4.4 | Update `BuildNavigation()` in composer | `DashboardAlertCenterComposer.cs` | S |
| 4.5 | Update InvestigationRegistry step labels via registry | `InvestigationRegistry.cs` | M |
| 4.6 | Add backend unit tests | `PortalMenuRegistryTest.cs`, update `InvestigationRegistryTest.cs` | M |
| 4.7 | Register new file in csproj if needed | `btr.application.csproj`, `btr.test.csproj` | S |

**Acceptance:** Investigation steps show `CODE · Label`; Alert Center API returns 18 domain dashboard links with codes.

---

## Phase 5 — Frontend API Models

| ID | Task | File(s) | Est. |
| -- | ---- | ------- | ---- |
| 5.1 | Add `PortalMenuLinkDto` interface | `src/models/dashboard.ts` | S |
| 5.2 | Extend `DashboardAlertCenterNavigationLinks` with `DomainDashboards` | `src/models/dashboard.ts` | S |

**Acceptance:** TypeScript models match C# DTOs.

---

## Phase 6 — Investigation Display (Frontend)

| ID | Task | File(s) | Est. |
| -- | ---- | ------- | ---- |
| 6.1 | Enrich investigation step display via route lookup (fallback to server label) | `InvestigationStepsList.vue` | S |

**Acceptance:** Investigation breadcrumbs/steps show menu codes when route is known.

---

## Phase 7 — Documentation

| ID | Task | File(s) | Est. |
| -- | ---- | ------- | ---- |
| 7.1 | Replace §Navigation Structure with domain-group hierarchy + code table | `docs/features/btr-portal/btr-portal-operational.md` | M |
| 7.2 | Update scattered "Sidebar → Dashboard →" navigate instructions | `btr-portal-operational.md` | M |

**Acceptance:** Operational doc matches §4.7 authoritative menu table.

---

## Phase 8 — Testing & Verification

| ID | Task | File(s) | Est. |
| -- | ---- | ------- | ---- |
| 8.1 | Run frontend unit tests (`vitest`) | — | S |
| 8.2 | Run backend unit tests (`PortalMenuRegistry`, `InvestigationRegistry`) | — | S |
| 8.3 | Manual regression checklist (see parent plan §9) | — | M |
| 8.4 | Write implementation summary | `implementation-summary-navigation-refactoring.md` | S |

---

## Dependency Graph

```text
Phase 1 (Registry)
  ├── Phase 2 (Sidebar)
  ├── Phase 3 (Cross-nav) — depends on 1.4 helpers
  ├── Phase 5 (API models) — parallel with Phase 4
  └── Phase 6 (Investigation display)

Phase 4 (Backend registry) — parallel with Phase 1 after types defined
  └── Phase 3.7 (Alert Center) — needs 4.3 + 5.2

Phase 7 (Docs) — after Phase 2 complete
Phase 8 (Testing) — after all phases
```

---

## Effort Summary

| Phase | Size |
| ----- | ---- |
| Phase 1 | ~2h |
| Phase 2 | ~1h |
| Phase 3 | ~2h |
| Phase 4 | ~2h |
| Phase 5 | ~0.5h |
| Phase 6 | ~0.5h |
| Phase 7 | ~1.5h |
| Phase 8 | ~1h |
| **Total** | **~10.5h** |
