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
- Referenced only from `Data.DbAccess` (where the `DbContext` and `*ReadDataAccess`/`*UpdateDataAccess`
  classes live) — never from `Services.Core` or `Api`. If a helper here seems useful in
  `Services.Core`, that's a sign it belongs in `Common.Shared` instead, or that the calling code is
  reaching past `Data.DbAccess` (a rule-4 violation — see the repo's `CLAUDE.md`).

---

## Implementing a Value Type

Every domain primitive lives in `Common.Types/ValueTypes/<Group>` (see `naming-conventions` for the
file/folder pattern) as a `sealed class` implementing the matching `Common.Shared` contract, with a
`Value` init-only property and a static `Parse(...)` returning `Result<TSelf, InvalidData>`. There is
no way to get an instance that skips validation through `Parse` — the object initializer
(`new TSelf { Value = ... }`) used internally by `ValueTypeParser` and the EF Core converter is the
only other construction path, and both are trusted-input-only call sites inside the framework, never
something application code calls directly.

**Bounded number** — implement `IValueType<TSelf,T>` directly when no specialised contract fits:

```csharp
public sealed class EstimatedMinutes : IValueType<EstimatedMinutes, int>
{
    public required int Value { get; init; }

    public static Result<EstimatedMinutes, InvalidData> Parse(int value) =>
        value is >= 1 and <= 15
            ? new EstimatedMinutes { Value = value }
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
public sealed class LessonId : IIdValueType<LessonId>
{
    public required int Value { get; init; }

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
public sealed class LessonTitle : IStringValueType<LessonTitle>
{
    public required string Value { get; init; }

    public static int MaxLength => 200;

    public static Result<LessonTitle, InvalidData> Parse(string? value) =>
        ValueTypeParser.StringNotEmptyAndMaxLength<LessonTitle>(value, nameof(LessonTitle));
}
```

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
