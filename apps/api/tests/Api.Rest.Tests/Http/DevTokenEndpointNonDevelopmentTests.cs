using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Milese.Api.Rest.Tests.Http;

public sealed class DevTokenEndpointNonDevelopmentTests : HttpIntegrationTest
{
    [Test]
    public async Task An_unauthenticated_request_to_the_minting_endpoint_is_rejected_outside_Development()
    {
        var client = CreateClientForEnvironment("Production");

        var response = await client.GetAsync("/dev/token");

        await Assert.That(response.StatusCode).IsStrictlyEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task The_minting_endpoint_does_not_exist_outside_Development_even_for_an_authenticated_caller()
    {
        var client = CreateAuthenticatedClientForEnvironment("Production");

        var response = await client.GetAsync("/dev/token");

        await Assert.That(response.StatusCode).IsStrictlyEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task A_token_shaped_like_the_dev_endpoints_output_is_rejected_outside_Development()
    {
        var client = CreateClientForEnvironment("Production");
        var handler = new JwtSecurityTokenHandler();
        var signingKey = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32));
        var devShapedToken = new JwtSecurityToken(
            issuer: "milese-dev",
            audience: "milese-dev",
            claims: [new Claim("oid", "00000000-0000-0000-0000-000000000001")],
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", handler.WriteToken(devShapedToken));
        var response = await client.GetAsync("/api/lessons?conceptId=1");

        await Assert.That(response.StatusCode).IsStrictlyEqualTo(HttpStatusCode.Unauthorized);
    }
}
