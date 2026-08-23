# @milese/api-types

TypeScript types generated from `apps/api`'s OpenAPI spec via `openapi-typescript` (types only — no client or hooks, per [ADR-0029](../../docs/adr/0029-web-api-client-openapi-typescript.md)). Consumed by `apps/web` (`openapi-fetch`) and `apps/mobile` (a thin `fetch` wrapper).

## Regenerating

Regenerate whenever `apps/api`'s OpenAPI contract changes. `src/generated.ts` is committed generated code — never hand-edit it.

1. Start the API in Development (exposes `/openapi/v1.json`):
   ```bash
   cd apps/api
   dotnet run --project src/Aspire/Aspire.AppHost
   ```
2. From this package, regenerate and typecheck:
   ```bash
   pnpm --filter @milese/api-types generate
   pnpm --filter @milese/api-types typecheck
   ```
