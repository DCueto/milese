using System;
using System.Threading;
using System.Threading.Tasks;
using Milese.Tests.Integration.Configuration;

namespace Milese.Tests.Integration.Databases;

public static class TestDatabaseFactory
{
    public static async Task<ITestDatabase> CreateAsync(
        IntegrationTestSettings settings,
        CancellationToken cancellationToken)
    {
        ITestDatabase database = settings.Provider switch
        {
            TestDatabaseProvider.Postgres => new PostgresTestDatabase(RequireConnectionString(settings)),
            TestDatabaseProvider.Sqlite => new SqliteTestDatabase(),
            _ => throw new NotSupportedException("TestDatabaseProvider type not defined"),
        };

        await database.InitializeAsync(cancellationToken);
        return database;
    }

    private static string RequireConnectionString(IntegrationTestSettings settings) =>
        !string.IsNullOrWhiteSpace(settings.ConnectionString)
            ? settings.ConnectionString
            : throw new InvalidOperationException(
                "The Postgres provider requires 'IntegrationTests:ConnectionString' in appsettings.json "
                + "or the MILESE_INTEGRATION_TESTS_CONNECTIONSTRING environment variable.");
}
