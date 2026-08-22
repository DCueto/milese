using System;
using System.Threading.Tasks;
using Milese.Common.Shared;

namespace Milese.Common.Types.ValueTypes.Identity;

public sealed class LessonId : IIdValueType<LessonId>
{
    public int Value { get; init; }

    public static string FieldName => nameof(LessonId);

    public static Result<LessonId, InvalidData> Parse(int value) =>
        ValueTypeParser.StrictlyPositive<LessonId>(value, FieldName);

    public static Task<Result<LessonId, InvalidData>> ParseAsync(int value, Func<LessonId, Task<bool>> exists) =>
        ValueTypeParser.ParseIdAsync(value, FieldName, exists);
}
