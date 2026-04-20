# TOOLS.md — workspace tool docs

Playbooks and rules for this agent live under **`tools/`** (Markdown next to `AGENTS.md`). Read the relevant file before acting; **`HEARTBEAT.md`** routes to these by section.

## Index (`tools/`)

| File | Role |
|------|------|
| [`tools/conventions.md`](tools/conventions.md) | Conventional commits, branch names, PR titles — required for any git write or GitHub change beyond read-only work. |
| [`tools/target-repos.md`](tools/target-repos.md) | Pool of `owner/name` repos; random pick (one per HEARTBEAT) and selection recipe. |
| [`tools/git-worktrees.md`](tools/git-worktrees.md) | Bare mirror under `git/mirrors/`, linked trees under `git/wt/` via `git -C … worktree add ../wt/…`; push URL for `mirror=true`; teardown; session pitfalls. **Read before any `git worktree`.** |
| [`tools/scan-paths.md`](tools/scan-paths.md) | What to analyse after a worktree checkout; priority order (security, CI, local verify, enhancements) and path rules. |
| [`tools/github-issues.md`](tools/github-issues.md) | Issues flow: routing probes, create vs work-on-issue vs PR follow-up; links to conventions and PRs doc. |
| [`tools/github-prs.md`](tools/github-prs.md) | PR flow: draft PRs, mirror worktrees, `gh pr` usage, follow-up when open PRs exist. |
- Coding agent skill

## What goes in *this* file (`TOOLS.md`)

Short-lived or operator-specific notes that are **not** worth a new file under `tools/` — for example local CLI quirks, one-off URLs, or reminders. Prefer adding durable procedures as a new or updated **`tools/*.md`** and link it from here or from `AGENTS.md` / `HEARTBEAT.md`.

## Why `tools/` vs skills?

Skills are shared patterns. **`tools/`** is this workspace’s contract: repo list, git/GitHub rules, and heartbeat routing. Keep them separate so you can change playbooks without conflating them with global skill content.

---

Add whatever helps you do your job; for anything agents must follow every run, put it in **`tools/`** and list it in the table above.
