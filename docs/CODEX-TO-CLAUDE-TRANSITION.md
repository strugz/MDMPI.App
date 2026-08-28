# Codex → Claude Code Transition Workflow

**Repo:** MDMPI.App · **Assessed:** 2026-08-28 · **Branch:** `prod/websocket-location`

This is the migration plan for moving this project from Codex/Copilot-driven development
onto Claude Code. Phases are ordered by dependency — Phase 0 is done, Phase 1 blocks
everything that touches config.

---

## Baseline (verified, not assumed)

| Check | Result |
|---|---|
| `dotnet build MDMPI.App.sln` | Succeeds — 0 errors, 5 warnings |
| `dotnet test MDMPI.App.sln` | **49/49 pass**, ~2s |
| SDKs present | 9.0.312, 10.0.400 (projects target `net8.0`, rolls forward) |
| CI | None — no workflows in `.github/` |
| Agent instructions found | `.github/copilot-instructions.md` only |
| `AGENTS.md` / `CLAUDE.md` | Neither existed |

Re-run the build and test commands after each phase; this table is the regression gate.

---

## Phase 0 — Context handoff ✅ Done

Codex read `.github/copilot-instructions.md`. Claude Code reads `CLAUDE.md`. Rather than
duplicating ~90 lines of still-correct conventions, `CLAUDE.md` **delegates** to the
Copilot file and overrides only where it has drifted.

- ✅ `CLAUDE.md` created at repo root.
- ✅ `.github/copilot-instructions.md` left in place — Copilot/Codex still work.
- ✅ Drift documented (see Phase 2).

**Why delegate instead of copy:** two full copies of the conventions guarantees they
diverge again. One source of truth, one override layer.

---

## Phase 1 — Secrets 🔴 Blocking

`MDMPI.App.Api/appsettings.json` is tracked in git and contains **live** credentials:
SQL Server `sa` password, MySQL password, PostgreSQL password, Gemini API key, WebSocket
API key. `.gitignore` has no `appsettings*` rule. They are in git history, so removing
the file now does **not** make them safe.

This needs a maintainer decision — it involves rotating credentials that other systems use.

1. **Rotate first.** All five. Anything else is cosmetic while the old values stay valid.
2. Move runtime values to environment variables. The double-underscore form already works
   with no code change:
   - `ConnectionStrings__PostgreSqlDB`, `ConnectionStrings__DB`
   - `WebSocket__ApiKey`, `GeminiAI__ApiKey`
3. Replace the tracked file with an `appsettings.Example.json` holding placeholders, and
   add `appsettings.json` + `appsettings.*.local.json` to `.gitignore`.
4. Decide on history: rewriting it (`git filter-repo`) breaks every existing clone and all
   five open branches. Given rotation in step 1, leaving history alone is defensible —
   **make that call explicitly rather than by default.**

Until this is done: no secret values in chat, commits, PR bodies, or new files.

---

## Phase 2 — Reconcile instruction drift

`.github/copilot-instructions.md` says *"Target database: SQL Server via
Microsoft.Data.SqlClient."* The code says otherwise:

| Context | Used by |
|---|---|
| `PostgreSqlAppDbContext` (Npgsql) | All 5 Logistic repos, all Common repos, all 4 ID generators |
| `AppDbContext` (SQL Server, compat 120) | `CollectionRepository`, `ClientLookupRepository` only |

Both are registered in `Program.cs`. The PostgreSQL context also normalizes `DateTime`
to UTC on save.

- [ ] Update the DB section of `.github/copilot-instructions.md` to describe the dual-context
      reality, so Copilot and Claude give the same answer.
- [ ] Confirm the intended end state: is `AppDbContext` permanent (legacy CRM `ACCMST`
      lookup lives there), or is Collection also migrating? **This is the single most
      valuable unknown to close** — it decides what every new repository targets.
- [ ] Once confirmed, drop the now-redundant override from `CLAUDE.md` §3.

---

## Phase 3 — Branch consolidation

`master` is ~7 months stale. Divergence from `master`:

| Branch | Commits ahead | Last activity |
|---|---|---|
| `prod/websocket-location` (current) | 36 | 2026-08-28 |
| `migrate/postgresql` | 34 | 2026-05-26 |
| `forms/logistic` | 30 | 2026-05-06 |
| `re-model` | 26 | 2026-04-16 |
| `forms/collection` | 18 | 2026-03-31 |
| `feature/unit-testing` | 0 (13 behind) | 2025-09-16 |

**Verified:** `git branch --no-merged prod/websocket-location` returns **nothing** — every
branch, including `master`, is fully contained in `prod/websocket-location`. There is no
unmerged work to rescue, so this phase is pure cleanup with no risk of losing commits.

- [ ] Delete the five redundant local branches (`feature/unit-testing`, `forms/collection`,
      `forms/logistic`, `migrate/postgresql`, `re-model`) and their `origin/` counterparts.
- [ ] Fast-forward `master` to `prod/websocket-location` — it is a clean fast-forward.
      Right now "the main branch" and "the branch with the code" are different, which
      misleads any agent (or human) that defaults to `master`.
- [ ] Push the current branch: it is 1 commit ahead of `origin/prod/websocket-location`.

---

## Phase 4 — Guardrails Codex didn't have

No CI exists, so nothing currently catches a broken build but a human running it.

- [ ] Add a GitHub Actions workflow: `dotnet build` + `dotnet test` on push and PR.
      Cheap, and it makes the 49-test baseline enforceable instead of advisory.
- [ ] Add a secret-scanning step (or enable GitHub push protection) once Phase 1 lands,
      so `appsettings.json` cannot regress.
- [ ] Optional: clear the 5 build warnings (3 duplicate `using` directives in Core, 2
      nullability mismatches in test mocks). All trivial; keeps a clean signal so real
      warnings stand out.

---

## Phase 5 — Working conventions

- **Knowledge graph.** `graphify-out/` already holds a full graph (1848 nodes, 3205 edges,
  108 communities). Use it for architecture questions instead of grepping blind. It is
  untracked and 2.8 MB — add `graphify-out/` to `.gitignore` and regenerate with
  `/graphify` rather than committing it.
- **Untracked files needing a decision:** `graphify-out/` (ignore) and
  `mdmpi_app_db_schema.sql` (44 KB PostgreSQL schema dump — commit it as reference
  documentation, or ignore it as a local artifact? It is genuinely useful context).
- **Dead code.** `Engineer/`, `Ims/`, `ProductSpecialist/`, `Sales/` are `Compile Remove`'d
  across three csproj files. Either delete them or add a README saying why they stay —
  right now every new agent re-discovers them and has to ask.
- **Definition of done:** build clean + 49/49 tests + no new warnings.

---

## Open questions for the maintainer

1. Is `AppDbContext` (SQL Server) permanent, or does Collection migrate to PostgreSQL too?
2. Rewrite git history for the leaked secrets, or rotate-and-move-on?
3. Does `master` get fast-forwarded, or replaced as the default branch?
4. Keep or delete the four excluded module folders?
