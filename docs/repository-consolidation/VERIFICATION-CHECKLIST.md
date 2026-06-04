# Verification checklist

**Migration date:** 2026-06-05  
**Sign-off:** Automated migration run — see [MIGRATION-RESULTS.md](./MIGRATION-RESULTS.md)

## Git structure

- [x] Exactly one `.git` at `btr-platform` root
- [x] No `src/**/.git` directories (count: **0**)
- [x] `docs/repository-consolidation/` present and README status updated
- [x] `_src_migration_staging` removed (contained nested `.git`)

## History preservation (per project)

| Project | Baseline SHA | Ancestor of HEAD | `rev-list --count <sha>` |
|---------|--------------|------------------|--------------------------|
| BTrade3 | `6b351c9` | Yes | 55 |
| j05-btr-distrib | `232fc14` | Yes | 582 |
| j06-pkl-btrade-api | `8ec3e35` | Yes | 34 |
| j07-btr-gudang | `de1c951` | Yes | 17 |
| j07-btrade-sync | `f4ecb00` | Yes | 21 |

- [x] Latest commit messages match pre-migration inventory
- [x] Tags **not** imported (per decision; `setup-rute` not in monorepo)

## Working tree parity

- [x] All five `src/<project>/` trees present
- [x] Untracked publish archives remain gitignored (`publish-*.zip`, `publish-*.7z`)
- [x] Per-project `.gitignore` files unchanged under `src/`

## Build and open verification

| Project | Checklist item | Result |
|---------|----------------|--------|
| j06-pkl-btrade-api | `dotnet build btrade.webapi` | **Pass** |
| j06-pkl-btrade-api | Full `.sln` with `btrade.sqldb` | **Fail** — SSDT (MSB4278); pre-existing CLI limitation |
| j07-btr-gudang | MSBuild WinForms Release | **Pass** (with `packages/`) |
| j07-btrade-sync | MSBuild solution Release | **Pass** (with `packages/`) |
| j05-btr-distrib | MSBuild full solution | **Fail** — Syncfusion + SSDT; environment/vendor deps |
| BTrade3 | `gradlew assembleDebug` | **Fail** — Android SDK / `local.properties` |

- [x] Solutions/projects open at same paths as before migration
- [x] No solution structure or project reference changes made

## Sign-off

| Role | Name | Date | Notes |
|------|------|------|-------|
| Executor | Cursor agent | 2026-06-05 | Subtree migration + docs |
| Reviewer | _(pending)_ | | Human review recommended for j05/BTrade3 build env |

Record updates in [README.md](./README.md) when human review completes.
