using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Milese.Data.Db;
using Milese.Tests.Integration.Configuration;
using Milese.Tests.Integration.Databases;
using TUnit.Core.Interfaces;

namespace Milese.Tests.Integration;

public abstract class DatabaseIntegrationTest : IAsyncInitializer, IAsyncDisposable
{
    private ITestDatabase? database;

    protected IDbContextFactory<MileseDbContext> DbContextFactory =>
        (database ?? throw new InvalidOperationException(
            "The database has not been initialized yet.")).ContextFactory;

    public virtual async Task InitializeAsync()
    {
        var settings = IntegrationTestSettingsLoader.Load();
        database = await TestDatabaseFactory.CreateAsync(settings, CancellationToken.None);
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (database is not null)
            await database.DisposeAsync();

        GC.SuppressFinalize(this);
    }
}
