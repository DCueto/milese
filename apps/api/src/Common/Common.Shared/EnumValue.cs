using System;

namespace Milese.Common.Shared;

public static class EnumValue
{
    public static Result<T, InvalidData> Parse<T>(T value) where T : struct, Enum =>
        Enum.IsDefined(value) ?
            value :
            new InvalidData
            {
                Constraint = new InvalidDataConstraint.EnumNotDefined(),
                FieldName = typeof(T).Name,
                InnerValue = value
            };

}
