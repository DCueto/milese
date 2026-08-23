using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Milese.Common.Types.Entities.Identity;
using Milese.Data.Db;

namespace Milese.Data.DbAccess.Identity;

public sealed class UsersReadDataAccess
{
    private readonly IDbContextFactory<MileseDbContext> dbCntxFactory;
    private readonly CancellationToken cancellationToken;

    public UsersReadDataAccess(
        IDbContextFactory<MileseDbContext> dbCntxFactory,
        CancellationToken cancellationToken)
    {
        this.dbCntxFactory = dbCntxFactory;
        this.cancellationToken = cancellationToken;
    }

    public async Task<UserBo?> FindByEntraObjectIdAsync(Guid entraObjectId)
    {
        await using var ctx = await dbCntxFactory.CreateDbContextAsync(cancellationToken);

        var db = await ctx.Users.SingleOrDefaultAsync(x => x.EntraObjectId == entraObjectId, cancellationToken);
        return db?.ToBo();
    }
}
