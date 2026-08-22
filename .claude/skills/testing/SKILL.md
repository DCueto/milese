---
name: testing
description: Writing or changing tests for any apps/api layer. Trigger whenever new production code needs a test, or an existing test needs updating — not only when the user says "test" explicitly.
---

# Testing conventions

Framework: **TUnit** (`[Test]`, runs on Microsoft.Testing.Platform) + **Shouldly** for assertions. Every public method gets a test — this is enforced narratively, not just aspirationally: don't consider a change done until its public surface is covered.

## Structure

- One test project per source project: `apps/api/tests/<Project>.Tests`, folder structure mirroring `src/<Group>/<Project>` with the project name itself stripped (a test for `Services.Core/Tasks/Foo.cs` lives at `Services.Core.Tests/Tasks/FooTests.cs`).
- `Tests/ArchTests` (via `NetArchTest.Rules`) holds architecture fitness tests — cross-cutting rules like "no `*Db` type is referenced outside `Data.DbAccess`." Add one here on the *second* time a layering rule gets violated, not the first (see `CLAUDE.md` → Done means).
- A shared `DatabaseIntegrationTest` base class (once it exists) gives DataAccess/Services tests a real schema-per-test database — don't hand-roll DB setup per test class.

## Rules

- Value Type / pure-logic tests: `Shouldly`'s `ShouldBe`.
- Integration-layer tests (DataAccess, Services.Core, Api): prefer TUnit-native `Assert.That(...).IsEquivalentTo(...)` / strict equality assertions over loose `ShouldBe` — a loose comparison between a `Bo`'s Value Type and a raw primitive can false-pass. Be deliberate about which style you're using and why, don't mix arbitrarily within one test class.
- Local dev and CI run the *same* test bodies — a test that only passes against an in-memory DB but not a real one (or vice versa) is a bug in the test, not something to special-case or skip in one environment.
- Test names describe the scenario and expected outcome (`CreateLessonCompletion_WhenConceptLocked_ReturnsFailure`), not the method under test alone.
