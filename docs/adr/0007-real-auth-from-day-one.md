# Real (minimal) auth exists from day one, despite the MVP having one Learner

Every table and sync payload is scoped to a real `UserId` via a single OAuth provider from the start, even though the MVP is architected for a single Learner (the founder). We considered skipping auth entirely for the MVP (an implicit local user, no `UserId` anywhere), but rejected it: the long-term goal is a real multi-user product, and retrofitting `UserId` onto an already-populated schema *and* an offline sync protocol later is a genuine migration, not a small addition — a single OAuth sign-in screen now is cheap by comparison.

**Consequences:** don't build any "local-only, no login" code path — the app always operates as an authenticated Learner, even when only one exists.
