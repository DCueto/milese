---
name: services-layer
description: Write or change anything in Services.Core (business logic, orchestration, the Tutor). Trigger on any "when a Learner does X, then Y should happen" task — not only when the user names the project explicitly.
---

# Services layer conventions

`Services.Core` holds business logic and orchestration. It composes `Data.DbAccess` calls and other services, works exclusively with `Bo`s and `Result`s, and never sees a `DbContext` or a `*Db` type.

## What belongs here (vs. elsewhere)

- **Business rules** — "a Concept is Unlocked once every Lesson in the prior Concept has a Lesson Completion," "a Lesson Completion can't be created for a Locked Concept" (the actual gating enforcement point, per [ADR-0002](../../../docs/adr/0002-concept-level-gating.md)). Not in `Data.DbAccess` (see `data-layer`), not re-validated here if it's already guaranteed by a `Bo`'s Value Types (see `railway-oriented-programming`).
- **Orchestration** — a single Learner action that touches multiple DataAccess calls (e.g. "create a Lesson Completion, then check whether the parent Concept just became complete, then compute the next Unlocked Concept").
- **The Tutor** — Microsoft Agent Framework orchestration (retrieval scoped to the Learner's current Lesson/Concept/Subject, per [PROJECT-BRIEF.md](../../../PROJECT-BRIEF.md) §7) lives in a dedicated `Services.Core/Tutor` area, not mixed into curriculum services.

## Rules

- A service method's public signature only ever mentions `Bo`s, Value Types, and `Result<TValue, TError>` — never a `*Db` type, never a raw untyped primitive where a Value Type exists.
- No format/range validation here — that's already guaranteed by the `Bo`'s Value Types by the time it reaches a service. If a service is re-checking "is this string non-empty," something upstream is broken — fix the `Bo`'s construction, don't add a defensive check here.
- Prefer composition of small, named methods (`ParseCommonFieldsAsync`, `AddInternalObservationAsync`-style helpers) over one large method doing everything — mirrors the split already used for multi-step writes elsewhere in the layer.
- No `try/catch` for expected failures (see `railway-oriented-programming`) — propagate `Result.Failure` from the DataAccess call that produced it.
