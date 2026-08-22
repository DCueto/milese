using Milese.Common.Shared;

namespace Milese.Common.Types.ValueTypes.Curriculum;

public sealed class ConceptTitle : IStringValueType<ConceptTitle>
{
    public string Value { get; init; } = default!;

    public static int MaxLength => 120;

    public static Result<ConceptTitle, InvalidData> Parse(string? value) =>
        ValueTypeParser.StringNotEmptyAndMaxLength<ConceptTitle>(value, nameof(ConceptTitle));
}
