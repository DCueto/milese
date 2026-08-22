using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Milese.Tests.Integration.Databases.SchemaIsolation;

/// <summary>
/// Model customizer that, after the context's real model is built, rewrites every entity's
/// schema by appending the test's random suffix (e.g. <c>curriculum</c> becomes
/// <c>curriculum_it_ab12cd34</c>), so each test gets its own set of tables inside the same
/// physical database.
/// </summary>
public sealed class SchemaSuffixModelCustomizer : RelationalModelCustomizer
{
    public SchemaSuffixModelCustomizer(ModelCustomizerDependencies dependencies)
        : base(dependencies) { }

    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);

        var suffix = context
            .GetService<IDbContextOptions>()
            .FindExtension<SchemaSuffixExtension>()
            ?.Suffix;

        if (string.IsNullOrEmpty(suffix))
            return;

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var schema = entityType.GetSchema();
            if (!string.IsNullOrEmpty(schema))
                entityType.SetSchema(schema + suffix);
        }
    }
}
