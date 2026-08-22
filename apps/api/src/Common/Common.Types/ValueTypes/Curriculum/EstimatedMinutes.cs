using Milese.Common.Shared;

namespace Milese.Common.Types.ValueTypes.Curriculum;

public sealed class EstimatedMinutes : IValueType<EstimatedMinutes, int>
{
    public int Value { get; init; }

    public static Result<EstimatedMinutes, InvalidData> Parse(int value) =>
        value is >= 1 and <= 15
            ? new EstimatedMinutes { Value = value }
            : new InvalidData
            {
                FieldName = nameof(EstimatedMinutes),
                InnerValue = value,
                Constraint = new InvalidDataConstraint.GenericError(),
            };
}
