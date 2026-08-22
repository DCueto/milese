namespace Milese.Tests.Integration.Configuration;

/// <summary>
/// Settings that determine how integration test databases are created.
/// </summary>
/// <param name="Provider">Database provider to use.</param>
/// <param name="ConnectionString">
/// Connection string to the real server. Only used when <see cref="Provider"/> is
/// <see cref="TestDatabaseProvider.Postgres"/>; ignored for in-memory SQLite.
/// </param>
public sealed record IntegrationTestSettings(
    TestDatabaseProvider Provider,
    string? ConnectionString
);
