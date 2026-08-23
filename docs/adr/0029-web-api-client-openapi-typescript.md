# Web API client: openapi-typescript + openapi-fetch, not a full generated client

The shared TypeScript-types package (PROJECT-BRIEF §9) is generated with `openapi-typescript` (types only, from the API's OpenAPI spec), and `apps/web` calls the API through `openapi-fetch` — a thin typed wrapper around native `fetch` that uses those generated types for fully-typed requests/responses. TanStack Query hooks (ADR-0017) are hand-written around it. We considered Orval, which generates a full client with ready-to-use TanStack Query hooks directly from the spec.

**Why:** `openapi-typescript` generates exactly what PROJECT-BRIEF §9 describes — types, nothing more — keeping the shared package a types-only artifact rather than a generated-client dependency both apps would need to keep in lockstep with a codegen pipeline. Orval's hook generation saves boilerplate but adds a heavier code-generation pipeline (its own config/templates) and more indirection between the OpenAPI spec and what a component actually calls.

**Consequences:** this also settles `apps/web`'s HTTP client — it's `openapi-fetch` (fetch-based), not Axios, matching the fetch-based approach `apps/mobile` uses (its own thin wrapper, no OpenAPI codegen involved there since the same shared types package covers both).
