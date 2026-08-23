using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Milese.Data.Db;

namespace Milese.Api.Rest.Tests.Http;

public sealed class RealAuthenticationTests : HttpIntegrationTest
{
    private const string Issuer = "https://milese-test.example.com/";
    private const string Audience = "api://milese-test";

    private static readonly SymmetricSecurityKey SigningKey = new(RandomNumberGenerator.GetBytes(32));

    [Test]
    public async Task A_request_with_no_token_is_rejected_with_401()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/lessons?conceptId=1");

        await Assert.That(response.StatusCode).IsStrictlyEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task A_request_with_an_expired_token_is_rejected_with_401()
    {
        var client = CreateClientWithFixedJwtSigningKey(Issuer, Audience, SigningKey);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateToken(expired: true, audience: Audience));

        var response = await client.GetAsync("/api/lessons?conceptId=1");

        await Assert.That(response.StatusCode).IsStrictlyEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task A_request_with_the_wrong_audience_is_rejected_with_401()
    {
        var client = CreateClientWithFixedJwtSigningKey(Issuer, Audience, SigningKey);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateToken(expired: false, audience: "api://someone-else"));

        var response = await client.GetAsync("/api/lessons?conceptId=1");

        await Assert.That(response.StatusCode).IsStrictlyEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task A_valid_real_scheme_token_with_identity_claims_is_accepted_and_JIT_provisions_a_user()
    {
        var client = CreateClientWithFixedJwtSigningKey(Issuer, Audience, SigningKey);
        var entraObjectId = Guid.NewGuid();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateToken(expired: false, audience: Audience, entraObjectId));

        var response = await client.GetAsync("/api/lessons?conceptId=1");

        await Assert.That(response.StatusCode).IsStrictlyEqualTo(HttpStatusCode.OK);

        await using var context = await DbContextFactory.CreateDbContextAsync();
        var exists = await context.Users.AnyAsync(u => u.EntraObjectId == entraObjectId);
        await Assert.That(exists).IsTrue();
    }

    private static string CreateToken(bool expired, string audience, Guid? entraObjectId = null)
    {
        var handler = new JwtSecurityTokenHandler();
        var now = DateTime.UtcNow;
        var claims = new[]
        {
            new Claim("oid", (entraObjectId ?? Guid.NewGuid()).ToString()),
            new Claim("email", "real-scheme-learner@example.com"),
            new Claim("name", "Real Scheme Learner"),
        };
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: audience,
            claims: claims,
            notBefore: expired ? now.AddMinutes(-10) : now.AddMinutes(-1),
            expires: expired ? now.AddMinutes(-5) : now.AddMinutes(5),
            signingCredentials: new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256));

        return handler.WriteToken(token);
    }
}
