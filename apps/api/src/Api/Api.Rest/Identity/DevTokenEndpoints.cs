using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.IdentityModel.Tokens;

namespace Milese.Api.Rest.Identity;

internal static class DevTokenEndpoints
{
    internal static IEndpointRouteBuilder MapDevTokenEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet("/dev/token", (int? userId) => Results.Ok(Mint(userId is > 0 ? userId.Value : 1)))
            .AllowAnonymous();

        return endpoints;
    }

    private static DevTokenResponse Mint(int userId)
    {
        var entraObjectId = new Guid($"00000000-0000-0000-0000-{userId:D12}");
        var claims = new[]
        {
            new Claim("oid", entraObjectId.ToString()),
            new Claim("email", $"dev-user-{userId}@milese.local"),
            new Claim("name", $"Dev User {userId}"),
        };

        var token = new JwtSecurityToken(
            issuer: DevTokenAuthentication.Issuer,
            audience: DevTokenAuthentication.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: new SigningCredentials(DevTokenAuthentication.SigningKey, SecurityAlgorithms.HmacSha256));

        return new DevTokenResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            UserId = userId,
        };
    }
}

internal sealed record DevTokenResponse
{
    public required string Token { get; init; }

    public required int UserId { get; init; }
}
