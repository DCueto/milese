using Milese.Common.Shared;

namespace Milese.Common.Types.ValueTypes.Curriculum;

public readonly record struct LessonTitle(string Value) : IStringValueType<LessonTitle>
{
    public static int MaxLength => 200;

    public static Result<LessonTitle, InvalidData> Parse(string? value) =>
        ValueTypeParser.StringNotEmptyAndMaxLength<LessonTitle>(value, nameof(LessonTitle));
}
