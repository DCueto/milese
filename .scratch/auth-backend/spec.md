Status: ready-for-agent

# Auth backend: Entra External ID identity core for apps/api

## Problem Statement

`apps/api` has no concept of who is calling it. Every endpoint, including the existing Curriculum endpoints, is open to anyone with network access, and there is no `UserId` to attribute anything to. Future per-Learner data — Lesson Completions, Progress, Tutor conversations — cannot be built until the API can answer "who is this request from," and retrofitting that onto an already-populated schema later is a real migration, not a small change.

## Solution

Stand up a minimal identity core backed by Microsoft Entra External ID (federated to Google): a Learner signs in through Entra's hosted, browser-delegated login page, the API validates the Entra-issued access token directly (no re-issuance), and resolves it to a `UserId` — a single, sequential-int-keyed `UserDb` row created automatically the first time each person signs in. Every API endpoint requires a valid token by default, including the existing `LessonsController`. Local development and automated tests both get a way to act as an authenticated caller without a real Entra sign-in.

## User Stories

1. As a Learner, I want to sign in with my Google account through Entra, so that the API knows who I am without me creating a separate Milese-specific password.
2. As a Learner signing in for the first time, I want a `UserDb` row created for me automatically, so that I don't have to go through a separate registration step before using the app.
3. As a returning Learner, I want signing in again to reuse my existing `UserDb` row rather than create a duplicate one.
4. As a returning Learner, I want my stored `Email`/`DisplayName` refreshed from my latest token's claims on each sign-in, so that a name or email change on my Google/Entra account eventually shows up in the app.
5. As the API, I want to resolve an incoming Entra token's `oid` claim to an internal `UserId` via a stored `EntraObjectId`, so that every other table's foreign keys reference a stable internal id rather than being welded to Entra's identifier shape.
6. As the API, I want `UserId` to be a sequential int (not a Guid), consistent with every other entity's id (`TrackId`, `SubjectId`, `ConceptId`, `LessonId`), even though this means a Learner's id is technically enumerable — accepted knowingly per ADR-0015, not accidentally.
7. As the API, I want to validate an incoming access token's signature, issuer, and audience directly against Entra's own signing keys (via `Microsoft.Identity.Web`), so that no token-signing key or rotation logic is owned or hand-rolled by this codebase.
8. As the API, I want every endpoint to require a valid, authenticated caller by default (a global fallback authorization policy), so that a new endpoint is secure unless someone deliberately opts it out, not the other way around.
9. As the API, I want the existing `LessonsController` to now require authentication, consistent with every other endpoint, even though Curriculum data itself isn't per-Learner.
10. As a caller with no token, I want a request to a protected endpoint to be rejected with `401 Unauthorized`.
11. As a caller with an expired, malformed, or wrong-audience token, I want the request rejected with `401 Unauthorized`, the same as having no token at all.
12. As a local developer running the Aspire AppHost, I want a Development-only endpoint that mints a locally-signed token for a fixed dev `UserId`, so that I can curl protected endpoints by hand without a real Entra/Google sign-in round trip every time.
13. As the API running outside the Development environment, I want the token-minting endpoint to not exist at all (not just be logically blocked), so that a misconfigured environment variable can't accidentally expose it.
14. As an automated test author, I want to simulate an authenticated caller with a specific `UserId` without touching real JWT signing/validation, so that tests of business logic stay decoupled from Entra configuration.
15. As an automated test author, I want a small, dedicated test that verifies an unauthenticated request is actually rejected, so that the real `[Authorize]`/token-validation mechanism itself has coverage, not just the business logic behind it.
16. As an automated test author, I want a test verifying that signing in twice with the same `EntraObjectId` reuses the same `UserDb` row rather than creating a second one.
17. As a future engineer, I want the `UserDb` entity to hold only identity fields (`EntraObjectId`, `Email`, `DisplayName`) and nothing learning-specific (no `LearningMode`, no progress fields), so that this spec doesn't silently expand into the Progression feature's scope.
18. As a future engineer adding a second role (e.g. Admin, Curator), I want that to be addable as a field on the existing `UserDb` rather than requiring a schema split, per the single-entity decision recorded when this was grilled.

## Implementation Decisions

