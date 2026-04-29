# LuckyMaze — Dev Setup

## Prerequisites

- .NET 10 SDK
- Docker Desktop
- `dotnet-ef` global tool: `dotnet tool install --global dotnet-ef`

## Start the containers

```bash
docker compose -f compose.dev.yml up -d
```

This starts three containers:

- **Postgres** on port `3135` (mapped from container port `5432`)
- **Pocket ID** (OIDC provider) on port `1411`

Pocket ID needs a one-time setup before the API can authenticate against it —
see [`dev_pocket_id_setup.md`](./dev_pocket_id_setup.md).

## Configure secrets

All secrets live in **user secrets**.

Pick one of the two options below.

### Option 1 — CLI

```bash
cd src/LuckyMaze.API

dotnet user-secrets set "ConnectionStrings:LuckyMazeDatabase" "Host=localhost;Port=3135;Database=luckymaze-dev;Username=postgres;Password=d4vpas8w0rd13!!!"

dotnet user-secrets set "Oidc:Authority" "http://localhost:1411"
dotnet user-secrets set "Oidc:RequireHttpsMetadata" "false"
dotnet user-secrets set "Oidc:ClientId" "<client-id-from-pocket-id>"
dotnet user-secrets set "Oidc:RedirectUri" "http://localhost:5173/callback"
dotnet user-secrets set "Oidc:PostLogoutRedirectUri" "http://localhost:5173"
dotnet user-secrets set "Oidc:Scope" "openid profile email groups"

dotnet user-secrets set "OpenRouter:ApiKey" "<your-openrouter-api-key>"
dotnet user-secrets set "OpenRouter:DefaultModel" "openai/gpt-5"
```

The `OpenRouter:ApiKey` is obtained from [openrouter.ai/keys](https://openrouter.ai/keys).

The `Oidc:ClientId` is obtained during the Pocket ID setup — see step 7 of
[`dev_pocket_id_setup.md`](./dev_pocket_id_setup.md).

To verify:

```bash
dotnet user-secrets list
```

### Option 2 — Edit `secrets.json` directly

In Visual Studio: right-click the `LuckyMaze.API` project → **Manage User Secrets**.

Paste in:

```json
{
  "ConnectionStrings": {
    "LuckyMazeDatabase": "Host=localhost;Port=3135;Database=luckymaze-dev;Username=postgres;Password=d4vpas8w0rd13!!!"
  },
  "Oidc": {
    "Authority": "http://localhost:1411",
    "RequireHttpsMetadata": false,
    "ClientId": "<client-id-from-pocket-id>",
    "RedirectUri": "http://localhost:5173/callback",
    "PostLogoutRedirectUri": "http://localhost:5173",
    "Scope": "openid profile email groups picture"
  },
  "OpenRouter": {
    "ApiKey": "<your-openrouter-api-key>",
    "DefaultModel": "openai/gpt-5"
  }
}
```

## Run the API

```bash
cd src/LuckyMaze.API
dotnet run
```

Migrations are applied automatically on startup.
OpenAPI / Swagger UI is available at `/swagger`.

## Migrations

From the `scripts/` folder:

| Command | What it does |
|---|---|
| `migration add <Name>` | Create a new migration |
| `migration update` | Apply pending migrations |
| `migration list` | List all migrations |
| `migration remove` | Remove the last (unapplied) migration |
| `migration drop` | Drop the database |

Windows uses `migration.bat`, Linux/Mac use `./migration.sh`.

## Stop & reset

```bash
docker compose -f compose.dev.yml down          # stop, keep data
docker compose -f compose.dev.yml down -v       # stop, wipe data
```
