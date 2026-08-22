using Milese.Common.Types.Entities.Curriculum;
using Milese.Data.Db.Curriculum;

namespace Milese.Data.DbAccess.Curriculum;

public static class ConceptMappers
{
    public static ConceptBo ToBo(this ConceptDb db) => new()
    {
        Id = db.Id,
        SubjectId = db.SubjectId,
        Title = db.Title,
        Order = db.Order,
    };

    public static ConceptDb ToDb(this ConceptBo bo) => new()
    {
        Id = bo.Id,
        SubjectId = bo.SubjectId,
        Title = bo.Title,
        Order = bo.Order,
    };
}
