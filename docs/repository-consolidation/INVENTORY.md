# Repository inventory

Inspection date: **2026-06-05**  
Workspace: `D:\Project.Private\btr-platform`

## Root monorepo

| Property | Value |
|----------|-------|
| Path | `D:\Project.Private\btr-platform` |
| `.git` at root | No |
| Intended layout | `src/{BTrade3,j05-btr-distrib,j06-pkl-btrade-api,j07-btr-gudang,j07-btrade-sync}`, `docs/`, `architecture/` |

---

## `src/BTrade3`

| Property | Value |
|----------|-------|
| Type | Android (Kotlin/Gradle) — `build.gradle.kts`, no `.sln` |
| Nested `.git` | Yes |
| **Remote (origin)** | `https://github.com/druryyl/BTrade3.git` |
| **Default branch** | `master` (checked out) |
| **Commits on master** | 55 |
| **First commit** | `4084585` — 2025-08-03 — Initial commit |
| **Latest commit** | `6b351c9` — Re-Design CheckIn History screen |
| **Tracking** | `master...origin/master` (up to date) |
| **Dirty state** | Clean (no staged/unstaged tracked changes) |
| **Approx. size** | ~2,651 files, ~196 MB (includes local `app/build` artifacts) |

### Branches

**Local**

- `master` (current)
- `dev-login-validate-email-by-opus`
- `dev-login-with-validate-email`
- `dev-multi-server-2`

**Remote (`origin`)**

- `master`
- `dev-login-validate-email-by-opus`
- `dev-login-with-validate-email`

Note: `dev-multi-server-2` exists locally but was not listed under `remotes/origin/` at inspection time.

### Tags

None.

---

## `src/j05-btr-distrib`

| Property | Value |
|----------|-------|
| Type | .NET — `j05-btr-distrib.sln` at repo root |
| Nested `.git` | Yes |
| **Remote (origin)** | `https://github.com/druryyl/j05-btr-distrib.git` |
| **Default branch** | `master` (`origin/HEAD` → `origin/master`) |
| **Commits on master** | 582 |
| **First commit** | `29aed97` — 2023-07-26 — Add README.md and LICENSE.txt. |
| **Latest commit** | `232fc14` — Ubah design Sales-Omzet report menjadi Materialized Report |
| **Dirty state** | Clean |
| **Approx. size** | ~5,171 files, ~1.42 GB |

### Branches

**Local**

- `master` (current)
- `dev-faktur-klaim`
- `dev-posting-retur`
- `dev-retur-beli`
- `dev-unifikasi-btr-sync`
- `refactor-pelunasan-form`

**Remote (`origin`)**

- `master`
- `dev-fake-faktur`
- `dev-faktur-klaim`
- `dev-jude-printer-param`
- `dev-pelunasan-piutang`
- `dev-posting-retur`
- `dev-retur-beli`
- `printout-using-rdlc`
- `refactor-pelunasan-form`

### Tags

- `setup-rute`

---

## `src/j06-pkl-btrade-api`

| Property | Value |
|----------|-------|
| Type | .NET — `j06-pkl-btrade-api.sln` at repo root |
| Nested `.git` | Yes |
| **Remote (origin)** | `https://github.com/druryyl/j06-pkl-btrade-api.git` |
| **Default branch** | `master` (`origin/HEAD` → `origin/master`) |
| **Commits on master** | 34 |
| **First commit** | `07d13eb` — 2025-08-04 — Add .gitattributes. |
| **Latest commit** | `8ec3e35` — Tambah endpoint cek email apakah exist atau tidak |
| **Dirty state** | Clean |
| **Approx. size** | ~1,806 files, ~292 MB |

### Branches

**Local:** `master` (current), `dev-multi-server`  
**Remote:** `master`, `dev-multi-server`

### Tags

None.

---

## `src/j07-btr-gudang`

| Property | Value |
|----------|-------|
| Type | .NET WinForms/libraries — multiple `.csproj`, **no** `.sln` at repo root |
| Nested `.git` | Yes |
| **Remote (origin)** | `https://github.com/druryyl/j07-btr-gudang.git` |
| **Default branch** | `master` (`origin/HEAD` → `origin/master`) |
| **Commits on master** | 17 |
| **First commit** | `205b0ae` — 2026-01-30 — Add .gitattributes, .gitignore, README.md, and LICENSE.txt. |
| **Latest commit** | `de1c951` — Add LastDownload Timestamp |
| **Dirty state** | **Untracked only:** `BtrGudang.Winform/publish-107.zip` |
| **Approx. size** | ~1,265 files, ~263 MB |

### Branches

**Local / remote:** `master` only (at inspection).

### Tags

None.

---

## `src/j07-btrade-sync`

| Property | Value |
|----------|-------|
| Type | .NET — `j07-btrade-sync.sln` at repo root |
| Nested `.git` | Yes |
| **Remote (origin)** | `https://github.com/druryyl/j07-btrade-sync.git` |
| **Default branch** | `master` (`origin/HEAD` → `origin/master`) |
| **Commits on master** | 21 |
| **First commit** | `7eb4ff8` — 2025-08-06 — Add .gitattributes and .gitignore. |
| **Latest commit** | `f4ecb00` — Re-Published (new Version Number) |
| **Dirty state** | **Untracked only:** `j07-btrade-sync/publish-v21.7z` |
| **Approx. size** | ~716 files, ~75 MB |

### Branches

**Local:** `master` (current), `dev-packing-order`  
**Remote:** `master`

### Tags

None.

---

## Cross-repo notes

- All five remotes are under GitHub user/org path `github.com/druryyl/`.
- Histories are **unrelated** (different root trees and commit graphs); merges require `--allow-unrelated-histories` if not using `git subtree add` (subtree handles this internally).
- No submodule metadata was inspected; if `.gitmodules` exists in any repo, re-check before deleting nested `.git`.
