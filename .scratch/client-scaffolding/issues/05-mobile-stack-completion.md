# 05: Mobile stack completion — remaining ADR-mandated libraries wired

**What to build:** Every library decided for `apps/mobile` (ADR-0017 through ADR-0025) beyond what ticket 04 already needed is installed and demonstrated with at least one minimal, real usage, mirroring ticket 03's role for web.

**Blocked by:** 04 (mobile app skeleton)

**Status:** ready-for-agent

- [ ] Reanimated + `react-native-svg` (ADR-0024) are wired with at least one animated element (e.g. a placeholder progress ring), proving the animation/graphics pipeline works before the first real gamification screen is built.
- [ ] Zustand is available and demonstrated for client/UI state; TanStack Query is available and demonstrated for server state (ADR-0017), mirroring web's ticket-03 usage.
- [ ] React Hook Form + Zod (ADR-0018) are available and demonstrated with at least one form, even if placeholder, mirroring web.
- [ ] A Jest + React Native Testing Library setup exists for mobile unit/component tests, and covers at least the pieces built in this ticket and ticket 04.
- [ ] No new user-facing screen beyond what ticket 04 already delivered — this ticket rounds out the decided stack against the existing app.
