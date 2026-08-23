using System;
using System.Threading;
using System.Threading.Tasks;
using Milese.Common.Types.Entities.Identity;
using Milese.Common.Types.ValueTypes.Identity;
using Milese.Data.DbAccess.Identity;
using Milese.Tests.Integration;

namespace Milese.Data.DbAccess.Tests.Identity;

public sealed class UpdateProfileAsyncTests : DatabaseIntegrationTest
{
    [Test]
    public async Task Persists_a_refreshed_Email_and_DisplayName_for_an_existing_user()
    {
        var updateDataAccess = new UsersUpdateDataAccess(DbContextFactory, CancellationToken.None);
        var readDataAccess = new UsersReadDataAccess(DbContextFactory, CancellationToken.None);
        var entraObjectId = Guid.NewGuid();

        var created = await updateDataAccess.CreateAsync(
            entraObjectId,
            new Email { Value = "old@example.com" },
            new DisplayName { Value = "Old Name" });

        await updateDataAccess.UpdateProfileAsync(new UserBo
        {
            Id = created.Id,
            EntraObjectId = created.EntraObjectId,
            Email = new Email { Value = "new@example.com" },
            DisplayName = new DisplayName { Value = "New Name" },
        });

        var fetched = await readDataAccess.FindByEntraObjectIdAsync(entraObjectId);

        await Assert.That(fetched).IsNotNull();
        await Assert.That(fetched!.Email.Value).IsStrictlyEqualTo("new@example.com");
        await Assert.That(fetched.DisplayName.Value).IsStrictlyEqualTo("New Name");
    }
}
