# CLAUDE.md — MDMPI.App

Guidance for Claude Code when working in this repository.

> **Provenance:** This project was previously driven with Codex/Copilot. The original
> agent instructions live at `.github/copilot-instructions.md`. That file is still the
> authority on **naming, folder layout, and architecture rules** — this file does not
> repeat them. Where the two disagree, **this file wins** (see *Corrections* below).

---

## 1. What this is

.NET 8 Web API for logistics and collection management — deliveries, pickups,
pull-out/returns, backloads, air/sea requests, plus collection transactions.
Clean Architecture across five projects:

| Project | Role |
|---|---|
| `MDMPI.App.Api` | Controllers, DI wiring (`Program.cs`), Swagger, WebSocket endpoint |
| `MDMPI.App.Core` | Entities, DTOs, service interfaces + implementations. **No** reference to Data or Api |
| `MDMPI.App.Data` | EF Core DbContexts, repositories, ID generators |
| `MDMPI.App.Common` | Shared utilities (`Utilities/TextHelpers.cs`) |
| `MDMPI.App.Tests` | xUnit + Moq unit/integration tests |

Dependency direction: **Api → Core ← Data**.

## 2. Build, test, run

```bash
dotnet build MDMPI.App.sln
```
```bash
dotnet test MDMPI.App.sln
```

**Verified baseline (2026-08-28, branch `prod/websocket-location`): build succeeds
with 5 warnings, 49/49 tests pass.** If you see a different number, something changed —
investigate before assuming it was already broken.

Running the API locally:

```bash
dotnet run --project MDMPI.App.Api --launch-profile http
```

- Swagger at `/swagger` (Development only).
- **`Program.cs` throws on startup in Development unless `ALLOW_PRODUCTION_DB=true`.**
  This is deliberate: local config points at *production* databases. Do not remove the
  guard, and do not set the flag casually — read §4 first.
- In Development only, a middleware rewrites `/api4/*` → `/api/*` so local calls match
  the production route prefix. Production serves this app beneath `/api4`.

Installed SDKs are 9.0.312 and 10.0.400; all projects target `net8.0` and roll forward
fine. Do not "helpfully" retarget to net9/net10.

## 3. Corrections to `.github/copilot-instructions.md`

The Codex instructions are largely accurate but have drifted. Trust this section instead:

1. **Database is no longer SQL Server-only.** The repo is mid-migration to PostgreSQL.
   - `PostgreSqlAppDbContext` (Npgsql) is the primary context — used by all Logistic
     repositories and all Common repositories/ID generators.
   - `AppDbContext` (SQL Server, compatibility level 120) still backs
     `CollectionRepository` and `ClientLookupRepository` (the `ACCMST` client lookup
     lives in the legacy CRM database).
   - Both contexts are registered in `Program.cs`. **Ask which context a new repository
     should target — do not guess.** New Logistic/Common work → PostgreSQL.
2. **Dates are UTC.** `PostgreSqlAppDbContext.SaveChangesAsync` calls
   `NormalizeTrackedDateTimesToUtc()`, which stamps `DateTimeKind.Utc` on every tracked
   `DateTime` property. Npgsql `timestamptz` is unforgiving — never write
   `DateTime.Now`, use `DateTime.UtcNow`.
   The same override also auto-sets `UpdatedAt` on modified Air/Sea, PickUp, and
   PullOut/Return entities — **do not set `UpdatedAt` manually on those three**, you
   will just be overwritten.
3. `Microsoft.Data.SqlClient` is not the whole story — `Npgsql.EntityFrameworkCore.PostgreSQL`
   is in `MDMPI.App.Data.csproj` alongside the SQL Server provider.

Everything else in the Codex file (naming conventions, folder-by-domain structure,
`RequestID`-as-PK rule with the `ItemModel`/`RequestItemID` exception, DI scoping,
async-everywhere, custom ID generators via counter tables) still holds.

## 4. Secrets — read before touching config

`MDMPI.App.Api/appsettings.json` is **tracked in git and contains live credentials**:
SQL Server `sa` password, a MySQL password, the PostgreSQL password, the Gemini API key,
and the WebSocket API key. `.gitignore` has no rule for `appsettings*.json`.

Rules while this remains true:

- **Never paste connection strings, API keys, or passwords into chat, commit messages,
  PR bodies, or new files.** Mask them.
- Never add a new secret to `appsettings.json`. Use environment variables
  (`WebSocket__ApiKey`, `ConnectionStrings__PostgreSqlDB`, …) or user-secrets.
- Remediation is tracked in `docs/CODEX-TO-CLAUDE-TRANSITION.md` Phase 1. It requires
  credential rotation and is the maintainer's call, not something to do unprompted.

## 5. WebSockets

Endpoint `/api/ws`, api-key gated via query string. `WebSocketConnectionHandler` is a
singleton; `WebSocketMessages.cs` holds the payload contracts. Manual test script and
payload shapes: `MDMPI.App.Api/WebSockets/README.md`. Senders do not receive their own
broadcast. Automated coverage: `MDMPI.App.Tests/WebSockets/`.

## 6. Excluded / dead code

`Engineer/`, `Ims/`, `ProductSpecialist/`, and `Sales/` folders are explicitly
`<Compile Remove="...">`'d in `MDMPI.App.Api`, `.Core`, and `.Data` csproj files. They
exist on disk but are not compiled. Do not treat them as live code, and do not
"fix" them — check with the maintainer before reviving any of it.

## 7. Knowledge graph

`graphify-out/` holds a prebuilt graph of this codebase (1848 nodes, 3205 edges,
108 communities; `GRAPH_REPORT.md` is the readable summary). For architecture or
"what connects to what" questions, query the graph before grepping. It is untracked —
regenerate with `/graphify` rather than committing it.

## 8. Repository conventions

- Working branch: `prod/websocket-location`. `master` is ~7 months stale and **36 commits
  behind** this branch — do not target PRs at `master` without asking.
- Four other unmerged branches exist (`migrate/postgresql`, `forms/logistic`,
  `re-model`, `forms/collection`). Confirm the intended base before branching.
- No CI workflows exist under `.github/`. Build and tests are run locally — run them
  before declaring work done.
- Commit only when asked.
