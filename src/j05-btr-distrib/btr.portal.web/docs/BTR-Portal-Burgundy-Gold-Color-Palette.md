# BTR Portal — Burgundy + Yellow Gold Color Palette

**Status:** UX-approved direction (Lori Evans) — ready for implementation  
**Scope:** Color system only (no layout/IA redesign in this task)  
**App:** `btr.portal.web` (Vue 3 + PrimeVue 4 Aura)  
**Repo path:** `D:\Project.Private\btr-platform\src\j05-btr-distrib\btr.portal.web`  
**Authoring agent:** Product UX Architect  
**Implementing agent:** apply tokens in code; do not invent alternate brand hues

---

## 1. Locked product decisions

| Decision | Value |
|---|---|
| Brand direction | **Burgundy + Yellow Gold** (premium) |
| Rejected brand hues | Green / teal / emerald as brand (Aura default primary feel) |
| Also considered, not chosen | Navy+Gold, Plum+Gold |
| Shell / menu | **Dark background** (near-black burgundy), **not white** |
| Canvas / content | Keep **light** warm off-white for dense dashboards (readability) |
| Gold meaning | Primary action, active nav accent, brand highlight — **never** error/critical |
| Burgundy meaning | Brand chrome, titles, selected emphasis |
| Status colors | Stay **semantic and distinct** from brand burgundy |

**Priority order for tradeoffs:** Business outcome → productivity → usability → visual design.

---

## 2. Codebase map (where color lives today)

Read these first before editing:

| File | Role today |
|---|---|
| `src/main.ts` | PrimeVue `Aura` preset, **no custom primary** (`darkModeSelector: false`) |
| `src/styles/main.css` | Global body text/bg via `--p-text-color`, `--p-surface-50` |
| `src/styles/dashboard-tokens.css` | Domain accents, KPI status, rank medals, table tints |
| `src/styles/investigation-workspace-tokens.css` | Investigation workspace neutrals (mostly slate) |
| `src/layouts/MainLayout.vue` | Header + **sidebar** (`background: var(--p-surface-0)` = white) |
| `src/views/auth/LoginView.vue` | Login gradient uses `--p-primary-50` → surfaces; icon `--p-primary-color` |

**Implication:** Changing brand green → burgundy is primarily a **PrimeVue primary preset** change. Dark nav is a **MainLayout (+ optional portal nav tokens)** change. Domain/KPI tokens in `dashboard-tokens.css` should be **harmonized**, not blindly recolored to burgundy.

---

## 3. Recommended palette (canonical hex)

### 3.1 Brand — Burgundy scale (PrimeVue `primary`)

Use as Aura `semantic.primary` scale (50–950).

| Token step | Hex | Usage |
|---|---|---|
| primary-50 | `#FBF5F6` | Soft brand wash, login gradient top |
| primary-100 | `#F4E4E8` | Chip / soft selected bg on light canvas |
| primary-200 | `#E5C5CD` | Borders tied to brand |
| primary-300 | `#C98A98` | Rare |
| primary-400 | `#A84D63` | Icons on light |
| primary-500 | `#8B1E3A` | Mid brand |
| primary-600 | `#7A1832` | Hover on light brand fills |
| **primary-700** | **`#6B0F1A`** | **Default brand / `--p-primary-color` target** |
| primary-800 | `#4A0E1C` | Strong text on gold buttons if needed |
| primary-900 | `#2A0A12` | Dark nav hover / elevated dark chrome |
| primary-950 | `#1A050A` | Darkest nav background |

### 3.2 Brand — Yellow Gold (custom CSS, not PrimeVue status)

| Token | Hex | Usage |
|---|---|---|
| `--brand-gold-500` | `#D4AF37` | Primary CTA fill, active nav accent bar |
| `--brand-gold-600` | `#C9A227` | CTA hover |
| `--brand-gold-100` | `#F7F0DC` | Soft gold chip / nav brand wordmark on dark |
| `--brand-gold-ink` | `#3F2E0A` | Text on gold buttons when white fails contrast |

