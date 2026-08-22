# Gating unlocks at the Concept level, not the Lesson level

Under Gated Learning Mode, the next **Concept** unlocks once the current one is fully complete — but the Lessons *within* an Unlocked Concept can be read in any order. We considered Lesson-by-Lesson gating (closer to Duolingo's "next exercise" feel) but rejected it: a Concept can span 2-3 Lessons authored at different depths, and forcing strict in-Concept ordering added enforcement complexity without a clear learning benefit, since the real curriculum-progression signal that matters is "have you covered this idea," not "did you read its parts in a specific sequence."

**Consequences:** the only enforcement point for gating is "does the Learner's prior Concept have Lesson Completions for all its Lessons" — nothing checks ordering within an Unlocked Concept.
