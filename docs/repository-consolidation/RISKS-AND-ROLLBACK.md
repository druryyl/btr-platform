# Risks and rollback plan

## Risk register

| ID | Risk | Likelihood | Impact | Mitigation |
|----|------|------------|--------|------------|
| R1 | Loss of commit history | Low if using subtree/filter-repo | Critical | Backup + verify `git log -- <prefix>` before deleting nested `.git` |
| R2 | Wrong path prefix (double `src/j05/j05/...`) | Medium | High | Always use `--prefix=src/<exact-folder-name>` matching current layout |
| R3 | Merge conflicts on subtree add | Low–medium | Medium | Resolve without code refactors; abort and use filter-repo alternative |
| R4 | Duplicate or missing files vs working tree | Medium | High | Directory diff before removing nested `.git`; keep backup |
| R5 | Accidental commit of build artifacts / publish zips | Medium | Medium | Do not `git add` untracked zips in j07; existing `.gitignore` rules preserved per subtree |
| R6 | Broken local builds after migration | Low | High | Run verification builds from same paths as before (see checklist) |
| R7 | Lost branches/tags (master-only import) | High if not planned | Medium | Explicit decision: import only `master` vs all refs |
| R8 | Changed commit SHAs (filter-repo) | Certain with filter-repo | Low | Document that old SHAs ≠ monorepo SHAs; use messages/dates for archaeology |
| R9 | IDE locks `.git` on Windows | Medium | Low | Close Visual Studio/Android Studio during `.git` removal |
| R10 | Push to wrong remote | Low | High | Add monorepo `origin` only after review; never force-push old repos without intent |

## Rollback levels

### Level 1 — Before nested `.git` removal

**Safest rollback point.**

1. Delete root `.git` only: `Remove-Item -Recurse -Force "D:\Project.Private\btr-platform\.git"`
2. Nested repositories under `src/*/` are unchanged and still fully functional.
3. Restore from backup if working tree was modified during conflict resolution.

### Level 2 — After nested `.git` removal

1. Restore entire folder from `btr-platform-backup-*` (filesystem copy).
2. Or re-clone each project from GitHub remotes listed in [INVENTORY.md](./INVENTORY.md) into `src/<name>/`.

Original remotes (as of inspection):

- `https://github.com/druryyl/BTrade3.git`
- `https://github.com/druryyl/j05-btr-distrib.git`
- `https://github.com/druryyl/j06-pkl-btrade-api.git`
- `https://github.com/druryyl/j07-btr-gudang.git`
- `https://github.com/druryyl/j07-btrade-sync.git`

### Level 3 — After monorepo pushed to GitHub

1. Do not force-push without team agreement.
2. Reset local monorepo to pre-push commit or delete remote repo and re-run migration from Level 1 backup.
3. Keep old per-repo GitHub repositories intact until monorepo is validated in production/CI.

## Pre-flight checklist

- [ ] Full directory backup completed
- [ ] `git status` clean on all five repos (or only acceptable untracked zips)
- [ ] Import scope agreed (master only vs branches/tags)
- [ ] Team notified not to push to old repos during migration window
- [ ] Disk space sufficient (~2.5 GB+ working set, plus backup)

## During migration — stop conditions

Stop and reassess if:

- Subtree merge reports conflicts outside `src/<project>/`
- `git rev-list --count HEAD -- src/<project>` differs significantly from pre-migration master count
- Unexpected deletion or rename of project files
- `git filter-repo` run against non-bare clone by mistake (repository corruption)

## Communication

After successful migration:

- Point developers to single clone URL
- Document that per-repo `src/*/.git` is gone
- Optional: archive old GitHub repos with README pointing to monorepo