**Do not reuse** existing `--rank-gold: #d97706` for brand CTAs. Rank medals stay amber/bronze semantic.

### 3.3 Portal shell — dark navigation

| Token | Hex | Usage |
|---|---|---|
| `--portal-nav-bg` | `#1A050A` | Sidebar background |
| `--portal-nav-bg-elevated` | `#2A0A12` | Hover row |
| `--portal-nav-bg-active` | `#3D1220` | Active row fill |
| `--portal-nav-border` | `#4A0E1C` | Sidebar right edge |
| `--portal-nav-text` | `#F7F0DC` | Nav link text |
| `--portal-nav-muted` | `#C4A8B0` | Section headings (`EXECUTIVE`, etc.) |
| `--portal-nav-icon` | `#E5C5CD` | Idle icons |
| `--portal-nav-active-accent` | `#D4AF37` | 3px left bar / active indicator |
| `--portal-nav-brand` | `#D4AF37` | “BTR Portal” wordmark in sidebar (if moved) |

Header can stay light (`--p-surface-0`) for density/readability **or** match dark chrome in a follow-up. **This task:** dark **menu/sidebar** required; header may remain light unless trivial to align.

### 3.4 Light canvas / content (keep)

| Token | Hex | Usage |
|---|---|---|
| `--portal-canvas` | `#FBF7F5` | App content bg (warm, replaces cold gray-blue feel) |
| `--portal-surface` | `#FFFFFF` | Cards |
| `--portal-border` | `#E8DFE2` | Card/table borders (warm neutral) |
| `--portal-text` | `#1F1218` | Primary text on light |
| `--portal-text-muted` | `#6B5A61` | Secondary text |

Optional: map body background from `--p-surface-50` toward `#FBF7F5` via preset surface tweak or `:root` override in `main.css`.

### 3.5 Semantic status (keep meaning; tune to avoid brand clash)

Brand burgundy is **deep wine**. Status critical must stay **clearly “bad”**, not “brand.”

| Token (existing) | Current | Proposed | Notes |
|---|---|---|---|
| `--kpi-status-healthy-color` | `#15803d` | `#0F766E` | Teal-green OK for **status**; not brand |
| `--kpi-status-healthy-bg` | `#dcfce7` | `#CCFBF1` | Match teal healthy |
| `--kpi-status-warning-color` | `#c2410c` | `#B45309` | Unchanged intent |
| `--kpi-status-warning-bg` | `#ffedd5` | `#FFEDD5` | Keep |
| `--kpi-status-critical-color` | `#b91c1c` | `#9F1239` | Rose-red; distinct from brand `#6B0F1A` |
| `--kpi-status-critical-bg` | `#fee2e2` | `#FFE4E6` | Keep light rose |
| `--kpi-status-unknown-*` | slate | keep | |
| `--kpi-status-stable-*` | blue | keep | |

**Rule:** Never paint critical badges with `--brand-gold-*`. Never paint CTAs with critical red.

### 3.6 Domain accents (`dashboard-tokens.css`)

Keep multi-hue domain coding (helps scan). Soften only where brand-green inventory reads as “old brand.”

| Token | Current | Proposed | Rationale |
|---|---|---|---|
| `--domain-sales-color` | `#2563eb` | keep | |
| `--domain-finance-color` | `#dc2626` | `#BE123C` | Align with rose critical family |
| `--domain-collection-color` | `#ea580c` | keep | |
| `--domain-inventory-color` | `#16a34a` | `#0F766E` | Avoid bright “brand green” association |
| `--domain-inventory-tint` | `#f7fff7` | `#F0FDFA` | Match |
| `--domain-purchasing-color` | `#9333ea` | keep | |
| `--domain-customer-color` | `#0891b2` | keep | |
| `--domain-salesman-color` | `#4f46e5` | keep | |
| `--domain-alert-color` | `#be123c` | keep | Already rose |
| `--domain-portfolio-color` | `#0891b2` | keep | |

