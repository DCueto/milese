using Milese.Common.Shared;

namespace Milese.Common.Types.ValueTypes.Curriculum;

public sealed class LessonTitle : IStringValueType<LessonTitle>
{
    public string Value { get; init; } = default!;

    public static int MaxLength => 200;

    public static Result<LessonTitle, InvalidData> Parse(string? value) =>
        ValueTypeParser.StringNotEmptyAndMaxLength<LessonTitle>(value, nameof(LessonTitle));
}
