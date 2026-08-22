# PostgreSQL is the database; general hosting is deliberately left open

The database is PostgreSQL with pgvector for Tutor retrieval embeddings — chosen for being free/portable across any host (self-managed, Supabase, Railway, Neon, any cloud), unlike SQL Server whose licensing gets expensive outside Azure's managed offering. General app hosting (API, database, web/mobile builds) is intentionally *not* decided yet and will be chosen later on cost grounds. The one exception is Azure AI Foundry, which is a hard dependency of the Microsoft Agent Framework piece powering the Tutor specifically — that one service is Azure-locked regardless of where everything else ends up.

**Consequences:** don't assume Azure-specific services (e.g. Azure SQL, Azure Blob Storage) anywhere outside the Tutor/Agent Framework integration itself — keep the rest of the stack host-agnostic.
