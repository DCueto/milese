using System;
using System.Threading.Tasks;
using Milese.Common.Types.Entities.Identity;
using Milese.Common.Types.ValueTypes.Identity;
using Milese.Data.DbAccess.Identity;

namespace Milese.Services.Core.Identity;

public sealed class UsersUpdateService
{
    private readonly UsersReadDataAccess usersReadDataAccess;
    private readonly UsersUpdateDataAccess usersUpdateDataAccess;

    public UsersUpdateService(UsersReadDataAccess usersReadDataAccess, UsersUpdateDataAccess usersUpdateDataAccess)
    {
        this.usersReadDataAccess = usersReadDataAccess;
        this.usersUpdateDataAccess = usersUpdateDataAccess;
    }

    public async Task<UserBo> ResolveAsync(Guid entraObjectId, Email email, DisplayName displayName)
    {
        var existing = await usersReadDataAccess.FindByEntraObjectIdAsync(entraObjectId);
        if (existing is null)
            return await usersUpdateDataAccess.CreateAsync(entraObjectId, email, displayName);

        if (existing.Email == email && existing.DisplayName == displayName)
            return existing;

        var refreshed = new UserBo
        {
            Id = existing.Id,
            EntraObjectId = existing.EntraObjectId,
            Email = email,
            DisplayName = displayName,
        };
        await usersUpdateDataAccess.UpdateProfileAsync(refreshed);

        return refreshed;
    }
}
