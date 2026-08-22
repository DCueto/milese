using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Milese.Tests.Integration.Databases.SchemaIsolation;

/// <summary>
/// Model cache key factory that folds the test's schema suffix into the key.
/// <para>
/// EF Core caches the model per context type. Without this, the first test's model (with its
/// schema) would be reused by every other test, breaking isolation. Including the suffix in the
/// key means each suffix produces and caches a distinct model.
/// </para>
/// </summary>
public sealed class SchemaAwareModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        var suffix = context
            .GetService<IDbContextOptions>()
            .FindExtension<SchemaSuffixExtension>()
            ?.Suffix;

        return (context.GetType(), suffix, designTime);
    }
}
