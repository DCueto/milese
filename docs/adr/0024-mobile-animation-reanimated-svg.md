# Mobile animation/graphics use Reanimated + react-native-svg, not the built-in Animated API

`apps/mobile` implements progress rings, streaks, and unlock animations (PROJECT-BRIEF §11) with React Native Reanimated for animation and `react-native-svg` for the vector shapes (a progress ring has no native RN primitive), rather than RN's built-in `Animated` API.

**Why:** Reanimated runs animations on the UI thread, so they stay smooth even when the JS thread is busy (e.g. a network call completing) — the built-in `Animated` API runs on the JS thread by default and can visibly stutter under that same load. `react-native-svg` is needed either way to actually draw a ring shape, so choosing `Animated` over Reanimated wouldn't have avoided a second dependency, only picked a worse animation engine. Both are first-class supported in Expo's managed workflow.
