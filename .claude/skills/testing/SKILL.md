---
name: testing
description: >-
  The testing rules for apps/api: TUnit on Microsoft.Testing.Platform, one dedicated `<Project>.Tests`
  twin project per source project (folder structure mirrors the source, one file per public method),
  Shouldly for Common.Shared/Common.Types (pure logic), TUnit-native `Assert.That(...).IsStrictlyEqualTo(...)`
  for integration-layer projects (Data.DbAccess/Services.Core/Api.Rest), the shared
  `DatabaseIntegrationTest` base class (SQLite in-memory by default, real PostgreSQL with per-test
  schema isolation when configured) from `Tests.Integration`, and the `Tests/ArchTests` project holding
  `NetArchTest.Rules` layering fitness tests. Read and apply this BEFORE writing or changing any test,
  adding a public method, or deciding a task is done. Trigger on any testing work, not only when
  "testing"/"test" is named.
---

# Testing conventions

**Coverage rule: every public method gets a test.** Not aspirational — a change is not done until the
corresponding test file exists or is updated. `Tests/ArchTests` and `Tests.Integration` are the two
exceptions (see below): they support every other test project rather than testing a single one.

## Framework

**TUnit** on **Microsoft.Testing.Platform (MTP)**, never VSTest. `[Test]`, not xUnit's `[Fact]`. Every
test project is `OutputType=Exe` — an MTP test project *is* the runner, so it can also be executed
directly as a binary. `apps/api/global.json` opts the whole solution into MTP mode
(`"test": { "runner": "Microsoft.Testing.Platform" }`) — `dotnet test` therefore only understands MTP
syntax:

- Name the target: `--project <csproj>` or `--solution Milese.slnx`. A bare path
  (`dotnet test tests/X`) is VSTest-era syntax and fails.
- Filter with `--treenode-filter "/*/*/<ClassName>/*"`, not `--filter "Name=…"`.
- No `--` separator.

## Project structure — twin project rule

Every production project has a twin test project, same name plus `.Tests`, under `apps/api/tests/`:

```
src/Common/Common.Shared     -> tests/Common.Shared.Tests
src/Common/Common.Types      -> tests/Common.Types.Tests
src/Data/Data.DbAccess       -> tests/Data.DbAccess.Tests
src/Services/Services.Core   -> tests/Services.Core.Tests
src/Api/Api.Rest             -> tests/Api.Rest.Tests
```

A twin project only tests its own production project. Two projects under `tests/` are **not** twins:

| Project | Role |
|---|---|
| `Tests.Integration` | shared infrastructure — `DatabaseIntegrationTest` and the SQLite/Postgres test-database backends. A library, referenced by the integration-layer twins; not runnable on its own. |
| `Tests` | solution-wide `ArchTests/` (`NetArchTest.Rules` layering fitness tests) — covers no single project. |

`Data.Db` has no twin — it's entities and a `DbContext`, exercised indirectly through
`Data.DbAccess.Tests`. `Common.Server` has no twin either — it's exercised the same way via
`Data.DbAccess.Tests` and `MileseDbContext`'s use of it.

### File location — one file per public method

Test files mirror the source project's **internal** folder structure, source project name stripped:

```
Source: src/Services/Services.Core/Curriculum/LessonsUpdateService.cs
Test:   tests/Services.Core.Tests/Curriculum/CreateAsyncTests.cs
        tests/Services.Core.Tests/Curriculum/UpdateAsyncTests.cs
```

Each public method gets its own `<MethodName>Tests.cs`, holding every scenario for that method
(success, each failure branch, boundary conditions) as separate `[Test]`s. Mappers, Bo records, and
other pure-data types with no behavior beyond field assignment don't need a dedicated test file — there
is nothing to assert beyond what the compiler already guarantees.

### Convention tests over one file per Value Type

A family of Value Types that share the exact same contract (every `IIdValueType<TSelf>` implementer:
`Parse`, `FieldName`, `ParseAsync`) gets **one reflection-based convention test**
(`ValueTypes/Identity/IdValueTypeConventionTests.cs`) that discovers every implementer in the assembly
and runs the same checks against each — not a dozen near-identical per-type files. A Value Type whose
contract isn't shared this way (a bounded number, a string with its own `MaxLength`) still gets its own
`<Name>Tests.cs`.

## Assertion policy

| Layer | Style |
|---|---|
| `Common.Shared`, `Common.Types` (pure logic, Value Types) | **Shouldly** (`result.ShouldBe(...)`) |
| `Data.DbAccess`, `Services.Core`, `Api.Rest` (integration-layer) | **TUnit-native** `Assert.That(...)` — no Shouldly dependency |

Both are intentional. Don't add Shouldly to an integration-layer test project, and don't switch
TUnit-native assertions to Shouldly there.

**Use `IsStrictlyEqualTo`, never `IsEqualTo`, in TUnit-native assertions.** `IsEqualTo` applies a
conversion, so a test can pass while comparing a Value Type against a raw primitive — the very
confusion Value Types exist to prevent. `IsStrictlyEqualTo` compares declared types and catches that.
This is also why every Value Type is a `readonly record struct`, never a plain `class` (see
`common-layer`): `IsStrictlyEqualTo`/`==` need value equality, which only `record` generates for free.
`Bo`s are the opposite — plain `sealed class`, no structural equality — so compare their fields
individually in assertions, never a whole `Bo` against another.

