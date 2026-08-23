using System;
using System.Threading;
using System.Threading.Tasks;
using Milese.Common.Types.ValueTypes.Identity;
using Milese.Data.DbAccess.Identity;
using Milese.Tests.Integration;

namespace Milese.Data.DbAccess.Tests.Identity;

public sealed class CreateAsyncTests : DatabaseIntegrationTest
{
    [Test]
    public async Task Creates_a_user_and_assigns_it_a_positive_id()
    {
        var updateDataAccess = new UsersUpdateDataAccess(DbContextFactory, CancellationToken.None);
        var entraObjectId = Guid.NewGuid();

        var user = await updateDataAccess.CreateAsync(
            entraObjectId,
            new Email { Value = "learner@example.com" },
            new DisplayName { Value = "Daniel Cueto" });

        await Assert.That(user.Id.Value).IsGreaterThan(0);
        await Assert.That(user.EntraObjectId).IsStrictlyEqualTo(entraObjectId);
        await Assert.That(user.Email.Value).IsStrictlyEqualTo("learner@example.com");
        await Assert.That(user.DisplayName.Value).IsStrictlyEqualTo("Daniel Cueto");
    }

    [Test]
    public async Task Created_user_is_readable_afterward_by_entra_object_id()
    {
        var updateDataAccess = new UsersUpdateDataAccess(DbContextFactory, CancellationToken.None);
        var readDataAccess = new UsersReadDataAccess(DbContextFactory, CancellationToken.None);
        var entraObjectId = Guid.NewGuid();

        var created = await updateDataAccess.CreateAsync(
            entraObjectId,
            new Email { Value = "another@example.com" },
            new DisplayName { Value = "Another Learner" });

        var fetched = await readDataAccess.FindByEntraObjectIdAsync(entraObjectId);

        await Assert.That(fetched).IsNotNull();
        await Assert.That(fetched!.Id).IsStrictlyEqualTo(created.Id);
        await Assert.That(fetched.DisplayName.Value).IsStrictlyEqualTo("Another Learner");
    }

    [Test]
    public async Task Two_concurrent_creates_for_the_same_EntraObjectId_resolve_to_one_row()
    {
        var first = new UsersUpdateDataAccess(DbContextFactory, CancellationToken.None);
        var second = new UsersUpdateDataAccess(DbContextFactory, CancellationToken.None);
        var readDataAccess = new UsersReadDataAccess(DbContextFactory, CancellationToken.None);
        var entraObjectId = Guid.NewGuid();

        var firstTask = first.CreateAsync(
            entraObjectId, new Email { Value = "racer@example.com" }, new DisplayName { Value = "Racer" });
        var secondTask = second.CreateAsync(
            entraObjectId, new Email { Value = "racer@example.com" }, new DisplayName { Value = "Racer" });
        var results = await Task.WhenAll(firstTask, secondTask);

        await Assert.That(results[0].Id).IsStrictlyEqualTo(results[1].Id);

        var fetched = await readDataAccess.FindByEntraObjectIdAsync(entraObjectId);
        await Assert.That(fetched).IsNotNull();
        await Assert.That(fetched!.Id).IsStrictlyEqualTo(results[0].Id);
    }
}
