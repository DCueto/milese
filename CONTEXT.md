# Milese

Milese is a micro-learning app that teaches software engineering and computer science theory through short, curated Lessons organized into a gated curriculum, with an AI Tutor available for on-demand help.

## Language

**Track**
The top level of the curriculum — a broad area of study (e.g. "Foundations"). Contains an ordered sequence of Subjects.
_Avoid_: Course, Path

**Subject**
A single area of study within a Track (e.g. "Data Structures & Algorithms"). Contains an ordered sequence of Concepts.
_Avoid_: Topic, Module

**Concept**
A single idea within a Subject that a Learner needs to understand (e.g. "Hash Tables"). Contains one or more Lessons. **The unit of progression gating**: a Concept is Locked or Unlocked as a whole — its Lessons don't gate each other individually.
_Avoid_: Topic (ambiguous with Subject), Chapter

**Lesson**
The atomic reading unit — a single short article, readable in 10-15 minutes, covering part of a Concept. A Concept may need several Lessons to cover fully; there's no fixed count per Concept. A Lesson's identity is stable across edits: correcting its text later does not affect a Learner's existing Lesson Completion for it. A Lesson exists as one edition per Content Language (Spanish, English); Lesson Completion is tracked against the Lesson itself, not a specific edition — reading either edition completes the same Lesson.
_Avoid_: Article, Micro-lesson, Page

**Code Language**
The programming language a Lesson's code examples are written in (C#, Rust, Go, TypeScript, or Python). A Lesson may exist in more than one Code Language; at present only C# is authored, others are placeholders.
_Avoid_: Language (ambiguous — see Content Language)

**Content Language**
The spoken/human language a Lesson's prose (and the app's UI) is written in — currently Spanish and English, both authored for MVP. Orthogonal to Code Language: a Lesson's Spanish edition and English edition both show the same Code Language tabs (only C# at present), since code examples don't change with the reader's spoken language.
_Avoid_: Language (ambiguous — see Code Language), Locale (implies region/formatting, not just language)

**Published**
The state of a Lesson once it's visible to Learners. An unpublished Lesson is still being authored/curated and isn't part of the live curriculum yet.
_Avoid_: Live, Released

**Learner**
A person using the app to read Lessons and track their progress. The domain-facing term — prefer this over "User" (an auth/identity-layer term) anywhere the app's learning behavior is being described.
_Avoid_: User (reserve for auth/API-boundary contexts only), Student

**Lesson Completion**
The record that a Learner has finished reading a specific Lesson. Append-only — once created it is never edited or removed, and it remains valid even if that Lesson's text is corrected later. A Concept counts as complete for a Learner once every one of its Lessons has a Lesson Completion.
_Avoid_: Progress (see below), Read status

**Progress**
The Learner-facing, derived view of how far a Learner has gotten — which Concepts are complete, in progress, Locked, or Unlocked. Always computed from Lesson Completions; never stored as its own mutable fact.
_Avoid_: Status

**Learning Mode**
A per-Learner setting: **Gated** (default — Concepts unlock in curriculum order as prior Concepts are completed) or **Free Browse** (every Concept is Unlocked regardless of order). A Learner switches this in settings; it doesn't change the underlying curriculum order, only whether it's enforced.
_Avoid_: Progression Mode, Unlock Mode

**Locked / Unlocked**
The state of a Concept for a given Learner: Locked means its Lessons cannot yet produce a Lesson Completion (only possible under Gated Learning Mode, when the preceding Concept isn't yet complete); Unlocked means they can. Always computed, never stored.

**Tutor**
The conversational AI a Learner can ask questions of on demand, grounded in the Learner's current Lesson and its surrounding Concept/Subject — distinct from the curated Lesson library itself.
_Avoid_: Chat, Assistant, Bot
