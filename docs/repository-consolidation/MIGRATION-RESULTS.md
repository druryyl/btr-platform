# Migration results

**Completed:** 2026-06-05  
**Strategy:** `git subtree add` — `master` only, no feature branches, no tags  
**Executor:** Cursor agent (automated)

## Decisions applied

| Decision | Value |
|----------|--------|
| Strategy | git subtree |
| Branches imported | `master` only |
| Tags | None (e.g. `setup-rute` not imported) |
| Refactoring | None |

## Pre-migration baselines (preserved as ancestors)

| Project | Master SHA | Message (abbrev.) |
|---------|------------|-------------------|
| BTrade3 | `6b351c9` | Re-Design CheckIn History screen |
| j05-btr-distrib | `232fc14` | Ubah design Sales-Omzet report… |
| j06-pkl-btrade-api | `8ec3e35` | Tambah endpoint cek email… |
| j07-btr-gudang | `de1c951` | Add LastDownload Timestamp |
| j07-btrade-sync | `f4ecb00` | Re-Published (new Version Number) |

All five SHAs verified: `git merge-base --is-ancestor <sha> HEAD` → true.

Total commits on monorepo `master`: **715** (includes scaffold + five subtree merge commits + full imported histories).

## Subtree merge commits

| Prefix | Subtree commit message |
|--------|------------------------|
| `src/j05-btr-distrib` | Add from `232fc14…` |
| `src/j06-pkl-btrade-api` | Add from `8ec3e35…` |
| `src/j07-btr-gudang` | Add from `de1c951…` |
| `src/j07-btrade-sync` | Add from `f4ecb00…` |
| `src/BTrade3` | Add from `6b351c9…` |

## Nested `.git` removal

| Location | Status |
|----------|--------|
| `src/**/.git` | **None** (subtree does not import `.git`; verified post-migration) |
| `_src_migration_staging/**/.git` | Staging folder **deleted** after verification |
| Root `.git` | Single monorepo repository |

## Root artifacts added

- `.gitignore` — consolidated (see PROPOSED-ROOT-GITIGNORE.md)
- `architecture/.gitkeep`
- `docs/system-map.md`, `docs/agent-handbook.md`

## Build verification (2026-06-05)

| Project | Command | Result | Notes |
|---------|---------|--------|-------|
| j06 `btrade.webapi` | `dotnet build` | **Pass** | Full sln fails on SSDT `btrade.sqldb` without Visual Studio — same as CLI-only builds pre-migration |
| j07-btr-gudang | MSBuild `BtrGudang.Winform.csproj` | **Pass** | After restoring gitignored `packages/` from pre-migration workspace |
| j07-btrade-sync | MSBuild `j07-btrade-sync.sln` | **Pass** | After `packages/` restore |
| j05-btr-distrib | MSBuild `j05-btr-distrib.sln` | **Fail** | Syncfusion WinForms refs + SSDT `btr.sql`; requires vendor assemblies / full VS environment (not caused by monorepo layout) |
| BTrade3 | `gradlew assembleDebug` | **Fail** | `SDK location not found` — needs `local.properties` / `ANDROID_HOME` (environment) |

**Conclusion:** Source tree and project structure match pre-migration Git `master`. Desktop/API builds that succeeded depend on local `packages/` and tooling unchanged from standalone repos.

## Follow-up (optional, out of scope)

- Add monorepo GitHub `origin` and push
- Import feature branches/tags if needed later
- Document Syncfusion install path for j05 builders
- Add CI restore steps for NuGet `packages.config` solutions
