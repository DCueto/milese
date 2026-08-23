# 02: Identity data layer — UserId, UserDb, UserBo, UserMapper, migration

**What to build:** The `Identity` module's data layer, mirroring the existing `Curriculum` module's shape: a `UserId` Value Type, an immutable `UserBo` and mutable `UserDb` entity, a `UserMapper`, and the migration that creates the table in Postgres. No auth wiring and no HTTP surface yet — this is the schema `Data.Db`/`Data.DbAccess` foundation everything else in this spec builds on.

**Blocked by:** None (can start immediately)

**Status:** ready-for-agent

- [ ] `UserId` is a `readonly record struct` wrapping a strictly-positive `int` (sequential, not a Guid — ADR-0015), implementing the same `IIdValueType<T>` pattern as `TrackId`/`SubjectId`/`ConceptId`/`LessonId`, with `Parse()`/`ParseAsync()`.
- [ ] `Common.Types.Tests` covers `UserId.Parse()` for valid and invalid (non-positive) input, mirroring the existing `TrackId` test pattern.
- [ ] `UserDb` holds exactly `Id` (PK, `UserId`), `EntraObjectId` (Guid, unique index — the sign-in lookup key), `Email`, `DisplayName` — nothing learning-specific (no `LearningMode`, no progress fields).
- [ ] `UserBo` is the immutable domain record counterpart, and `UserMapper` maps `UserDb <-> UserBo` following the existing Db/Bo mapping convention.
- [ ] `Data.DbAccess` exposes `FindByEntraObjectIdAsync`/`CreateAsync`-shaped primitives only — no business logic (lookup-or-create belongs in a later ticket's `Services.Core` work).
- [ ] An EF Core migration creates the `UserDb` table; applying it against a real Postgres database succeeds.
- [ ] A DB integration test (via the existing `DatabaseIntegrationTest`/`TestDatabaseFactory` infrastructure) proves a `UserDb` row can be created and read back by `EntraObjectId` — no HTTP, no auth involved.
