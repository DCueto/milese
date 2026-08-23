# Web app styling uses Tailwind CSS

`apps/web` styles components with Tailwind CSS utility classes rather than CSS Modules or a CSS-in-JS library (vanilla-extract, styled-components).

**Why:** zero runtime cost (compiled at build time, unlike CSS-in-JS), first-class Next.js support, and it's the ecosystem the chosen UI-primitives approach (ADR-0027, shadcn/ui) is written assuming — picking a different styling solution would mean fighting shadcn's own examples/conventions on every component.
