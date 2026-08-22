using Milese.Common.Shared;

namespace Milese.Common.Types.ValueTypes.Curriculum;

public readonly record struct SortOrder(int Value) : IValueType<SortOrder, int>
{
    public static Result<SortOrder, InvalidData> Parse(int value) =>
        ValueTypeParser.StrictlyPositive<SortOrder>(value, nameof(SortOrder));
}
