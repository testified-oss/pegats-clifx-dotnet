# Target repos (random pick — one per HEARTBEAT)

Each HEARTBEAT run processes **exactly one** repository, chosen **uniformly at random** from the pool below (do not round-robin unless you change this file).

## Pool (`owner/name`)

| URL | owner/name |
|-----|------------|
| https://github.com/testified-oss/behave-bdd-python | `testified-oss/behave-bdd-python` |
| https://github.com/testified-oss/pytest-api-testing | `testified-oss/pytest-api-testing` |
| https://github.com/testified-oss/pegats-clifx-dotnet | `testified-oss/pegats-clifx-dotnet` |
| https://github.com/testified-oss/locust-terraform-aws | `testified-oss/locust-terraform-aws` |
| https://github.com/testified-oss/devcontainer-base | `testified-oss/devcontainer-base` |
| https://github.com/testified-oss/specflow-nunit-template | `testified-oss/specflow-nunit-template` |
| https://github.com/testified-oss/supertest-cucumber-ts | `testified-oss/supertest-cucumber-ts` |
| https://github.com/testified-oss/cf-worker-llm-agent | `testified-oss/cf-worker-llm-agent` |
| https://github.com/testified-oss/agent-skills | `testified-oss/agent-skills` |
| https://github.com/testified-oss/dotnet-wiremock | `testified-oss/dotnet-wiremock` |
| https://github.com/testified-oss/api-artillery | `testified-oss/api-artillery` |
| https://github.com/testified-oss/k6-grafana-influxdb | `testified-oss/k6-grafana-influxdb` |

## Random selection (required)

After Section B (`gh auth`), pick **`OWNER/REPO`** for this run using **one** of:

```bash
printf '%s\n' \
  "testified-oss/behave-bdd-python" \
  "testified-oss/pytest-api-testing" \
  "testified-oss/pegats-clifx-dotnet" \
  "testified-oss/locust-terraform-aws" \
  "testified-oss/devcontainer-base" \
  "testified-oss/specflow-nunit-template" \
  "testified-oss/supertest-cucumber-ts" \
  "testified-oss/cf-worker-llm-agent" \
  "testified-oss/agent-skills" \
  "testified-oss/dotnet-wiremock" \
  "testified-oss/api-artillery" \
  "testified-oss/k6-grafana-influxdb" \
  | shuf -n1
```

(or equivalent: pool file + `shuf -n1`, or documented RNG—**must** be random each run, not always the first row).

Log the chosen repo in **`memory/YYYY-MM-DD.md`** (e.g. `picked_repo=testified-oss/behave-bdd-python`).

## Rules

- **No** `gh repo list` for scope—the pool is **only** the rows in the table above unless a human edits this file.
- **Git worktrees:** before any `git worktree` / mirror work, read **`tools/git-worktrees.md`** (same workspace as this file).
