# Milese — Project Brief

Micro-learning app for software engineering theory and CS fundamentals — 10-15 minute articles for dead time (commute, public transport), covering the concepts a self-taught dev learns *around* the code, not the syntax itself. Inspired by Brilliant's explanatory depth, without the puzzle-only format and price tag.

This document captures the shared understanding reached during the initial grilling/domain-modeling session, before any implementation began. It is the source of truth for *why* each decision was made, not just *what* was decided.

---

## 1. Scope & Audience

**Decision:** Build for a single user (the founder) first. Architect with a realistic path to a real multi-user product later — auth, content moderation, caching for concurrent traffic, broader content, onboarding, profiles, settings, and a commercial/payment plan are all real future requirements, but none of them are built now.

**Why it matters:** This framing anchors every other decision below — it's why the MVP favors "correct data model, minimal UI polish" over "broad feature set, thin data model."

---

## 2. Content Model

**Decision:** Two distinct content surfaces:
- **Curated lessons** — AI-generated in batch, human-reviewed/curated, then published as **immutable, versioned** content (a correction is a new version, not an edit-in-place).
- **AI tutor** — a live, conversational surface for ad-hoc Q&A, separate from the curated library.

These have different cost, latency, and quality-control profiles and are treated as separate systems.

---

## 3. MVP Syllabus

**Decision:** The syllabus is scaffolded from the **Foundations Track** (9 subjects) and **Engineering Craft** (cross-cutting layer) tracks already defined in the founder's personal learning roadmap (`Backend-AI-Engineering-Roadmap/03 - Foundations Track.md` and `04 - Engineering Craft.md`). The full backlog is visible/scaffolded from day one; content is authored in the roadmap's own priority order, starting with **3.1 Data Structures & Algorithms**.

### Foundations Track (source of truth for ordering)

| # | Subject | Priority | Phase |
|---|---------|----------|-------|
| 3.1 | Data Structures & Algorithms | 🔴 | 0–2 |
| 3.2 | Programming (SICP) | 🟢 | 4 |
| 3.3 | Computer Architecture | 🟡 | 4 |
| 3.4 | Operating Systems & Concurrency | 🔴 | 1–2 |
| 3.5 | Computer Networking & HTTP | 🔴 | 1–2 |
| 3.6 | Databases | 🔴 | 2 |
| 3.7 | Mathematics for CS | 🟡 early · 🟢 later | 0–1 → 4 |
| 3.8 | Languages & Compilers | 🟢 | 4 |
| 3.9 | Distributed Systems & System Design | 🟡→🔴 | 3 |

3.1's own "Learn:" breakdown (the first Concepts to author): Big-O · arrays, linked lists · hash tables · stacks & queues · trees & BSTs · heaps · graphs + BFS/DFS · sorting · recursion · intro dynamic programming.

**Engineering Craft** (04) is explicitly *not* a phase — a parallel, always-on shelf: Pragmatic Programmer → A Philosophy of Software Design → Unit Testing (Khorikov) → Refactoring → Design Patterns → Dependency Injection → SOLID → Domain-Driven Design, plus OWASP Top 10 as an always-on security thread.

---

## 4. Code Examples & Language Scope

