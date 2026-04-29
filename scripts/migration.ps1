param(
    [Parameter(Position=0)]
    [string]$Command,

    [Parameter(Position=1, ValueFromRemainingArguments=$true)]
    [string[]]$Rest
)

$Project = Join-Path $PSScriptRoot "..\src\LuckyMaze.Infrastructure"

function Show-Usage {
    Write-Host "Usage:"
    Write-Host "  migration add <Name>    Add a new migration"
    Write-Host "  migration remove        Remove the last migration"
    Write-Host "  migration list          List all migrations"
    Write-Host "  migration update [Name] Apply migrations (optionally to a specific one)"
    Write-Host "  migration drop          Drop the database"
}

switch ($Command) {
    "add" {
        if (-not $Rest -or -not $Rest[0]) {
            Write-Host "Error: migration name required." -ForegroundColor Red
            Write-Host "Example: migration add InitialCreate"
            exit 1
        }
        dotnet ef migrations add $Rest[0] --project $Project
    }
    "remove" { dotnet ef migrations remove --project $Project }
    "list"   { dotnet ef migrations list --project $Project }
    "update" {
        if ($Rest -and $Rest[0]) {
            dotnet ef database update $Rest[0] --project $Project
        } else {
            dotnet ef database update --project $Project
        }
    }
    "drop"   { dotnet ef database drop --project $Project }
    default  { Show-Usage; if ($Command) { exit 1 } }
}
