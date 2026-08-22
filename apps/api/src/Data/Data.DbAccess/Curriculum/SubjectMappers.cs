using Milese.Common.Types.Entities.Curriculum;
using Milese.Data.Db.Curriculum;

namespace Milese.Data.DbAccess.Curriculum;

public static class SubjectMappers
{
    public static SubjectBo ToBo(this SubjectDb db) => new()
    {
        Id = db.Id,
        TrackId = db.TrackId,
        Title = db.Title,
        Order = db.Order,
    };

    public static SubjectDb ToDb(this SubjectBo bo) => new()
    {
        Id = bo.Id,
        TrackId = bo.TrackId,
        Title = bo.Title,
        Order = bo.Order,
    };
}
