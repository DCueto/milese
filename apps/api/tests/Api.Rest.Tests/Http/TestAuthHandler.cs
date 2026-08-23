using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Milese.Api.Rest.Tests.Http;

public sealed class TestAuthHandlerOptions : AuthenticationSchemeOptions
{
    public const string DefaultEntraObjectId = "11111111-1111-1111-1111-111111111111";
    public const string DefaultEmail = "learner@example.com";
    public const string DefaultDisplayName = "Test Learner";

    public string EntraObjectId { get; set; } = DefaultEntraObjectId;

    public string Email { get; set; } = DefaultEmail;

    public string DisplayName { get; set; } = DefaultDisplayName;
}

public sealed class TestAuthHandler : AuthenticationHandler<TestAuthHandlerOptions>
{
    public const string SchemeName = "Test";

    public TestAuthHandler(IOptionsMonitor<TestAuthHandlerOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity(
            [
                new Claim("oid", Options.EntraObjectId),
                new Claim("email", Options.Email),
                new Claim("name", Options.DisplayName),
            ],
            SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
