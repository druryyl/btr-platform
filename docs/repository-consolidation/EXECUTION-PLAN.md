# Execution plan

**Prerequisite:** Approval recorded in [README.md](./README.md).  
**Prerequisite:** Full filesystem backup of `btr-platform`.

## Strategy comparison

| Approach | History | Prefix layout | All branches/tags | Tooling |
|----------|---------|---------------|-------------------|---------|
| **git subtree add** (recommended) | Full on merged ref | Correct via `--prefix` | Manual per branch | Built into Git |
| **git filter-repo + merge** | Full; rewrites SHAs | `--to-subdirectory-filter src/Name` | Scriptable | Install `git-filter-repo` |
| **git filter-branch** | Full but slow/risky | Same as filter-repo | Scriptable | Deprecated path |
| **Copy files + new commit** | **Lost** | N/A | N/A | Do not use |

## Recommended: git subtree add

Imports each repository’s history so paths appear under `src/<project>/`, matching the current working tree layout (project files live at the root of each `src/<project>` folder, not double-nested).

### Proposed import order

1. `j05-btr-distrib` (largest history — surfaces merge issues early)
2. `j06-pkl-btrade-api`
3. `j07-btr-gudang`
4. `j07-btrade-sync`
5. `BTrade3`

Order is adjustable; unrelated histories make order mostly independent.

### Phase 0 — Backup and baseline

```powershell
# Example: backup sibling folder (adjust drive/path)
Copy-Item -Recurse "D:\Project.Private\btr-platform" "D:\Project.Private\btr-platform-backup-20260605"

# Optional: record tree hashes per project (for post-merge compare)
Get-FileHash -Algorithm SHA256 -Path (Get-ChildItem "D:\Project.Private\btr-platform\src\j05-btr-distrib" -Recurse -File | Select-Object -First 20 FullName)
```

Capture for each repo before any destructive step:

```powershell
Set-Location "D:\Project.Private\btr-platform\src\<repo>"
git rev-parse HEAD
git log -1 --oneline
git branch -a
```

### Phase 1 — Prepare root (non-destructive to nested repos)

```powershell
Set-Location "D:\Project.Private\btr-platform"

# Initialize monorepo (only if .git still absent)
git init

# Optional seed: docs + architecture placeholder (no src yet)
New-Item -ItemType Directory -Force -Path architecture | Out-Null
# git add docs architecture
# git commit -m "chore: monorepo scaffold (docs and architecture)"
```

If you prefer the first commit to contain all projects, skip the seed commit and proceed directly to subtree imports.

### Phase 2 — Import each repository (master)

Run from `btr-platform` root. Use the **existing** nested repo as the source path (Git accepts a local path as repository).

```powershell
Set-Location "D:\Project.Private\btr-platform"

git subtree add --prefix=src/j05-btr-distrib `
  "D:\Project.Private\btr-platform\src\j05-btr-distrib" master

git subtree add --prefix=src/j06-pkl-btrade-api `
  "D:\Project.Private\btr-platform\src\j06-pkl-btrade-api" master

git subtree add --prefix=src/j07-btr-gudang `
  "D:\Project.Private\btr-platform\src\j07-btr-gudang" master

git subtree add --prefix=src/j07-btrade-sync `
  "D:\Project.Private\btr-platform\src\j07-btrade-sync" master

git subtree add --prefix=src/BTrade3 `
  "D:\Project.Private\btr-platform\src\BTrade3" master
```

**Expected behavior:** Each command creates a merge commit linking unrelated history; files under `src/<name>/` should match the pre-import working tree for tracked files.

**If conflicts occur:** Resolve only path conflicts under `src/<name>/`; do not refactor code. Abort and switch to filter-repo plan if conflicts are widespread.

### Phase 3 — Optional: import additional branches and tags

**Only if approved** (increases ref count and merge complexity).

Example for a feature branch:

```powershell
git subtree merge --prefix=src/j05-btr-distrib `
  "D:\Project.Private\btr-platform\src\j05-btr-distrib" dev-faktur-klaim
```

Tag `setup-rute` on j05 (after master is in monorepo):

```powershell
# From a fresh fetch of j05 remote, or from nested repo before .git removal:
git -C "src/j05-btr-distrib" show-ref --tags
# Then cherry-pick tag onto monorepo commit that matches j05 master, or:
git fetch "D:\Project.Private\btr-platform\src\j05-btr-distrib" refs/tags/setup-rute:refs/tags/setup-rute
```

Document each imported ref in README.md after execution.

### Phase 4 — Verify before removing nested `.git`

```powershell
Set-Location "D:\Project.Private\btr-platform"

# Commit counts per subtree (should match pre-migration master counts)
git rev-list --count HEAD -- src/j05-btr-distrib
git rev-list --count HEAD -- src/BTrade3

# History accessible
git log --oneline -5 -- src/j06-pkl-btrade-api
git log --follow --oneline -5 -- src/j07-btr-gudang/BtrGudang.Winform/BtrGudang.Winform.csproj

# Compare HEAD file trees to nested repo (while nested .git still exists)
git -C src/j05-btr-distrib rev-parse master
git rev-parse HEAD:src/j05-btr-distrib  # tree-ish compare via diff tools
```

### Phase 5 — Remove nested Git metadata (destructive)

**Only after Phase 4 passes.**

```powershell
Remove-Item -Recurse -Force `
  "D:\Project.Private\btr-platform\src\BTrade3\.git",
  "D:\Project.Private\btr-platform\src\j05-btr-distrib\.git",
  "D:\Project.Private\btr-platform\src\j06-pkl-btrade-api\.git",
  "D:\Project.Private\btr-platform\src\j07-btr-gudang\.git",
  "D:\Project.Private\btr-platform\src\j07-btrade-sync\.git"
```

Confirm no nested repos remain:

```powershell
Get-ChildItem "D:\Project.Private\btr-platform\src" -Recurse -Directory -Filter ".git" -Force
```

### Phase 6 — Root remote (post-migration)

After consolidation, add a **new** monorepo remote (do not overwrite nested remotes until removed):

```powershell
git remote add origin <new-monorepo-url>
git branch -M main   # only if renaming default branch is desired — optional
git push -u origin master   # or main
```

Original GitHub repos remain as historical read-only archives unless you deprecate them explicitly.

---

## Alternative: git filter-repo + unrelated merge

Use when subtree merges fail or you need batch import of all branches.

### Install (once)

```powershell
pip install git-filter-repo
# or: winget install git-filter-repo
```

### Per-repo rewrite (bare clone recommended)

```powershell
$work = "D:\Project.Private\btr-platform-migration-work"
New-Item -ItemType Directory -Force -Path $work | Out-Null

git clone --bare "D:\Project.Private\btr-platform\src\j05-btr-distrib" "$work\j05.git"
Set-Location "$work\j05.git"
git filter-repo --to-subdirectory-filter src/j05-btr-distrib

Set-Location "D:\Project.Private\btr-platform"
git init   # if needed
git remote add j05 "$work\j05.git"
git fetch j05
git merge j05/master --allow-unrelated-histories -m "Import j05-btr-distrib history"
```

Repeat for each project with the correct subdirectory name. **All commit SHAs change** on the filtered clone; compare trees, not old SHAs.

---

## Post-execution documentation updates

Update [README.md](./README.md) with:

- Final root branch name
- New monorepo remote URL
- Import method used (subtree vs filter-repo)
- List of imported branches/tags
- Date completed and verification sign-off
