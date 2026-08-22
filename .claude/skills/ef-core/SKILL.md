---
name: ef-core
description: >-
  How EF Core is configured and used in apps/api against PostgreSQL: code-first, snake_case naming +
  DB check constraints always on, `NoTracking` by default with tracking opted in per write, attributes
  over Fluent API for `*Db` entities, `[MaxLength]`/precision sourced automatically from a value type's
  `MaxLength`/`Precision` via `Common.Server`'s converter registry (never a hardcoded number), and the
  fixed two-parameter `*DataAccess` constructor (context factory + `CancellationToken`, no other
  state). Read and apply this BEFORE you define or change a `*Db` entity, add a column, write or modify
  a query, add a migration, or touch the `DbContext` — and whenever you wonder "attributes or Fluent
  API?", "where does `[MaxLength]` come from?", "tracked or not-tracked?", "how do I name this
  migration?". Trigger on any EF Core, persistence, schema, or migration work, not only when "EF Core"
  is named.
---

# EF Core Patterns

How EF Core is configured and used in `apps/api`'s persistence layer, targeting **PostgreSQL** (see
[docs/adr/0010](../../../docs/adr/0010-postgres-hosting-open.md) — hosting is open, the database
engine is decided). The layer structure itself (`Data.Db` / `Data.DbAccess` projects) is covered by
the **data-layer** skill; this is the EF-specific detail that lives inside it. The value-type
conversion machinery referenced below lives in `Common.Server` — see the **common-layer** skill.

---

## Configuration

- `Npgsql.EntityFrameworkCore.PostgreSQL` — code-first.
- `EFCore.NamingConventions` — snake_case table and column names. Always enabled.
- `EFCore.CheckConstraints` — database-level check constraints. Always enabled (PostgreSQL supports
  them).
- Default tracking: `QueryTrackingBehavior.NoTracking`, set on the context. DataAccess enables
  tracking only for the specific write operations that need it.

```csharp
builder.Services.AddDbContextFactory<MileseDbContext>(options => options
    .UseNpgsql(connectionString)
    .UseSnakeCaseNamingConvention()
    .UseValidationCheckConstraints()
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
```

`connectionString` comes from `appsettings.json`'s `ConnectionStrings` section. This call lives in
`Api`'s composition root (`Program.cs`) — the one place allowed to reference `Data.Db` directly, to
register the concrete `MileseDbContext` (see the repo's `CLAUDE.md`, Architecture rule 4).
`UseSnakeCaseNamingConvention()` (`EFCore.NamingConventions`) and `UseValidationCheckConstraints()`
(`EFCore.CheckConstraints`) only need setting at registration; `UseValueTypeTranslation()`
(`Common.Server` — lets `.Value` access on a value type translate to SQL instead of failing or
evaluating client-side) and `RegisterValueTypeConverters(...)` are called once each, inside
`MileseDbContext` itself (`OnConfiguring` / `ConfigureConventions` respectively) — not repeated at
registration.

## Entity definition

Prefer **attributes** over the Fluent API. Only add `OnModelCreating` configuration when attributes
are insufficient (composite keys, table splitting):

```csharp
[Table("lessons")]
public sealed class LessonDb
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public LessonId Id { get; set; } = null!;   // DB-generated — never required, see below

    public required LessonTitle Title { get; set; }
}
```

`*Db` properties are typed with the domain value type itself (`LessonId`, `LessonTitle`), not the raw
primitive — `Common.Server`'s converter registry (`RegisterValueTypeConverters`, called once from
`MileseDbContext.ConfigureConventions`) round-trips every property whose type implements `IValueType`
to/from its underlying column automatically, including a string value type's `MaxLength` and a numeric
one's `Precision`. Never add `[MaxLength(...)]`/`HasPrecision(...)` yourself — the number would just
duplicate (and could drift from) the value type's own `MaxLength`/`Precision`.

An identity-generated key (`[DatabaseGenerated(DatabaseGeneratedOption.Identity)]`) must **not** be
`required` — C#'s `new()` generic constraint (used by `IIdValueType<TSelf>` and every
`ValueTypeParser` method) cannot be satisfied by a type with required members, and every value type
implementing `IIdValueType<TSelf>` is exactly such a type. Give it `= null!` instead and let EF Core
populate it after `SaveChangesAsync`. Non-generated columns typed with a value type stay `required` as
normal — this only applies to the generated key itself.

## DataAccess constructor

Every `*ReadDataAccess`/`*UpdateDataAccess` class has exactly two constructor parameters — no more,
no less (see **naming-conventions** and **data-layer** for the Read/Update split each class
independently follows):

```csharp
public sealed class LessonsReadDataAccess
{
    private readonly IDbContextFactory<MileseDbContext> dbCntxFactory;
    private readonly CancellationToken cancellationToken;

    public LessonsReadDataAccess(
        IDbContextFactory<MileseDbContext> dbCntxFactory,
        CancellationToken cancellationToken)
    {
        this.dbCntxFactory = dbCntxFactory;
        this.cancellationToken = cancellationToken;
    }
}
```

No other instance state. No injected services. No configuration objects.

## Tracking for write operations

Reads use the `NoTracking` default. Opt into tracking explicitly for the entity you are about to
mutate:

```csharp
await using var ctx = await dbCntxFactory.CreateDbContextAsync(cancellationToken);
var entity = await ctx.Lessons
    .AsTracking()
    .SingleAsync(x => x.Id == id, cancellationToken);
entity.Title = input.Title.Value;
await ctx.SaveChangesAsync(cancellationToken);
```

## Migrations

```bash
# Create a migration (run from the solution root — apps/api)
dotnet ef migrations add <MigrationName> --project src/Data/Data.Db --startup-project src/Api/Api.Rest

# Apply manually
dotnet ef database update --project src/Data/Data.Db --startup-project src/Api/Api.Rest
```

Migrations live inside `Data.Db` itself (no separate `Data.Migrations` project) — Milese's a single
personal-scale API host, not a multi-host solution that needs migrations decoupled from the DbContext
project. `--startup-project` points at `Api.Rest` because that's where the connection string and
`AddDbContextFactory<MileseDbContext>` registration live.

Migration names: PascalCase describing the schema change (`AddTitleToLesson`, `CreateProgressTable`).
Never hand-edit generated migration files — add a new migration to correct mistakes.

**Do not read existing migration files for common tasks.** Generated migration files are large,
mechanical, and rarely inform work outside their own concern — reading them by default wastes
context. Only open/read migration files when the task is migration-related: a `*Db` entity or
`DbContext` change that affects the schema, generating or reviewing a new migration, or a task that
specifically requires comparing/reconciling migration history against the current model.

---

## See also

- **data-layer** — the persistence layer projects and the `*Db`/`*DataAccess`/`*Bo` rules this fits into
- **common-layer** — `Common.Server`'s converter/translator/pagination helpers, and where a Value
  Type's `MaxLength`/`Precision` come from
