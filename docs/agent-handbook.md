# Agent handbook — btr-platform

Guidance for AI agents and developers working in the consolidated monorepo. **Do not refactor across `src/` boundaries unless explicitly requested.**

## Monorepo rules

1. **One Git root** — `.git` only at `btr-platform/`. No nested repositories under `src/`.
2. **Fixed paths** — Products live only at:
   - `src/BTrade3`
   - `src/j05-btr-distrib`
   - `src/j06-pkl-btrade-api`
   - `src/j07-btr-gudang`
   - `src/j07-btrade-sync`
3. **No cross-project refactors by default** — Duplication between repos is intentional during consolidation.
4. **History** — Use `git log -- src/<project>/` or `git log <sha>`; imported `master` tips: `6b351c9`, `232fc14`, `8ec3e35`, `de1c951`, `f4ecb00`.

## Where to work

| Task type | Location |
|-----------|----------|
| Mobile sales / Android | `src/BTrade3` |
| Distrib desktop ERP | `src/j05-btr-distrib` — open `j05-btr-distrib.sln` |
| HTTP API | `src/j06-pkl-btrade-api` — `btrade.webapi` |
| Warehouse desktop | `src/j07-btr-gudang` — `BtrGudang.Winform` |
| Sync desktop | `src/j07-btrade-sync` — `j07-btrade-sync.sln` |
| Repo / migration docs | `docs/repository-consolidation/` |
| System overview | [system-map.md](./system-map.md) |

## Build commands (from repo root)

```powershell
# j06 — API (dotnet CLI; skip sqlproj if SSDT missing)
dotnet build "src/j06-pkl-btrade-api/btrade.webapi/btrade.webapi.csproj" -c Release

# j07 gudang / sync — prefer Visual Studio MSBuild after NuGet packages/ restore
# j05 — full solution needs packages/, Syncfusion, SSDT

# BTrade3 — requires ANDROID_HOME or src/BTrade3/local.properties
Set-Location src/BTrade3
.\gradlew.bat assembleDebug
```

**Packages folder:** .NET Framework projects use `packages/` at project root (gitignored). After a clean clone, run NuGet restore in Visual Studio or copy from a machine that already built the project.

## What agents should avoid

- Moving code between `src/*` trees or creating shared libraries without approval
- Renaming solutions, namespaces, or folders
- Changing CI/build pipelines unless asked
- Importing feature branches/tags into Git without a documented decision
- Committing `packages/`, `bin/`, `obj/`, `publish/`, `*.7z`, or Android `local.properties`
- Deleting or rewriting subtree history (`git filter-repo`) without explicit approval

## Safe change patterns

- Fix a bug **inside one** `src/<project>` tree
- Add docs under `docs/` or `architecture/`
- Update root `.gitignore` only for monorepo-wide artifacts (see PROPOSED-ROOT-GITIGNORE.md)
- Run builds scoped to the project you touched

## Git subtree notes

Imports used `git subtree add --prefix=src/<name>`. To pull future updates from an old standalone remote (if still used temporarily):

```powershell
git subtree pull --prefix=src/j05-btr-distrib <remote> master
```

Prefer monorepo-only workflow once `origin` points to the new platform repository.

## Verification after edits

1. `git status` — changes only under intended `src/<project>` or `docs/`
2. Build the affected solution (see [repository-consolidation/VERIFICATION-CHECKLIST.md](./repository-consolidation/VERIFICATION-CHECKLIST.md))
3. `git log -1 -- src/<project>/` — confirm history still reachable

## Related documents

- [system-map.md](./system-map.md) — products and dependencies
- [repository-consolidation/README.md](./repository-consolidation/README.md) — migration status and sign-off
