# A Lesson Completion survives later edits to that Lesson's text

Correcting a published Lesson (a typo, a clarified example) does not invalidate a Learner's existing Lesson Completion for it, and does not re-lock a Concept that was already unlocked because of it. We considered tying completion to a specific published version so a substantive correction could force a re-read, but rejected it: distinguishing "trivial fix" from "the content was actually wrong" automatically isn't reliable, and an already-unlocked Concept silently re-locking itself is confusing UX for something the Learner didn't do. Since content is curated by hand before publish (see content-as-code pipeline), factual errors reaching a published Lesson should be rare enough to handle manually if they ever happen.

**Consequences:** `Lesson Completion` references a Lesson's stable identity, not a specific published version of its text.
