# GitHub — issues (v1 playbook)

## Per-HEARTBEAT flow (summary)

1. **Random repo** — exactly one repo from the pool in **`tools/target-repos.md`** (see that file for `shuf` recipe). Set **`REPO="OWNER/REPO"`** and use **`--repo "$REPO"`** on every `gh` call (**`HEARTBEAT.md` Section C**).
2. **Route gate first** — **`HEARTBEAT.md` Section E then E.2** *before* mirror: use **existence probes** (`-L 1`, `jq 'length'` → **0** or **1**) for **open issues** and **open PRs** (E.2 **always** runs). **Never** infer routing without both probes.
3. **Local copy always** — after routing, mirror + **at least one worktree** on the default branch per **`tools/git-worktrees.md`** (**`HEARTBEAT.md` Section D**), **before** `gh issue create` or implementation reads.
4. **Branch on probes** (see **`HEARTBEAT.md` Routing law**):
   - **`has_open_issues=0`** and **`has_open_prs=0`** → **create** path after D: scan + dedupe + **`HEARTBEAT.md` Section F preflight** + `gh issue create`.
   - **`has_open_prs≠0`** (issues **0** or **>0**) → **PR follow-up** after D — **`HEARTBEAT.md` Section J** + **`tools/github-prs.md`**: **no** `gh issue create`, **no** `gh pr create`, **no** merge; **yes** triage **checks + comments** on one PR; **commits + push** only on that PR’s head branch when fixing CI/review. (With **`-L 1`** probes, “≠0” is exactly **`1`**.)

   - **`has_open_issues≠0`** and **`has_open_prs=0`** → **work on issue** after D — **no** `gh issue create`; pick an issue, **`gh issue view`**, **second worktree** on a **topic branch** per **`tools/conventions.md`** / **`HEARTBEAT.md` Section G**, implement, **commit**, **`gh pr create --draft`** when there is at least one commit, then **`gh issue comment`** linking the PR—never merge without human approval.

**Anti-footgun:** **`gh issue list` / `gh pr list` must include `-L 1`** for these existence probes. Without it, `jq 'length'` is the **real** count (2, 3, …). Treat **any `length` > 0** as “has at least one” for routing. **Always** run the PR probe (E.2); skipping it when **`has_open_issues=0`** caused **open PRs** to be ignored and **`gh issue create`** to run while a PR still needed triage.

## Config (edit for your org)

**Current values:**

- **`issue_template_title`:**  must match conventional commit **`name:`** on the repo; **verify** per operator section below. Refer to [`conventions.md`](conventions.md) for any copy that suggests titles and commits.

- **`max_issues_per_run`:** `1` (one heartbeat = one repo; at most one new issue when the create path runs)
- **`dedupe_search_limit`:** `20`

| Key | Example | Meaning |
|-----|---------|---------|
| `issue_template_title` | conventional style | Template **`name:`** for **`gh issue create --template`** |
| `max_issues_per_run` | `1` | Cap **new** issues created when the “no open issues” path runs |

## Canonical probes (existence: 0 or 1)

Use **`REPO`** from **`HEARTBEAT.md` Section C**. **`-L 1`** matters: you are asking “is there **at least one**?”, not “how many total?” (avoids default `--limit` / pagination confusion).

```bash
has_open_issues=$(gh issue list --repo "$REPO" --state open -L 1 --json number --jq 'length')
has_open_prs=$(gh pr list --repo "$REPO" --state open -L 1 --json number --jq 'length')
```

- **`has_open_issues`:** **`0`** = no open issues; **`1`** (with **`-L 1`**) = one or more open issues. If you omitted **`-L 1`**, normalize: **any value `> 0`** means “has open issues” (same as **`1`** in the routing table).
- **`has_open_prs`:** **`0`** = no open PRs; **non‑zero** = at least one open PR → **Section J** (with **`-L 1`**, non‑zero is exactly **`1`**). Routing: **J** if PRs≠**0**; **G** only when issues≠**0** and PRs=**0**; **F** only when issues=**0** and PRs=**0**.

Log both variables and the exact commands to **`memory/YYYY-MM-DD.md`** every run.

## Create path (no open issues **and** no open PRs)

1. Scan files per **`tools/scan-paths.md`** inside the **default-branch worktree** (local copy required).
2. Dedupe: `gh issue list --repo "$REPO" --state all --search "<keywords>" -L 20` if you need to avoid reopening closed dupes.
3. `gh issue create` as below.

## Work-on-issue path (`has_open_issues` **≠ 0** and **`has_open_prs=0`**)

**Precondition:** **`has_open_prs`** from the probe above must be **`0`**. Otherwise **Section J** (PR follow-up), not this section.

1. List: `gh issue list --repo "$REPO" --state open --json number,title,labels -L 30`
2. **Pick one** issue `N` (document choice in memory).
3. **`gh issue view N --repo "$REPO"`** (web or `--json` as needed).
4. **Topic-branch worktree:** `git worktree add` a second path from the mirror when needed (see **`tools/git-worktrees.md`**). Read relevant paths and **implement** a smallest coherent change for issue `N`; branch naming per **`tools/conventions.md`**.
5. **Commit** on the topic branch (messages per **`tools/conventions.md`**). Only commit files in worktree. Never commit files outside worktree such as openclaw's workspace or agent files
6. **`gh pr create --draft`** when there is at least one commit — see **`tools/github-prs.md`** and **`HEARTBEAT.md` Section G**.
7. **`gh issue comment N --repo "$REPO" --body-file scratch/comment-N.md`** — required: concrete repo-relative paths, summary, and **draft PR link** when step 6 ran. If no commits were possible, explain in the comment; still **do not** `gh issue create` while **`has_open_issues` is not `0`**.

If **`gh issue create --template`** fails with “template not found”, **log and skip** create (no interactive prompt).

### Issue forms (YAML) only

GitHub CLI may not support some YAML issue forms. If so: log to memory, use body-only create only if allowed here, or skip create.

## Dedupe (before create)

```bash
gh issue list --repo "$REPO" --state open --search "keywords here" -L 20
```

If a substantively duplicate **open** issue exists, **do not** create another.

## `gh issue create` — exact flags

```bash
gh issue create \
  --repo "$REPO" \
  --title "short imperative title" \
  --template "ISSUE_TEMPLATE_NAME_FROM_TOOLS" \
  --body-file "/Users/luucrew/.openclaw/workspaces/testified-oss-coder/scratch/issue-body-OWNER-REPO.md"
```

Replace `ISSUE_TEMPLATE_NAME_FROM_TOOLS` with **`issue_template_title`** above.

Fallback (body-only), only if operators document approval in this file:

```bash
gh issue create --repo "$REPO" --title "..." --body-file "..."
```

## Scratch files

Use `scratch/issue-body-*.md` and `scratch/comment-*.md`; directory is gitignored.

## Operator verification (run once per repo template)

On **`testified-oss/behave-bdd-python`** and **`testified-oss/awesome-testing-resources`**:

1. `gh api "repos/OWNER/REPO/contents/.github/ISSUE_TEMPLATE" --jq '.[].name'` (or GitHub UI **Issues → New issue**).
2. Align **`issue_template_title`** with the real **`name:`** string (may differ per repo—if so, document both in this file or pick a template name common to both).


## Rules (do not duplicate)

- Branch names: see [`conventions.md`](conventions.md).
- **Every** commit message and Issue title: [`conventions.md`](conventions.md) only.