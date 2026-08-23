# Web animation uses Motion (formerly Framer Motion)

`apps/web` implements progress rings, streaks, and unlock animations (PROJECT-BRIEF §11 — the same requirement ADR-0024 solved for mobile with Reanimated) using Motion, rather than plain CSS animations/transitions. Progress rings themselves are inline SVG (native browser support, no extra library needed there, unlike React Native).

**Why:** Motion is the de facto standard for declarative React animation, handles layout animations and gesture-driven interactions well, and pairs naturally with Tailwind/shadcn components (ADR-0026/0027). Orchestrating a multi-step "unlock" sequence in raw CSS keyframes gets unwieldy fast compared to a declarative library built for exactly that.
