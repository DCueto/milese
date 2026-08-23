using Milese.Common.Shared;

namespace Milese.Common.Types.ValueTypes.Identity;

public readonly record struct DisplayName(string Value) : IStringValueType<DisplayName>
{
    public static int MaxLength => 200;

    public static Result<DisplayName, InvalidData> Parse(string? value) =>
        ValueTypeParser.StringNotEmptyAndMaxLength<DisplayName>(value, nameof(DisplayName));
}
