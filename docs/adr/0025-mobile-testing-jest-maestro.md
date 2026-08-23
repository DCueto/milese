# Mobile testing uses Jest + RN Testing Library, e2e via Maestro (not Detox)

`apps/mobile` uses Jest (Expo/RN's default test runner) with React Native Testing Library for unit/component tests. For e2e, it uses Maestro rather than Detox.

**Why:** Playwright (used for `apps/web`'s e2e, ADR-0019) doesn't drive native mobile apps, so mobile needs its own e2e tool regardless. Maestro's YAML-defined flows don't require configuring and maintaining a native build specifically for test runs the way Detox does — less moving parts for a solo project, at the cost of the deeper native-level control (real gesture simulation, etc.) Detox offers.

**Consequences:** mobile e2e authenticates the same way web's Playwright suite does (ADR-0019) — a pre-acquired token is seeded into `expo-secure-store` before the flow runs, never driving the real Entra/Google login screen.
