using Milese.Common.Shared;

namespace Milese.Common.Types.ValueTypes.Curriculum;

public sealed class SubjectTitle : IStringValueType<SubjectTitle>
{
    public string Value { get; init; } = default!;

    public static int MaxLength => 100;

    public static Result<SubjectTitle, InvalidData> Parse(string? value) =>
        ValueTypeParser.StringNotEmptyAndMaxLength<SubjectTitle>(value, nameof(SubjectTitle));
}
