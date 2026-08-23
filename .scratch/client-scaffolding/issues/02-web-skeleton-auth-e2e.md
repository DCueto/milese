# 02: Web app skeleton — sign in, call a real endpoint, e2e-proven

**What to build:** A Learner can open `apps/web`, see a sign-in screen, sign in via Entra's hosted login page (federated to Google), land on one authenticated screen that calls a real protected `apps/api` endpoint (e.g. `GET /api/lessons`) and renders the response, and have that session survive a page reload. This is the smallest possible proof that auth, the typed API client, and rendering all actually work together on web — not a real Curriculum-browsing screen.

**Blocked by:** 01 (pnpm workspace + shared types package), and the `auth-backend` spec's tickets that stand up Entra token validation and the Development-only token-minting/dev-auth seam (tracked separately in `.scratch/auth-backend/spec.md`, not yet published as tickets) — the e2e test in this ticket needs a way to acquire a valid token without a real interactive Entra/Google sign-in.

**Status:** ready-for-agent

- [ ] `apps/web` is a Next.js (App Router) + TypeScript app, styled with Tailwind CSS (ADR-0026), consuming the shared types package from ticket 01.
- [ ] `@azure/msal-browser` + `@azure/msal-react` (ADR-0020) are wired with `MsalProvider` at the app root; unauthenticated visitors see a sign-in screen.
- [ ] Signing in goes through Entra's hosted, browser-delegated login page (federated to Google) — no native/embedded login UI.
- [ ] The session (MSAL's token cache) persists across a page reload — no forced re-sign-in on refresh.
- [ ] `openapi-fetch` (ADR-0029) is configured to acquire the access token silently via MSAL and attach it as a Bearer header automatically — no component manually manages the token.
- [ ] At least one authenticated screen calls a real protected `apps/api` endpoint (e.g. `GET /api/lessons`) and renders something from the actual response.
- [ ] Whatever interactive component the sign-in screen needs (at minimum) is built on shadcn/ui over Radix primitives (ADR-0027), not hand-rolled.
- [ ] One Playwright e2e smoke test: launches the app, injects a pre-acquired token into browser storage (never drives the real Google/Entra login UI), hits the authenticated screen, and asserts the real API response rendered — run against the real, Aspire-orchestrated `apps/api` (ADR-0019).