Update matching `*-tint` only when the solid changes.

### 3.7 Rank medals

Keep as-is (semantic ranking, not brand):

- `--rank-gold: #d97706` / `--rank-gold-bg: #fef3c7`
- silver / bronze unchanged

---

## 4. Implementation plan (for coding agent)

### Step A — PrimeVue primary preset (brand)

**File:** `src/main.ts`

- Import `definePreset` from `@primevue/themes`.
- Create `BtrAura = definePreset(Aura, { semantic: { primary: { 50..950: <section 3.1> } } })`.
- Pass `preset: BtrAura` instead of raw `Aura`.
- Keep `darkModeSelector: false` unless product asks otherwise.

This should recolor Login primary icon/gradient and most `var(--p-primary-*)` usages app-wide.

### Step B — Portal brand + nav tokens

**File (new or extend):** `src/styles/main.css` or `src/styles/portal-brand-tokens.css` (prefer new file imported from `main.ts` next to other token CSS).

Define:

- `--brand-gold-*` (section 3.2)
- `--portal-nav-*` (section 3.3)
- `--portal-canvas` / text neutrals (section 3.4)

Import order in `main.ts`: after PrimeVue CSS / with existing token imports.

### Step C — Dark sidebar (required)

**File:** `src/layouts/MainLayout.vue` scoped styles

Replace white sidebar:

| Selector | Today | Target |
|---|---|---|
| `.layout__sidebar` | `background: var(--p-surface-0)` | `var(--portal-nav-bg)` + border `var(--portal-nav-border)` |
| `.layout__nav-heading` | muted Prime text | `var(--portal-nav-muted)` |
| `.layout__nav-link` | `var(--p-text-color)` | `var(--portal-nav-text)` |
| `.layout__nav-link:hover` | `var(--p-surface-100)` | `var(--portal-nav-bg-elevated)` |
| `.layout__nav-link--active` | `primary-50` / `primary-700` | `var(--portal-nav-bg-active)` + gold accent + `var(--brand-gold-100)` text |
| `.layout__nav-icon` | inherit | `var(--portal-nav-icon)`; active → gold |

Do **not** leave active state as light `primary-50` on a dark sidebar (contrast bug).

### Step D — Gold CTAs

Where dashboard home uses PrimeVue `severity="primary"` / severity primary for **Open Alert Center** / **Refresh**:

- Either map PrimeVue primary button to gold via preset component tokens, **or**
- Add a local `.portal-btn--gold` using `--brand-gold-500` for those hero actions.

Prefer preset primary button = burgundy fill **or** gold fill — pick one system-wide:

**Recommendation:**  
- **Primary button = gold** (`#D4AF37` bg, `#3F2E0A` or white text — verify contrast).  
- **Secondary / outlined = burgundy**.  

Document the choice in the PR if you flip default primary button away from burgundy fill.

### Step E — Dashboard tokens harmonize

**File:** `src/styles/dashboard-tokens.css`

- Apply section 3.5–3.6 replacements.
- Change `--dashboard-table-row-hover` from blue-tint `#f0f7ff` to warm `#FBF5F6` (primary-50).
- Change `--dashboard-table-row-top` from blue alpha to burgundy alpha, e.g. `rgb(107 15 26 / 4%)`.

### Step F — Login check

**File:** `src/views/auth/LoginView.vue`

- Gradient already uses `--p-primary-50`; should become warm burgundy wash after Step A.
- Optionally set Login button to gold utility class for parity with mockups.

### Step G — Out of scope (do not expand unless asked)

- Full dark-mode content canvas
- Chart.js color arrays (search separately if charts stay emerald)
- IA / Attention Center layout redesign
- Investigation workspace full re-skin (optional later: only if `--iw-*` clashes)

