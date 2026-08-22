using System;
using System.Threading.Tasks;
using Milese.Common.Shared;

namespace Milese.Common.Types.ValueTypes.Identity;

public readonly record struct SubjectId(int Value) : IIdValueType<SubjectId>
{
    public static string FieldName => nameof(SubjectId);

    public static Result<SubjectId, InvalidData> Parse(int value) =>
        ValueTypeParser.StrictlyPositive<SubjectId>(value, FieldName);

    public static Task<Result<SubjectId, InvalidData>> ParseAsync(int value, Func<SubjectId, Task<bool>> exists) =>
        ValueTypeParser.ParseIdAsync(value, FieldName, exists);
}
