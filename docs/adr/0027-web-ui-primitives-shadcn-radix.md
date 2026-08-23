# Web UI primitives: shadcn/ui on Radix, not a pre-styled component kit or from-scratch

`apps/web`'s interactive components (dialogs, dropdowns, tooltips, etc.) are built on Radix primitives via shadcn/ui, rather than building accessibility behavior from scratch or adopting a pre-styled kit (Chakra, MUI, Mantine).

**Why:** Radix solves the genuinely hard parts (focus trapping, keyboard navigation, ARIA attributes) that are easy to get subtly wrong when hand-rolled. shadcn/ui isn't installed as an npm dependency — its CLI copies component source directly into the repo (styled with Tailwind, ADR-0026), so it's fully owned and customizable for the gamified UI PROJECT-BRIEF §11 calls for, rather than fighting a pre-styled kit's default look.

**Consequences:** shadcn components live in the repo as regular source files once added, not as a package.json dependency to bump — updates are pulled in deliberately per-component via its CLI, not automatically.
