# Repository consolidation — btr-platform monorepo

**Status:** **Completed** (2026-06-05)

Five standalone repositories were merged into this monorepo using **`git subtree add`** on each project’s **`master`** branch only. Full `master` history is preserved in the Git graph; original tip SHAs remain reachable as ancestors.

## Approval (recorded)

| Decision | Value |
|----------|--------|
| Strategy | git subtree |
| Branches | `master` only |
| Tags / feature branches | Not imported |
| Refactoring | None |

## Final layout

```text
btr-platform/
├── .git
├── .gitignore          # consolidated monorepo ignores
├── architecture/
├── docs/
│   ├── agent-handbook.md
│   ├── system-map.md
│   └── repository-consolidation/
└── src/
    ├── BTrade3
    ├── j05-btr-distrib
    ├── j06-pkl-btrade-api
    ├── j07-btr-gudang
    └── j07-btrade-sync
```

## Quick verification

```powershell
# Single root repo
Test-Path .git                                    # True
(Get-ChildItem src -Recurse -Directory -Filter .git -Force).Count  # 0

# History preserved (example j05)
git merge-base --is-ancestor 232fc14 HEAD         # exit 0
git rev-list --count 232fc14                      # 582
```

## Build sign-off summary

See [MIGRATION-RESULTS.md](./MIGRATION-RESULTS.md).

| Project | Build status (2026-06-05) |
|---------|----------------------------|
| j06 `btrade.webapi` | Pass (`dotnet build`) |
| j07-btr-gudang | Pass (MSBuild + `packages/`) |
| j07-btrade-sync | Pass (MSBuild + `packages/`) |
| j05 full solution | Blocked on Syncfusion + SSDT (environment) |
| BTrade3 | Blocked on Android SDK config (environment) |

## Document index

| Document | Purpose |
|----------|---------|
| [INVENTORY.md](./INVENTORY.md) | Pre-migration remotes, branches, sizes |
| [EXECUTION-PLAN.md](./EXECUTION-PLAN.md) | Planned steps (executed via subtree) |
| [PROPOSED-ROOT-GITIGNORE.md](./PROPOSED-ROOT-GITIGNORE.md) | Ignore consolidation analysis |
| [MIGRATION-RESULTS.md](./MIGRATION-RESULTS.md) | **Execution record and verification** |
| [RISKS-AND-ROLLBACK.md](./RISKS-AND-ROLLBACK.md) | Failure modes and recovery |
| [VERIFICATION-CHECKLIST.md](./VERIFICATION-CHECKLIST.md) | Checklist with sign-off |

## Next steps (optional)

- [ ] Add monorepo `git remote` and push to GitHub
- [ ] Archive legacy per-repo remotes with pointer README
- [ ] Import tags/feature branches later if needed

## Related

- [System map](../system-map.md)
- [Agent handbook](../agent-handbook.md)
