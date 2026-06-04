# Proposed root `.gitignore` — consolidation analysis

Generated: **2026-06-05** (pre-migration, approved strategy: git subtree, `master` only)

## Sources merged

| Repository | `.gitignore` path | Notes |
|------------|-------------------|--------|
| BTrade3 | `src/BTrade3/.gitignore`, `app/.gitignore`, `.idea/.gitignore` | Android/Gradle/IDE |
| j05-btr-distrib | `src/j05-btr-distrib/.gitignore` | Visual Studio template + `/btr.distrib/*.7z` |
| j06-pkl-btrade-api | `src/j06-pkl-btrade-api/.gitignore` | Visual Studio template |
| j07-btr-gudang | `src/j07-btr-gudang/.gitignore` | Visual Studio template (no `.idea/`) |
| j07-btrade-sync | `src/j07-btrade-sync/.gitignore` | Visual Studio template |

Per-repo `.gitignore` files under `src/**` are **unchanged** during migration (no refactoring). The root file adds monorepo-wide coverage and documents artifacts found locally.

## Build artifacts to remain excluded

These were observed in working trees or called out in repo-specific rules:

| Artifact | Found in | Covered by |
|----------|----------|------------|
| `bin/`, `obj/`, `Debug/`, `Release/` | All .NET repos | VS template patterns |
| `.vs/`, `.idea/` (partial) | .NET / Android | VS + Android rules |
| `publish/`, `*.pubxml`, ClickOnce output | .NET | VS template |
| `**/packages/*` (NuGet) | .NET | VS template |
| `*.nupkg`, `artifacts/` | .NET | VS template |
| Gradle `.gradle`, `build/`, `.cxx` | BTrade3 | Android rules |
| `app/build/` | BTrade3 | `/build` + `**/build/` |
| `local.properties` | BTrade3 | Android (SDK paths, local) |
| `*.apk` (debug outputs under `app/build`) | BTrade3 | build dirs |
| `/btr.distrib/*.7z` | j05 only (repo rule) | Preserved at `src/j05-btr-distrib/` + root `**/*.7z` optional |
| `publish-107.zip`, `publish-v21.7z` | j07 (untracked) | Root `publish-*.zip`, `publish-*.7z` |
| `*.7z` publish archives | j05 rule | `**/*.7z` at root (broader; matches j05 intent) |
| SQL `*.mdf`, `*.ldf` | VS template | Local DB files |
| `node_modules/` | VS template | If any front-end tooling added |
| OS junk `.DS_Store`, `Thumbs.db` | — | Root monorepo section |

## Intentionally not ignored

- Source `.cs`, `.kt`, `.xml`, project files, `.sln`, `.csproj`
- `**/[Pp]ackages/build/` (MSBuild target — negation from VS template)
- Committed `publish/` **folders** that are source assets (only `publish/` directory name at repo root is ignored per VS rule — same as before)

## Implementation

The applied root file is [`.gitignore`](../../.gitignore) at monorepo root. Nested `src/*/.gitignore` files remain authoritative for paths relative to each project; Git applies all ignore rules from root downward.

## Post-migration note

If `git check-ignore -v` shows a tracked file unexpectedly ignored, fix at the **most specific** existing project `.gitignore` first; only then adjust root.
