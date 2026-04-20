# Git mirrors + worktrees (Testified-OSS workspace)

**Workspace root:** the directory that contains `git/mirrors/` and `git/wt/` (see `HEARTBEAT.md`). All paths below are relative to that root unless noted.

This file replaces any external **using-git-worktrees** skill for this agent: everything needed for HEARTBEAT lives here and under `HEARTBEAT.md` / `tools/github-prs.md`.

## Layout (canonical)

| Artifact | Path pattern | Notes |
|----------|--------------|--------|
| Bare mirror | `git/mirrors/${REPO//\//__}.git` | e.g. `testified-oss/pegats-clifx-dotnet` → `testified-oss__pegats-clifx-dotnet.git` |
| Linked worktrees | `git/wt/…` | Sibling of `mirrors/`; paths passed to `git worktree add` are **relative to the bare repo** (`../wt/...`) so worktrees land under `git/wt/`, not nested inside `.git` |

Use a **single** mirror name per `REPO` so probes, fetch, and worktree list stay consistent. If an older mirror exists under a different name (e.g. plain `repo.git` from an early clone), prefer renaming or recloning to the `owner__repo.git` pattern to match `HEARTBEAT.md` Section D.

## Before any `git worktree` command

1. Set `REPO` to `owner/name` (`HEARTBEAT.md` Section C).
2. `MIRROR="git/mirrors/${REPO//\//__}.git"`.
3. Run mirror ensure + worktree add **only** with `git -C "$MIRROR" …` (or `git -C "git/mirrors/…"`). **Do not** run `git worktree add` from the workspace root without `-C` pointing at the bare mirror — that attaches to the wrong repository (session `3edf5434`: skill path unreadable in sandbox; agent then used root-level `git worktree add git/wt/scan`, which does not match this layout).

## D — Ensure mirror (used successfully in agent sessions)

From workspace root:

```bash
REPO="owner/name"   # already set in HEARTBEAT C
MIRROR="git/mirrors/${REPO//\//__}.git"
mkdir -p git/mirrors git/wt

if [ ! -d "$MIRROR" ]; then
  git clone --mirror "https://github.com/${REPO}.git" "$MIRROR"
else
  git -C "$MIRROR" fetch origin --prune
fi
```

**Session reference:** `bf23af9d-d9f0-44e5-953f-545e417b5b85` — `mkdir -p …/git/mirrors`, `git clone --mirror https://github.com/…`, `git -C … fetch --prune`, then `git -C …/pegats-clifx-dotnet.git worktree add …` (see **Pitfalls** for path form).

## Default-branch scan worktree

Resolve default branch (already required in HEARTBEAT):

```bash
DEF_BRANCH="$(gh repo view "$REPO" --json defaultBranchRef -q .defaultBranchRef.name)"
SLUG="${REPO//\//__}"
WT_SCAN="git/wt/${SLUG}-${DEF_BRANCH}-scan"
```

Add **from the bare mirror** using a path relative to the mirror so the checkout appears under `git/wt/`:

```bash
if [ -d "$WT_SCAN" ]; then
  git -C "$MIRROR" worktree list | grep -q "$WT_SCAN" || true
else
  git -C "$MIRROR" worktree add "../wt/${SLUG}-${DEF_BRANCH}-scan" "$DEF_BRANCH"
fi
```

After `worktree add`, the real tree is at `git/wt/${SLUG}-${DEF_BRANCH}-scan` (because `../wt/` from `git/mirrors/<mirror>.git` resolves to `git/wt/`).

## Topic branch (Section G)

From the same `$MIRROR`:

```bash
ISSUE=N
BRANCH="feat/issue-${ISSUE}-triage"   # adjust per tools/conventions.md
WT_TOPIC="git/wt/${SLUG}-issue-${ISSUE}"
git -C "$MIRROR" worktree add -b "$BRANCH" "../wt/${SLUG}-issue-${ISSUE}" "$DEF_BRANCH"
# implement, commit inside "$WT_TOPIC"
```

(Equivalent: `git worktree add -b …` with paths as in `HEARTBEAT.md` once `-C "$MIRROR"` is set.)

## PR head worktree (Section J)

Same as `tools/github-prs.md`: after `git -C "$MIRROR" fetch origin "$HEAD"`:

```bash
P=<pr_number>
git -C "$MIRROR" worktree add "../wt/${SLUG}-pr-${P}" "$HEAD"
```

## Push from a mirror-backed worktree (`remote.origin.mirror=true`)

Mirrors set `mirror=true`; normal `git push -u origin <branch>` often fails with *`--mirror can't be combined with refspecs`*. From the **topic or PR worktree**, after commits:

```bash
BRANCH="$(git branch --show-current)"
git push "https://github.com/${REPO}.git" "HEAD:refs/heads/${BRANCH}"
```

(Full draft PR flow: `tools/github-prs.md`.)

## Teardown (Section H)

Remove each linked path **as registered on that mirror**:

```bash
git -C "$MIRROR" worktree remove --force "../wt/${SLUG}-${DEF_BRANCH}-scan" 2>/dev/null || true
git -C "$MIRROR" worktree remove --force "../wt/${SLUG}-issue-${ISSUE}" 2>/dev/null || true
git -C "$MIRROR" worktree remove --force "../wt/${SLUG}-pr-${P}" 2>/dev/null || true
```

If removal from workspace root fails with *`not a working tree`*, the path was never linked to the repo you think — use `git -C "$MIRROR" worktree list` and remove the **exact** path shown (session `3edf5434`: `git worktree remove --force git/wt/scan` from root failed).

**Keep** bare mirrors; only remove ephemeral worktrees.

## Pitfalls (from sessions)

| What went wrong | Why | Fix |
|-----------------|-----|-----|
| `git worktree add git/wt/scan main` from workspace root | Worktree binds to wrong Git dir, or path nested under mirror | Always `git -C "$MIRROR" worktree add "../wt/<name>" <branch>` |
| `worktree add` second arg `HEAD` vs branch name | Detached HEAD at arbitrary tip | Prefer explicit `"$DEF_BRANCH"` for scan worktree |
| Worktree path `git/wt/...` passed as **relative to mirror** without `../` | Git creates `git/mirrors/<mirror>.git/git/wt/...` | Use `../wt/...` from mirror root |
| Skill path outside sandbox | Agent could not read `~/.claude/skills/...` | Use **this file** only |

## Cross-links

- `HEARTBEAT.md` — Sections A, D, G, H, J
- `tools/github-prs.md` — draft PR + PR worktree + push URL
- `tools/github-issues.md` — when to add second worktree
- `tools/scan-paths.md` — what to do inside the scan worktree
