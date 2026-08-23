using System;
using System.Threading.Tasks;
using Milese.Common.Types.ValueTypes.Identity;
using Milese.Tests.Integration;

namespace Milese.Services.Core.Tests.Identity;

public sealed class ResolveAsyncTests : DatabaseIntegrationTest
{
    [Test]
    public async Task Creates_a_new_user_when_no_row_matches_the_EntraObjectId()
    {
        var updateService = UsersArrange.BuildUpdateService(DbContextFactory);
        var entraObjectId = Guid.NewGuid();

        var user = await updateService.ResolveAsync(
            entraObjectId,
            new Email { Value = "learner@example.com" },
            new DisplayName { Value = "Learner One" });

        await Assert.That(user.Id.Value).IsGreaterThan(0);
        await Assert.That(user.EntraObjectId).IsStrictlyEqualTo(entraObjectId);
    }

    [Test]
    public async Task Reuses_the_existing_row_for_a_second_sign_in_with_the_same_EntraObjectId()
    {
        var updateService = UsersArrange.BuildUpdateService(DbContextFactory);
        var entraObjectId = Guid.NewGuid();

        var first = await updateService.ResolveAsync(
            entraObjectId,
            new Email { Value = "learner@example.com" },
            new DisplayName { Value = "Learner One" });
        var second = await updateService.ResolveAsync(
            entraObjectId,
            new Email { Value = "learner@example.com" },
            new DisplayName { Value = "Learner One" });

        await Assert.That(second.Id).IsStrictlyEqualTo(first.Id);
    }

    [Test]
    public async Task Refreshes_Email_and_DisplayName_from_the_latest_claims()
    {
        var updateService = UsersArrange.BuildUpdateService(DbContextFactory);
        var entraObjectId = Guid.NewGuid();

        var created = await updateService.ResolveAsync(
            entraObjectId,
            new Email { Value = "old@example.com" },
            new DisplayName { Value = "Old Name" });
        var refreshed = await updateService.ResolveAsync(
            entraObjectId,
            new Email { Value = "new@example.com" },
            new DisplayName { Value = "New Name" });

        await Assert.That(refreshed.Id).IsStrictlyEqualTo(created.Id);
        await Assert.That(refreshed.Email.Value).IsStrictlyEqualTo("new@example.com");
        await Assert.That(refreshed.DisplayName.Value).IsStrictlyEqualTo("New Name");
    }
}
