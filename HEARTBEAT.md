# HEARTBEAT.md — Testified-OSS improvement (random repo + worktrees + issues)

**Workspace root:** `/Users/luucrew/.openclaw/workspaces/testified-oss-coder`

**Git worktrees:** Before any `git worktree`, mirror, or teardown command, read **`tools/git-worktrees.md`** (workspace-local; cron sandboxes cannot read `~/.claude/skills/...`).

## A — Install playbooks

- `TOOLS.md` — startup order
- `tools/conventions.md`
- `tools/target-repos.md`
- `tools/git-worktrees.md` — mirror paths, `git -C`, `git/wt/` layout, push/teardown (**required** before worktree commands)
- `tools/github-issues.md`
- `tools/github-prs.md`
- `tools/scan-paths.md`

## B — Auth

```bash
gh auth status
```
On failure: log to `memory/YYYY-MM-DD.md` and stop.

## C — Random repo (exactly one)

Pick **`OWNER/REPO`** uniformly at random from `tools/target-repos.md` (use `shuf`). Set:

```bash
REPO="OWNER/REPO"
```
Every `gh` call must use `--repo "$REPO"`.

## D — Mirror + default-branch worktree (always)

Full commands, path pitfalls, and teardown: **`tools/git-worktrees.md`**.

Summary (from workspace root; `SLUG="${REPO//\//__}"`, mirror `git/mirrors/${SLUG}.git`):

- Ensure mirror: `git clone --mirror "https://github.com/${REPO}.git" "git/mirrors/${SLUG}.git"` if missing, else `git -C "git/mirrors/${SLUG}.git" fetch origin --prune`.
- Resolve default branch: `DEF_BRANCH="$(gh repo view "$REPO" --json defaultBranchRef -q .defaultBranchRef.name)"`.
- Add scan checkout: `git -C "git/mirrors/${SLUG}.git" worktree add "../wt/${SLUG}-${DEF_BRANCH}-scan" "$DEF_BRANCH"` (linked tree at `git/wt/${SLUG}-${DEF_BRANCH}-scan`).
- All file reads use that worktree path — **not** paths nested under the bare `.git` dir from a mistaken relative `git/wt/...` add.

## E — Open issues? (existence probe, `-L 1`)

```bash
has_open_issues=$(gh issue list --repo "$REPO" --state open -L 1 --json number --jq 'length')
```
Hard gate: probe must succeed; log raw command + output to `memory/YYYY-MM-DD.md`.

- If **`has_open_issues` is `0`** → continue to **E.2**
- If **non‑zero** → continue to **E.2** (routing decided after both probes)

## E.2 — Open PRs? (existence probe, `-L 1`)

```bash
has_open_prs=$(gh pr list --repo "$REPO" --state open -L 1 --json number --jq 'length')
```
Hard gate: probe must succeed; log to `memory/YYYY-MM-DD.md`.

## F — Create new issue (`run_path=create`)

**Precondition:** both probes returned `0`. If not, abort to J or G.

**Preflight (repeat probes):** re-run E and E.2; if issues/PRs appear, abort to J or G.

Steps:
1. Scan files per `scan-paths.md` in the worktree.
2. Dedupe per `github-issues.md`.
3. Scan for existing open issues and just add comment of additions if relevant issue exist
4. Use coding-agent skill to Review improvements and generate tasks to note in issue description
3. `gh issue create --repo "$REPO" --title "..." --template "..." --body-file scratch/issue-body-OWNER-REPO.md`
4. Use coding-agent skill to Suggested commit lines must match `conventions.md`.

## G — Work on existing issue (`run_path=work`)

**Precondition:** `has_open_issues != 0` and `has_open_prs = 0`.

1. `gh issue list --repo "$REPO" --state open --json number,title,labels -L 30` — pick one.
2. `gh issue view N`.
3. Use coding-agent skill to scan open issue for instructions to fix and if open PR, checkout existing PR and continue to resolve PR tasks, fix ci errors, scan PR for gaps/issues
4. Second worktree + topic branch from mirror: see **`tools/git-worktrees.md`** (topic branch section) and `tools/conventions.md` for branch name.
5. Use coding-agent skill to Implement fix/update completely, do not just make single line commit; commit with conventional commits.
6. `gh pr create --draft` (if ≥1 commit) per `github-prs.md`. or commit to existing PR/branch
7. `gh issue comment N` with PR link and summary.

## H — Teardown

Use **`git -C "$MIRROR" worktree remove --force "../wt/<same-relative-path-used-at-add>"`** for each linked tree (see **`tools/git-worktrees.md`**). Keep bare mirrors. Append `memory/YYYY-MM-DD.md`.

## I — Final Discord message

Full summary: repo, run_path, probe values, worktree paths, issue/PR links, checks summary. Send to channel **1069257061533233182**.
