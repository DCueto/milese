using Milese.Common.Types.ValueTypes.Curriculum;
using Milese.Common.Types.ValueTypes.Identity;

namespace Milese.Common.Types.Entities.Curriculum;

public sealed record LessonBo
{
    public required LessonId Id { get; init; }

    public required ConceptId ConceptId { get; init; }

    public required LessonTitle Title { get; init; }

    public required EstimatedMinutes EstimatedMinutes { get; init; }

    public required SortOrder Order { get; init; }
}
