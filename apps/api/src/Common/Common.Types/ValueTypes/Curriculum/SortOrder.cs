using Milese.Common.Shared;

namespace Milese.Common.Types.ValueTypes.Curriculum;

public sealed class SortOrder : IValueType<SortOrder, int>
{
    public int Value { get; init; }

    public static Result<SortOrder, InvalidData> Parse(int value) =>
        ValueTypeParser.StrictlyPositive<SortOrder>(value, nameof(SortOrder));
}
