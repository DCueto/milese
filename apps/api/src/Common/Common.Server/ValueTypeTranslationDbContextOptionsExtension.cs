using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Milese.Common.Server;

/// <summary>
/// <see cref="DbContext"/> options extension that registers
/// <see cref="ValueTypeMemberTranslatorPlugin"/> in EF Core's internal service
/// provider. It's agnostic of the relational provider (works with Npgsql or
/// others) because it only adds an <see cref="IMemberTranslatorPlugin"/>.
/// </summary>
public sealed class ValueTypeTranslationDbContextOptionsExtension : IDbContextOptionsExtension
{
    private DbContextOptionsExtensionInfo? info;

    /// <inheritdoc />
    public DbContextOptionsExtensionInfo Info => info ??= new ExtensionInfo(this);

    /// <inheritdoc />
    public void ApplyServices(IServiceCollection services) =>
        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IMemberTranslatorPlugin, ValueTypeMemberTranslatorPlugin>());

    /// <inheritdoc />
    public void Validate(IDbContextOptions options) { }

    private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        public ExtensionInfo(IDbContextOptionsExtension extension)
            : base(extension) { }

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "using ValueTypeTranslation ";

        public override int GetServiceProviderHashCode() => 0;

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) =>
            other is ExtensionInfo;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) =>
            debugInfo["Milese:ValueTypeTranslation"] = "1";
    }
}

/// <summary>
/// Extension methods to enable server-side translation of a value type's
/// <c>.Value</c> access in EF Core queries.
/// </summary>
public static class ValueTypeTranslationDbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Enables SQL translation of a value type's <c>Value</c> property access, so
    /// operations like <c>property.Value.Contains(text)</c> run server-side
    /// (e.g. as a <c>LIKE</c>) instead of client-side.
    /// </summary>
    /// <param name="optionsBuilder">The context options builder.</param>
    /// <returns>The same <paramref name="optionsBuilder"/>, for chaining.</returns>
    public static DbContextOptionsBuilder UseValueTypeTranslation(
        this DbContextOptionsBuilder optionsBuilder)
    {
        var extension =
            optionsBuilder.Options.FindExtension<ValueTypeTranslationDbContextOptionsExtension>()
            ?? new ValueTypeTranslationDbContextOptionsExtension();

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }
}
