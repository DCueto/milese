# Milese.Common.Shared

Pure functional primitives — zero domain knowledge, zero platform dependencies. Safe to reference from every project in the solution.

**Allowed dependencies:** none (no other Milese project).

> Skills: **common-layer** · **railway-oriented-programming** · **localization**

---

## Contents

| Type | Purpose |
|---|---|
| `Result<T, TErr>` | Railway-oriented result for fallible operations |
| `Either<TLeft, TRight>` | Discriminated union — `Left` is the error/alternative case, `Right` is the success/primary case |
| `NotEmptyList<T>` | Enforces non-empty collections at the type level |
| `AllOrSome<T>` | Replaces the "empty list means all" convention with an explicit, EF-translatable type |
| `IValueType<TSelf, T>` / `IIdValueType<TSelf>` / `IStringValueType<TSelf>` / `INumericValueType<TSelf>` / `IDateTimeValueType<TSelf>` | Value-type contracts consumed by `ValueTypeParser` (here) and `ValueTypeConverter`/`ValueTypeModelConfigurationExtensions` (`Common.Server`) |
| `ValueTypeParser` | Shared `Parse()` building blocks (not-empty, max-length, strictly-positive, date-in-range, ...) |
| `InvalidData` / `InvalidDataConstraint` | Structured validation error — see **localization** for how it becomes user text |
| `PagedQuery<TFilter, TField>` / `PagedItems<T>` / `SortableField<TDb, TField>` | Pagination primitives (Common.Server's `QueryablePaginationExtensions` executes them) |

## Result<T, TErr> API

```csharp
Result<T, TErr>.Success(value)
Result<T, TErr>.Failure(error)
result.Map(fn)
result.Match(onSuccess, onFailure)
result.OnSuccess(action) / result.OnFailure(action)
```

Never throw for expected failures — return `Result<T, TErr>` instead. See the **railway-oriented-programming** skill.

## AllOrSome<T>

`AllOrSome<T>.IsAll` is banned via `BannedSymbols.txt` (RS0030) — read it directly and you can silently forget the other branch. Always go through `Match`/`Switch`.

## Localization

Owns cross-cutting validation-error templates in `Resources/Strings.resx` (Spanish, default/fallback) + `Strings.en.resx`.

`InvalidData.ClientMessage()` is the single chokepoint where a structured error becomes user-facing text. Call it at the edge (an `Api` controller/endpoint) — never inside `Parse()`.
