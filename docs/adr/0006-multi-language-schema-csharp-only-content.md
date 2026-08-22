# Lesson schema supports 5 Code Languages; only C# is authored for MVP

The Lesson/Code Language data model supports C#, Rust, Go, TypeScript, and Python from day one, but MVP content generation only produces C#. We considered curating all 5 languages immediately, but rejected it: the founder can only reliably review C# code, and shipping AI-generated Rust/Go without real verification risks silently-wrong "lessons" — worse than not having them yet. Other Code Language tabs are expected to render empty/"coming soon" until there's real capacity to curate them.

**Consequences:** don't assume every Lesson has content in every Code Language — querying for a specific language's text must handle "not yet authored," not just "not found."
