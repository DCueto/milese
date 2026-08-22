using Microsoft.EntityFrameworkCore;
using Milese.Data.Db;

namespace Milese.Tests.Integration.Databases;

/// <summary>
/// <see cref="IDbContextFactory{TContext}"/> implementation for tests that reuses a single
/// <see cref="DbContextOptions{TContext}"/> instance.
/// <para>
/// Every project under test consumes the context through an
/// <see cref="IDbContextFactory{TContext}"/> (never an injected <see cref="DbContext"/>), so
/// each call to <see cref="CreateDbContext"/> creates a new context. Sharing the same options
/// means every context points at the same database: for in-memory SQLite the options wrap the
/// same open connection, and for PostgreSQL the same isolated test schema.
/// </para>
/// </summary>
public sealed class TestDbContextFactory : IDbContextFactory<MileseDbContext>
{
    private readonly DbContextOptions<MileseDbContext> options;

    public TestDbContextFactory(DbContextOptions<MileseDbContext> options) => this.options = options;

    public MileseDbContext CreateDbContext() => new(options);
}
