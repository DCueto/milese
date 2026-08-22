using System.Threading.Tasks;
using Milese.Common.Types.Entities.Curriculum;
using Milese.Common.Types.ValueTypes.Curriculum;
using Milese.Common.Types.ValueTypes.Identity;
using Milese.Data.DbAccess.Curriculum;

namespace Milese.Services.Core.Curriculum;

public sealed class LessonsUpdateService
{
    private readonly LessonsUpdateDataAccess lessonsUpdateDataAccess;

    public LessonsUpdateService(LessonsUpdateDataAccess lessonsUpdateDataAccess) =>
        this.lessonsUpdateDataAccess = lessonsUpdateDataAccess;

    public Task<LessonBo> CreateAsync(
        ConceptId conceptId,
        LessonTitle title,
        EstimatedMinutes estimatedMinutes,
        SortOrder order) =>
        lessonsUpdateDataAccess.CreateAsync(conceptId, title, estimatedMinutes, order);

    public Task UpdateAsync(LessonBo lesson) =>
        lessonsUpdateDataAccess.UpdateAsync(lesson);
}
