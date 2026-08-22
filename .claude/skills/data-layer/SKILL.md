---
name: data-layer
description: Write or change anything in Data.DbAccess, Data.Db, or the Bo/Mapper pair a new domain entity needs (queries, DbContext usage, migrations). Trigger on any "fetch/save/query the database" task, or whenever a new noun in the domain (Lesson, Concept, LessonCompletion, ...) needs to be persisted — not only when the user names a project explicitly.
---

# Data layer conventions

An entity is three things, in three different projects, connected by one mapper. Design the `Bo`
first — that's the shape the rest of the app (`Services.Core`, `Api`) actually works with — never
start from the `*Db` type and bolt a `Bo` on afterward.

## `Bo` — `Common.Types/Entities/<Group>/<Name>Bo.cs`

An immutable **`sealed class`** (not `record` — see `csharp-standards`: `record` is reserved for Value
Types; matches `iplan-nexus-core`'s own `Bo`s) built entirely from Value Types (see `common-layer`) and
other `Bo`s, with `required ... { get; init; }` properties. No EF Core attributes, no navigation
properties, no knowledge that a database exists.

```csharp
public sealed class LessonBo
{
    public required LessonId Id { get; init; }
    public required ConceptId ConceptId { get; init; }
    public required SortOrder Order { get; init; }
    public required EstimatedMinutes EstimatedMinutes { get; init; }
}
```

Being a plain class, a `Bo` has no `with` expression and no structural equality — to change one field,
reconstruct explicitly (`new LessonBo { Id = existing.Id, ..., Title = newTitle }`), and compare
field-by-field in tests rather than comparing two whole `Bo` instances.

If a Value Type doesn't exist yet for one of the `Bo`'s fields, create it first (`common-layer`)
rather than using a raw primitive "for now."

## `*Db` — `Data.Db/<Group>/<Name>Db.cs`

The EF Core-tracked entity — see `ef-core` for the attribute-based configuration convention. Plain
primitives/foreign keys and navigation properties, whatever EF Core needs. Ugly on purpose; nothing
outside `Data.DbAccess` ever sees this type.

## `Mapper` — `Data.DbAccess/<Group>/<Name>Mappers.cs`

A static class with `ToBo(XDb db) => XBo` and `ToDb(XBo bo) => XDb`, one direction each, no shared
logic beyond field assignment. If mapping needs data the `Bo`/`Db` alone doesn't have (a joined value,
like a `TrackCode` alongside a `Lesson`), pass it as an extra parameter — don't make the mapper query
anything itself.

## `Data.DbAccess` — the only place that touches `DbContext`/`*Db`

Everything it exposes is a `Bo` (or `Result<Bo, InvalidData>`, via the `Mapper` above), never a `*Db`
type.

- **Read** and **Update** are separate classes per entity: `<Name>sReadDataAccess` /
  `<Name>sUpdateDataAccess`, each implementing a small interface (`I<Name>sReadDataAccess`) only when
  there's more than one real implementation to substitute (a fake for tests, a cached variant) — don't
  add an interface "for testability" if nothing else will ever implement it. A concrete class is
  simpler and just as testable against a real (in-memory or containerized) database.
- Query logic (filtering, joins, includes) lives entirely inside the DataAccess class —
  `Services.Core` never constructs an `IQueryable` or touches `DbSet<T>`.
- Any business rule ("a Concept unlocks once every Lesson in it has a Completion") is **not**
  data-layer logic — it belongs in `Services.Core`, even if it's expressed as a query here for
  performance. If the query encodes a rule, name the method after the rule
  (`GetUnlockedConceptsAsync`, not `QueryConceptsWithJoin`), and keep the *decision* of what
  "unlocked" means documented in `Services.Core` or `CONTEXT.md`, not buried in SQL.
- Migrations live inside `Data.Db` itself (no separate migrations project — see `ef-core`), generated
  via `dotnet ef migrations add <Name>` — never hand-edit a generated migration's `Up`/`Down`.

## Rules

- A `*Db` type is created via `Mapper.ToDb(bo)` and read via `Mapper.ToBo(db, ...)` — never construct
  or read fields off a `*Db` type anywhere else.
- Async all the way — every DataAccess method is `Task`-returning and awaited; never `.Result`/
  `.Wait()` (banned, see `apps/api/BannedSymbols.txt`).
- Prefer explicit `Include(...)` over lazy-loading — this project does not use lazy-loading proxies.
- One entity = one `Bo` file + one `Db` file + one `Mapper` file (or a shared `Mappers` file per group,
  matching an existing group's convention — check the group's other entities before choosing).
- Never let a `*Db` type appear in a method signature outside `Data.DbAccess`.

## See also

- **common-layer** — Value Types, `Result`/`InvalidData`, and `Common.Server`'s converter/pagination
  helpers this layer builds on
- **ef-core** — attribute-based `*Db` configuration, tracking, migrations
- **naming-conventions** — the full suffix table (`Bo`, `Db`, `Mappers`, `ReadDataAccess`, ...)
