# Repository consolidation — btr-platform monorepo

**Status:** Planning complete — **awaiting approval before execution**

This folder documents consolidating five independent Git repositories under `src/` into a single root repository at `btr-platform`, preserving full commit history for each project.

## Current state (inspected 2026-06-05)

| Item | Finding |
|------|---------|
| Root `.git` | **Not present** — monorepo not initialized yet |
| `src/` projects | Five folders, each with its own nested `.git` |
| `docs/` | Exists (this documentation) |
| `architecture/` | **Not present** — create at root during migration (empty or placeholder only) |
| `git-filter-repo` | **Not installed** on this machine (`git subtree` is available) |
| Git version | 2.45.1.windows.1 |

## Repository inventory (summary)

| Folder | Remote | Default branch | Commits (master) | Working tree |
|--------|--------|----------------|------------------|--------------|
| `src/BTrade3` | `https://github.com/druryyl/BTrade3.git` | `master` | 55 | Clean |
| `src/j05-btr-distrib` | `https://github.com/druryyl/j05-btr-distrib.git` | `master` | 582 | Clean; tag `setup-rute` |
| `src/j06-pkl-btrade-api` | `https://github.com/druryyl/j06-pkl-btrade-api.git` | `master` | 34 | Clean |
| `src/j07-btr-gudang` | `https://github.com/druryyl/j07-btr-gudang.git` | `master` | 17 | Untracked `BtrGudang.Winform/publish-107.zip` |
| `src/j07-btrade-sync` | `https://github.com/druryyl/j07-btrade-sync.git` | `master` | 21 | Untracked `j07-btrade-sync/publish-v21.7z` |

See [INVENTORY.md](./INVENTORY.md) for branches, sizes, and first-commit dates.

## Recommended strategy

**Primary:** `git subtree add` (merge each repository’s `master` under `src/<name>/` with full history).

**Alternative:** `git filter-repo --to-subdirectory-filter` on bare clones, then `git merge --allow-unrelated-histories` (better if you need all remote branches/tags rewritten into the monorepo in one pass).

**Not recommended here:** `git subtree split` / squash imports — they drop or flatten history. Plain copy without Git merge — no history.

Rationale: working trees already match the desired layout (repo root = `src/<folder>/` content). Subtree add imports history under the correct prefix without rewriting every commit by hand. `filter-repo` is the fallback if subtree merges conflict or you require importing **all** branches and tags systematically.

## Execution plan (high level)

Detailed steps: [EXECUTION-PLAN.md](./EXECUTION-PLAN.md).

1. **Backup** — full copy of `D:\Project.Private\btr-platform` and confirm GitHub remotes are up to date.
2. **Initialize** root `git init` at `btr-platform` (no source commit yet, or docs-only seed commit).
3. **Import** each repo in a fixed order via `git subtree add --prefix=src/<name> <path-to-repo> master` (or `filter-repo` + merge).
4. **Import optional refs** — fetch/merge additional branches and tag `setup-rute` if required (see inventory).
5. **Verify** history per path (`git log -- src/<name>/`) and file tree vs pre-migration snapshot.
6. **Remove** nested `src/*/.git` directories only after verification passes.
7. **Build verify** — solutions/projects listed in [VERIFICATION-CHECKLIST.md](./VERIFICATION-CHECKLIST.md).
8. **Document** final state and remote setup for new monorepo.

## Risks and rollback

See [RISKS-AND-ROLLBACK.md](./RISKS-AND-ROLLBACK.md).

## Verification

See [VERIFICATION-CHECKLIST.md](./VERIFICATION-CHECKLIST.md).

## Approval gate

Do **not** proceed with steps that delete nested `.git` directories or rewrite history until you confirm:

- [ ] Approved migration strategy (subtree vs filter-repo)
- [ ] Import order (proposed: j05 → j06 → j07-gudang → j07-sync → BTrade3 — oldest/largest first; adjustable)
- [ ] Whether to import **only `master`** or **all local/remote branches and tags**
- [ ] Whether untracked publish archives in j07 repos should stay untracked (recommended: yes, add root `.gitignore` patterns later if needed — out of scope for code changes unless requested)

Reply with approval (and any choices above) to begin execution.

## Document index

| Document | Purpose |
|----------|---------|
| [INVENTORY.md](./INVENTORY.md) | Remotes, branches, tags, sizes |
| [EXECUTION-PLAN.md](./EXECUTION-PLAN.md) | Step-by-step commands |
| [RISKS-AND-ROLLBACK.md](./RISKS-AND-ROLLBACK.md) | Failure modes and recovery |
| [VERIFICATION-CHECKLIST.md](./VERIFICATION-CHECKLIST.md) | Post-migration checks |
