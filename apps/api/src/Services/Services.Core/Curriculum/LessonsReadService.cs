using System.Collections.Generic;
using System.Threading.Tasks;
using Milese.Common.Shared;
using Milese.Common.Types.Entities.Curriculum;
using Milese.Common.Types.ValueTypes.Identity;
using Milese.Data.DbAccess.Curriculum;

namespace Milese.Services.Core.Curriculum;

public sealed class LessonsReadService
{
    private readonly LessonsReadDataAccess lessonsReadDataAccess;

    public LessonsReadService(LessonsReadDataAccess lessonsReadDataAccess) =>
        this.lessonsReadDataAccess = lessonsReadDataAccess;

    public async Task<Result<LessonBo, InvalidData>> GetByIdAsync(LessonId id)
    {
        var lesson = await lessonsReadDataAccess.TryGetByIdAsync(id);

        return lesson is not null
            ? lesson
            : new InvalidData
            {
                FieldName = LessonId.FieldName,
                InnerValue = id.Value,
                Constraint = new InvalidDataConstraint.IdNotFound(),
            };
    }

    public Task<IReadOnlyCollection<LessonBo>> ListByConceptAsync(ConceptId conceptId) =>
        lessonsReadDataAccess.ListByConceptAsync(conceptId);
}
