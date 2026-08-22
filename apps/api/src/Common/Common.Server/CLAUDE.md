# Milese.Common.Server

Server-side shared utilities — EF Core helpers that cannot run outside a server process, so they're kept out of `Common.Shared`.

**Allowed dependencies:** `Milese.Common.Shared` only. Never referenced by `Data.Db` or anything above `Data.DbAccess` — `Api` and `Services.Core` never see EF Core directly (see the repo's `CLAUDE.md` → Architecture rules).

> Skill: **common-layer**

---

## Contents

| Type | Purpose |
|---|---|
| `ValueTypeConverter<TValueType, T>` | EF Core `ValueConverter` — persists any `IValueType<TSelf, T>` as its underlying `T` |
| `ValueTypeModelConfigurationExtensions` | `RegisterValueTypeConverters(...)` — scans an assembly and wires every value type's converter (+ `[MaxLength]`/precision) by convention |
| `ValueTypeMemberTranslator(Plugin)` | Translates `valueType.Value` access inside a LINQ query to SQL against the underlying column, instead of failing or evaluating client-side |
| `ValueTypeTranslationDbContextOptionsExtension` | `UseValueTypeTranslation()` — registers the translator plugin on `DbContextOptionsBuilder` |
| `QueryableFilterExtensions` | `WhereIfValue` / `WhereIfNotEmpty` — skip a filter when the caller didn't provide one |
| `QueryablePaginationExtensions` | `ToPagedResultAsync` / `ToPagedResultMappedAsync` — turns a query + `PagedQuery<TFilter, TField>` into `PagedItems<TBo>` |

## Rules

- Keep lean: if something belongs to a specific layer (`Data.DbAccess`, `Services.Core`), put it there instead.
- No ASP.NET Core dependencies here — those belong in `Api`.
- A helper only belongs here if every server-side layer could plausibly need it without creating an inappropriate dependency.
