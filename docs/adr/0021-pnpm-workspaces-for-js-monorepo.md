# JS/TS side of the monorepo uses pnpm workspaces

`apps/web`, `apps/mobile`, and the shared package of TypeScript types generated from the API's OpenAPI spec (PROJECT-BRIEF §9) live under pnpm workspaces, not npm or Yarn workspaces.

**Why:** pnpm's workspace support is native and fast, and its disk-efficient (content-addressable, symlinked) `node_modules` is well suited to a monorepo with multiple apps sharing a package — the shape ADR-0009 already committed this repo to on the JS/TS side. npm workspaces would work too but is less mature for this pattern; Yarn was not preferred elsewhere in the project.