## Layer isolation — trust the layers below

Each layer assumes the functions it calls into other layers are already tested. A `Services.Core` test
covering `LessonsUpdateService.CreateAsync` doesn't re-verify `LessonsUpdateDataAccess`'s own SQL
translation — that's `Data.DbAccess.Tests`' job. Substitute or configure collaborators only as needed to
keep the test focused on the layer under test's own logic. When a rule moves down a layer (e.g. a
filter moves from a service-side loop into a database query), its test moves with it rather than being
duplicated at both levels.

## Integration-test infrastructure (`Tests.Integration`)

`DatabaseIntegrationTest` (`Tests.Integration/DatabaseIntegrationTest.cs`) is the base class for any
test needing a real database:

```csharp
public sealed class CreateAsyncTests : DatabaseIntegrationTest
{
    [Test]
    public async Task Creates_a_lesson_with_a_positive_id()
    {
        var conceptId = await LessonsArrange.InsertConceptAsync(DbContextFactory, "Hash Tables");
        var updateDataAccess = new LessonsUpdateDataAccess(DbContextFactory, CancellationToken.None);

        var lesson = await updateDataAccess.CreateAsync(conceptId, title, estimatedMinutes, order);

        await Assert.That(lesson.Id.Value).IsGreaterThan(0);
    }
}
```

It exposes `DbContextFactory` (`IDbContextFactory<MileseDbContext>`) — the same factory injected into
the `*DataAccess` class under test — and initializes/tears down the backend automatically
(`IAsyncInitializer`/`IAsyncDisposable`, both wired by TUnit).

### Backend selection

Controlled by each test project's `appsettings.json` (`IntegrationTests:Provider`/`ConnectionString`)
or the `MILESE_INTEGRATION_TESTS_PROVIDER`/`MILESE_INTEGRATION_TESTS_CONNECTIONSTRING` environment
variables (env vars win):

- **`Sqlite` (default)** — a single in-memory SQLite connection kept open for the test's lifetime, so
  every `IDbContextFactory.CreateDbContext()` call shares the same in-memory database. No container,
  no server — this is what runs locally and is what every test project's `appsettings.json` ships with.
- **`Postgres`** — the real provider, with each test isolated by a random schema suffix appended to
  `curriculum` (e.g. `curriculum_it_ab12cd34`) so many tests can share one physical database without
  colliding; the suffixed schema is dropped when the test finishes. Requires
  `IntegrationTests:ConnectionString` (or the env var). Use this to verify Postgres-specific behavior
  (check constraints, snake_case columns) that SQLite's own check-constraint support might not catch
  identically — reach for it deliberately, not as the default loop.

Write tests so the same body passes against either backend — a test that only passes on SQLite because
it relies on something the real provider enforces differently (or vice versa) is a bug in the test, not
something to special-case.

### `*Arrange` helpers

Each integration-layer twin project keeps its own `<Entity>Arrange.cs` (e.g.
`Data.DbAccess.Tests/Curriculum/LessonsArrange.cs`) with static helpers that seed rows directly via the
`DbContextFactory` and, for `Services.Core.Tests`/`Api.Rest.Tests`, build the real object graph under
test (`BuildReadService`/`BuildUpdateService`/`BuildController`, wiring the real `*DataAccess` — never
a mock). Each twin project keeps its own copy — don't share `*Arrange` classes across projects that
test different layers; a small amount of duplicated seeding code is the cost of each layer's tests
staying independently runnable.

### Controller tests (`Api.Rest.Tests`)

Controller tests instantiate the controller directly — no `TestServer`, no HTTP involved — via the
project's `*Arrange.BuildController(...)`. One test file per action, same file-per-method rule as
everywhere else.

## Architecture fitness tests (`Tests/ArchTests`)

`LayerDependencyTests.cs` asserts the dependency rules from `CLAUDE.md`'s Architecture rules hold in
the compiled assemblies (`Common.Shared` depends on nothing, `Services.Core` never touches
`Microsoft.EntityFrameworkCore`/`Data.Db`, `Api.Rest` controllers never touch `Data.Db`/`Data.DbAccess`
except the `Milese.Api.Rest.Extensions` composition root and `Program.cs`, ...). Update it whenever a
project is added to the solution or a layer-boundary rule changes — a stale arch test gives false
confidence.

## Commands

```bash
# every test project
dotnet test --solution Milese.slnx

# one project
dotnet test --project tests/<Name>.Tests/<Name>.Tests.csproj

# one class, or one test
dotnet test --project tests/<Name>.Tests/<Name>.Tests.csproj --treenode-filter "/*/*/<ClassName>/*"
dotnet test --project tests/<Name>.Tests/<Name>.Tests.csproj --treenode-filter "/*/*/*/<TestName>"

# run a project's binary directly
./tests/<Name>.Tests/bin/Debug/net10.0/Milese.<Name>.Tests
```

## See also

- **common-layer** — why every Value Type is a `readonly record struct` (equality, the `new()`
  constraint) and every `Bo` a plain `sealed class`
- **ef-core** — `MileseDbContext` configuration that `Tests.Integration` mirrors for its test backends
- **railway-oriented-programming** — `Result<T, InvalidData>`, the shape most integration assertions
  check
