# Milese

Micro-learning app for software engineering theory and CS fundamentals — 10-15 minute lessons for dead time (commute, public transport), covering the concepts a self-taught developer learns *around* the code, not just the syntax. Curated content plus a RAG-grounded AI Tutor for on-demand questions.

> [!NOTE]
> Milese is a personal project built for a single user (the founder) first, architected with a realistic path to a real multi-user product later. See [PROJECT-BRIEF.md](./PROJECT-BRIEF.md) for the full reasoning behind that and every other major decision.

## Features

- **Gated curriculum** — content organized as `Track → Subject → Concept → Lesson`, unlocked sequentially by default, with a free-browse toggle.
- **Immutable, versioned lessons** — content is authored as Markdown/MDX in the monorepo and published through an ordinary git commit; a correction is a new version, never a silent edit.
- **Offline-first progress** — lessons read on-device work fully offline; progress is an append-only completion log, so sync never needs conflict resolution.
- **AI Tutor** — a conversational surface grounded in the Learner's current lesson and its surrounding concept/subject, backed by the Microsoft Agent Framework.
- **Real auth from day one** — Entra External ID (federated to Google), no password flows to build or maintain.

## Architecture

Milese is a monorepo with one backend and two client apps sharing generated API types:

| App | Stack | Status |
|---|---|---|
| `apps/api` | C#/.NET, ASP.NET Core, PostgreSQL, Microsoft Agent Framework | In progress |
| `apps/web` | Next.js, TypeScript | Planned — see [docs/adr](./docs/adr) |
| `apps/mobile` | React Native (Expo), TypeScript | Planned — see [docs/adr](./docs/adr) |
| `apps/content` | Markdown/MDX lessons + sync tooling | Planned |
| `packages/api-types` | TypeScript types generated from `apps/api`'s OpenAPI spec | In progress |

The JS/TS side (`apps/web`, `apps/mobile`, `packages/api-types`) is a [pnpm workspace](./pnpm-workspace.yaml) ([ADR-0021](./docs/adr/0021-pnpm-workspaces-for-js-monorepo.md)).

`apps/api` follows a pragmatic functional-core / mutable-shell split (recorded in [ADR-0005](./docs/adr/0005-pragmatic-fp-split.md)): an immutable domain core with `Result`/`Either`-based error handling, EF Core confined to the boundary.

```
Common.Shared  → zero-dependency base: Result/Either/NotEmptyList monads, IValueType contracts
Common.Server  → EF Core-only glue over Common.Shared (converters, pagination)
Common.Types   → Value Types (smart-constructor Parse() -> Result<T, InvalidData>) + immutable Bo records
Data.Db        → mutable EF Core entities, DbContext
Data.DbAccess  → Db <-> Bo mappers, Read/Update data access classes
Services.Core  → business logic; orchestrates DataAccess + the AI Tutor
Api            → ASP.NET Core Web API; controllers call Services.Core only
```

Each layer only depends on the ones above it in this list — enforced by an architecture fitness test (`Tests/ArchTests`), not just convention.

## Getting started

### Prerequisites

- [.NET SDK 10.0.400](https://dotnet.microsoft.com/download) (pinned in [`apps/api/global.json`](./apps/api/global.json))
- [Docker](https://www.docker.com/) (for the local PostgreSQL container)
- [pnpm](https://pnpm.io/) (JS/TS workspace — `apps/web`, `apps/mobile`, `packages/api-types`)

### Run the API

From `apps/api`, the [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/) AppHost is the normal way to run everything locally — it starts a Postgres container, applies migrations, and boots the API wired up via service discovery:

```bash
cd apps/api
dotnet run --project src/Aspire/Aspire.AppHost
```

This starts the API at `http://localhost:5080`, with a Scalar API reference available at `/scalar` in Development.

> [!TIP]
> Working across more than one branch at once? See the `worktrees` skill/doc — the AppHost auto-detects a linked git worktree and picks its own port and database slot, no flags needed.

### Build and test

```bash
cd apps/api
dotnet build                        # zero warnings is the bar
dotnet test --solution Milese.slnx  # every test project
```

> [!IMPORTANT]
> "Done" for any change in `apps/api` means: the solution builds with zero warnings, its tests pass, and no `Tests/ArchTests` fitness test regresses.

## Project structure

```
apps/
  api/            C#/.NET backend (ASP.NET Core, EF Core, PostgreSQL, Aspire orchestration)
  web/            Next.js web client (planned)
  mobile/         React Native (Expo) mobile client (planned)
  content/        Lesson content as code + sync tooling (planned)
packages/
  api-types/      TypeScript types generated from apps/api's OpenAPI spec (pnpm workspace)
docs/
  adr/            One file per hard-to-reverse architectural decision
  agents/         Notes for AI coding agents working in this repo (issue tracker, triage, domain docs)
CONTEXT.md        Domain glossary — Track, Subject, Concept, Lesson, Learner, Progress, ...
PROJECT-BRIEF.md  Product scope and the reasoning behind every major decision
```

## Documentation

- [CONTEXT.md](./CONTEXT.md) — the domain glossary. Read before touching anything about the curriculum or a Learner's progress.
- [PROJECT-BRIEF.md](./PROJECT-BRIEF.md) — product scope and the *why* behind content pipeline, offline sync, auth, and platform choices.
- [docs/adr/](./docs/adr/) — one file per hard-to-reverse decision, in the order it was made.
