---
name: railway-oriented-programming
description: Model an operation that can fail in an expected way (validation, not-found, a business rule, a conflict). Trigger whenever writing code with a failure path that isn't a genuine bug/unrecoverable-state — not only when the user says "Result type" explicitly.
---

# Railway-oriented programming

`Result<T, TErr>` (`Common.Shared/Result.cs`) represents "this operation either succeeded with a
value or failed with a known reason" — used for every *expected* failure. Exceptions are reserved for
genuinely exceptional, unrecoverable states (a corrupted config, a bug that should crash a background
job) — never for "the input was invalid" or "the row wasn't found."

## Core shape

```csharp
public readonly struct Result<T, TErr>
{
    public static Result<T, TErr> Success(T value);
    public static Result<T, TErr> Failure(TErr error);

    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public T Value { get; }      // throws InvalidOperationException if IsFailure
    public TErr Error { get; }   // throws InvalidOperationException if IsSuccess

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<TErr, TOut> onFailure);
    public Result<TOut, TErr> Map<TOut>(Func<T, TOut> mapper);
    public Result<TOut, TErr> Map<TOut>(Func<T, Result<TOut, TErr>> mapper);
    public Result<T, TErr> OnSuccess(Action<T> action);
    public Result<T, TErr> OnFailure(Action<TErr> action);

    public static implicit operator Result<T, TErr>(T value);      // Success(value)
    public static implicit operator Result<T, TErr>(TErr error);   // Failure(error)
}
```

`T`/`TErr` implicit conversions mean a `Parse()` method can `return new EstimatedMinutes(value);` or
`return someInvalidData;` directly, without wrapping in `Result<...>.Success(...)`/`Failure(...)`.

`TErr` is almost always `InvalidData` (`Common.Shared/InvalidData.cs`) for validation failures — see
the `common-layer` skill for its shape and how a Value Type's `Parse()` builds one.

## Chaining

`MapExtensions` (`Common.Shared/MapExtensions.cs`) and `ResultLinqExtensions`
(`Common.Shared/ResultLinqExtensions.cs`) provide the chaining vocabulary — prefer these over manual
`IsSuccess` checks:

- `.Map(fn)` — transform the success value (or short-circuit on failure), sync or `Task`-returning `fn`.
- `.Map(fn)` where `fn` returns another `Result<TOut, TErr>` — chain a second fallible step, same
  error type (this is the `Bind`/`Then` operator, spelled `Map` here).
- LINQ query syntax works directly, via `Select`/`SelectMany`:
  ```csharp
  from title in LessonTitle.Parse(input.Title)
  from minutes in EstimatedMinutes.Parse(input.EstimatedMinutes)
  select new LessonBo { Title = title, EstimatedMinutes = minutes };
  ```
- `.Map<TOut, TErr2>(fn)` where `fn`'s failure type differs from the source — combines both into
  `Result<TOut, Either<TErr, TErr2>>` automatically. Reach for this only when two steps can fail with
  genuinely different error types; don't force it when both already use `InvalidData`.
- `.OnSuccess(action)` / `.OnFailure(action)` — side effects (logging, an event) without leaving the
  rail; both return the original `Result` unchanged.

## Rules

- Never `try/catch` around an expected failure — return `Result.Failure(...)` (or the implicit
  conversion) instead. A `try/catch` in application code is a signal something is modeled wrong, not a
  normal control-flow tool.
- Never throw to signal "not found," "invalid," or "already exists" — those are `Result.Failure`s
  carrying an `InvalidData` with the matching `InvalidDataConstraint` case (`IdNotFound`,
  `FormatInvalid`, `AlreadyExists`, ...), not exceptions.
- Check `IsSuccess`/`IsFailure` (or use `Match`) before touching `.Value`/`.Error` — reading the wrong
  one throws `InvalidOperationException`. Prefer `Match`/`Map`/`OnSuccess`/`OnFailure` over a bare
  `if (result.IsSuccess)` block wherever the chaining vocabulary above already covers the shape you need.
- When chaining multiple fallible steps, propagate the first failure and stop — don't run subsequent
  steps against a failed intermediate result. `.Map(...)` and LINQ query syntax already do this; don't
  hand-roll the short-circuiting.
- A `Bo` built from Value Types (see `common-layer`) is valid by construction — a `Services.Core`
  method receiving a `Bo` should never re-validate fields the `Bo`'s own construction already
  guaranteed.
- `Either<TLeft, TRight>` (`Common.Shared/Either.cs`) is the general discriminated union underneath
  the two-different-error-types case above — reach for it directly only when you need "one of two
  types" outside a `Result`.

## See also

- **common-layer** — where `Result`, `Either`, and the chaining extensions live (`Common.Shared`),
  `InvalidData`/`InvalidDataConstraint`'s shape, and how a Value Type's `Parse()` uses this pattern
- **localization** — how `InvalidData` becomes a user-facing message (`ClientMessage()`), never inline
