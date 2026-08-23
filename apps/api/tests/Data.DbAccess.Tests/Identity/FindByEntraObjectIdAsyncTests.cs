using System;
using System.Threading;
using System.Threading.Tasks;
using Milese.Data.DbAccess.Identity;
using Milese.Tests.Integration;

namespace Milese.Data.DbAccess.Tests.Identity;

public sealed class FindByEntraObjectIdAsyncTests : DatabaseIntegrationTest
{
    [Test]
    public async Task Returns_null_when_no_user_matches()
    {
        var readDataAccess = new UsersReadDataAccess(DbContextFactory, CancellationToken.None);

        var user = await readDataAccess.FindByEntraObjectIdAsync(Guid.NewGuid());

        await Assert.That(user).IsNull();
    }
}