**Decision:** The Lesson schema and UI support **multi-language code tabs from day one** (C#, Rust, Go, TypeScript, Python). MVP **content is authored in C# only** — the one language the founder can actually review/curate with confidence. Other language tabs show "coming soon" until there's real capacity (human or automated) to verify them.

**Why:** Curating code in languages you can't fully vet (Rust, Go) risks silently shipping wrong "lessons" — worse than not having them yet.

---

## 5. Content Hierarchy

**Decision:** `Track → Subject → Concept → Lesson(s)`.

- A **Concept** maps 1:1 to one roadmap "Learn:" bullet (e.g., "hash tables," "graphs + BFS/DFS").
- Each Concept has **one or more ordered Lessons**, sized to fit the 10-15 minute cap — decided per-concept at authoring time, not forced to a fixed count. A concept like "Big-O" might be one lesson; "trees & BSTs" might need two or three.
- Mirrors Brilliant's Track → Subject → Lesson shape, with the Concept layer inserted to match how the roadmap itself is already structured.

---

## 6. Progression Model

**Decision:** **Gated/sequential by default** (Duolingo/Brilliant-style — complete lesson N to unlock N+1), with a **settings toggle to switch into free-browse mode** (Exercism-style — unlock everything, read in any order).

**Data model implication:** Progress is both a lock-state machine (`locked → in_progress → completed` per lesson) *and* a mode flag that bypasses the gate check without deleting the underlying order. Progress itself is modeled as an **append-only "lesson completed" event log**, not a mutable field — this is also what makes offline sync (§8) conflict-free.

---

## 7. AI Tutor

**Decision:** **RAG-grounded** — retrieval is scoped primarily to the current lesson + its concept/subject neighborhood, with the model instructed to stay consistent with that material. Falls back gracefully to general knowledge for questions outside what's been authored yet, clearly framed as "not yet covered in a lesson."

**Why:** Keeps tutor answers consistent with curated terminology/depth, and doubles as real practice for the founder's AI-engineering specialization (RAG, tool calls, and eventually evals — per the roadmap's own emphasis on measuring with evals).

---

## 8. Content Pipeline ("the harness")

**Decision:** **Content-as-code.**

