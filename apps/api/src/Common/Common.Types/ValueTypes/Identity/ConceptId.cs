using System;
using System.Threading.Tasks;
using Milese.Common.Shared;

namespace Milese.Common.Types.ValueTypes.Identity;

public readonly record struct ConceptId(int Value) : IIdValueType<ConceptId>
{
    public static string FieldName => nameof(ConceptId);

    public static Result<ConceptId, InvalidData> Parse(int value) =>
        ValueTypeParser.StrictlyPositive<ConceptId>(value, FieldName);

    public static Task<Result<ConceptId, InvalidData>> ParseAsync(int value, Func<ConceptId, Task<bool>> exists) =>
        ValueTypeParser.ParseIdAsync(value, FieldName, exists);
}
