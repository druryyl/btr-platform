# Implementation prompt — BTR Portal Burgundy + Gold palette

Copy everything below the line into the implementation agent.

---

## Task

Implement the **Burgundy + Yellow Gold** color system for **BTR Portal** (`btr.portal.web`) exactly as specified in the UX handoff document. This is a **color / theming change only** — no layout redesign, no IA changes, no API or business-logic changes.

## Source of truth (read first)

Open and follow this document end-to-end before coding:

`D:\Project.Private\btr-platform\src\j05-btr-distrib\btr.portal.web\docs\BTR-Portal-Burgundy-Gold-Color-Palette.md`

(Also available at `C:\Users\drury\BTR-Portal-Burgundy-Gold-Color-Palette.md`.)

**Do not invent alternate brand hues.** Use the hex values, token names, file targets, and acceptance criteria in that doc. If something is ambiguous, prefer the doc’s “Locked product decisions” and “Implementation plan” sections over improvisation.

## Repo / stack

- Path: `D:\Project.Private\btr-platform\src\j05-btr-distrib\btr.portal.web`
- Vue 3 + Vite + PrimeVue 4 (`Aura` preset today, **no custom primary** in `src/main.ts`)
- Token CSS already exists: `src/styles/dashboard-tokens.css`, `src/styles/main.css`, `src/styles/investigation-workspace-tokens.css`
- Shell/sidebar today is **white** (`MainLayout.vue` → `.layout__sidebar { background: var(--p-surface-0) }`)
- Login: `src/views/auth/LoginView.vue` (uses `--p-primary-*`)

## Required outcomes

1. **Brand primary** becomes deep burgundy (PrimeVue `semantic.primary` 50–950 per the doc; default brand target `#6B0F1A`).
2. **Yellow gold** tokens for CTAs / active nav accent (`#D4AF37` family) — gold is **never** used for error/critical.
3. **Sidebar / menu** uses **dark burgundy background** (`#1A050A` family), not white; active state readable on dark with gold accent.
4. **Content canvas stays light** (warm off-white); do not flip the whole app to dark mode.
5. Harmonize `dashboard-tokens.css` per the doc (status/domain tables): especially move inventory/healthy off bright leaf-green brand association; keep critical visually distinct from brand wine; keep `--rank-gold` amber separate from brand gold.
6. Spot-check **Login**, **Dashboard Home (Management Attention Center)**, and **one domain dashboard** after changes.

## Implementation order (follow the doc)

A. `src/main.ts` — `definePreset(Aura, …)` with burgundy primary scale; keep `darkModeSelector: false`.  
B. Add portal brand/nav CSS tokens (`--brand-gold-*`, `--portal-nav-*`, canvas neutrals) and import them.  
C. Restyle `src/layouts/MainLayout.vue` sidebar to dark nav tokens (fix light `primary-50` active styles on dark sidebar).  
D. Gold CTAs per doc recommendation (primary button = gold **or** document if you keep burgundy fill + gold only for accents — prefer doc’s gold primary CTA guidance).  
E. Update `src/styles/dashboard-tokens.css` current→proposed tables.  
F. Verify Login gradient/icon pick up new primary.

## Out of scope

- Attention Center layout / IA redesign  
- Full dark content chrome  
- Chart.js palette pass unless a chart still hardcodes emerald as obvious brand chrome  
- Full investigation-workspace re-skin (only touch if something clashes badly)

## Constraints

- No drive-by refactors.  
- No business logic / API / store changes.  
- Prefer existing CSS variables and PrimeVue tokens over scattered hardcoded hex in components.  
- Exclude `node_modules` / `dist` from search-replace.

## Done when (acceptance)

Match section **Acceptance criteria** in the palette markdown, including:

- No emerald/green **brand** primary on Login, header brand icon, or default primary chrome  
- Dark sidebar  
- Gold for action/active accent only  
- Critical ≠ brand burgundy; rank gold ≠ brand gold  
- WCAG-feasible contrast for nav text on dark and text on gold CTAs  

## Deliverable

- Implement the changes on a branch / PR as your workflow requires  
- PR / summary must cite: `docs/BTR-Portal-Burgundy-Gold-Color-Palette.md`  
- List files touched and any intentional deviation (should be none)

Start by reading the palette markdown, then inspect `src/main.ts` and `MainLayout.vue`, then implement in the order above.
