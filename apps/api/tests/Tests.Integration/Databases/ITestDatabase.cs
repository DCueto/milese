using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Milese.Data.Db;

namespace Milese.Tests.Integration.Databases;

/// <summary>
/// Represents an isolated database for a single integration test. Creates the schema and
/// releases resources on completion.
/// </summary>
public interface ITestDatabase : IAsyncDisposable
{
    /// <summary>
    /// Context factory pointing at the test's isolated database — the same factory injected
    /// into the <c>*DataAccess</c> classes under test.
    /// </summary>
    IDbContextFactory<MileseDbContext> ContextFactory { get; }

    /// <summary>
    /// Creates the database (schema).
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken);
}
