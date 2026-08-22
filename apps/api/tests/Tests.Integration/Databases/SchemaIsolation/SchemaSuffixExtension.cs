using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Milese.Tests.Integration.Databases.SchemaIsolation;

/// <summary>
/// <c>DbContext</c> options extension carrying the random schema suffix applied to a test
/// against the real provider. Read by both <see cref="SchemaSuffixModelCustomizer"/> and
/// <see cref="SchemaAwareModelCacheKeyFactory"/>.
/// </summary>
public sealed class SchemaSuffixExtension : IDbContextOptionsExtension
{
    private DbContextOptionsExtensionInfo? info;

    public SchemaSuffixExtension(string suffix) => Suffix = suffix;

    /// <summary>
    /// Suffix appended to each schema name (e.g. <c>_it_ab12cd34</c>).
    /// </summary>
    public string Suffix { get; }

    public DbContextOptionsExtensionInfo Info => info ??= new ExtensionInfo(this);

    public void ApplyServices(IServiceCollection services)
    {
        // Suffix is read from options; no services to register.
    }

    public void Validate(IDbContextOptions options) { }

    private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        public ExtensionInfo(SchemaSuffixExtension extension)
            : base(extension) { }

        private new SchemaSuffixExtension Extension => (SchemaSuffixExtension)base.Extension;

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => $"SchemaSuffix={Extension.Suffix} ";

        // Must not affect the internal service provider (shared across tests); model differentiation is done by SchemaAwareModelCacheKeyFactory.
        public override int GetServiceProviderHashCode() => 0;

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) =>
            other is ExtensionInfo;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) =>
            debugInfo["Milese:SchemaSuffix"] = Extension.Suffix;
    }
}
