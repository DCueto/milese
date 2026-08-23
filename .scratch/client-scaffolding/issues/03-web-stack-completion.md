# 03: Web stack completion — remaining ADR-mandated libraries wired

**What to build:** Every library decided for `apps/web` (ADR-0017 through ADR-0030) beyond what ticket 02 already needed is installed and demonstrated with at least one minimal, real usage — proven wired, not just present in `package.json` — so the first real feature that needs any of them doesn't also have to prove the plumbing works.

**Blocked by:** 02 (web app skeleton)

**Status:** ready-for-agent

- [ ] At least one element on the web app uses Motion (ADR-0028) for animation.
- [ ] The web app's UI text (app shell, not Lesson content) renders in Spanish or English via next-intl (ADR-0030), consistent with Content Language support (ADR-0011).
- [ ] Zustand is available and demonstrated for client/UI state; TanStack Query is available and demonstrated for server state (ADR-0017) — real usage can be minimal (e.g. wrapping the ticket-02 API call in a query), since no larger feature exists yet to drive it.
- [ ] React Hook Form + Zod (ADR-0018) are available and demonstrated with at least one form, even if it's a placeholder with no real user-facing purpose yet, with types inferred via `z.infer`.
- [ ] A Vitest setup exists for web unit/component tests, and covers at least the pieces built in this ticket and ticket 02 (e.g. the sign-in screen's rendering).
- [ ] No new user-facing screen beyond what ticket 02 already delivered — this ticket rounds out the decided stack against the existing app, it doesn't add new features.
