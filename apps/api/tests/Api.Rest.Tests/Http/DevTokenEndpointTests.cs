using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Milese.Data.Db;

namespace Milese.Api.Rest.Tests.Http;

public sealed class DevTokenEndpointTests : HttpIntegrationTest
{
    [Test]
    public async Task Defaults_to_dev_UserId_1_when_no_query_parameter_is_given()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/dev/token");

        await Assert.That(response.StatusCode).IsStrictlyEqualTo(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        await Assert.That(body.RootElement.GetProperty("userId").GetInt32()).IsStrictlyEqualTo(1);
    }

    [Test]
    public async Task A_minted_token_is_accepted_by_a_protected_endpoint_and_JIT_provisions_a_deterministic_user()
    {
        var client = CreateClient();

        var mintResponse = await client.GetAsync("/dev/token?userId=7");
        using var body = JsonDocument.Parse(await mintResponse.Content.ReadAsStringAsync());
        var token = body.RootElement.GetProperty("token").GetString();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.GetAsync("/api/lessons?conceptId=1");

        await Assert.That(response.StatusCode).IsStrictlyEqualTo(HttpStatusCode.OK);

        await using var context = await DbContextFactory.CreateDbContextAsync();
        var expectedEntraObjectId = new Guid("00000000-0000-0000-0000-000000000007");
        var exists = await context.Users.AnyAsync(u => u.EntraObjectId == expectedEntraObjectId);
        await Assert.That(exists).IsTrue();
    }
}