---

## 5. Search / replace hints for implementor

Run ripgrep in `btr.portal.web` (exclude `node_modules`, `dist`):

```text
--p-primary
--p-surface-0
layout__sidebar
#16a34a
#15803d
#2563eb
severity="primary"
definePreset|preset:\s*Aura
```

Hardcoded report green noted: `PurchasingReportView.vue` uses `var(--p-green-700)` — leave unless it reads as brand chrome.

---

## 6. Acceptance criteria

1. No emerald/green **brand** primary on Login, header brand icon, or default primary links.
2. Sidebar/menu background is **dark burgundy** (`#1A050A` family), not white.
3. Active nav uses **gold accent**, readable on dark.
4. Gold never used for critical/error badges.
5. KPI critical/warning/healthy still distinguishable at a glance; critical ≠ brand wine.
6. Domain inventory accent no longer bright leaf-green (`#16a34a`).
7. Rank gold medals still amber (`#d97706`), visually distinct from brand gold (`#D4AF37`).
8. Contrast: nav text on dark and text on gold CTAs meet WCAG AA for normal UI text where feasible.

---

## 7. Reference mock intent (UX)

- Login: burgundy gradient + gold Login CTA (approved taste).
- Home: dark nav + light Management Attention Center canvas; gold for Alert Center / Refresh; burgundy for critical chips.

Visual mocks were explored in Product UX Architect chat; **this markdown is the source of truth for hex/token application.**

---

## 8. Copy-paste starter: CSS variables

```css
:root {
  /* Brand gold */
  --brand-gold-100: #f7f0dc;
  --brand-gold-500: #d4af37;
  --brand-gold-600: #c9a227;
  --brand-gold-ink: #3f2e0a;

  /* Dark nav */
  --portal-nav-bg: #1a050a;
  --portal-nav-bg-elevated: #2a0a12;
  --portal-nav-bg-active: #3d1220;
  --portal-nav-border: #4a0e1c;
  --portal-nav-text: #f7f0dc;
  --portal-nav-muted: #c4a8b0;
  --portal-nav-icon: #e5c5cd;
  --portal-nav-active-accent: #d4af37;
  --portal-nav-brand: #d4af37;

  /* Light canvas */
  --portal-canvas: #fbf7f5;
  --portal-surface: #ffffff;
  --portal-border: #e8dfe2;
  --portal-text: #1f1218;
  --portal-text-muted: #6b5a61;
}
```

```ts
// src/main.ts — conceptual; implementor verifies PrimeVue 4 definePreset shape
import { definePreset } from '@primevue/themes'
import Aura from '@primevue/themes/aura'

const BtrAura = definePreset(Aura, {
  semantic: {
    primary: {
      50: '#fbf5f6',
      100: '#f4e4e8',
      200: '#e5c5cd',
      300: '#c98a98',
      400: '#a84d63',
      500: '#8b1e3a',
      600: '#7a1832',
      700: '#6b0f1a',
      800: '#4a0e1c',
      900: '#2a0a12',
      950: '#1a050a',
    },
  },
})
```

---

## 9. Handoff checklist for implementing agent

- [ ] Read this file fully before coding  
- [ ] Edit `main.ts` preset  
- [ ] Add portal brand/nav tokens CSS + import  
- [ ] Restyle `MainLayout.vue` sidebar to dark nav tokens  
- [ ] Harmonize `dashboard-tokens.css` per tables above  
- [ ] Spot-check Login + Dashboard Home + one domain dashboard  
- [ ] Do not change business logic / API  
- [ ] PR description cites this document path  

**Document path (workspace):** `/workspace/portal-ux/BTR-Portal-Burgundy-Gold-Color-Palette.md`  
**Also copy into repo (recommended):** `btr.portal.web/docs/BTR-Portal-Burgundy-Gold-Color-Palette.md` or project `docs/` equivalent.
