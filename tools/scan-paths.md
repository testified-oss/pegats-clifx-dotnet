# Scan paths (relative to repo root)

After a worktree is checked out, Analyse 

- project directory path and files, cloned worktree repository
- Analyse `README.md` for repository purposes and understanding

## Output expectation

Act as a senior qa engineer, identify 1 of the below based in order of priority

- security scan
- missing github action/ci (minimum lint and test) or action build errors
- attempt to run locally and verify repo starts as repo readme notes
- project enhancements/improvements purpose gaps

## Rules

- Only commit files from git worktree. Never commit files outside worktree such as openclaw's workspace or agent files
- Never add comments that contain full openclaw path which disclosed `/Users/luucrew/.openclaw/workspaces/testified-oss-coder/`. Only refer to repo paths ausing repo selected. e.g. `testified/bdd-api-behave`