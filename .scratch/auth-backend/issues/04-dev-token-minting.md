# 04: Development-only token minting

**What to build:** A local developer running the Aspire AppHost can mint a locally-signed token for a fixed (or caller-specified) dev `UserId` from a Development-only endpoint, and use it to successfully call a protected endpoint by hand — no real Entra/Google sign-in round trip needed. Outside the Development environment, neither the minting endpoint nor the authentication scheme that would accept its tokens exist at all, so a misconfigured environment variable can't accidentally expose it.

**Blocked by:** 03 (Entra token validation, global auth-by-default, JIT provisioning)

**Status:** ready-for-agent

- [ ] A Development-only endpoint, mapped only inside `if (app.Environment.IsDevelopment())` (the existing pattern already used for Scalar/OpenAPI), issues a locally-signed token for a fixed or caller-specified dev `UserId`.
- [ ] A second, Development-only authentication scheme is registered alongside the real Entra scheme, validating only tokens signed by this endpoint — the real Entra scheme is untouched.
- [ ] In Development, a token from this endpoint is accepted by a protected endpoint (verified by a `WebApplicationFactory` test configured for the Development environment).
- [ ] Outside Development, a `WebApplicationFactory` test configured for a non-Development environment proves the minting endpoint returns `404`/doesn't exist, and a token shaped like its output is rejected (the accepting scheme isn't registered at all).
