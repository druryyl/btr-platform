# Implementation prompt — Fix BTR Portal Login background (burgundy mock)

Copy everything below the line into the implementation agent.

---

## Task

Fix the **Login page background** so it matches the approved **Burgundy + Gold mock**: a visible **deep burgundy → warm ivory/cream vertical gradient** behind the white login card — not a near-white / off-white wash.

This is a **Login background polish only**. Do not redesign the form layout. Do not change API/auth logic.

## Why it looks white today (root cause)

Current Login styling (approx.):

- File: `src/views/auth/LoginView.vue`
- Background uses something like:
  `linear-gradient(180deg, var(--p-primary-50) 0%, var(--p-surface-50) 45%, var(--p-surface-100) 100%)`
- After the burgundy palette work, `--p-primary-50` is still a **very light wash** (e.g. `#FBF5F6`), and `--p-surface-50/100` are light surfaces.
- So primary icon + gold button updated, but the **page chrome still reads as white**.

The UX mock is **not** “tint white with primary-50”. It is a **strong burgundy field** that softens toward cream at the bottom.

## Source of truth

1. Palette doc (tokens / brand rules):
   `D:\Project.Private\btr-platform\src\j05-btr-distrib\btr.portal.web\docs\BTR-Portal-Burgundy-Gold-Color-Palette.md`
2. Visual target: deep burgundy top → warm cream bottom; centered white card unchanged; gold Login button; burgundy brand icon.

Repo: `D:\Project.Private\btr-platform\src\j05-btr-distrib\btr.portal.web`

## Required visual result

- Login route background clearly **burgundy-led**, not white/gray.
- Suggested gradient stops (use these unless contrast forces a slight tweak):
  - `0%`: `#4A0E1C` (primary-800 / deep burgundy)
  - `45%`: `#6B0F1A` (primary-700)
  - `75%`: `#C98A98` soft mid (optional)
  - `100%`: `#F7F0DC` or `#FBF5F6` (warm ivory / primary-50)
- Prefer a dedicated token over hijacking `--p-primary-50` alone, e.g. in portal brand tokens CSS:
  - `--portal-login-bg-top: #4A0E1C`
  - `--portal-login-bg-mid: #6B0F1A`
  - `--portal-login-bg-bottom: #F7F0DC`
- Apply on the Login page root wrapper (full viewport), e.g.:
  `background: linear-gradient(180deg, var(--portal-login-bg-top) 0%, var(--portal-login-bg-mid) 42%, var(--portal-login-bg-bottom) 100%);`
- Keep the **card white** with shadow so the form stays readable.
- Do **not** make the whole app shell dark — **Login page only** (and only its background).

## Files to inspect / edit

1. `src/views/auth/LoginView.vue` — replace the near-white `--p-primary-50` gradient with the burgundy mock gradient (via tokens).
2. `src/styles/portal-brand-tokens.css` (or wherever brand tokens were added) — add `--portal-login-bg-*` if missing.
3. Confirm `src/main.ts` / global `body` background does not override Login (if `body` is light, Login wrapper must still cover full viewport with its own background).

## Constraints

- Color / CSS only for this fix.
- No drive-by refactors.
- Do not change form fields, validation, or auth.
- Gold stays on the Login CTA; do not use gold as the page background.
- Preserve WCAG: white card + dark labels must remain readable; ensure card contrast against burgundy.

## Acceptance

1. At `/portal/login` (or `/login` as routed), the area **outside** the card is unmistakably burgundy gradient, not white.
2. Card remains white/light and centered.
3. Brand icon burgundy, Login button gold — unchanged intent.
4. Other authenticated pages unchanged by this fix (sidebar dark nav from prior task stays as already implemented).

## Done

- Summarize files touched.
- Note the exact gradient / token values used.
- Cite the palette doc path in the summary.

Start by opening `LoginView.vue`, finding the current background gradient, then replacing it with the mock-equivalent burgundy gradient tokens above.
