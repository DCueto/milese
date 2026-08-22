---
name: milese-domain
description: Primer on Milese's curriculum/progress domain. Trigger on any task touching Track, Subject, Concept, Lesson, Learner, progress, gating, the Tutor, or Content Language — not only when those words appear literally (e.g. also "unlock the next chapter", "mark this as read", "translate a lesson").
---

# Milese domain primer

Before writing code in this area, read:
- **[CONTEXT.md](../../../CONTEXT.md)** — canonical terms. If you're about to use a word not defined there (or use a defined word loosely), stop and sharpen it first — see `record-decision` if it turns into a real decision, or just edit `CONTEXT.md` directly if it's a straightforward clarification.
- **[PROJECT-BRIEF.md](../../../PROJECT-BRIEF.md)** — why the model looks the way it does.

## Load-bearing facts, so you don't have to re-derive them

- **Gating is Concept-level, not Lesson-level** ([ADR-0002](../../../docs/adr/0002-concept-level-gating.md)). Lessons inside an Unlocked Concept have no ordering enforcement between them.
- **Lesson Completion is append-only and survives content edits** ([ADR-0001](../../../docs/adr/0001-append-only-progress-log.md), [ADR-0003](../../../docs/adr/0003-completion-survives-edits.md)). Never model "completed" as a mutable flag; it's the presence of a `Lesson Completion` row for that `(Learner, Lesson)` pair. Never invalidate one because the Lesson's text changed.
- **A Lesson has one edition per Content Language** (Spanish + English, both authored — [ADR-0011](../../../docs/adr/0011-content-language-spanish-and-english-at-mvp.md)), and separately, up to one variant per Code Language (only C# authored — [ADR-0006](../../../docs/adr/0006-multi-language-schema-csharp-only-content.md)). These two axes are orthogonal — don't conflate them, and don't assume every combination has content.
- **The Tutor is RAG-grounded** in the Learner's current Lesson + its Concept/Subject neighborhood, with graceful fallback to general knowledge — it's not a free-floating assistant.
- Anything server-authoritative about progress must work under **offline-first**: the mobile app queues `Lesson Completion` events locally and syncs later. Don't design an endpoint that assumes the Learner is always online.
