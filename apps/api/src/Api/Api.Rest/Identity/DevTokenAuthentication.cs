using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Milese.Api.Rest.Identity;

internal static class DevTokenAuthentication
{
    public const string SchemeName = "DevToken";
    public const string Issuer = "milese-dev";
    public const string Audience = "milese-dev";

    public static readonly SymmetricSecurityKey SigningKey = new(RandomNumberGenerator.GetBytes(32));
}
