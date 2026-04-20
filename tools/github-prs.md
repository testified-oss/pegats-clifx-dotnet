# GitHub — PRs

When **working an open issue** (`tools/github-issues.md`, **`HEARTBEAT.md` Section G**), open a **draft** PR from the topic branch when there is at least one commit (typically from a **second worktree**; same mirror layout as [`tools/git-worktrees.md`](git-worktrees.md)). Use **`--repo "$REPO"`** on all **`gh pr`** calls.

If **`has_open_prs` is not `0`**, you are on **`HEARTBEAT.md` Section J** (**PR follow-up**) — whether **`has_open_issues`** is **`0`** or not — **do not** `gh pr create` (no second PR), **do not** `gh issue create`. Inspect checks and comments on **one** picked PR; push fixes only to **that PR’s head branch**.

If **`has_open_prs` is not `0`** while executing **Section G** (work-on-issue) steps, you are **not** in Section G — that is a routing bug; switch to **Section J**.

## Rules (do not duplicate)

- Branch and PR titlenames: see [`conventions.md`](conventions.md).
- **Every** commit message and PR title: [`conventions.md`](conventions.md) only.

## Commands (reference)

```bash
gh pr list --repo "OWNER/REPO" --state open
gh pr view <number> --repo "OWNER/REPO"
```

## Push then draft PR (**mirror worktrees** — required reading)

Worktrees created from **`git clone --mirror`** inherit **`remote.origin.mirror=true`**. In that setup:

```bash
git push -u origin <your-topic-branch>
```

often fails with:

```text
fatal: --mirror can't be combined with refspecs
```

Then **`gh pr create`** fails with *“you must first push the current branch…”* or GraphQL errors (*head sha blank*, *no commits between base and head*) because **GitHub never received the branch**.

**Fix (use every Section G run after commits on a topic branch):**

1. From the **topic-branch worktree** (same place you committed), set **`BRANCH=$(git branch --show-current)`** and ensure **`REPO`** is still `owner/name` from **`HEARTBEAT.md` Section C**.
2. **Push without using the mirror remote’s refspec rules** — push to a full GitHub URL (same repo; `gh auth` / credential helper supplies HTTPS auth):

```bash
git push "https://github.com/${REPO}.git" "HEAD:refs/heads/${BRANCH}"
```

3. Then create the draft PR (branch now exists on GitHub):

```bash
gh pr create --draft --repo "$REPO" --head "$BRANCH" --title "type(scope): …" --body "…"
```

4. Log **verbatim** push/PR stderr on failure in **`memory/YYYY-MM-DD.md`**. **Do not** claim “branch pushed” or “PR opened” unless **`git push`** and **`gh pr create`** both exited **0**.

## Policy

- No merge without explicit human approval.
- No force-push from automation unless a human updates this file to allow it.

---

## Section J — Checks, comments, and PR-branch worktree

Use when **`run_path=pr_followup`** (**`HEARTBEAT.md` Section J**) — **any** open PR count path, including **no open issues** but **≥1 open PR**. **`REPO`** is `owner/name` (no `https://`).

1. Scan open PR for description, ci status, completeness
2. Checkout existing PR locally and review existing commits
3. Check file changes for completeness
4. Continue to resolve PR tasks, fix ci errors, scan PR for gaps/issues
5. Push changes to branch and add comment to PR
6. Add comment to PR summarising new commit

### Pick PR `P`

```bash
gh pr list --repo "$REPO" --state open --json number,title,isDraft,headRefName,url -L 15
```

Prefer **`isDraft: true`**, else smallest **`number`**. Set **`P=<number>`** and **`HEAD=<headRefName>`** from that row.

### Build / CI status

```bash
gh pr view "$P" --repo "$REPO" --json statusCheckRollup,mergeStateStatus,headRefName,baseRefName,url
gh pr checks "$P" --repo "$REPO"
```

**Interpreting `gh pr checks`:** exit code **`0`** usually means all required checks passed; **`1`** often means one or more failed or pending—read stdout/stderr and the rollup JSON. If ambiguous, open **`statusCheckRollup`** in the JSON and treat **`COMPLETED` + `FAILURE`/`ERROR`** as **fail**, **`PENDING`/`QUEUED`/`IN_PROGRESS`** as **pending**.

### PR conversation + reviews (addressable threads)

```bash
gh pr view "$P" --repo "$REPO" --comments
gh pr view "$P" --repo "$REPO" --json reviews,latestReviews,body --jq .
```

**Inline review comments** (code threads):

```bash
gh api "repos/${REPO}/pulls/${P}/comments" --jq '.[] | {user: .user.login, body: .body, path: .path}'
```

Treat as **`actionable_feedback=yes`** when: a human **requested changes**, CI/bot **posted a fixable error** (lint, test name, missing file), or **you** can resolve a **clear** typo/config issue without policy guesswork. **`actionable_feedback=no`** when only **LGTM**, approvals, or **pending** checks with no local fix yet.

### Worktree on the PR head branch

After **`git -C "git/mirrors/OWNER__REPO.git" fetch origin "$HEAD"`** (use real owner/repo from **`$REPO`**):

```bash
git -C "git/mirrors/OWNER__REPO.git" worktree add "../wt/OWNER__REPO-pr-${P}" "$HEAD"
```

Path mirrors **`tools/git-worktrees.md`** (`../wt/` relative to the bare mirror). **Remove** this worktree in **Section H** with the scan (and any issue) worktrees.

### Push fixes (mirror remote — same as Section G)

From the **PR worktree**, after commits on **`HEAD`**:

```bash
BRANCH=$(git branch --show-current)   # must match PR headRefName
git push "https://github.com/${REPO}.git" "HEAD:refs/heads/${BRANCH}"
```

Then **`gh pr comment "$P" --repo "$REPO" --body-file …`** summarizing the fix.

### Staleness (optional)

If **all** checks are **pending** and the PR was **updated recently**, you may log `pr_followup_outcome=waiting_on_ci` and skip code changes—**do not** spam comments every heartbeat.
