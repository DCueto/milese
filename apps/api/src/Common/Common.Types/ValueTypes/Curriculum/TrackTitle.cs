using Milese.Common.Shared;

namespace Milese.Common.Types.ValueTypes.Curriculum;

public sealed class TrackTitle : IStringValueType<TrackTitle>
{
    public string Value { get; init; } = default!;

    public static int MaxLength => 100;

    public static Result<TrackTitle, InvalidData> Parse(string? value) =>
        ValueTypeParser.StringNotEmptyAndMaxLength<TrackTitle>(value, nameof(TrackTitle));
}
