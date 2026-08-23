# Both clients build forms with React Hook Form + Zod

`apps/web` and `apps/mobile` forms are built with React Hook Form for form state/rendering and Zod for validation schemas, with the form's TypeScript types inferred from the same Zod schema rather than declared separately. Both libraries are RN-compatible: React Hook Form's `Controller` component wraps React Native's `TextInput` and similar controlled components, since RN has no native `<input>`.

**Why:** React Hook Form avoids re-rendering on every keystroke (uncontrolled-by-default), which matters for the gamified/animated UI PROJECT-BRIEF §11 calls for on both platforms. Deriving types from the Zod schema (`z.infer<...>`) means the validation rule and the TypeScript type can't drift apart — a schema change automatically changes the type everywhere it's used, rather than needing the same shape maintained twice. Using the same two libraries on both clients also means one mental model and one set of validation schemas, not two.
