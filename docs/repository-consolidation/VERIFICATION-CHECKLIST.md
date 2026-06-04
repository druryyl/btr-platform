# Verification checklist

Use after history import and **before** removing nested `.git` directories. Repeat key items after removal and after any CI setup.

## Git structure

- [ ] Exactly one `.git` at `btr-platform` root
- [ ] No `src/**/.git` directories remain (post Phase 5 only)
- [ ] `docs/repository-consolidation/` present and README status updated

## History preservation (per project)

Replace `<N>` with expected master commit count from [INVENTORY.md](./INVENTORY.md).

| Project | Expected commits (master path) | Commands |
|---------|-------------------------------|----------|
| BTrade3 | 55 | `git log --oneline -3 -- src/BTrade3` |
| j05-btr-distrib | 582 | `git log --oneline -3 -- src/j05-btr-distrib` |
| j06-pkl-btrade-api | 34 | `git log --oneline -3 -- src/j06-pkl-btrade-api` |
| j07-btr-gudang | 17 | `git log --oneline -3 -- src/j07-btr-gudang` |
| j07-btrade-sync | 21 | `git log --oneline -3 -- src/j07-btrade-sync` |

- [ ] `git rev-list --count HEAD -- src/<project>` is in the expected ballpark (subtree merges may not equal a naive count; investigate large deltas)
- [ ] `git log --follow` works on a sample file in each project
- [ ] Latest commit message per project matches pre-migration `git log -1` (see inventory)
- [ ] Tag `setup-rute` present if tag import was approved

## Working tree parity

- [ ] No unintended deletes/renames under `src/`
- [ ] Untracked files still only: `j07-btr-gudang/.../publish-107.zip`, `j07-btrade-sync/.../publish-v21.7z` (or explicitly ignored)
- [ ] Optional: compare file list hash sample between backup and current `src/<project>`

## Build and open verification (no structural changes)

Run from the same relative paths as before migration. Adjust configuration (Release/Debug) to match your usual workflow.

### `src/j05-btr-distrib`

```powershell
Set-Location "D:\Project.Private\btr-platform\src\j05-btr-distrib"
dotnet build "j05-btr-distrib.sln" -c Release
```

- [ ] Solution opens in Visual Studio
- [ ] Build succeeds (same as pre-migration baseline)

### `src/j06-pkl-btrade-api`

```powershell
Set-Location "D:\Project.Private\btr-platform\src\j06-pkl-btrade-api"
dotnet build "j06-pkl-btrade-api.sln" -c Release
```

- [ ] Solution opens
- [ ] Build succeeds

### `src/j07-btr-gudang`

No solution file at repo root; build entry project:

```powershell
Set-Location "D:\Project.Private\btr-platform\src\j07-btr-gudang"
dotnet build "BtrGudang.Winform\BtrGudang.Winform.csproj" -c Release
```

- [ ] Primary WinForms project opens
- [ ] Build succeeds

### `src/j07-btrade-sync`

```powershell
Set-Location "D:\Project.Private\btr-platform\src\j07-btrade-sync"
dotnet build "j07-btrade-sync.sln" -c Release
```

- [ ] Solution opens
- [ ] Build succeeds

### `src/BTrade3` (Android / Gradle)

```powershell
Set-Location "D:\Project.Private\btr-platform\src\BTrade3"
.\gradlew.bat assembleDebug
```

- [ ] Project opens in Android Studio
- [ ] Gradle build succeeds (requires JDK/Android SDK as before)

## Sign-off

| Role | Name | Date | Notes |
|------|------|------|-------|
| Executor | | | |
| Reviewer | | | |

Record results in [README.md](./README.md) when complete.
