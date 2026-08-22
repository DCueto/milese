---
name: record-decision
description: Write a new ADR, or update CONTEXT.md. Trigger when a decision is hard to reverse, would surprise a future reader, and was a real trade-off between genuine alternatives — or when a term's meaning is ambiguous or being used two different ways. Not for easy-to-reverse or obvious choices.
---

# Recording a decision

## ADR — `docs/adr/NNNN-slug.md`

Only write one when **all three** are true:
1. Hard to reverse (real cost to changing your mind later)
2. Surprising without context (a future reader would wonder "why?")
3. The result of a genuine trade-off (real alternatives existed)

If any is missing, skip it — don't write an ADR for the obvious choice.

**Format** (see existing files in `docs/adr/` for the actual bar — keep new ones just as tight):
```md
# {Short title of the decision}

{1-3 sentences: context, what we decided, why. State the rejected alternative and why it lost — that's the part worth remembering.}

**Consequences:** {optional — only if there's a non-obvious downstream effect a reader should know before "fixing" it.}
```
Number sequentially — check the highest existing number in `docs/adr/` and increment.

## CONTEXT.md

Update it *inline*, the moment a term resolves — don't batch this up. Follow the existing format: term, one-to-two-sentence definition (what it **is**, not what it does), an `_Avoid_` list of terms not to use for the same idea. Only add terms specific to Milese's domain — general programming/architecture vocabulary (`Bo`, `Result`, `Value Type`, ...) belongs in a skill (see `naming-conventions`, `railway-oriented-programming`), not `CONTEXT.md`.

If a new term collides with or sharpens an existing one, call it out explicitly before writing anything down — e.g. "you're using 'language' — do you mean Code Language or Content Language? Those are different axes." Don't silently pick one.
