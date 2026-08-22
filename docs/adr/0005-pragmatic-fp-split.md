# Backend uses a pragmatic FP split: immutable domain core, mutable EF Core only at the persistence boundary

The C# backend mirrors the pattern used in `iplan-nexus-core`: domain Value Types are `readonly record struct`s with smart-constructor `Parse()` methods returning `Result<T, InvalidData>` (invalid states are unrepresentable), domain entities are immutable `Bo` records, and EF Core's mutable change-tracked entities (`Db` classes) are confined entirely to the data-access layer behind explicit `Mapper` classes. We considered full purity (no EF change-tracking at all, hand-written SQL) and rejected it as unnecessary friction; we also considered using EF Core idiomatically throughout and rejected that as abandoning the immutability goal everywhere it matters most (the domain logic actually being reasoned about).

**Consequences:** a `Db`-suffixed type must never be returned from or accepted by a `Services.*` method — only `Bo`s and `Result`s cross that boundary.
