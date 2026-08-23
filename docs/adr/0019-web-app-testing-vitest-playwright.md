# Web app testing uses Vitest (unit/component) and Playwright (e2e)

`apps/web` uses Vitest for unit and component tests and Playwright for end-to-end tests, rather than Jest and/or Cypress.

**Why:** Vitest's ESM-native, Jest-compatible API fits a modern Next.js/TypeScript toolchain without Jest's extra transform configuration. Playwright covers real cross-browser e2e with built-in auto-waiting; per ADR-0016/0020, its e2e suite authenticates by injecting a pre-acquired token into browser storage before navigating, never driving the real Entra/Google login UI.
