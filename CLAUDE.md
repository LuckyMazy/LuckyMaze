# Notes for Claude (and humans)

Living notes about non-obvious things in this repo. Keep this short and only add things that are noteworthy enough to matter for future work.

## Project

ASP.NET Core 10 Web API on .NET 10 SDK. Clean-architecture layout: `LuckyMaze.API` (controllers, DI), `LuckyMaze.Application` (commands/queries via Mediator source generator), `LuckyMaze.Domain` (entities/enums), `LuckyMaze.Infrastructure` (EF Core + Npgsql, OpenRouter client, Pocket ID OIDC, Semantic Kernel). Tests use TUnit + NSubstitute + EF Core InMemory.

## Running

```sh
docker compose -f compose.dev.yml up -d   # Postgres (3135) + Pocket ID (1411)
cd src/LuckyMaze.API && dotnet run         # API; migrations apply on startup
```

User secrets are required (connection string, OIDC client id, OpenRouter key) — see `docs/dev_setup.md`. Pocket ID needs one-time admin setup — see `docs/dev_pocket_id_setup.md`.

## Migrations

Run from `scripts/`: `migration.bat` / `migration.ps1` / `migration.sh` with `add <Name>`, `update`, `list`, `remove`, `drop`. They wrap `dotnet ef` against `src/LuckyMaze.Infrastructure`.

## Workflow rules (enforced)

- **Never work on `main`.** Create an issue (labeled) → branch `feature/<issue#>_PascalCase` or `fix/<issue#>_PascalCase` → PR (labeled) with `Closes #<issue>` → squash-merge + delete branch.
- **Use CLI generators whenever one exists.** `dotnet new`, `dotnet ef migrations add`, `gh issue create`, `gh pr create`, etc.
- **No AI / Claude attribution** in commits or PRs. Ever.
- **No test plans in PRs.** PR body is Summary + `Closes #<issue>` only.
- **Commit subject**: short imperative.
- **PR labels**: `bug`, `enhancement`, `refactor`, `stale`.
