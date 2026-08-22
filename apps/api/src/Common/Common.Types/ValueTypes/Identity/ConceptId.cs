using System;
using System.Threading.Tasks;
using Milese.Common.Shared;

namespace Milese.Common.Types.ValueTypes.Identity;

public sealed class ConceptId : IIdValueType<ConceptId>
{
    public int Value { get; init; }

    public static string FieldName => nameof(ConceptId);

    public static Result<ConceptId, InvalidData> Parse(int value) =>
        ValueTypeParser.StrictlyPositive<ConceptId>(value, FieldName);

    public static Task<Result<ConceptId, InvalidData>> ParseAsync(int value, Func<ConceptId, Task<bool>> exists) =>
        ValueTypeParser.ParseIdAsync(value, FieldName, exists);
}
