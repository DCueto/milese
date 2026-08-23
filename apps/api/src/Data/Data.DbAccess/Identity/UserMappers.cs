using Milese.Common.Types.Entities.Identity;
using Milese.Data.Db.Identity;

namespace Milese.Data.DbAccess.Identity;

public static class UserMappers
{
    public static UserBo ToBo(this UserDb db) => new()
    {
        Id = db.Id,
        EntraObjectId = db.EntraObjectId,
        Email = db.Email,
        DisplayName = db.DisplayName,
    };

    public static UserDb ToDb(this UserBo bo) => new()
    {
        Id = bo.Id,
        EntraObjectId = bo.EntraObjectId,
        Email = bo.Email,
        DisplayName = bo.DisplayName,
    };
}
