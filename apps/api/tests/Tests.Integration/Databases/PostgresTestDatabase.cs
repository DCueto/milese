using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Milese.Data.Db;
using Milese.Tests.Integration.Databases.SchemaIsolation;

namespace Milese.Tests.Integration.Databases;

/// <summary>
/// Database against the real PostgreSQL provider, isolated per test via a random schema suffix.
/// <para>
/// To avoid recreating the physical database on every test, all tests share it but each one uses
/// its own set of schemas (e.g. <c>curriculum</c> becomes <c>curriculum_it_ab12cd34</c>). Tables
/// are created with <see cref="IRelationalDatabaseCreator.CreateTablesAsync"/> and the schemas are
/// dropped when the test finishes.
/// </para>
/// </summary>
public sealed class PostgresTestDatabase : ITestDatabase
{
    private readonly string connectionString;
    private readonly string schemaSuffix;
    private IDbContextFactory<MileseDbContext>? factory;

    public PostgresTestDatabase(string connectionString)
    {
        this.connectionString = connectionString;
        schemaSuffix = "_it_" + Guid.NewGuid().ToString("N")[..8];
    }

    public IDbContextFactory<MileseDbContext> ContextFactory =>
        factory ?? throw new InvalidOperationException(
            "The database has not been initialized. Call InitializeAsync first.");

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var builder = new DbContextOptionsBuilder<MileseDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .ReplaceService<IModelCacheKeyFactory, SchemaAwareModelCacheKeyFactory>()
            .ReplaceService<IModelCustomizer, SchemaSuffixModelCustomizer>();

        ((IDbContextOptionsBuilderInfrastructure)builder)
            .AddOrUpdateExtension(new SchemaSuffixExtension(schemaSuffix));

        factory = new TestDbContextFactory(builder.Options);

        await using (var context = await factory.CreateDbContextAsync(cancellationToken))
        {
            var creator = context.GetService<IRelationalDatabaseCreator>();

            // The physical database is shared: ensure it once, then create only this test's (suffixed) tables.
            if (!await creator.ExistsAsync(cancellationToken))
                await creator.CreateAsync(cancellationToken);

            await creator.CreateTablesAsync(cancellationToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (factory is null)
            return;

        await using var context = await factory.CreateDbContextAsync(CancellationToken.None);

        var schemas = context
            .Model.GetEntityTypes()
            .Select(entityType => entityType.GetSchema())
            .Where(schema => !string.IsNullOrEmpty(schema))
            .Distinct()
            .ToList();

        foreach (var schema in schemas)
        {
            // Schema names are generated internally (random suffix + module name) — not from user input.
#pragma warning disable EF1002, CA2100, S2077 // Raw SQL over an internally-controlled identifier
            await context.Database.ExecuteSqlRawAsync(
                $"DROP SCHEMA IF EXISTS \"{schema}\" CASCADE",
                CancellationToken.None);
#pragma warning restore EF1002, CA2100, S2077 // Raw SQL over an internally-controlled identifier
        }
    }
}
