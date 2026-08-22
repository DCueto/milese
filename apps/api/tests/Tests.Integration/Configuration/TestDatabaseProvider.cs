namespace Milese.Tests.Integration.Configuration;

/// <summary>
/// Database backend used by integration tests.
/// </summary>
public enum TestDatabaseProvider
{
    /// <summary>
    /// In-memory SQLite (fast, no server). The default.
    /// </summary>
    Sqlite = 1,

    /// <summary>
    /// The real PostgreSQL provider. Each test gets its own isolated schema.
    /// </summary>
    Postgres,
}
