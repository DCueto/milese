using System.Threading;
using Microsoft.EntityFrameworkCore;
using Milese.Data.DbAccess.Identity;
using Milese.Data.Db;
using Milese.Services.Core.Identity;

namespace Milese.Services.Core.Tests.Identity;

internal static class UsersArrange
{
    public static UsersUpdateService BuildUpdateService(IDbContextFactory<MileseDbContext> dbContextFactory) =>
        new(
            new UsersReadDataAccess(dbContextFactory, CancellationToken.None),
            new UsersUpdateDataAccess(dbContextFactory, CancellationToken.None));
}
