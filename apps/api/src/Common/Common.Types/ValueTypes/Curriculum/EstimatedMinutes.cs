using Milese.Common.Shared;

namespace Milese.Common.Types.ValueTypes.Curriculum;

public readonly record struct EstimatedMinutes(int Value) : IValueType<EstimatedMinutes, int>
{
    public static Result<EstimatedMinutes, InvalidData> Parse(int value) =>
        value is >= 1 and <= 15
            ? new EstimatedMinutes(value)
            : new InvalidData
            {
                FieldName = nameof(EstimatedMinutes),
                InnerValue = value,
                Constraint = new InvalidDataConstraint.GenericError(),
            };
}