- **New module, mirroring the existing `Curriculum` module's shape**: a `UserId` Value Type (`Common.Types/ValueTypes/Identity`, sequential int, strictly positive — same pattern as `TrackId`), a `UserBo` immutable record and `UserDb` mutable EF Core entity (both in an `Identity` folder alongside `Curriculum`, per the existing `Data.Db`/`Data.DbAccess`/`Services.Core` layout), and a `UserMapper` following the existing `Db <-> Bo` mapping convention.
- **`UserDb` fields**: `Id` (PK, `UserId`), `EntraObjectId` (Guid, unique index — the lookup key on sign-in), `Email`, `DisplayName`. Nothing else; `LearningMode`/progress fields are explicitly out of scope for this spec (see Out of Scope).
- **Identity provider**: Microsoft Entra External ID, browser-delegated authentication, federated to Google (ADR-0014). `Microsoft.Identity.Web`'s `AddMicrosoftIdentityWebApi()` validates the incoming bearer token directly against Entra's metadata (signature, issuer, audience) — no token re-issuance, no JWT signing key owned by this codebase.
- **Global auth-by-default**: a fallback authorization policy (`RequireAuthenticatedUser()`) applied at the ASP.NET Core level, rather than `[Authorize]` attributes added per-controller. This is what makes the existing `LessonsController` require auth with no change to the controller itself.
- **JIT provisioning**: on a successful authenticated request, resolve the caller's `EntraObjectId` from the validated token's claims; if no matching `UserDb` row exists, create one (with `Email`/`DisplayName` from the token's claims); if one exists, refresh `Email`/`DisplayName` from the current token. This lookup-or-create logic belongs in `Services.Core` (business logic), not `Data.DbAccess` (which only exposes `FindByEntraObjectIdAsync`/`CreateAsync`-shaped primitives) — per the existing layering rules.
- **Local dev token minting**: a Development-only endpoint (mapped only inside `if (app.Environment.IsDevelopment())`, same pattern already used for Scalar/OpenAPI) that issues a token for a fixed (or caller-specified) dev `UserId`. Since `Microsoft.Identity.Web`'s validation is scoped to Entra's real signing keys, this requires a second, Development-only authentication scheme (a locally-signed token, validated by a matching Development-only scheme) registered alongside the real Entra scheme — the exact multi-scheme wiring is an implementation detail for `/implement` to work out, but the behavior is: in Development, a token from this endpoint is accepted by the API; outside Development, neither the endpoint nor the scheme that would accept its tokens exist.
- **Entra tenant/app-registration provisioning is out of scope for `/implement`**: creating the actual external tenant, registering the app, and configuring Google federation happens in the Entra admin center — an interactive, credentialed, human-only step. This spec covers everything the codebase needs to *validate* tokens once a tenant exists; provisioning the tenant itself is a prerequisite step for a human (a candidate for `/wizard`), not something an AFK agent can do.

## Testing Decisions

- **Primary (and new) seam: `WebApplicationFactory<Program>`-based HTTP tests.** Confirmed with the founder: no test in this codebase currently sends a real HTTP request through the ASP.NET Core pipeline (existing `Api.Rest.Tests` call controller methods directly in C#), but `[Authorize]`/token validation is middleware — unreachable from a direct method call. This spec introduces the one new seam needed: a base test fixture (alongside the existing `DatabaseIntegrationTest`) that boots the app via `WebApplicationFactory<Program>` and issues real `HttpClient` requests.
- **`TestAuthHandler`**: a custom `AuthenticationHandler<AuthenticationSchemeOptions>` substituted into the `WebApplicationFactory`'s test host (via `ConfigureTestServices`) that always succeeds with a fixed/configurable `UserId` claim. Used for the bulk of authenticated-endpoint tests, which should exercise business logic, not real JWT mechanics.
- **A small, separate test class specifically covers the real auth mechanism**: no `TestAuthHandler` substituted, asserting a protected endpoint returns `401` with no token — this is what actually verifies `[Authorize]`/the fallback policy is wired correctly, since every other test bypasses real token validation on purpose.
- **JIT provisioning** is tested at the same `WebApplicationFactory` seam: two authenticated requests with the same simulated `EntraObjectId` should resolve to the same `UserId`/`UserDb` row (asserted via the existing `DbContextFactory`/`DatabaseIntegrationTest` infrastructure, reused rather than replaced).
- **`UserId` Value Type** gets a small `Common.Types.Tests` entry (`Parse()` strictly-positive/invalid cases), mirroring the existing `TrackId` test pattern.
- **Only external behavior is tested**: HTTP status codes, response shape, and database state via the shared `DbContextFactory` — not which internal ASP.NET Core scheme name handled a given request.
- **Prior art**: `DatabaseIntegrationTest`, `ITestDatabase`/`TestDatabaseFactory` (dual Postgres/Sqlite backend) already exist in `Tests.Integration` and should be extended, not duplicated.

## Out of Scope

- `apps/web` and `apps/mobile` client integration (MSAL React, `expo-auth-session`, etc.) — covered by a separate spec (`client-scaffolding`).
- Provisioning the actual Entra External ID tenant and app registration in the Entra admin center — a human, interactive, credentialed step; not buildable by an AFK agent.
- Roles/permissions beyond "is a valid authenticated caller" — `UserDb` stays single-role until a second role is real.
- `LearningMode`, Lesson Completions, Progress, or any other Progression-feature data — `UserDb` is identity-only in this spec.
- Per-resource authorization (e.g. "can this caller access this specific Learner's data") — not yet relevant, since no endpoint currently returns per-Learner data. Revisit once Progression is built.
- Refresh-token handling beyond what `Microsoft.Identity.Web` and the client SDKs manage themselves.

## Further Notes

Backed by ADR-0014 (Entra External ID, browser-delegated, federated to Google), ADR-0015 (`UserId` is a sequential int, trade-off accepted knowingly), and ADR-0016 (Bearer tokens on both clients, no cookie-based BFF — this spec is the API side that both clients' tokens get validated against).

`CONTEXT.md`'s existing `User`/`Learner` glossary entries already anticipated this exact split (`User` for auth/API-boundary contexts, `Learner` for domain-behavior prose) — no glossary changes needed for this spec.
