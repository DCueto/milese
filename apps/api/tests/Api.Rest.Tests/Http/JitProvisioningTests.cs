using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Milese.Data.Db;

namespace Milese.Api.Rest.Tests.Http;

public sealed class JitProvisioningTests : HttpIntegrationTest
{
    [Test]
    public async Task Two_authenticated_requests_with_the_same_EntraObjectId_resolve_to_the_same_UserDb_row()
    {
        const string entraObjectId = "33333333-3333-3333-3333-333333333333";
        var client = CreateAuthenticatedClient(entraObjectId);

        var first = await client.GetAsync("/api/lessons?conceptId=1");
        var second = await client.GetAsync("/api/lessons?conceptId=1");

        await Assert.That(first.StatusCode).IsStrictlyEqualTo(HttpStatusCode.OK);
        await Assert.That(second.StatusCode).IsStrictlyEqualTo(HttpStatusCode.OK);

        await using var context = await DbContextFactory.CreateDbContextAsync();
        var userCount = await context.Users.CountAsync(u => u.EntraObjectId == Guid.Parse(entraObjectId));
        await Assert.That(userCount).IsStrictlyEqualTo(1);
    }
}