1. Lessons are generated (via Claude Code) and authored as **Markdown/MDX files with frontmatter** (concept, order, language, "done when," etc.) inside the monorepo, e.g. `apps/content/...`.
2. The founder reviews/edits the file directly — normal git diff/PR review *is* the curation step.
3. **A git commit is the immutable version** — no separate content-versioning system needed.
4. On merge to `main`, a **CI-triggered sync script** (C#) reads changed files, parses frontmatter + body, and writes rows into PostgreSQL via EF Core — a plain, ordinary file-read → parse → DB-write operation, nothing exotic.
5. The same publish step generates **embeddings** for each lesson (for tutor RAG retrieval) and stores them alongside the row (pgvector).
6. **The runtime API only ever reads from the database** — never touches the filesystem at request time. This also satisfies offline-sync's need to query "everything changed since timestamp X," which loose files can't answer.

---

## 9. Repository Structure

**Decision:** **One monorepo** — `apps/api` (C#/.NET), `apps/web` (Next.js), `apps/mobile` (Expo), `apps/content` (Markdown lesson files + sync tooling). A shared package holds TypeScript types generated from the C# API's OpenAPI spec, consumed by both frontends.

**Why:** The founder is the sole developer; full-stack changes (new Concept → new API endpoint → new mobile/web screen) are common, and a single Claude Code session/worktree needs to see all of it at once.

---

## 10. Backend Stack

**Decision:** **C#/.NET**, using **Microsoft Agent Framework** for the AI tutor and content-generation orchestration.

**Why:** Directly matches the founder's actual specialization target (C# + Functional Programming, AI engineering via Microsoft Agent Framework/Azure AI Foundry). Building this app *is* deliberate practice toward the roadmap, not a side project competing with it — and it keeps one language across day job, this project, and the specialization goal.

---

## 11. Frontend Stack

**Decision:**
- **Mobile:** React Native (Expo) + TypeScript.
- **Web:** Next.js/React + TypeScript.
- **Two separate codebases** — no Solito/Tamagui/NativeWind cross-platform-UI monorepo layer. Shared only via generated TypeScript API types (OpenAPI codegen).

**Why:** The product needs animated, gamified UI (progress rings, streaks, unlock animations — Brilliant/Duolingo-style) on both platforms, and this is the founder's first React Native app. Stacking "learn RN" + "learn a cross-platform UI abstraction" simultaneously was judged too much new surface area at once. Native modules (camera, biometrics, native animation/gesture libraries) remain available via Expo **development builds** (EAS Build) — this doesn't require Expo Go or block native-level work. Migrating to a shared-UI monorepo later (Solito/Tamagui) remains a well-trodden, incremental path if duplication becomes painful.

---

## 12. Offline Support

**Decision:** **Core to the MVP, not deferred.** Lessons a user has opened (or explicitly downloaded) are cached on-device; reading and progress tracking work fully offline. Progress events queue locally and sync to the server once back online.

**Why:** The entire premise of the app is reading during commute/subway dead time, where connectivity is often zero. Because published lesson content is immutable (§2) and progress is an append-only event log (§6), offline sync requires **no conflict resolution** — cached content never goes stale under you, and offline writes are just events waiting to be appended, never overwrites.

---

## 13. Authentication

**Decision:** **Real, minimal auth from day one** — a single OAuth provider (e.g., Google/Microsoft sign-in) via ASP.NET Identity or a lightweight JWT scheme. No password reset flows, no email verification, no subscription tiers yet — just a real `UserId` as a first-class concept from the start.

**Why:** Offline progress events (§12) need to be attributed to someone, and the long-term goal is a real multi-user product. Retrofitting `UserId` onto an already-populated schema and an offline sync protocol later is a real migration; one OAuth screen now is cheap.

---

## 14. Backend Architecture — Pragmatic Functional Split

**Decision:** Immutable domain core, mutable persistence at the boundary only — mirrored directly from an existing reference codebase (`iplan-nexus-core`), applying the pattern Scott Wlaschin's *Domain Modeling Made Functional* teaches (already on the founder's Engineering Craft reading list).

Concretely:

- **Value Types** (`Common.Types/ValueTypes`) — `readonly record struct` types (e.g. `TaskId`, `PowerKw`) implementing a shared interface (`IIdValueType<T>`, `INumericValueType<T>`), each exposing a static **smart-constructor** `Parse(...)` that returns `Result<T, InvalidData>`. Invalid states (e.g. a negative `PowerKw`) can never be constructed.
- **Domain entities** (`Common.Types/Entities`) — immutable `...Bo` (Business Object) records, EF-agnostic.
- **Persistence entities** (`Data.Db`) — mutable, EF Core change-tracked `...Db` classes, confined to the persistence layer.
- **Boundary mapping** (`Data.DbAccess`) — explicit static `...Mappers` classes doing one-directional `ToBo(db) ↔ ToDb(bo)` conversion. A `Db` type never leaks past this layer.
- **Services** (`Services.Core`) — business logic operating *only* on immutable `Bo`s and `Result<T, InvalidData>`, composing data-access calls, never touching a `DbContext` directly.

**Why:** Gets real FP practice where it matters (domain logic, the stuff actually reasoned about) without fighting EF Core's fundamentally mutable change-tracker on every query — the "pure core, impure shell" pattern, proven in a codebase the founder already works in.

---

## 15. Database

**Decision:** **PostgreSQL**, with **pgvector** for tutor embeddings.

**Why:** Free/open-source and portable across any host (self-managed, Supabase, Railway, Neon, any cloud) — consistent with keeping infra choice open (§16). Avoids SQL Server licensing costs outside of Azure's managed offering. EF Core supports Postgres well via Npgsql, so the persistence pattern in §14 is unaffected. pgvector avoids needing a separate vector database service for RAG retrieval.

---

## 16. Hosting / Infra

**Decision:** **Intentionally left open**, decided later on cost grounds — with one exception: **Azure AI Foundry is a hard dependency**, since it's required by the Microsoft Agent Framework piece (§10) specifically. General app hosting (API, database, web/mobile builds) is not locked to Azure and will be chosen once real usage/cost tradeoffs are clearer.

---

## Open Items (deferred, not forgotten)

These were named as real future requirements but explicitly out of scope for the MVP:
- Full multi-user account management (password reset, email verification, subscription tiers, payments)
- Content moderation
- Caching/scaling for concurrent traffic
- Additional code-example languages beyond C# (Rust, Go, TypeScript, Python) — schema/UI ready, content not authored
- Interactive puzzles / gamified exercises (Duolingo/Brilliant-style) — noted as a long-term direction, not an MVP requirement
- Shared cross-platform UI layer (Solito/Tamagui) for mobile+web — deferred until RN fundamentals are solid and/or duplication becomes painful
- Final hosting/infra provider selection

---

*Generated from a grilling/domain-modeling session. Next step: derive the concrete domain model (entities, relationships, invariants) from this brief.*
