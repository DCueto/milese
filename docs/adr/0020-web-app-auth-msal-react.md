# Web app auth uses @azure/msal-browser + @azure/msal-react, not NextAuth.js

`apps/web` authenticates against Entra External ID using `@azure/msal-browser` + `@azure/msal-react` directly, rather than NextAuth.js/Auth.js (which has a built-in Microsoft Entra provider and is the more commonly reached-for Next.js auth library).

**Why:** NextAuth.js is fundamentally a server-side, cookie/session-backed library — adopting it would reintroduce the cookie-based backend-for-frontend pattern ADR-0016 already rejected in favor of both clients carrying an Entra-issued Bearer token directly. MSAL React is also the only option that actually supports federated Google sign-in: Entra's newer native-authentication SDK explicitly restricts federated identity providers to browser-delegated authentication (ADR-0014), which is exactly what MSAL React implements.

**Consequences:** the web app manages its own token acquisition/refresh via MSAL's `acquireTokenSilent`, attached as an `Authorization: Bearer` header on TanStack Query's fetcher — no server-side session of any kind on the Next.js side.
