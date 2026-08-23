# 04: Mobile app skeleton — sign in, call a real endpoint, e2e-proven

**What to build:** A Learner can open `apps/mobile` on Android, see a sign-in screen, sign in via the same Entra/Google browser-delegated flow as web, land on one authenticated screen that calls the same real protected `apps/api` endpoint and renders the response, and have the session survive closing and reopening the app. This proves the same end-to-end stack (auth, typed API client, rendering) works on mobile as it does on web.

**Blocked by:** 01 (pnpm workspace + shared types package), and the same `auth-backend` prerequisites as ticket 02 (Entra token validation + Development-only token-minting/dev-auth seam, tracked separately in `.scratch/auth-backend/spec.md`).

**Status:** ready-for-agent

- [ ] `apps/mobile` is an Expo (managed workflow, Android target) app using Expo Router (ADR-0022), configured with an EAS development-build profile — not Expo Go, since a custom Entra redirect URI needs it.
- [ ] Styled with NativeWind, using its own local Tailwind config, explicitly not shared with `apps/web`'s config (ADR-0023).
- [ ] `expo-auth-session` drives the same browser-delegated Entra flow (federated to Google) used on web — no embedded/native-only login UI.
- [ ] The access token is stored via `expo-secure-store` and survives closing and reopening the app.
- [ ] API calls attach the stored access token as a Bearer header automatically, via a thin `fetch` wrapper mirroring web's `openapi-fetch` pattern, consuming the same shared generated types package from ticket 01.
- [ ] At least one authenticated screen calls the same real protected `apps/api` endpoint (e.g. `GET /api/lessons`) used on web and renders something from the actual response.
- [ ] One Maestro e2e flow: launches the app, signs in via an injected pre-acquired token (never drives the real Google/Entra login UI), hits the authenticated screen, and asserts the real API response rendered — run against the real, Aspire-orchestrated `apps/api` (ADR-0025).
