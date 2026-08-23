# 03: Entra token validation, global auth-by-default, JIT provisioning

**What to build:** The core feature. A real Entra-issued bearer token is validated directly against Entra's own signing keys; every `apps/api` endpoint (including the existing `LessonsController`, with no change to the controller itself) requires a valid authenticated caller by default; the first successful authenticated request for a given `EntraObjectId` creates a `UserDb` row automatically, and every subsequent one reuses it and refreshes `Email`/`DisplayName` from the latest token's claims.

**Blocked by:** 01 (HTTP test seam), 02 (Identity data layer)

**Status:** ready-for-agent

- [ ] `Microsoft.Identity.Web`'s `AddMicrosoftIdentityWebApi()` validates an incoming bearer token's signature, issuer, and audience against Entra External ID's metadata — no token re-issuance, no signing key owned by this codebase (ADR-0014).
- [ ] A fallback authorization policy (`RequireAuthenticatedUser()`) is applied at the ASP.NET Core level so every endpoint requires authentication by default, with no `[Authorize]` attribute added to `LessonsController` or any other existing controller.
- [ ] On a successful authenticated request, the caller's `EntraObjectId` claim (`oid`) is resolved to a `UserId`: if no matching `UserDb` row exists, one is created with `Email`/`DisplayName` from the token's claims; if one exists, `Email`/`DisplayName` are refreshed from the current token. This lookup-or-create logic lives in `Services.Core`, calling the `Data.DbAccess` primitives from ticket 02.
- [ ] A test at the ticket-01 seam (with `TestAuthHandler`) proves two authenticated requests simulating the same `EntraObjectId` resolve to the same `UserId`/`UserDb` row (JIT-provisioning idempotency).
- [ ] A test at the ticket-01 seam proves a request to `LessonsController` (or any existing protected endpoint) succeeds when authenticated and is rejected when not.
- [ ] A small, separate test class — with no `TestAuthHandler` substituted, exercising the real Entra authentication scheme — proves a request with no token, an expired token, or a wrong-audience token against a protected endpoint is rejected with `401 Unauthorized`. This is what actually verifies the real wiring, since every other test in this spec bypasses real token validation on purpose.
