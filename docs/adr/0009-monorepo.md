# Backend, web, mobile, and content pipeline live in one monorepo

`apps/api` (C#), `apps/web` (Next.js), `apps/mobile` (Expo), and `apps/content` (Lesson Markdown + sync tooling) all live in a single repository. We considered separate repos per component for cleaner independent deploy/versioning, but rejected it for now: the founder is the sole developer, full-stack changes (new Concept → new API endpoint → new mobile/web screen) are the common case, and a single Claude Code session/worktree needs visibility across all of it to make those changes coherently.

**Consequences:** splitting into separate repos later (e.g. once there's a team or an independent deploy cadence per component) is a mechanical extraction, not a redesign — this decision isn't meant to be permanent.
