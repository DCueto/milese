using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;

namespace Milese.Api.Rest.Identity;

internal static class DevOrEntraAuthenticationScheme
{
    public const string Name = "EntraOrDev";

    public static string SelectScheme(HttpContext context)
    {
        const string bearerPrefix = "Bearer ";
        var header = context.Request.Headers.Authorization.Count > 0
            ? context.Request.Headers.Authorization[0]
            : null;
        if (header is null || !header.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
            return JwtBearerDefaults.AuthenticationScheme;

        var token = header[bearerPrefix.Length..];
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
            return JwtBearerDefaults.AuthenticationScheme;

        var jwt = handler.ReadJwtToken(token);
        return jwt.Issuer == DevTokenAuthentication.Issuer
            ? DevTokenAuthentication.SchemeName
            : JwtBearerDefaults.AuthenticationScheme;
    }
}
