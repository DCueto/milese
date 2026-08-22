# Milese

Micro-learning app for software engineering theory. C#/.NET backend (Microsoft Agent Framework for the AI Tutor), React Native/Expo mobile + Next.js web (TypeScript), PostgreSQL. Monorepo: `apps/api`, `apps/web`, `apps/mobile`, `apps/content`.

## Read first

- **[CONTEXT.md](./CONTEXT.md)** — the domain glossary (Track, Subject, Concept, Lesson, Learner, Lesson Completion, ...). Read before touching anything that talks about the curriculum or a Learner's progress. Sharpen it — see the `domain-modeling` skill.
- **[PROJECT-BRIEF.md](./PROJECT-BRIEF.md)** — product scope and the *why* behind every major decision (content pipeline, offline sync, auth, platform choices).
- **[docs/adr/](./docs/adr/)** — one file per hard-to-reverse decision, numbered sequentially. Read the ones relevant to what you're touching before assuming a different approach is fine.

## How we work

Two units of work:
- **A change** — fits in one PR, no doc needed beyond the code itself (and a new ADR/CONTEXT.md entry if it introduces a decision or a term).
- **A plan** — spans multiple PRs. Gets a `docs/plans/<slug>/` folder (`PLAN.md`, `TASKS.md`, a `progress/` note per slice). Create this *before* starting multi-PR work, not after.

Branches: `<type>/<kebab-slug>` off `main` (`feat/`, `fix/`, `chore/`, `docs/`). Commits: conventional commits (`feat:`, `fix:`, `chore:`, `docs:`, `refactor:`, `test:`). Never commit without explicit confirmation; never `git add -A` — stage named files.

**Done means:** the relevant project builds with zero warnings, its tests pass (see `testing` — every public method needs one), and no `Tests/ArchTests` fitness test regresses. Don't add a fitness test pre-emptively for a rule that's only been violated once; add it on the *second* violation.

### Where a rule belongs

| Scope | Goes in |
|---|---|
| Always true, whole repo | This file (keep it short — route to a skill instead of growing it) |
| True for one app only | `apps/<name>/CLAUDE.md` (create when the first app-specific rule appears) |
| A repeatable activity ("add a Value Type", "add an entity") | A skill in `.claude/skills/` |
| A decision that's hard to reverse, surprising, and the result of a real trade-off | An ADR in `docs/adr/` |
| A term whose meaning could be read two ways | `CONTEXT.md` |

## Backend layer stack (`apps/api`)

Mirrors the pragmatic-FP pattern recorded in [docs/adr/0005](./docs/adr/0005-pragmatic-fp-split.md) — immutable domain core, mutable EF Core confined to the boundary.

```
Common.Shared  → zero-dependency base: Result/Either/NotEmptyList/AllOrSome monads, IValueType contracts, ValueTypeParser
Common.Server  → EF Core-only glue over Common.Shared (ValueTypeConverter, member translator, pagination) — never referenced above Data.DbAccess
Common.Types   → Value Types (smart-constructor Parse() -> Result<T, InvalidData>) + immutable Bo domain records
Data.Db        → mutable EF Core entities (*Db suffix), DbContext
Data.DbAccess  → *Mapper classes (Db <-> Bo), *ReadDataAccess / *UpdateDataAccess classes
Services.Core  → business logic; operates only on Bo + Result; orchestrates DataAccess + the Tutor (Microsoft Agent Framework)
Api            → ASP.NET Core Web API; controllers/endpoints call Services.Core only
```

Physical layout: `apps/api/src/<Group>/<Project>` (e.g. `src/Common/Common.Shared`, `src/Common/Common.Types`, `src/Data/Data.DbAccess`, `src/Services/Services.Core`, `src/Api/Api.Rest`). Tests mirror this 1:1 under `apps/api/tests/<Project>.Tests`.

### Architecture rules

