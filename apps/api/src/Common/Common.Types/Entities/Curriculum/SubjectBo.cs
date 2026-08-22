using Milese.Common.Types.ValueTypes.Curriculum;
using Milese.Common.Types.ValueTypes.Identity;

namespace Milese.Common.Types.Entities.Curriculum;

public sealed record SubjectBo
{
    public required SubjectId Id { get; init; }

    public required TrackId TrackId { get; init; }

    public required SubjectTitle Title { get; init; }

    public required SortOrder Order { get; init; }
}
