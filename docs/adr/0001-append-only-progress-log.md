# Progress is an append-only Lesson Completion log, not mutable state

We track a Learner's progress as an append-only stream of `Lesson Completion` events rather than a mutable "completed: bool" flag per Lesson. This was chosen specifically because offline reading is core to the MVP (the app's whole premise is subway/commute dead time, where connectivity is often zero): an event that's only ever appended, never overwritten, has no conflict to resolve when a device that completed Lessons offline reconnects and syncs — it just uploads events it hasn't sent yet. A mutable flag would require last-write-wins or merge logic the moment two devices (or an offline queue vs. a fresh server read) disagree.

**Consequences:** "Is this Concept complete?" is always a derived query (do Lesson Completions exist for all its Lessons?), never a stored fact — don't add a `completed` column anywhere in the progress model.
