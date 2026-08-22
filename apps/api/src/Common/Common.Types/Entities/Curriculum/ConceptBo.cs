using Milese.Common.Types.ValueTypes.Curriculum;
using Milese.Common.Types.ValueTypes.Identity;

namespace Milese.Common.Types.Entities.Curriculum;

public sealed record ConceptBo
{
    public required ConceptId Id { get; init; }

    public required SubjectId SubjectId { get; init; }

    public required ConceptTitle Title { get; init; }

    public required SortOrder Order { get; init; }
}
