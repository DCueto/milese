using System;
using System.Threading.Tasks;
using Milese.Common.Shared;

namespace Milese.Common.Types.ValueTypes.Identity;

public sealed class TrackId : IIdValueType<TrackId>
{
    public int Value { get; init; }

    public static string FieldName => nameof(TrackId);

    public static Result<TrackId, InvalidData> Parse(int value) =>
        ValueTypeParser.StrictlyPositive<TrackId>(value, FieldName);

    public static Task<Result<TrackId, InvalidData>> ParseAsync(int value, Func<TrackId, Task<bool>> exists) =>
        ValueTypeParser.ParseIdAsync(value, FieldName, exists);
}
