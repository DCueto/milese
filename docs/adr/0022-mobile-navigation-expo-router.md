# Mobile navigation uses Expo Router, not plain React Navigation

`apps/mobile` uses Expo Router's file-based routing (`app/lessons/[id].tsx`, etc.) rather than manually configuring React Navigation's stack/tab navigators in code. Expo Router is built on React Navigation, not a replacement for it — it removes the manual navigator-tree setup.

**Why:** it's Expo's own current default for new projects, and it's mentally consistent with the file-based routing `apps/web`'s Next.js App Router already uses — one fewer new concept on a platform (React Native) that's already new to the founder.
