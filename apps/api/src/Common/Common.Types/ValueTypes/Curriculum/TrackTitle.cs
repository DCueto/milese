using Milese.Common.Shared;

namespace Milese.Common.Types.ValueTypes.Curriculum;

public readonly record struct TrackTitle(string Value) : IStringValueType<TrackTitle>
{
    public static int MaxLength => 100;

    public static Result<TrackTitle, InvalidData> Parse(string? value) =>
        ValueTypeParser.StringNotEmptyAndMaxLength<TrackTitle>(value, nameof(TrackTitle));
}
