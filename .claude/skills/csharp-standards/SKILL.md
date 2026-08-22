---
name: csharp-standards
description: >-
  The C# coding standards for apps/api: a functional-first style with `sealed` classes,
  `required` init-only properties, file-scoped namespaces, explicit usings, no primary
  constructors, enums starting at 1, LINQ idioms (`Any()` over `Count`, `Single()` over `Find()`),
  never-nullable collections, and no external mapping libraries. Read and apply this BEFORE writing
  or reviewing ANY C# here — even a one-line edit — and whenever you add a class, property, enum,
  collection, LINQ query, or type-to-type mapping, or wonder "sealed or not?", "should this be
  required?", "where do enums start?", "Any() vs Count?", "can I use AutoMapper?", "braces on this
  if?". Trigger on any C#/.NET authoring or review in this repo, not only when "coding standards" is
  named explicitly.
---

# C# Coding Standards

These standards exist to make code **immutable by default, correct by construction, and uniform**
across `apps/api`. The throughline is *functional-first*: model data as values, make illegal states
unrepresentable, and let the types carry correctness so the runtime has less to check.

## Functional-first, not functional-dogmatic

C# is multi-paradigm with strong functional support (records, pattern matching, LINQ,
expression-bodied members, immutability). Lean into it: prefer immutable values, pure
transformations, and errors-as-values. Use OOP where the platform is built around it — dependency
injection, controllers, the EF Core `DbContext` and entities. When a problem can be modelled either
way, choose the functional option; when functional would fight the framework or hurt clarity, choose
readable idiomatic C# and move on. The concrete rules below are expressions of that aim.

Two pillars have their own skills — apply them, don't restate them here:
- **Errors are values** → `Result<T, TErr>`, never `throw` for an expected failure. See the
  `railway-oriented-programming` skill.
- **Validate at the boundary, once** → value types: constructor for trusted data, `Parse()` for
  untrusted. See the `common-layer` skill.

## Class rules

- **`sealed` by default.** Open for inheritance only with a deliberate reason.
- **`required` properties.** Init-only `required` members so an object can't exist half-built.
- **File-scoped namespaces** — `namespace X.Y;`, never braced.
- **No primary constructors** — use a regular constructor.
- **Value types for almost every property** rather than bare primitives (see `common-layer`).

```csharp
public sealed class LessonsUpdateService
{
    private readonly LessonsUpdateDataAccess lessonsUpdateDataAccess;

    public LessonsUpdateService(LessonsUpdateDataAccess lessonsUpdateDataAccess) =>
        this.lessonsUpdateDataAccess = lessonsUpdateDataAccess;
}
```

## Specific patterns

- **`record` only for value types**; use `class` everywhere else.
- **Collections are never nullable** — initialize to empty (`[]`, `new List<T>()`), never `null`.
- **Return `IReadOnlyCollection<T>`** from a method handing a collection to a caller. The caller gets
  the count and can enumerate; it cannot mutate a collection it doesn't own.
- **Single-line `if` without `else` → no braces.**
- **Expression-bodied members** when the body is a single expression.
- **Encode invariants in types**, not in scattered runtime checks.

## Enums

- **Always start at `1`, never `0`** — `0` is the silent default for an uninitialized value, so
  starting at 1 means "unset" can't masquerade as a valid member.
- **Validate when loading from external input** — never assume an arbitrary number maps to a
  defined member (see `EnumValue.Parse<T>` in `Common.Shared`).
- **Never write a discard arm (`_ =>`) when switching on an enum.** Listing every member makes the
  compiler flag the switch the day a member is added; a discard arm swallows the new member into
  whatever the fallback happened to be, silently and at runtime. Handle a genuinely-invalid value by
  throwing in a named arm, not by catching everything in a discard.

```csharp
public enum LearningMode
{
    Gated = 1,
    FreeBrowse = 2,
}
```

## LINQ

Use LINQ for all collection processing, and pick the operator that states intent:
- **`Any()`**, never `Count != 0`.
- **`Single()`** when exactly one element is expected; **`SingleOrDefault()`** for at most one.
- **`First()`/`FirstOrDefault()`** when more than one is possible and you want the first.
- **Never `Find()`** — use the LINQ operators above.

## No external mapping libraries

No AutoMapper, Mapster, or similar. Write **explicit extension methods** for type conversions
(e.g. entity↔business-object, in a `*Mapper` class per the `data-layer` skill). Reflection-based
mapping hides transformations and breaks silently when a field is added; a hand-written `ToBo()` is
visible and type-checked.

## Project / build conventions

- Target `net10.0`, latest C# language version (see `apps/api/Directory.Build.props`).
- Nullable reference types **enabled**.
- `ImplicitUsings` **disabled** — write explicit `using` statements.
- `TreatWarningsAsErrors` **always on** — warnings are failures.
- Standard analyzers on (SonarAnalyzer, .NET analyzers, threading analyzers), configured centrally
  via `Directory.Build.props` / `Directory.Packages.props`.

## Comments

**Do not write comments.** No inline comments, no XML doc comments, no `<summary>` blocks, no region
markers, no TODOs — unless the human explicitly asks for a comment at that specific place. Well-named
identifiers are the documentation.

This is absolute and has **no "but the WHY is non-obvious" exception.** A subtle invariant, a hidden
constraint, a workaround, or a surprising behaviour is not a licence to add a comment — that explanation
belongs in the PR description, the commit message, or an ADR, where it is versioned, reviewed and
findable. A comment in the source is none of those things, and it rots in place the moment the code
around it moves.

The one deliberate exception in this repo is `Common.Shared`/`Common.Server`: they're a vendored
functional-programming framework, not application code, and keep their XML doc comments because they're
a public API surface consumed by every other layer. Don't extend that exception to `Common.Types`,
`Data.*`, `Services.*`, or `Api` — those follow the no-comments rule above.

- Do not restore comments you were previously asked to remove, and do not reintroduce the same
  explanation reworded on a later edit.
- Leave **existing** comments alone. Don't delete or rewrite one unless your change makes it factually
  wrong — then fix it or drop it.

## Language

**All code is written in English** — identifiers, types, methods, properties, comments, constants,
enums, and file names. This holds regardless of Content Language — see the repo's `CLAUDE.md` →
Always-on baseline and the `localization` skill for the distinction.
