using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Milese.Common.Types.ValueTypes.Identity;
using Milese.Services.Core.Identity;

namespace Milese.Api.Rest.Identity;

public sealed class UserProvisioningMiddleware
{
    private readonly RequestDelegate next;

    public UserProvisioningMiddleware(RequestDelegate next) => this.next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var entraObjectIdClaim = context.User.FindFirstValue("oid");
            var email = Email.Parse(context.User.FindFirstValue("email"));
            var displayName = DisplayName.Parse(context.User.FindFirstValue("name"));

            if (!Guid.TryParse(entraObjectIdClaim, out var entraObjectId) || email.IsFailure || displayName.IsFailure)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var usersUpdateService = context.RequestServices.GetRequiredService<UsersUpdateService>();
            await usersUpdateService.ResolveAsync(entraObjectId, email.Value, displayName.Value);
        }

        await next(context);
    }
}
