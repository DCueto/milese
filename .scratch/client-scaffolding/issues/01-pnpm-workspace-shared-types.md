# 01: pnpm workspace + shared generated-types package

**What to build:** A pnpm workspace rooted at the repo root that will host `apps/web`, `apps/mobile`, and a new shared package holding TypeScript types generated from `apps/api`'s OpenAPI spec. A developer can regenerate those types on demand from a real, running `apps/api` and get a package that type-checks — this is the foundation both client apps will consume so a backend contract change becomes a type error in either client, not a runtime surprise.

**Blocked by:** None (can start immediately)

**Status:** ready-for-agent

- [ ] `pnpm-workspace.yaml` (or equivalent) exists at the repo root and recognizes `apps/web`, `apps/mobile`, and the new shared types package as workspace members (ADR-0021).
- [ ] A new package (e.g. `packages/api-types`) generates its contents via `openapi-typescript` against `apps/api`'s OpenAPI spec (types only — no client/hooks generated, per ADR-0029).
- [ ] A documented or scripted regeneration command exists (e.g. an npm script) that, run against a live `apps/api` (Development environment, `MapOpenApi()` already exposed), produces types that compile with no errors.
- [ ] The generated output is committed as regular generated code — not hand-edited, and the regeneration script/instructions make that expectation clear.
- [ ] README or package-level doc note (kept short) explains when to regenerate (backend contract change) and how.