1. A `*Db` type never crosses out of `Data.DbAccess` — everything above it sees only `Bo`s.
2. No business logic in `Data.DbAccess` — it maps and queries, nothing else.
3. No format/range validation in `Services.Core` — that belongs in a Value Type's `Parse()`. A `Bo` holding a Value Type is already valid by construction.
4. No layer skipping (`Api` never touches `Data.Db`/`Data.DbAccess` directly; always through `Services.Core`) — except the composition root (`Program.cs` and `Api.Rest.Extensions`'s DI-wiring helpers), which references `Data.Db` to register `MileseDbContext` and `Data.DbAccess` as reflection scan markers for auto-registering `*DataAccess`/`*Service` classes. No controller or business logic touches either. Enforced by `Tests/ArchTests/LayerDependencyTests.cs`.
5. `Common.Server` is EF Core-only glue (converters, member translation, pagination) — it depends only on `Common.Shared` and is never referenced by `Services.Core` or `Api`. `Common.Shared` depends on nothing and is safe from every layer.

## Always-on baseline

- `Result<T, TError>` over throwing for any *expected* failure (validation, not-found, conflict). Exceptions are for genuinely exceptional/unrecoverable states only.
- Never `try/catch` around an expected failure path — model it with `Result` instead.
- Never `.Result` or `.Wait()` on a `Task` — always `await`. Never `DateTime.Now`/`.Today` — use `DateTime.UtcNow` (or an injected time source once one exists). Enforced by `apps/api/BannedSymbols.txt`, not just convention.
- No code comments unless they explain a non-obvious *why* (a workaround, a hidden constraint). Well-named code doesn't need a comment restating what it does.
- All code — identifiers, comments, commit messages — is in English, regardless of Content Language. **Content Language** (Spanish/English Lesson text, per [docs/adr/0011](./docs/adr/0011-content-language-spanish-and-english-at-mvp.md)) is data the app serves, not a property of the codebase.

## Commands

`apps/api` (run from `apps/api/`):

```bash
dotnet build                                    # zero warnings is the bar — TreatWarningsAsErrors is on
dotnet test --solution Milese.slnx              # every test project; see the testing skill for filters
dotnet ef migrations add <Name> --project src/Data/Data.Db --startup-project src/Api/Api.Rest
dotnet ef database update --project src/Data/Data.Db --startup-project src/Api/Api.Rest
dotnet run --project src/Aspire/Aspire.AppHost  # orchestrated: Postgres container + migrations + Api.Rest
dotnet run --project src/Api/Api.Rest           # Api.Rest alone, against a manually-run Postgres
```

The AppHost (`src/Aspire/Aspire.AppHost`) is the normal way to run the API locally — it starts a
Postgres container (fixed name `milese-postgres`, port `15432`, survives the AppHost stopping), waits
for `Aspire.MigrationService` to apply migrations, then starts `Api.Rest` on `http://localhost:5080`
wired to it via service discovery. It also self-detects whether it's running from the main checkout or
a linked git worktree and takes its own port/database slot automatically — no flags needed, see
`worktrees`. Running `Api.Rest` directly still works (`AddNpgsqlDbContext` falls back to
`ConnectionStrings:milesedb` in `appsettings.json`) for a manually-run Postgres container — see
`ef-core`.

`apps/web`/`apps/mobile`/`apps/content`: _(not yet scaffolded)_.

## Skills

| Skill | Use when |
|---|---|
| `milese-domain` | Starting any task that touches the curriculum or a Learner's progress — primes on `CONTEXT.md` + `PROJECT-BRIEF.md` before you write code. |
| `csharp-standards` | Writing or reviewing any C# in `apps/api`, even a one-line edit. |
| `common-layer` | Adding a shared utility or monad, or deciding whether something belongs in `Common.Shared` vs `Common.Server`. |
| `railway-oriented-programming` | Writing anything that can fail in an expected way — validation, lookups, business rules. |
| `data-layer` | Writing or changing anything in `Data.DbAccess`, or adding a new `Bo`/`*Db`/`Mapper` trio for a domain entity. |
| `ef-core` | Defining or changing a `*Db` entity, a column, a query, a migration, or the `DbContext`. |
| `services-layer` | Writing or changing anything in `Services.Core`. |
| `naming-conventions` | Naming a new project, class, or file and unsure which suffix/casing applies. |
| `localization` | Adding or changing user-facing text, a validation/error message, or culture handling. |
| `testing` | Writing tests for any layer. |
| `worktrees` | Creating/removing a git worktree, running more than one Milese instance at once, or debugging a port/database collision between them. |
| `record-decision` | You just made (or are about to make) a call that's hard to reverse, surprising, and the result of a real trade-off. |

## Agent skills

### Issue tracker

Local markdown under `.scratch/<feature-slug>/` — this repo has no git remote yet. See `docs/agents/issue-tracker.md`.

### Triage labels

Default vocabulary (`needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, `wontfix`). See `docs/agents/triage-labels.md`.

### Domain docs

Single-context: `CONTEXT.md` + `docs/adr/` at the repo root. See `docs/agents/domain.md`.
