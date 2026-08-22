using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Milese.Data.Db;

namespace Milese.Tests.Integration.Databases;

/// <summary>
/// In-memory SQLite database, isolated per test.
/// <para>
/// An in-memory SQLite database only exists while a connection is open against it. Since
/// <c>*DataAccess</c> classes create a new context per operation through
/// <see cref="IDbContextFactory{TContext}"/>, a single connection is kept open for the whole
/// test and reused by every context created from it, so they all share the same in-memory
/// database.
/// </para>
/// </summary>
public sealed class SqliteTestDatabase : ITestDatabase
{
    private readonly SqliteConnection connection = new("DataSource=:memory:");
    private IDbContextFactory<MileseDbContext>? factory;

    public IDbContextFactory<MileseDbContext> ContextFactory =>
        factory ?? throw new System.InvalidOperationException(
            "The database has not been initialized. Call InitializeAsync first.");

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        // Connection must stay open — SQLite in-memory databases are dropped when the last connection closes.
        await connection.OpenAsync(cancellationToken);

        var options = new DbContextOptionsBuilder<MileseDbContext>()
            .UseSqlite(connection)
            .UseSnakeCaseNamingConvention()
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .ConfigureMilese()
            .Options;

        factory = new TestDbContextFactory(options);

        await using (var context = await factory.CreateDbContextAsync(cancellationToken))
        {
            await context.Database.EnsureCreatedAsync(cancellationToken);
        }
    }

    public async ValueTask DisposeAsync() => await connection.DisposeAsync();
}
