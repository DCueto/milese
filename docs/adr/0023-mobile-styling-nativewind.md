# Mobile styling uses NativeWind, not React Native's built-in StyleSheet

`apps/mobile` styles components with NativeWind (Tailwind utility classes for React Native) rather than RN's built-in `StyleSheet` API. This reverses the original decision, made before Tailwind CSS was chosen for `apps/web` (ADR-0026).

**Why:** the original reasoning (ADR-0008's "don't stack learning RN with learning a styling abstraction at once") assumed `StyleSheet` was the only thing being learned. Now that Tailwind is already committed on web, `StyleSheet` would be the *second* styling mental model, not the first — reusing the same utility-class vocabulary already being learned for `apps/web` is the lower-total-surface-area choice, not a bigger one. ADR-0008 itself only rejected a *shared cross-platform component/UI layer* (Tamagui/Solito) — NativeWind doesn't share components between web and mobile, it only shares the styling syntax, so it was never actually what ADR-0008 ruled out; the earlier ADR-0023 applied that reasoning by analogy more broadly than ADR-0008 required.

**Consequences:** NativeWind isn't 1:1 identical to web Tailwind — React Native's layout is flexbox-only (no CSS grid), there's no real cascade, and some utilities/pseudo-classes don't apply on native. It's a lower learning cost than before, not a zero one.

`apps/web` and `apps/mobile` keep separate Tailwind configs rather than sharing one design-tokens package — simpler to start, at the cost of relying on manual discipline (not tooling) to keep colors/spacing/font scale from drifting apart between platforms. Extracting a shared config later, if drift becomes a real problem, is a mechanical change, not a redesign.

