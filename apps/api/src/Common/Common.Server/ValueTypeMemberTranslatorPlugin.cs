using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Milese.Common.Server;

/// <summary>
/// Member translator plugin that exposes <see cref="ValueTypeMemberTranslator"/> to
/// EF Core, so access to a value type's <c>.Value</c> translates to SQL server-side.
/// </summary>
public sealed class ValueTypeMemberTranslatorPlugin : IMemberTranslatorPlugin
{
    public ValueTypeMemberTranslatorPlugin(
        ISqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource)
    {
        Translators =
        [
            new ValueTypeMemberTranslator(sqlExpressionFactory, typeMappingSource),
        ];
    }

    /// <inheritdoc />
    public IEnumerable<IMemberTranslator> Translators { get; }
}
