# Both clients carry Entra-issued Bearer tokens directly; no cookie-based BFF

Web (Next.js, via MSAL.js's standard in-memory token cache) and mobile (Expo, via `expo-auth-session` + `expo-secure-store`) both authenticate against Entra External ID directly and attach the resulting access token as `Authorization: Bearer <token>` on every call to `apps/api`. We considered a Next.js backend-for-frontend that would handle the OAuth callback server-side and set an httpOnly cookie instead, giving the web token better XSS protection.

**Why:** rejected the BFF for now — it adds a proxy layer and CORS/cookie-domain handling between two already-separate origins (ADR-0008: web and API are independent codebases/deployments), and it would make the two clients' auth mechanisms asymmetric for no benefit mobile could share, since a cookie-based session doesn't translate to a mobile app anyway. Bearer-everywhere is also Microsoft's own documented pattern for a SPA calling a separate protected Web API.

**Consequences:** the web app's access token lives in browser memory, not an httpOnly cookie — accepted as an MVP trade-off given the single-founder-user scale; revisit if/when the web app has a real multi-user XSS attack surface worth hardening against.
