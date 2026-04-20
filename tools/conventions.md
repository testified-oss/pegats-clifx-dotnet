# Conventions — Testified-OSS (rules list)

Single source for **commits**, **branch names**, and **PR titles** the agent proposes or creates. **Violating this file is a red line** for any git write or GitHub change beyond read-only scans and issue body text that only *suggests* fixes.

## Conventional commits (required)

Subject line (first line of commit message) **must** match:

```text
<type>(<scope>): <description>
```

- **`type`** (allowlist): `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`, `ci`, `build`, `perf` — extend only by human edit to this file.
- **`scope`**: short slug for area (`deps`, `readme`, `ci`, `api`, `cli`, …). No spaces.
- **`description`**: imperative mood, **no trailing period** on the subject line.

Examples (valid):

- `feat(ci): add lint workflow`
- `docs(readme): clarify install steps`
- `chore(deps): bump patch versions`
- `fix(api): handle empty payload`

The shape **`feat(type): message`** means: `feat` is `type`, the parenthetical is **`scope`** (your “type” there = scope slug), the rest is **`description`**.

## When this applies

- **Issues (v1):** Suggested commit lines in issue bodies **must** use the pattern above only.
- **Future PRs** (`tools/github-prs.md`): Every agent-authored commit and PR title **must** validate against this file before push/open PR.

## Branch naming (recommended)

- `feat/<short-topic>`, `fix/<short-topic>`, `docs/<short-topic>` — lowercase, hyphenated slug; align with eventual PR title `type(scope): …`.

## Optional placeholders (enable when org adopts)

- [ ] DCO / sign-off line in commit body
- [ ] PR footer must reference issue: `Fixes #123`
- [ ] Max subject length 72 characters

## Rules

- [Conventional Commits](https://www.conventionalcommits.org/)
- Only commit files in worktree. Never commit files outside worktree such as openclaw's workspace or agent files