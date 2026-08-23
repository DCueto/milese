# 01: HTTP test seam — WebApplicationFactory + TestAuthHandler

**What to build:** A new base test fixture that boots `apps/api` through the real ASP.NET Core pipeline (via `WebApplicationFactory<Program>`) and issues genuine HTTP requests, plus a substitutable authentication handler that lets a test act as an authenticated caller with a specific `UserId` without touching real JWT signing/validation. This is the one new seam every later ticket in this spec needs — no test in this codebase currently exercises middleware (`[Authorize]`, token validation), since existing tests call controller/service methods directly in C#.

**Blocked by:** None (can start immediately)

**Status:** ready-for-agent

- [x] A base test fixture (alongside the existing `DatabaseIntegrationTest`) boots the app via `WebApplicationFactory<Program>` and exposes a real `HttpClient` for sending requests through the actual pipeline.
- [x] A custom `AuthenticationHandler<AuthenticationSchemeOptions>` (`TestAuthHandler`) can be substituted into the test host (via `ConfigureTestServices`) that always succeeds and carries a fixed/configurable `UserId` claim.
- [x] A smoke test proves the seam works end-to-end: a request sent through the real pipeline with `TestAuthHandler` substituted reaches an existing controller action and gets a successful response.
- [x] No product code in `apps/api` changes — this ticket is test infrastructure only. (Exception: `Program.cs` gains a `public sealed partial class Program;` marker with zero runtime-behavior change — the standard, unavoidable way to make a top-level-statements entry point resolvable as `WebApplicationFactory<Program>`'s generic argument from a separate test assembly.)
