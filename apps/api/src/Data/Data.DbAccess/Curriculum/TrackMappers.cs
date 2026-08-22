using Milese.Common.Types.Entities.Curriculum;
using Milese.Data.Db.Curriculum;

namespace Milese.Data.DbAccess.Curriculum;

public static class TrackMappers
{
    public static TrackBo ToBo(this TrackDb db) => new()
    {
        Id = db.Id,
        Title = db.Title,
        Order = db.Order,
    };

    public static TrackDb ToDb(this TrackBo bo) => new()
    {
        Id = bo.Id,
        Title = bo.Title,
        Order = bo.Order,
    };
}
