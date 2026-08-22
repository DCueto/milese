using Milese.Common.Types.Entities.Curriculum;
using Milese.Data.Db.Curriculum;

namespace Milese.Data.DbAccess.Curriculum;

public static class LessonMappers
{
    public static LessonBo ToBo(this LessonDb db) => new()
    {
        Id = db.Id,
        ConceptId = db.ConceptId,
        Title = db.Title,
        EstimatedMinutes = db.EstimatedMinutes,
        Order = db.Order,
    };

    public static LessonDb ToDb(this LessonBo bo) => new()
    {
        Id = bo.Id,
        ConceptId = bo.ConceptId,
        Title = bo.Title,
        EstimatedMinutes = bo.EstimatedMinutes,
        Order = bo.Order,
    };
}
