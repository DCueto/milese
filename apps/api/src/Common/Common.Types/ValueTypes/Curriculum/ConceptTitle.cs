using Milese.Common.Shared;

namespace Milese.Common.Types.ValueTypes.Curriculum;

public readonly record struct ConceptTitle(string Value) : IStringValueType<ConceptTitle>
{
    public static int MaxLength => 120;

    public static Result<ConceptTitle, InvalidData> Parse(string? value) =>
        ValueTypeParser.StringNotEmptyAndMaxLength<ConceptTitle>(value, nameof(ConceptTitle));
}
