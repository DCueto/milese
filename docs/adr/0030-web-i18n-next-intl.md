# Web i18n uses next-intl

`apps/web` renders Spanish/English UI and Lesson content (Content Language, ADR-0011) using next-intl, rather than react-i18next.

**Why:** next-intl is built specifically for Next.js's App Router — server components, static rendering, and routing are all supported natively. react-i18next predates the App Router's server/client component split and needs extra wiring to avoid shipping the full translation catalog to the client.
