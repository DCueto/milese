---
name: localization
description: >-
  How API-facing text (validation/error messages, value-type field labels) is localized: never
  hardcoded, always in `.resx` resource files keyed by name and resolved against the current culture,
  with the Spanish default-culture file complete (it is the fallback) and an English override. Not to
  be confused with Content Language (the Spanish/English editions of Lesson prose, authored as
  content and stored in the database per docs/adr/0011) — this skill covers code-level UI/API text,
  not Lesson content. Covers adding a localized string across both culture files, ordered `{0}`
  placeholders that stay position-identical across cultures, and the structured-error pattern where
  fallible parsing returns `Result<T, InvalidData>` carrying a localized field label + a
  machine-readable `Constraint` (a discriminated union), with the display sentence built lazily at the
  edge by `ClientMessage()` — never a pre-built message string. Read this BEFORE adding or changing
  user-facing text, a validation/error message, or culture handling. Trigger on i18n, `.resx`,
  cultures, or validation-message work.
---

# Localization (i18n)

API-facing text ships in Spanish (default/fallback) and English — the same two cultures decided for
Content Language in [docs/adr/0011](../../../docs/adr/0011-content-language-spanish-and-english-at-mvp.md),
but this is a **separate axis**: Content Language is Lesson prose authored as content and stored in
the database; this skill is about code-owned text (validation messages, field labels) that ships in
`.resx` files inside `apps/api`. See `CONTEXT.md`'s Content Language entry if the distinction is
unclear. Text is never hardcoded: it lives in `.resx` resource files keyed by name and is resolved at
runtime against the current culture.

> **Code is still English** (see the **csharp-standards** skill). Resource *keys* and identifiers are
> English; only the resource *values* are translated. The default culture being the fallback is a
> product choice, not a code-language choice.

## The model

- A resource has a **key** (e.g. `MaxLength`) and one **value per culture**.
- `.resx` files are wired by filename: `Strings.resx` is the default (Spanish); `Strings.en.resx`
  overrides for English. A key missing from `Strings.en.resx` falls back to `Strings.resx`, so **the
  default file must be complete**.
- The MSBuild tooling generates `Strings.Designer.cs` — a strongly-typed `static` accessor per key.
  Always read strings via `Strings.MyKey`, never `ResourceManager.GetString("MyKey")`.
- `.resx` files are compiled into the assembly as `EmbeddedResource`; a cached `ResourceManager` reads
  them. The thread's `CurrentUICulture` selects the language.

## Where resources live

Each project that owns user-facing text has its **own** resource set under `Resources/`. They are
independent assemblies → independent `ResourceManager`s. **Pick the set for the layer that consumes
the text** — cross-cutting validation-error templates live in `Common.Shared/Resources/` (see the
**common-layer** skill); a project higher up the stack that introduces its own user-facing text (e.g.
`Api` chrome) gets its own `Resources/` set. A key added to one project's resource set is **not**
visible from another's.

## Adding a localized string

1. Add the key to **both** culture files of the right project: `Strings.resx` (Spanish, default) and
   `Strings.en.resx`.
2. For parameterized text, use ordered placeholders `{0}`, `{1}` and **keep the order identical across
   both culture files** — `string.Format` fills by position, so a reordered translation fills the
   wrong slots. Document the placeholders in the `<comment>` (e.g. `{0}=FieldName, {1}=max length`).
3. Use it via the generated accessor: `Strings.MyKey`. The `.csproj` already declares the
   `EmbeddedResource` / `DependentUpon` / `Compile` plumbing for `Common.Shared`'s resource set — copy
   that block if a new project needs its own set.

## Validation errors: the `InvalidData` pattern

Fallible parsing returns `Result<T, InvalidData>` (see the **railway-oriented-programming** and
**common-layer** skills). `InvalidData` stores **structured data, not a sentence**:

```csharp
return new InvalidData
{
    FieldName  = nameof(LessonTitle),               // localized field label (a key)
    InnerValue = value,
    Constraint = new InvalidDataConstraint.CanNotBeEmptyOrNull()  // machine-readable reason (DU case)
};
```

- `Constraint` is a discriminated union (`abstract record InvalidDataConstraint` + `sealed record`
  cases: `MinLength`, `MaxLength`, `FormatInvalid`, `CanNotBeEmptyOrNull`, …, defined in
  `Common.Shared/InvalidData.cs`). Code can branch on it.
- The display sentence is built **lazily at the edge** by `InvalidData.ClientMessage()`, which
  `switch`es on `Constraint` and `string.Format`s the matching `Strings.*` template against the
  current culture.
- **Never** store a pre-built message string on `InvalidData`, and **never** call `.ToString()` on a
  constraint for display. `ClientMessage()` is the single chokepoint where an error becomes user text
  — call it from `Api`, not from `Parse()` or `Services.Core`.

---

## See also

- **csharp-standards** — Result pattern, functional-first, English-in-code
- **common-layer** — `Common.Shared`'s `InvalidData`/`Strings` resource set
- **common-layer** — value types and where validation lives
