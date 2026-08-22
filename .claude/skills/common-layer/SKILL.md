---
name: common-layer
description: >-
  The zero-dependency shared base every other apps/api layer sits on: browser-agnostic
  `Common.Shared` (monads `Result<T,TErr>`, `Either<TLeft,TRight>`, `NotEmptyList<T>`,
  `AllOrSome<T>`, the `IValueType`/`IIdValueType`/`IStringValueType`/`INumericValueType`/
  `IDateTimeValueType` contracts, `ValueTypeParser`) and server-only `Common.Server` (EF Core
  converters, member translation, pagination — never referenced above `Data.DbAccess`). Also covers
  how a concrete `Common.Types` Value Type implements those contracts. Read and apply this BEFORE you
  add a shared utility or monad, write a new Value Type, or decide whether a helper belongs in Shared
  or Server — and whenever you wonder "where do generic helpers go?", "can Services.Core use this?",
  "Shared or Server?", "how do I implement IStringValueType?". Trigger on any cross-cutting utility
  work or new domain primitive, not only when "common" is named.
---

# Common Layer

`apps/api/src/Common/` holds two projects. Both are vendored framework code (see their own
`CLAUDE.md` files) — extend them deliberately, don't casually add to them.

---

## Common.Shared — the always-safe base

Monads (`Result<T,TErr>`, `Either<TLeft,TRight>`, `NotEmptyList<T>`, `AllOrSome<T>`) and the
value-type contracts (`IValueType<TSelf,T>`, `IIdValueType<TSelf>`, `IStringValueType<TSelf>`,
`INumericValueType<TSelf>`, `IDateTimeValueType<TSelf>`) plus `ValueTypeParser`, with no platform
or server dependency.

- References **nothing else** in the solution.
- No EF Core, no ASP.NET Core, no server-only packages.
- Safe to reference from **every** project — `Common.Types`, `Data.*`, `Services.*`, `Api`, and any
  future non-.NET consumer that shares its DTOs over the wire.

The `Result<T,TErr>` type here is the backbone of error handling everywhere else; the usage pattern
is the `railway-oriented-programming` skill. `IStringValueType<TSelf>.MaxLength` /
`INumericValueType<TSelf>.Precision` are what let `Common.Server`'s converter registry configure
column length/precision automatically — see `ef-core`.

Milese doesn't have a browser boundary (mobile/web are separate TypeScript codebases, not a
Blazor/WASM edge referencing this project directly) — the Shared/Server split exists instead to keep
EF Core out of the pure functional core, so `Common.Shared` stays trivially unit-testable and
`Common.Types`/`Services.Core` never accidentally pull in `Microsoft.EntityFrameworkCore`.

---

## Common.Server — EF Core-only glue

Server-side helpers that only make sense with EF Core: the `ValueTypeConverter<TValueType,T>` +
`ValueTypeModelConfigurationExtensions.RegisterValueTypeConverters(...)` pair that lets every
`*Db` entity declare properties with the domain value type instead of the raw primitive,
`ValueTypeMemberTranslator(Plugin)` (translates `.Value` access to SQL), and
`QueryableFilterExtensions`/`QueryablePaginationExtensions`.

