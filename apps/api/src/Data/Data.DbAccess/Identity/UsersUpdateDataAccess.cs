using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Milese.Common.Types.Entities.Identity;
using Milese.Common.Types.ValueTypes.Identity;
using Milese.Data.Db;
using Milese.Data.Db.Identity;

namespace Milese.Data.DbAccess.Identity;

public sealed class UsersUpdateDataAccess
{
    private readonly IDbContextFactory<MileseDbContext> dbCntxFactory;
    private readonly CancellationToken cancellationToken;

    public UsersUpdateDataAccess(
        IDbContextFactory<MileseDbContext> dbCntxFactory,
        CancellationToken cancellationToken)
    {
        this.dbCntxFactory = dbCntxFactory;
        this.cancellationToken = cancellationToken;
    }

    public async Task<UserBo> CreateAsync(Guid entraObjectId, Email email, DisplayName displayName)
    {
        await using var ctx = await dbCntxFactory.CreateDbContextAsync(cancellationToken);

        var db = new UserDb
        {
            EntraObjectId = entraObjectId,
            Email = email,
            DisplayName = displayName,
        };

        await ctx.Users.AddAsync(db, cancellationToken);

        try
        {
            await ctx.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            await using var conflictCtx = await dbCntxFactory.CreateDbContextAsync(cancellationToken);
            var existing = await conflictCtx.Users.SingleAsync(x => x.EntraObjectId == entraObjectId, cancellationToken);
            return existing.ToBo();
        }

        return db.ToBo();
    }

    public async Task UpdateProfileAsync(UserBo user)
    {
        await using var ctx = await dbCntxFactory.CreateDbContextAsync(cancellationToken);

        var db = await ctx.Users
            .AsTracking()
            .SingleAsync(x => x.Id == user.Id, cancellationToken);

        db.Email = user.Email;
        db.DisplayName = user.DisplayName;

        await ctx.SaveChangesAsync(cancellationToken);
    }
}
