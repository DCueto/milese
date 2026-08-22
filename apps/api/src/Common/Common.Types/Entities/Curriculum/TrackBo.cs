using Milese.Common.Types.ValueTypes.Curriculum;
using Milese.Common.Types.ValueTypes.Identity;

namespace Milese.Common.Types.Entities.Curriculum;

public sealed class TrackBo
{
    public required TrackId Id { get; init; }

    public required TrackTitle Title { get; init; }

    public required SortOrder Order { get; init; }
}
