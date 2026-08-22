using Milese.Common.Shared;

namespace Milese.Common.Types.ValueTypes.Curriculum;

public readonly record struct SubjectTitle(string Value) : IStringValueType<SubjectTitle>
{
    public static int MaxLength => 100;

    public static Result<SubjectTitle, InvalidData> Parse(string? value) =>
        ValueTypeParser.StringNotEmptyAndMaxLength<SubjectTitle>(value, nameof(SubjectTitle));
}
