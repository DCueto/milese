using System;
using System.Threading.Tasks;
using Milese.Common.Shared;

namespace Milese.Common.Types.ValueTypes.Identity;

public readonly record struct UserId(int Value) : IIdValueType<UserId>
{
    public static string FieldName => nameof(UserId);

    public static Result<UserId, InvalidData> Parse(int value) =>
        ValueTypeParser.StrictlyPositive<UserId>(value, FieldName);

    public static Task<Result<UserId, InvalidData>> ParseAsync(int value, Func<UserId, Task<bool>> exists) =>
        ValueTypeParser.ParseIdAsync(value, FieldName, exists);
}
