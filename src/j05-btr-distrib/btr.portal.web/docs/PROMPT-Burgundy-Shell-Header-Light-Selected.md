# Implementation prompt — Burgundy shell + header, light selected menu

Copy everything below the line into the implementation agent.

---

## Task

Update **BTR Portal** chrome so:

1. **Menu shell (sidebar)** uses **burgundy** (brand wine), not near-black / charcoal that barely reads as burgundy.
2. **Header** uses the **same burgundy family** as the menu shell (today’s header is still light/cream — that must change).
3. **Selected menu item** uses a **light background** (cream / primary-50 wash) with dark/burgundy text — **not** a darker burgundy row + gold outline as the primary selected treatment.

Color / CSS only. No IA, routing, or business-logic changes.

## Repo

`D:\Project.Private\btr-platform\src\j05-btr-distrib\btr.portal.web`

## Source of truth

- Palette doc: `docs/BTR-Portal-Burgundy-Gold-Color-Palette.md`
- Brand primary target: `#6B0F1A` (primary-700); deep chrome: `#4A0E1C` / `#2A0A12`
- Gold (`#D4AF37`) = accent only (optional thin active indicator or icons), **not** the selected row fill

## Current problem (from live UI)

- Sidebar may already be very dark; it should clearly read **burgundy**, not black.
- Header bar is still **light** while sidebar is dark — chrome feels split.
- Selected nav (`EX01 Executive` etc.) currently uses a dark elevated row; product wants **light selected background** for clear affordance on a dark burgundy shell.

## Required visual result

### Sidebar (`.layout__sidebar`)

| Element | Target |
|---|---|
| Sidebar background | Burgundy chrome, e.g. `#4A0E1C` or `#6B0F1A` (prefer `#4A0E1C` as shell bg so it isn’t black) |
| Section headings | Muted rose/cream on burgundy, e.g. `#C4A8B0` / `#E5C5CD` |
| Idle nav link text/icons | Light cream `#F7F0DC` / `#FBF5F6` |
| Hover row | Slightly lighter burgundy, e.g. `#5C1224` |
| **Selected row** | **Light background** `#FBF5F6` or `#F7F0DC`; text/icon `#6B0F1A` or `#1F1218`; optional **3px gold left accent** `#D4AF37` only as secondary cue |
| Sidebar border | Burgundy edge `#2A0A12` / `#4A0E1C` |

**Do not** use white `#FFFFFF` or `--p-surface-0` for the sidebar.

### Header (`.layout__header`)

| Element | Target |
|---|---|
| Header background | Same burgundy family as sidebar (match shell: `#4A0E1C` recommended) |
| Border bottom | Darker burgundy `#2A0A12` |
| Brand title / subtitle | Cream / muted cream on burgundy |
| Brand icon | Gold `#D4AF37` **or** cream — readable on burgundy (burgundy icon on burgundy header fails) |
| User name | Cream |
| User role | Muted cream |
| Logout button | Outline cream/gold on burgundy, or solid gold CTA — readable; avoid white-filled button that looks like light-theme leftover |
| Presentation mode chip | Dark-burgundy-compatible (tinted burgundy/gold), not light purple-on-cream |

Header and sidebar should feel like **one chrome band**, not light header + dark rail.

### Content canvas

Keep **light** (`#FBF7F5` / surface) for `Management Attention Center` and cards. Do **not** make the main content dark in this task.

## Files to edit (primary)

1. `src/layouts/MainLayout.vue` — header + sidebar scoped styles (main work)
2. Brand/nav tokens CSS (e.g. `src/styles/portal-brand-tokens.css`) — update `--portal-nav-*` and add `--portal-header-*` if useful:
   - `--portal-nav-bg: #4A0E1C`
   - `--portal-nav-bg-hover: #5C1224`
   - `--portal-nav-bg-active: #FBF5F6`  ← **light selected**
   - `--portal-nav-text: #F7F0DC`
   - `--portal-nav-text-active: #6B0F1A`
   - `--portal-nav-muted: #C4A8B0`
   - `--portal-nav-active-accent: #D4AF37`
   - `--portal-header-bg: #4A0E1C` (same as nav bg)
   - `--portal-header-text: #F7F0DC`
   - `--portal-header-muted: #C4A8B0`

Replace any selected styles still using dark `--portal-nav-bg-active` + gold border-as-fill.

## Constraints

- CSS / tokens only; no store/API/router changes.
- No drive-by refactors of dashboard cards.
- Gold is accent, not error color, not selected row fill.
- Preserve WCAG contrast: cream on burgundy idle; dark text on light selected row; header text on burgundy.

## Acceptance

1. Sidebar background clearly **burgundy** (not black, not white).
2. Header background **burgundy**, visually continuous with sidebar.
3. Selected menu item has a **light** background and dark/burgundy label (obvious at a glance).
4. Idle items remain light text on burgundy.
5. Main content area stays light.
6. Logout / presentation controls remain readable on the dark header.

## Done

- List files touched and final token values.
- Cite `docs/BTR-Portal-Burgundy-Gold-Color-Palette.md`.
- Note any small contrast tweaks.

Start by opening `MainLayout.vue` header + `.layout__sidebar` / `.layout__nav-link--active` styles, then align tokens and selected-state to the table above.
