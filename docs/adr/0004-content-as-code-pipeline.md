# Content is authored as Markdown/MDX in git, synced to Postgres on publish

Lessons are generated and curated as Markdown/MDX files with frontmatter inside the monorepo — a git commit *is* the immutable published version, and reviewing a Lesson is just reviewing a PR diff. We considered writing AI-generated Lessons directly into the database via an admin tool, but rejected it: it would mean building a review UI just to curate AI output, and would lose git's diff/blame/history for free. A CI-triggered script parses published files on merge to `main` and writes rows into PostgreSQL (also generating embeddings for tutor retrieval at the same time); the runtime API only ever reads from the database, never the filesystem.

**Consequences:** "publish" always means "merge to main," never a direct database write; any tooling that wants to change Lesson content must go through a file + PR, not an admin panel.
