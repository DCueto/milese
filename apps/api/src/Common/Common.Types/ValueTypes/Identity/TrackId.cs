using System;
using System.Threading.Tasks;
using Milese.Common.Shared;

namespace Milese.Common.Types.ValueTypes.Identity;

public readonly record struct TrackId(int Value) : IIdValueType<TrackId>
{
    public static string FieldName => nameof(TrackId);

    public static Result<TrackId, InvalidData> Parse(int value) =>
        ValueTypeParser.StrictlyPositive<TrackId>(value, FieldName);

    public static Task<Result<TrackId, InvalidData>> ParseAsync(int value, Func<TrackId, Task<bool>> exists) =>
        ValueTypeParser.ParseIdAsync(value, FieldName, exists);
}