- **May depend on:** `Common.Shared` only.
- Referenced from `Data.Db` — the `DbContext` (`MileseDbContext`) calls
  `RegisterValueTypeConverters(...)` from `ConfigureConventions` and `UseValueTypeTranslation()` from
  `OnConfiguring` — and, when a query needs `WhereIfValue`/`ToPagedResultAsync`, from `Data.DbAccess`.
  Never from `Services.Core` or `Api`. If a helper here seems useful in `Services.Core`, that's a sign
  it belongs in `Common.Shared` instead, or that the calling code is reaching past `Data.DbAccess` (a
  rule-4 violation — see the repo's `CLAUDE.md`).
- `Api`'s composition root (`Program.cs`) is the one place allowed to reference `Data.Db` directly, to
  register `MileseDbContext` in DI — that's wiring, not the "no layer skipping" rule, which is about
  request-handling/business-logic code paths never touching `Data.Db`/`Data.DbAccess`.

---

## Implementing a Value Type

Every domain primitive lives in `Common.Types/ValueTypes/<Group>` (see `naming-conventions` for the
file/folder pattern) as a **`readonly record struct`** implementing the matching `Common.Shared`
contract, `Value` declared as its single positional parameter, with a static `Parse(...)` returning
`Result<TSelf, InvalidData>`. There is no way to get an instance that skips validation through `Parse`
in application code — the positional constructor and the object-initializer form
(`new TSelf { Value = ... }`) used internally by `ValueTypeParser` and the EF Core converter both exist,
but only the latter two are trusted-input-only call sites inside the framework.

**`readonly record struct`, not a class.** `IValueType<TSelf,T>` implements `IComparable<TSelf>` via a
default interface method, but interfaces cannot override `object.Equals`/`GetHashCode` — a plain
`class` falls back to reference equality, so two value types wrapping the identical underlying `Value`
compare unequal (`IsStrictlyEqualTo` fails, `==` fails, dictionary/set lookups fail). A `record` (class
or struct) generates value-based `Equals`/`GetHashCode`/`ToString` for free; `struct` is the right flavor
here — a value type wrapping one primitive has no business being heap-allocated, and struct's *implicit*
parameterless constructor (present alongside any declared constructor, unlike a class) is what keeps
`ValueTypeParser`'s `new()` constraint satisfied. This mirrors `iplan-nexus-core`'s own value types
exactly (e.g. `public readonly record struct CaseId(int Value) : IIdValueType<CaseId>`) — check a
reference project's actual concrete types before assuming a shape from its interfaces alone.

**`Value` is never `required`.** Every `ValueTypeParser` method (and `IIdValueType<TSelf>`/
`IDateTimeValueType<TSelf>` directly) constrains its own `TSelf` with `new()`, and C# forbids a `new()`
constraint from being satisfied by a type with `required` members (CS9040) — so a Value Type that goes
through `ValueTypeParser` at all can't mark `Value` `required` without breaking the build. A positional
record struct's generated property isn't `required` by default, so this is automatic — just don't add
the modifier yourself.

**Bounded number** — implement `IValueType<TSelf,T>` directly when no specialised contract fits:

```csharp
public readonly record struct EstimatedMinutes(int Value) : IValueType<EstimatedMinutes, int>
{
    public static Result<EstimatedMinutes, InvalidData> Parse(int value) =>
        value is >= 1 and <= 15
            ? new EstimatedMinutes(value)
            : new InvalidData
            {
                FieldName = nameof(EstimatedMinutes),
                InnerValue = value,
                Constraint = new InvalidDataConstraint.GenericError(),
            };
}
```

**Identifier** — implement `IIdValueType<TSelf>` and delegate to `ValueTypeParser`, which already
encodes "strictly positive" and the async exists-check:

```csharp
public readonly record struct LessonId(int Value) : IIdValueType<LessonId>
{
    public static string FieldName => nameof(LessonId);

    public static Result<LessonId, InvalidData> Parse(int value) =>
        ValueTypeParser.StrictlyPositive<LessonId>(value, FieldName);

    public static Task<Result<LessonId, InvalidData>> ParseAsync(int value, Func<LessonId, Task<bool>> exists) =>
        ValueTypeParser.ParseIdAsync(value, FieldName, exists);
}
```

Use `ParseAsync` only where the caller needs to confirm the referenced row actually exists (it costs a
query) — `Parse` alone is enough anywhere the ID is trusted to already reference something real (e.g.
reconstructing a `Bo` from a `*Db` row).

**String** — implement `IStringValueType<TSelf>` and delegate to `ValueTypeParser`; declaring
`MaxLength` is what lets `Common.Server`'s converter registry set the database column's length
automatically (see `ef-core`) — never hardcode the number in a `[MaxLength(...)]` attribute instead:

```csharp
public readonly record struct LessonTitle(string Value) : IStringValueType<LessonTitle>
{
    public static int MaxLength => 200;

    public static Result<LessonTitle, InvalidData> Parse(string? value) =>
        ValueTypeParser.StringNotEmptyAndMaxLength<LessonTitle>(value, nameof(LessonTitle));
}
```

On a `*Db` entity, a value-type `[Key]` column with `[DatabaseGenerated(DatabaseGeneratedOption.Identity)]`
needs no `= null!`/initializer at all — a struct's implicit default (`Value == 0`) is already what an
unset, not-yet-DB-generated ID should be, and it isn't subject to the nullable-reference-type warning
that forced that workaround for a class-shaped value type. See `ef-core`.

Rules:

- Never expose a way to construct a Value Type without going through `Parse` for untrusted input — no
  public parameterless constructor, no implicit conversion from the raw primitive.
- Reuse a `ValueTypeParser` method instead of hand-rolling validation whenever the shape matches one
  already there (not-empty, max-length, exact-length, format, strictly-positive, min-value,
  date-in-range). Add a new `ValueTypeParser` method — not ad hoc validation in the value type itself
  — when a genuinely new validation shape recurs across more than one value type.
- If two Value Types validate the same shape of thing (e.g. every ID is "strictly positive int"),
  share the logic via `ValueTypeParser`, not the boilerplate around it.

### `InvalidData` / `InvalidDataConstraint`

`InvalidData` (`Common.Shared/InvalidData.cs`) is a `record` with three members — always built via
object initializer, never a positional constructor:

```csharp
public record InvalidData
{
    public required InvalidDataConstraint Constraint { get; init; }
    public required string? FieldName { get; init; }
    public object? InnerValue { get; init; }
}
```

`Constraint` is a closed discriminated union (`abstract record InvalidDataConstraint` with `sealed
record` cases — `MinLength`, `MaxLength`, `MinValue`, `MaxValue`, `EnumNotDefined`, `IdNotFound`,
`FormatInvalid`, `CanNotBeNull`, `CanNotBeEmptyOrNull`, `StrictlyPositive`, `GenericError`,
`AlreadyExists`, `AlreadyResolved`, `AlreadyAssigned`). Pick the case that names the actual constraint
that failed — reach for `GenericError` only when none of the specific cases fit. Never build a display
string here: `InvalidData.ClientMessage()` is the only place a constraint becomes user-facing text
(see `localization`) — `Parse()` never formats a message itself.

## AllOrSome<T>

`AllOrSome<T>.IsAll` is banned via `BannedSymbols.txt` (RS0030) — read it directly and you can silently
forget the other branch. Always go through `Match`/`Switch`.

## Localization

`Common.Shared` owns cross-cutting validation-error templates in `Resources/Strings.resx` (Spanish,
default/fallback) + `Strings.en.resx`. See `localization` for the full model.
